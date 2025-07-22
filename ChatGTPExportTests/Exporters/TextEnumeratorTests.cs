using ChatGPTExport;

namespace ChatGTPExportTests.Exporters
{
    public class TextEnumeratorTests
    {
        [Theory]
        [InlineData("a", 1)] // regular character
        [InlineData("✅", 1)] // U+2705
        [InlineData("⚠️", 1)] // U+26A0 U+FE0F
        [InlineData("🚗", 2)] // U+1F697 
        [InlineData("🗂️", 2)] // U+1F5C2 U+FE0F
        [InlineData("🧑‍💻", 3)] // U+1F9D1 U+200D U+1F4BB
        [InlineData("🧛‍♂️", 2)] // U+1F9DB U+200D U+2642 U+FE0F
        [InlineData("👩‍👧", 3)] // U+1F469 U+200D U+1F467
        [InlineData("👍🏽", 2)] // U+1F44D U+1F3FD
        [InlineData("🇬🇧", 2)] // U+1F1EC U+1F1E7
        public void GetRealElementWidthTest(string input, int expected)
        {
            var length = input.GetRealElementWidth();
            Assert.Equal(expected, length);
        }

        [Fact]
        public void EnumeratorTest2()
        {
            const string stringWithEmoji = "no ✅emojis"; // U+2705

            var result = stringWithEmoji.GetRenderedElementIndexes();

            List<int> expectedResult =
            [
                0,1,2,3,4,5,6,7,8,9
            ];

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void EnumeratorTest()
        {
            const string stringWithEmoji = "a 🚗 emoji"; // U+1F697

            var result = stringWithEmoji.GetRenderedElementIndexes();

            List<int> expectedResult =
            [
                0,1,2,4,5,6,7,8,9
            ];

            Assert.Equal(expectedResult, result);

        }

        [Fact]
        public void EnumeratorTest2A()
        {
            const string stringWithEmoji = "a 🗂️ emoji"; // U+1F5C2 U+FE0F

            var result = stringWithEmoji.GetRenderedElementIndexes();

            List<int> expectedResult =
            [
                0,1,2,4,5,6,7,8,9
            ];

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Triple_width_Test()
        {
            const string stringWithEmoji = "a 🧑‍💻 emoji"; // U+1F9D1 U+200D U+1F4BB

            var result = stringWithEmoji.GetRenderedElementIndexes();

            List<int> expectedResult =
            [
                0,1,2,5,6,7,8,9,10
            ];

            Assert.Equal(expectedResult, result);
        }
    }
}
