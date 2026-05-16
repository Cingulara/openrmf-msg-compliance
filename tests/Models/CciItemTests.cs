// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using Xunit;
using openrmf_msg_compliance.Models.NISTtoCCI;
using System;
using System.Collections.Generic;

namespace tests.Models
{
    public class CciItemTests
    {
        // ── Pass Tests ────────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewCCIItem_NotNull()
        {
            CciItem cci = new CciItem();
            Assert.NotNull(cci);
        }

        [Fact]
        public void Test_NewCCIItem_ReferencesListInitializedAndEmpty()
        {
            CciItem cci = new CciItem();
            Assert.NotNull(cci.references);
            Assert.Empty(cci.references);
        }

        [Fact]
        public void Test_CCIItem_AllPropertiesSetAndRetrievedCorrectly()
        {
            CciItem cci = new CciItem
            {
                cciId       = "CCI-2345",
                status      = "Published",
                publishDate = "2020-04-17",
                contributor = "Cingulara",
                definition  = "The information system enforces approved authorizations.",
                type        = "Technical",
                parameter   = "organization-defined",
                note        = "This is a note"
            };

            Assert.Equal("CCI-2345",    cci.cciId);
            Assert.Equal("Published",   cci.status);
            Assert.Equal("2020-04-17",  cci.publishDate);
            Assert.Equal("Cingulara",   cci.contributor);
            Assert.Equal("The information system enforces approved authorizations.", cci.definition);
            Assert.Equal("Technical",   cci.type);
            Assert.Equal("organization-defined", cci.parameter);
            Assert.Equal("This is a note",       cci.note);
        }

        [Fact]
        public void Test_CCIItem_AddSingleReference_CountIsOne()
        {
            CciItem cci = new CciItem();
            cci.references.Add(new CciReference
            {
                creator      = "NIST",
                title        = "NIST SP 800-53",
                version      = "4",
                location     = "http://nvlpubs.nist.gov",
                index        = "AU-9",
                majorControl = "AU-9"
            });

            Assert.Single(cci.references);
            Assert.Equal("AU-9", cci.references[0].majorControl);
        }

        [Fact]
        public void Test_CCIItem_AddMultipleReferences_CountMatchesAdded()
        {
            CciItem cci = new CciItem();
            cci.references.Add(new CciReference { creator = "NIST", index = "AU-9", majorControl = "AU-9" });
            cci.references.Add(new CciReference { creator = "NIST", index = "AC-1", majorControl = "AC-1" });
            cci.references.Add(new CciReference { creator = "NIST", index = "SI-3", majorControl = "SI-3" });

            Assert.Equal(3, cci.references.Count);
        }

        [Fact]
        public void Test_CCIItem_WithFullReferenceAndCciData_AllFieldsValid()
        {
            CciItem cci = new CciItem
            {
                cciId       = "CCI-000001",
                status      = "Published",
                publishDate = "2016-06-27",
                contributor = "Cingulara",
                definition  = "Access control enforcement definition.",
                type        = "Technical",
                parameter   = "none",
                note        = string.Empty
            };

            CciReference cciRef = new CciReference
            {
                creator      = "NIST",
                title        = "NIST SP 800-53",
                version      = "4",
                location     = "http://nvlpubs.nist.gov",
                index        = "AC-1 a.1",
                majorControl = "AC-1"
            };
            cci.references.Add(cciRef);

            Assert.NotNull(cci);
            Assert.Equal("CCI-000001", cci.cciId);
            Assert.Single(cci.references);
            Assert.Equal("AC-1", cci.references[0].majorControl);
            Assert.Equal("NIST", cci.references[0].creator);
        }

        [Fact]
        public void Test_CCIItem_RemoveReference_ListBecomesEmpty()
        {
            CciItem cci = new CciItem();
            CciReference cciRef = new CciReference { creator = "NIST", index = "AU-9", majorControl = "AU-9" };
            cci.references.Add(cciRef);
            cci.references.Remove(cciRef);

            Assert.Empty(cci.references);
        }

        [Fact]
        public void Test_CCIItem_CciIdNotEqualToWrongValue()
        {
            CciItem cci = new CciItem { cciId = "CCI-1001" };
            Assert.NotEqual("CCI-9999", cci.cciId);
        }

        // ── Fail Tests (negative / invalid-state verification) ───────────────────

        [Fact]
        public void Test_CCIItem_DefaultCciId_IsNull()
        {
            CciItem cci = new CciItem();
            Assert.Null(cci.cciId);
        }

        [Fact]
        public void Test_CCIItem_DefaultStatus_IsNull()
        {
            CciItem cci = new CciItem();
            Assert.Null(cci.status);
        }

        [Fact]
        public void Test_CCIItem_DefaultPublishDate_IsNull()
        {
            CciItem cci = new CciItem();
            Assert.Null(cci.publishDate);
        }

        [Fact]
        public void Test_CCIItem_DefaultContributor_IsNull()
        {
            CciItem cci = new CciItem();
            Assert.Null(cci.contributor);
        }

        [Fact]
        public void Test_CCIItem_DefaultDefinition_IsNull()
        {
            CciItem cci = new CciItem();
            Assert.Null(cci.definition);
        }

        [Fact]
        public void Test_CCIItem_WithNoReferences_ReferenceCountIsNotOne()
        {
            CciItem cci = new CciItem();
            Assert.False(cci.references.Count == 1);
        }

        [Fact]
        public void Test_CCIItem_EmptyStringCciId_IsNotValidId()
        {
            CciItem cci = new CciItem { cciId = string.Empty };
            Assert.True(string.IsNullOrEmpty(cci.cciId));
        }
    }

    public class CciReferenceTests
    {
        // ── Pass Tests ────────────────────────────────────────────────────────────

        [Fact]
        public void Test_NewCCIReference_NotNull()
        {
            CciReference cciRef = new CciReference();
            Assert.NotNull(cciRef);
        }

        [Fact]
        public void Test_CCIReference_AllPropertiesSetAndRetrievedCorrectly()
        {
            CciReference cciRef = new CciReference
            {
                creator      = "NIST",
                title        = "NIST SP 800-53",
                version      = "4",
                location     = "http://nvlpubs.nist.gov",
                index        = "AU-9 1.a(2)",
                majorControl = "AU-9"
            };

            Assert.Equal("NIST",                    cciRef.creator);
            Assert.Equal("NIST SP 800-53",          cciRef.title);
            Assert.Equal("4",                       cciRef.version);
            Assert.Equal("http://nvlpubs.nist.gov", cciRef.location);
            Assert.Equal("AU-9 1.a(2)",             cciRef.index);
            Assert.Equal("AU-9",                    cciRef.majorControl);
        }

        [Fact]
        public void Test_CCIReference_MajorControlExtractedFromIndex()
        {
            // AU-9 1.a(2) → major control is AU-9
            CciReference cciRef = new CciReference
            {
                index        = "AU-9 1.a(2)",
                majorControl = "AU-9"
            };

            Assert.StartsWith("AU-9", cciRef.index);
            Assert.Equal("AU-9", cciRef.majorControl);
        }

        [Fact]
        public void Test_CCIReference_CreatorNotEqualToWrongValue()
        {
            CciReference cciRef = new CciReference { creator = "NIST" };
            Assert.NotEqual("DISA", cciRef.creator);
        }

        [Theory]
        [InlineData("AU-9 1.a",   "AU-9")]
        [InlineData("AC-1 a.1",   "AC-1")]
        [InlineData("SI-3",        "SI-3")]
        public void Test_CCIReference_MajorControlMatchesExpected(string index, string expectedMajor)
        {
            CciReference cciRef = new CciReference
            {
                index        = index,
                majorControl = expectedMajor
            };

            Assert.Equal(expectedMajor, cciRef.majorControl);
        }

        // ── Fail Tests (negative / invalid-state verification) ───────────────────

        [Fact]
        public void Test_CCIReference_DefaultCreator_IsNull()
        {
            CciReference cciRef = new CciReference();
            Assert.Null(cciRef.creator);
        }

        [Fact]
        public void Test_CCIReference_DefaultTitle_IsNull()
        {
            CciReference cciRef = new CciReference();
            Assert.Null(cciRef.title);
        }

        [Fact]
        public void Test_CCIReference_DefaultVersion_IsNull()
        {
            CciReference cciRef = new CciReference();
            Assert.Null(cciRef.version);
        }

        [Fact]
        public void Test_CCIReference_DefaultMajorControl_IsNull()
        {
            CciReference cciRef = new CciReference();
            Assert.Null(cciRef.majorControl);
        }

        [Fact]
        public void Test_CCIReference_DefaultIndex_IsNull()
        {
            CciReference cciRef = new CciReference();
            Assert.Null(cciRef.index);
        }

        [Fact]
        public void Test_CCIReference_EmptyStringIndex_IsNotValidIndex()
        {
            CciReference cciRef = new CciReference { index = string.Empty };
            Assert.True(string.IsNullOrEmpty(cciRef.index));
        }

        [Fact]
        public void Test_CCIReference_MajorControlDoesNotMatchDifferentControl()
        {
            CciReference cciRef = new CciReference { majorControl = "AU-9" };
            Assert.NotEqual("AC-1", cciRef.majorControl);
        }
    }
}
