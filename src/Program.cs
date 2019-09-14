﻿using System;
using System.Collections.Generic;
using NATS.Client;
using System.Text;
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

            // load this one time for the service
            // load the current XML document to get all CCI to NIST Major Controls
            List<CciItem> cciItems = NistCciGenerator.LoadNistToCci();

            // Create a new connection factory to create a connection.
            ConnectionFactory cf = new ConnectionFactory();

            // Creates a live connection to the default NATS Server running locally
            IConnection c = cf.CreateConnection(Environment.GetEnvironmentVariable("natsserverurl"));

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
            
            // The simple way to create an asynchronous subscriber
            // is to simply pass the event in.  Messages will start
            // arriving immediately.
            logger.Info("setting up the openRMF compliance CCI Listing subscription by filter");
            IAsyncSubscription asyncNewCCI = c.SubscribeAsync("openrmf.compliance.cci", getCCIListing);
            logger.Info("setting up the openRMF compliance CCI Listing by Control subscription by filter");
            IAsyncSubscription asyncNewCCIControl = c.SubscribeAsync("openrmf.compliance.cci.control", getCCIListingbyControl);
        }
    }
}