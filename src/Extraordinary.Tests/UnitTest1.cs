using Xunit;

namespace Extraordinary.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Good()
        {
            Assert.Equal(4, 4);
        }

        [Fact]
        public void Bad()
        {
            Assert.NotEqual(5, 4);
        }
    }
}
