// Copyright (c) Cingulara LLC 2019 and Tutela LLC 2019. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using System.Collections.Generic;
using openrmf_msg_compliance.Models.NISTtoCCI;
using System.IO;
using System.Xml;

namespace openrmf_msg_compliance.Classes
{
    public static class NistCciGenerator
    {
        /// <summary>
        /// Generate the list of CCI items from the NIST listing in the XML file included
        /// </summary>
        /// <returns>The list of CCI items for use in filtering</returns>
        public static List<CciItem> LoadNistToCci() {
            List<CciItem> cciList = new List<CciItem>();
            CciItem item; // the CCI item
            CciReference reference; // list of references
            int len = 0; // the length to find the major control piece
            XmlDocument xmlDoc = new XmlDocument();
            // get the file path for the CCI to NIST listing
            var ccipath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/U_CCI_List.xml";
            if (File.Exists(ccipath)) {
                xmlDoc.LoadXml(File.ReadAllText(ccipath));
                XmlNodeList itemList = xmlDoc.GetElementsByTagName("cci_item");
            
                foreach (XmlElement child in itemList) {
                    item = new CciItem();
                    // get all the main pieces of the XML record for this
                    foreach (XmlElement ccidata in child.ChildNodes) {
                        if (ccidata.Name == "status")
                            item.status = ccidata.InnerText;
                        else if (ccidata.Name == "publishdate")
                            item.publishDate = ccidata.InnerText;
                        else if (ccidata.Name == "contributor")
                            item.contributor = ccidata.InnerText;
                        else if (ccidata.Name == "definition")
                            item.definition = ccidata.InnerText;
                        else if (ccidata.Name == "type")
                            item.type = ccidata.InnerText;
                        else if (ccidata.Name == "parameter")
                            item.parameter = ccidata.InnerText;
                        else if (ccidata.Name == "note")
                            item.note = ccidata.InnerText;
                        else if (ccidata.Name == "references") {
                            // cycle through all the references
                            foreach (XmlElement cciref in ccidata.ChildNodes) {
                                reference = new CciReference();
                                foreach(XmlAttribute attr in cciref.Attributes) {
                                    if (attr.Name == "creator")
                                        reference.creator = attr.InnerText;
                                    else if (attr.Name == "title")
                                        reference.title = attr.InnerText;
                                    else if (attr.Name == "version")
                                        reference.version = attr.InnerText;
                                    else if (attr.Name == "location")
                                        reference.location = attr.InnerText;
                                    else if (attr.Name == "index") {
                                        reference.index = attr.InnerText;
                                        len = EndOfIndex(attr.InnerText);
                                        if( len > 0)
                                            reference.majorControl = attr.InnerText.Substring(0, len);
                                        else
                                            reference.majorControl = reference.index;
                                    }
                                }
                                item.references.Add(reference);
                            }
                        }
                    }
                    // get the CCI ID
                    if (child.Attributes.Count == 1) { 
                        item.cciId = child.Attributes[0].InnerText;
                    }
                    cciList.Add(item);
                }
            }
            return cciList;
        }

        /// <summary>
        // for the CCI index, pull out the major part of that separately
        // for instance AC-1.2 = AC-1
        //              AU-9 (a) = AU=9
        /// </summary>
        /// <param name="cciIndex">The index of the CCI you are looking for</param>
        /// <returns>The integer showing the NIST control</returns>
        private static int EndOfIndex(string cciIndex){
            int period = cciIndex.IndexOf(".");
            int space = cciIndex.IndexOf(" ");
            if (period < 0 && space < 0)
                return -1;
            else if (period < 0 && space > 0)
                return space;
            else if (space < 0 && period > 0)
                return period;
            else if (period < space)
                return period;
            else
                return space;
        }

    }
}