using NUnit.Framework;
using Gotcha.UI.Logic;

namespace Gotcha.Tests
{
    public class PageScrubTests
    {
        [Test]
        public void EndsOfTheBarAreFirstAndLastPage()
        {
            Assert.AreEqual(0, PageScrub.PageAt(0f, 500f, 29));
            Assert.AreEqual(0, PageScrub.PageAt(-40f, 500f, 29));
            Assert.AreEqual(28, PageScrub.PageAt(500f, 500f, 29));
            Assert.AreEqual(28, PageScrub.PageAt(900f, 500f, 29));
        }

        [Test]
        public void EveryPageIsReachableAndOrdered()
        {
            int last = -1;
            for (int i = 0; i <= 1000; i++)
            {
                int p = PageScrub.PageAt(i * 0.5f, 500f, 29);
                Assert.GreaterOrEqual(p, last);
                Assert.LessOrEqual(p - last, 1);
                last = p;
            }
            Assert.AreEqual(28, last);
            Assert.AreEqual(14, PageScrub.PageAt(250f, 500f, 29));
        }

        [Test]
        public void DegenerateInputsStayOnTheFirstPage()
        {
            Assert.AreEqual(0, PageScrub.PageAt(100f, 0f, 29));
            Assert.AreEqual(0, PageScrub.PageAt(100f, 500f, 1));
            Assert.AreEqual(0, PageScrub.PageAt(float.NaN, 500f, 29));
        }
    }
}
