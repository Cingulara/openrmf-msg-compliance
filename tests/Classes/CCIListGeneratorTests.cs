// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using Xunit;
using openrmf_msg_compliance.Classes;
using openrmf_msg_compliance.Models.NISTtoCCI;
using System.Collections.Generic;

namespace tests.Classes
{
    public class CCIListGeneratorTests
    {
        // ── Helpers ───────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds a small, deterministic CCI item list for use across tests.
        /// CCI-001 → AU-9, CCI-002 → AC-1, CCI-003 → AC-1 and SI-3, CCI-004 → no references.
        /// </summary>
        private static List<CciItem> BuildTestCciList()
        {
            return new List<CciItem>
            {
                new CciItem
                {
                    cciId = "CCI-001",
                    references = new List<CciReference>
                    {
                        new CciReference { creator = "NIST", index = "AU-9", majorControl = "AU-9" }
                    }
                },
                new CciItem
                {
                    cciId = "CCI-002",
                    references = new List<CciReference>
                    {
                        new CciReference { creator = "NIST", index = "AC-1 a.1", majorControl = "AC-1" }
                    }
                },
                new CciItem
                {
                    cciId = "CCI-003",
                    references = new List<CciReference>
                    {
                        new CciReference { creator = "NIST", index = "AC-1 a.2", majorControl = "AC-1" },
                        new CciReference { creator = "NIST", index = "SI-3",      majorControl = "SI-3" }
                    }
                },
                new CciItem
                {
                    cciId = "CCI-004",
                    references = new List<CciReference>()   // no references
                }
            };
        }

        // ── Pass Tests ────────────────────────────────────────────────────────────

        [Fact]
        public void Test_GetCCIListing_SingleMatchingControl_ReturnsOneCci()
        {
            var cciItems = BuildTestCciList();
            var result   = CCIListGenerator.GetCCIListing("AU-9", cciItems);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("CCI-001", result[0]);
        }

        [Fact]
        public void Test_GetCCIListing_ControlMatchedByMultipleItems_ReturnsBothCcis()
        {
            var cciItems = BuildTestCciList();
            var result   = CCIListGenerator.GetCCIListing("AC-1", cciItems);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("CCI-002", result);
            Assert.Contains("CCI-003", result);
        }

        [Fact]
        public void Test_GetCCIListing_EmptyCciItemList_ReturnsEmptyList()
        {
            var result = CCIListGenerator.GetCCIListing("AU-9", new List<CciItem>());

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Test_GetCCIListing_ItemWithTwoControls_IncludedForBothControls()
        {
            // CCI-003 has both AC-1 and SI-3 references
            var cciItems = BuildTestCciList();

            var acResult = CCIListGenerator.GetCCIListing("AC-1", cciItems);
            var siResult = CCIListGenerator.GetCCIListing("SI-3", cciItems);

            Assert.Contains("CCI-003", acResult);
            Assert.Contains("CCI-003", siResult);
        }

        [Fact]
        public void Test_GetCCIListing_DuplicateCciIds_ReturnsDistinctList()
        {
            // Two items with the same cciId that both reference the same control
            var cciItems = new List<CciItem>
            {
                new CciItem
                {
                    cciId = "CCI-DUP",
                    references = new List<CciReference>
                    {
                        new CciReference { creator = "NIST", index = "AU-9", majorControl = "AU-9" }
                    }
                },
                new CciItem
                {
                    cciId = "CCI-DUP",
                    references = new List<CciReference>
                    {
                        new CciReference { creator = "NIST", index = "AU-9", majorControl = "AU-9" }
                    }
                }
            };

            var result = CCIListGenerator.GetCCIListing("AU-9", cciItems);

            Assert.Single(result);
            Assert.Equal("CCI-DUP", result[0]);
        }

        [Fact]
        public void Test_GetCCIListing_ItemWithNoReferences_NotIncluded()
        {
            var cciItems = BuildTestCciList();
            // CCI-004 has no references, so it should never appear in any result
            var result = CCIListGenerator.GetCCIListing("AU-9", cciItems);

            Assert.DoesNotContain("CCI-004", result);
        }

        [Fact]
        public void Test_GetCCIListing_ValidControl_ReturnedListIsNotNull()
        {
            var cciItems = BuildTestCciList();
            var result   = CCIListGenerator.GetCCIListing("AU-9", cciItems);

            Assert.NotNull(result);
        }

        // ── Fail Tests (negative / invalid-state verification) ───────────────────

        [Fact]
        public void Test_GetCCIListing_UnknownControl_ReturnsEmptyList()
        {
            var cciItems = BuildTestCciList();
            var result   = CCIListGenerator.GetCCIListing("XX-999", cciItems);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Test_GetCCIListing_ControlWithWrongCase_ReturnsEmptyList()
        {
            // majorControl comparisons are case-sensitive; "au-9" should not match "AU-9"
            var cciItems = BuildTestCciList();
            var result   = CCIListGenerator.GetCCIListing("au-9", cciItems);

            Assert.Empty(result);
        }

        [Fact]
        public void Test_GetCCIListing_EmptyStringControl_ReturnsEmptyList()
        {
            var cciItems = BuildTestCciList();
            var result   = CCIListGenerator.GetCCIListing(string.Empty, cciItems);

            // None of the test items have an empty majorControl, so result should be empty
            Assert.Empty(result);
        }

        [Fact]
        public void Test_GetCCIListing_NullCciItemList_ThrowsNullReferenceException()
        {
            // Passing null for cciItems causes NullReferenceException in the foreach loop
            Assert.Throws<System.NullReferenceException>(() =>
                CCIListGenerator.GetCCIListing("AU-9", null));
        }

        [Fact]
        public void Test_GetCCIListing_KnownControl_DoesNotReturnUnrelatedCci()
        {
            var cciItems = BuildTestCciList();
            var result   = CCIListGenerator.GetCCIListing("AU-9", cciItems);

            Assert.DoesNotContain("CCI-002", result);
            Assert.DoesNotContain("CCI-003", result);
        }
    }
}
