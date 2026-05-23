// Copyright (c) Cingulara LLC 2025 and Tutela LLC 2025. All rights reserved.
// Licensed under the GNU GENERAL PUBLIC LICENSE Version 3, 29 June 2007 license. See LICENSE file in the project root for full license information.
using Xunit;
using openrmf_msg_compliance.Classes;
using System;
using System.Text;

namespace tests.Classes
{
    public class CompressionTests
    {
        // ── Pass Tests ────────────────────────────────────────────────────────────

        [Fact]
        public void Test_CompressString_ReturnsNonEmptyString()
        {
            string result = Compression.CompressString("Hello, World!");
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Test_DecompressString_ReturnsOriginalText()
        {
            const string original  = "Hello, World!";
            string       compressed = Compression.CompressString(original);
            string       restored   = Compression.DecompressString(compressed);

            Assert.Equal(original, restored);
        }

        [Fact]
        public void Test_CompressDecompress_RoundTrip_LongString()
        {
            // A longer, realistic payload (JSON-like)
            string original = "{\"cciId\":\"CCI-001\",\"status\":\"Published\",\"definition\":\""
                            + new string('A', 500)
                            + "\"}";

            string compressed = Compression.CompressString(original);
            string restored   = Compression.DecompressString(compressed);

            Assert.Equal(original, restored);
        }

        [Fact]
        public void Test_CompressDecompress_RoundTrip_SpecialCharacters()
        {
            const string original = "Special chars: <>&\"'\\n\t日本語 ñoño";

            string compressed = Compression.CompressString(original);
            string restored   = Compression.DecompressString(compressed);

            Assert.Equal(original, restored);
        }

        [Fact]
        public void Test_CompressDecompress_RoundTrip_EmptyString()
        {
            const string original = "";

            string compressed = Compression.CompressString(original);
            string restored   = Compression.DecompressString(compressed);

            Assert.Equal(original, restored);
        }

        [Fact]
        public void Test_CompressString_OutputIsDifferentFromInput()
        {
            const string original  = "This is some plain text that will be compressed.";
            string       compressed = Compression.CompressString(original);

            // The compressed base-64 string should not equal the original plain text
            Assert.NotEqual(original, compressed);
        }

        [Fact]
        public void Test_CompressString_OutputIsBase64Encoded()
        {
            const string original  = "OpenRMF compliance data payload";
            string       compressed = Compression.CompressString(original);

            // Should not throw — if it can be decoded it is valid base-64
            byte[] bytes = Convert.FromBase64String(compressed);
            Assert.NotNull(bytes);
            Assert.NotEmpty(bytes);
        }

        [Fact]
        public void Test_CompressTwiceSameInput_ReturnsSameOutput()
        {
            const string original = "Deterministic compression check";

            string first  = Compression.CompressString(original);
            string second = Compression.CompressString(original);

            Assert.Equal(first, second);
        }

        [Fact]
        public void Test_CompressDecompress_RoundTrip_SingleCharacter()
        {
            const string original = "X";

            string compressed = Compression.CompressString(original);
            string restored   = Compression.DecompressString(compressed);

            Assert.Equal(original, restored);
        }

        [Fact]
        public void Test_CompressDecompress_RoundTrip_NewlineAndWhitespace()
        {
            const string original = "Line one\nLine two\r\nLine three\t tabbed";

            string compressed = Compression.CompressString(original);
            string restored   = Compression.DecompressString(compressed);

            Assert.Equal(original, restored);
        }

        // ── Fail Tests (negative / invalid-state verification) ───────────────────

        [Fact]
        public void Test_DecompressString_InvalidBase64_ThrowsFormatException()
        {
            // A string that is not valid base-64 should throw
            Assert.Throws<FormatException>(() =>
                Compression.DecompressString("this-is-not-base64!!!"));
        }

        [Fact]
        public void Test_DecompressString_RandomBase64_ThrowsException()
        {
            // Valid base-64 that is not a valid GZip payload should throw an IO-level exception
            string fakeBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("not-gzip-data"));
            Assert.ThrowsAny<Exception>(() => Compression.DecompressString(fakeBase64));
        }

        [Fact]
        public void Test_CompressDecompress_DifferentInputs_ProduceDifferentOutputs()
        {
            string compressedA = Compression.CompressString("Input A");
            string compressedB = Compression.CompressString("Input B");

            Assert.NotEqual(compressedA, compressedB);
        }

        [Fact]
        public void Test_DecompressString_NullInput_ThrowsException()
        {
            Assert.ThrowsAny<Exception>(() => Compression.DecompressString(null));
        }

        [Fact]
        public void Test_CompressString_NullInput_ThrowsException()
        {
            Assert.ThrowsAny<Exception>(() => Compression.CompressString(null));
        }
    }
}
