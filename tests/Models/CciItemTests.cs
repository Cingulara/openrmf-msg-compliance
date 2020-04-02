using Xunit;
using openrmf_msg_compliance.Models.NISTtoCCI;
using System;

namespace tests.Models
{
    public class CciItemTests
    {
        [Fact]
        public void Test_NewCCIItemIsValid()
        {
            CciItem cci = new CciItem();
            Assert.True(cci != null);
        }
    
        [Fact]
        public void Test_CCIItemWithDataIsValid()
        {
            CciItem cci = new CciItem();
            
            cci.cciId  = "CCI-2345";
            cci.status = "myStatus";
            cci.publishDate = DateTime.Now.ToShortDateString();
            cci.contributor = "Cingulara";
            cci.definition = "This is my definition";
            cci.type = "thisType";
            cci.parameter = "myParams";
            cci.note = "man this is complicated!";

            // test things out
            Assert.True(cci != null);
            Assert.True (!string.IsNullOrEmpty(cci.cciId));
            Assert.True (!string.IsNullOrEmpty(cci.status));
            Assert.True (!string.IsNullOrEmpty(cci.publishDate));
            Assert.True (!string.IsNullOrEmpty(cci.contributor));
            Assert.True (!string.IsNullOrEmpty(cci.definition));
            Assert.True (!string.IsNullOrEmpty(cci.type));
            Assert.True (!string.IsNullOrEmpty(cci.parameter));
            Assert.True (!string.IsNullOrEmpty(cci.note));
            Assert.True (cci.references != null);
            Assert.True (cci.references.Count == 0);            
        }

        [Fact]
        public void Test_CCIItemWithReferenceDataIsValid()
        {
            CciItem cci = new CciItem();
            
            cci.cciId  = "CCI-2345";
            cci.status = "myStatus";
            cci.publishDate = DateTime.Now.ToShortDateString();
            cci.contributor = "Cingulara";
            cci.definition = "This is my definition";
            cci.type = "thisType";
            cci.parameter = "myParams";
            cci.note = "man this is complicated!";

            CciReference cciRef = new CciReference();
            
            cciRef.creator  = "NIST";
            cciRef.title = "This is my title here";
            cciRef.location = "My location";
            cciRef.index = "AU-9 1.a(2)";
            cci.references.Add(cciRef);

            // test things out
            Assert.True(cci != null);
            Assert.True (!string.IsNullOrEmpty(cci.cciId));
            Assert.True (!string.IsNullOrEmpty(cci.status));
            Assert.True (!string.IsNullOrEmpty(cci.publishDate));
            Assert.True (!string.IsNullOrEmpty(cci.contributor));
            Assert.True (!string.IsNullOrEmpty(cci.definition));
            Assert.True (!string.IsNullOrEmpty(cci.type));
            Assert.True (!string.IsNullOrEmpty(cci.parameter));
            Assert.True (!string.IsNullOrEmpty(cci.note));
            Assert.True (cci.references != null);
            Assert.True (cci.references.Count == 1);            
        }


        [Fact]
        public void Test_NewCCIReferenceIsValid()
        {
            CciReference cci = new CciReference();
            Assert.True(cci != null);
        }
    
        [Fact]
        public void Test_CCIReferenceWithDataIsValid()
        {
            CciReference cci = new CciReference();
            
            cci.creator  = "NIST";
            cci.title = "This is my title here";
            cci.location = "My location";
            cci.index = "AU-9 1.a(2)";
            cci.majorControl = "AU-9";

            // test things out
            Assert.True(cci != null);
            Assert.True (!string.IsNullOrEmpty(cci.creator));
            Assert.True (!string.IsNullOrEmpty(cci.title));
            Assert.True (!string.IsNullOrEmpty(cci.location));
            Assert.True (!string.IsNullOrEmpty(cci.index));
            Assert.True (!string.IsNullOrEmpty(cci.majorControl));
        }
    }
}
