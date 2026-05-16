// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using Xunit;
using openrmf_msg_compliance.Classes;
using openrmf_msg_compliance.Models.NISTtoCCI;
using System.Collections.Generic;
using System.Linq;

namespace tests.Classes
{
    /// <summary>
    /// Tests for NistCciGenerator.LoadNistToCci().
    /// The U_CCI_List.xml file is included in the src project with CopyToOutputDirectory="PreserveNewest"
    /// and is copied to the test output directory by the .NET build system via the ProjectReference.
    /// All tests in this class rely on that file being present at runtime.
    /// </summary>
    public class NistCciGeneratorTests
    {
        // Load once for the entire fixture to keep tests fast.
        private readonly List<CciItem> _cciItems;

        public NistCciGeneratorTests()
        {
            _cciItems = NistCciGenerator.LoadNistToCci();
        }

        // ── Pass Tests ────────────────────────────────────────────────────────────

        [Fact]
        public void Test_LoadNistToCci_ReturnsNonNullList()
        {
            Assert.NotNull(_cciItems);
        }

        [Fact]
        public void Test_LoadNistToCci_ReturnsNonEmptyList()
        {
            Assert.NotEmpty(_cciItems);
        }

        [Fact]
        public void Test_LoadNistToCci_AllItemsHaveNonNullCciId()
        {
            Assert.All(_cciItems, item => Assert.NotNull(item.cciId));
        }

        [Fact]
        public void Test_LoadNistToCci_AllItemsHaveNonEmptyCciId()
        {
            Assert.All(_cciItems, item => Assert.False(string.IsNullOrWhiteSpace(item.cciId)));
        }

        [Fact]
        public void Test_LoadNistToCci_AllCciIdsStartWithCCI()
        {
            Assert.All(_cciItems, item => Assert.StartsWith("CCI-", item.cciId));
        }

        [Fact]
        public void Test_LoadNistToCci_AllItemsHaveReferencesListInitialized()
        {
            Assert.All(_cciItems, item => Assert.NotNull(item.references));
        }

        [Fact]
        public void Test_LoadNistToCci_AtLeastSomeItemsHaveReferences()
        {
            // The real CCI list has references on every record; at minimum some should have them.
            bool anyWithReferences = _cciItems.Any(item => item.references.Count > 0);
            Assert.True(anyWithReferences);
        }

        [Fact]
        public void Test_LoadNistToCci_ReferencesHaveNonNullCreator()
        {
            var refsWithCreator = _cciItems
                .SelectMany(item => item.references)
                .Where(r => r.creator != null);

            // At least one reference should have a creator set
            Assert.NotEmpty(refsWithCreator);
        }

        [Fact]
        public void Test_LoadNistToCci_ReferencesHaveNonNullIndex()
        {
            var refsWithIndex = _cciItems
                .SelectMany(item => item.references)
                .Where(r => r.index != null);

            Assert.NotEmpty(refsWithIndex);
        }

        [Fact]
        public void Test_LoadNistToCci_ReferencesHaveMajorControlDerived()
        {
            // At least some references should have majorControl populated
            var refsWithMajor = _cciItems
                .SelectMany(item => item.references)
                .Where(r => !string.IsNullOrWhiteSpace(r.majorControl));

            Assert.NotEmpty(refsWithMajor);
        }

        [Fact]
        public void Test_LoadNistToCci_MajorControlNeverLongerThanIndex()
        {
            // majorControl is always a prefix/substring of index — it cannot be longer
            var violations = _cciItems
                .SelectMany(item => item.references)
                .Where(r => r.majorControl != null && r.index != null
                         && r.majorControl.Length > r.index.Length);

            Assert.Empty(violations);
        }

        [Fact]
        public void Test_LoadNistToCci_ListCountIsGreaterThanHundred()
        {
            // The official CCI list contains thousands of entries; a minimum sanity check.
            Assert.True(_cciItems.Count > 100,
                $"Expected more than 100 CCI items but found {_cciItems.Count}.");
        }

        [Fact]
        public void Test_LoadNistToCci_NoDuplicateCciIds()
        {
            int total    = _cciItems.Count;
            int distinct = _cciItems.Select(i => i.cciId).Distinct().Count();

            Assert.Equal(total, distinct);
        }

        [Fact]
        public void Test_LoadNistToCci_AllItemsHaveStatusSet()
        {
            // Status is mapped from the <status> element — should not be null in a well-formed XML
            Assert.All(_cciItems, item => Assert.False(string.IsNullOrWhiteSpace(item.status)));
        }

        // ── Fail Tests (negative / invalid-state verification) ───────────────────

        [Fact]
        public void Test_LoadNistToCci_NoCciIdIsEmpty()
        {
            // No item should have an empty or whitespace-only cciId
            var invalidItems = _cciItems.Where(i => string.IsNullOrWhiteSpace(i.cciId));
            Assert.Empty(invalidItems);
        }

        [Fact]
        public void Test_LoadNistToCci_NoItemHasNullReferencesList()
        {
            // The constructor always initialises the list; loading from XML must preserve that
            var nullRefItems = _cciItems.Where(i => i.references == null);
            Assert.Empty(nullRefItems);
        }

        [Fact]
        public void Test_LoadNistToCci_CciIdDoesNotContainWhitespace()
        {
            var itemsWithWhitespaceCciId = _cciItems
                .Where(i => i.cciId != null && i.cciId.Contains(' '));

            Assert.Empty(itemsWithWhitespaceCciId);
        }

        [Fact]
        public void Test_LoadNistToCci_IntegrationWithCCIListGenerator_MatchesKnownControl()
        {
            // Integration smoke test: using the loaded list with CCIListGenerator should
            // return at least one CCI for a commonly referenced NIST control.
            var result = CCIListGenerator.GetCCIListing("AU-9", _cciItems);

            // AU-9 is a well-known control that should have CCI mappings in the official list
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Test_LoadNistToCci_IntegrationWithCCIListGenerator_UnknownControlReturnsEmpty()
        {
            var result = CCIListGenerator.GetCCIListing("ZZ-9999", _cciItems);

            Assert.Empty(result);
        }
    }
}
