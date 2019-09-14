using System;
using openrmf_msg_compliance.Models.NISTtoCCI;
using System.Collections.Generic;
using System.Linq;

namespace openrmf_msg_compliance.Classes
{
    public static class CCIListGenerator 
    {
      public static List<string> GetCCIListing(string control, List<CciItem> cciItems)
      {
        try {
          // find all records where the NIST control is in the CciReference and add the parent CCI
          // return the list of strings
          List<string> cciList = new List<string>();
          foreach (CciItem item in cciItems) {
              if (item.references.Where(x => x.majorControl == control).FirstOrDefault() != null){
                cciList.Add(item.cciId); // add this listing
              }
          }

          return cciList.Distinct().ToList();
        }
        catch (Exception ex) {
            // log it here
            throw ex;
        }
      }
    }
}