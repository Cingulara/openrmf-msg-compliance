﻿// Copyright (c) Cingulara LLC 2019 and Tutela LLC 2019. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using NATS.Client;
using System.Text;
using System.Linq;
using NLog;
using NLog.Config;
using openrmf_msg_compliance.Models.NISTtoCCI;
using openrmf_msg_compliance.Classes;
using Newtonsoft.Json;

namespace openrmf_msg_compliance
{
    class Program
    {
        static void Main(string[] args)
        {
            LogManager.Configuration = new XmlLoggingConfiguration($"{AppContext.BaseDirectory}nlog.config");
            var logger = LogManager.GetLogger("openrmf_msg_compliance");
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOGLEVEL"))) // default
                LogManager.Configuration.Variables["logLevel"] = "Warn";
            else if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("LOGLEVEL"))) {
                switch (Environment.GetEnvironmentVariable("LOGLEVEL"))
                {
                    case "5":
                        LogManager.Configuration.Variables["logLevel"] = "Critical";
                        break;
                    case "4":
                        LogManager.Configuration.Variables["logLevel"] = "Error";
                        break;
                    case "3":
                        LogManager.Configuration.Variables["logLevel"] = "Warn";
                        break;
                    case "2":
                        LogManager.Configuration.Variables["logLevel"] = "Info";
                        break;
                    case "1":
                        LogManager.Configuration.Variables["logLevel"] = "Debug";
                        break;
                    case "0":
                        LogManager.Configuration.Variables["logLevel"] = "Trace";
                        break;
                    default:
                        LogManager.Configuration.Variables["logLevel"] = "Warn";
                        break;
                }
            }
            LogManager.ReconfigExistingLoggers();

            // load this one time for the service
            // load the current XML document to get all CCI to NIST Major Controls
            List<CciItem> cciItems = NistCciGenerator.LoadNistToCci();

            // Create a new connection factory to create a connection.
            ConnectionFactory cf = new ConnectionFactory();
            // add the options for the server, reconnecting, and the handler events
            Options opts = ConnectionFactory.GetDefaultOptions();
            opts.MaxReconnect = -1;
            opts.ReconnectWait = 2000;
            opts.Name = "openrmf-msg-compliance";
            opts.Url = Environment.GetEnvironmentVariable("NATSSERVERURL");
            opts.AsyncErrorEventHandler += (sender, events) =>
            {
                logger.Info("NATS client error. Server: {0}. Message: {1}. Subject: {2}", events.Conn.ConnectedUrl, events.Error, events.Subscription.Subject);
            };

            opts.ServerDiscoveredEventHandler += (sender, events) =>
            {
                logger.Info("A new server has joined the cluster: {0}", events.Conn.DiscoveredServers);
            };

            opts.ClosedEventHandler += (sender, events) =>
            {
                logger.Info("Connection Closed: {0}", events.Conn.ConnectedUrl);
            };

            opts.ReconnectedEventHandler += (sender, events) =>
            {
                logger.Info("Connection Reconnected: {0}", events.Conn.ConnectedUrl);
            };

            opts.DisconnectedEventHandler += (sender, events) =>
            {
                logger.Info("Connection Disconnected: {0}", events.Conn.ConnectedUrl);
            };
            
            // Creates a live connection to the NATS Server with the above options
            IConnection c = cf.CreateConnection(opts);

            // get all the CCI items and send back
            EventHandler<MsgHandlerEventArgs> getCCIListing = (sender, natsargs) =>
            {
                try {
                    // print the message
                    logger.Info("New NATS subject: {0}", natsargs.Message.Subject);
                    logger.Info("New NATS data: {0}",Encoding.UTF8.GetString(natsargs.Message.Data));
                    string msg = JsonConvert.SerializeObject(cciItems);
                    // publish back out on the reply line to the calling publisher
                    logger.Info("Sending back compressed CCI listing Data for the Compliance API");
                    c.Publish(natsargs.Message.Reply, Encoding.UTF8.GetBytes(Compression.CompressString(msg)));
                    c.Flush(); // flush the line
                }
                catch (Exception ex) {
                    // log it here
                    logger.Error(ex, "Error retrieving CCI full listing for the Compliance API");
                }
            };

            // send back a full listing of CCI items based on the control passed in
            EventHandler<MsgHandlerEventArgs> getCCIListingbyControl = (sender, natsargs) =>
            {
                try {
                    // print the message
                    logger.Info("New NATS subject: {0}", natsargs.Message.Subject);
                    logger.Info("New NATS data: {0}",Encoding.UTF8.GetString(natsargs.Message.Data));
                    string control = Encoding.UTF8.GetString(natsargs.Message.Data);
                    string msg = "";
                    if (!string.IsNullOrEmpty(control)) {
                        // with the full listing, to get the filtered list based on this control
                        var result = CCIListGenerator.GetCCIListing(control, cciItems);
                        // now publish it back out w/ the reply subject
                        msg = JsonConvert.SerializeObject(result);
                    }
                    // publish back out on the reply line to the calling publisher
                    logger.Info("Sending back compressed CCI listing Data for the Read API for control=" + control);
                    c.Publish(natsargs.Message.Reply, Encoding.UTF8.GetBytes(Compression.CompressString(msg)));
                    c.Flush(); // flush the line
                }
                catch (Exception ex) {
                    // log it here
                    logger.Error(ex, "Error getting the CCI listing for the Read API on control {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                }
            };

            // send back a the references for a given CCI as well as its title
            EventHandler<MsgHandlerEventArgs> getCCIReferences = (sender, natsargs) =>
            {
                try {
                    // print the message
                    logger.Info("New NATS subject: {0}", natsargs.Message.Subject);
                    logger.Info("New NATS data: {0}",Encoding.UTF8.GetString(natsargs.Message.Data));
                    string cciNumber = Encoding.UTF8.GetString(natsargs.Message.Data);
                    string msg = "";
                    if (!string.IsNullOrEmpty(cciNumber)) {
                        // with the full listing, to get the CCI item
                        var result = cciItems.Where(x => x.cciId == cciNumber).FirstOrDefault();
                        if (result != null) {
                            // now publish it back out w/ the reply subject
                            msg = JsonConvert.SerializeObject(result);
                        }
                    }
                    // publish back out on the reply line to the calling publisher
                    logger.Info("Sending back compressed CCI Reference Data for the Read API for CCI " + cciNumber);
                    c.Publish(natsargs.Message.Reply, Encoding.UTF8.GetBytes(Compression.CompressString(msg)));
                    c.Flush(); // flush the line
                }
                catch (Exception ex) {
                    // log it here
                    logger.Error(ex, "Error getting the CCI listing for the Read API on control {0}", Encoding.UTF8.GetString(natsargs.Message.Data));
                }
            };
            
            // The simple way to create an asynchronous subscriber
            // is to simply pass the event in.  Messages will start
            // arriving immediately.
            logger.Info("setting up the OpenRMF compliance CCI Listing subscription by filter");
            IAsyncSubscription asyncNewCCI = c.SubscribeAsync("openrmf.compliance.cci", getCCIListing);
            logger.Info("setting up the OpenRMF compliance CCI Listing by Control subscription by filter");
            IAsyncSubscription asyncNewCCIControl = c.SubscribeAsync("openrmf.compliance.cci.control", getCCIListingbyControl);
            logger.Info("setting up the OpenRMF compliance CCI References");
            IAsyncSubscription asyncNewCCIReferences = c.SubscribeAsync("openrmf.compliance.cci.references", getCCIReferences);
        }
    }
}
