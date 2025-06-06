using GitUI;

namespace GitExtUtilsTests
{
    [TestFixture]
    public class Win32ApiTests
    {
        [Test]
        public void LParam_to_Point_restores_original_point(
            [Values(short.MinValue, -1, 0, 1, short.MaxValue)] short x,
            [Values(short.MinValue, -1, 0, 1, short.MaxValue)] short y)
        {
            Point point = ToLParam(x, y).ToPoint();

            ClassicAssert.That(point.X, Is.EqualTo(x));
            ClassicAssert.That(point.Y, Is.EqualTo(y));
        }

        [Test]
        public void LParam_to_point_in_ICSharpCode_restores_original_point(
            [Values(short.MinValue, -1, 0, 1, short.MaxValue)] short x,
            [Values(short.MinValue, -1, 0, 1, short.MaxValue)] short y)
        {
            Point point = ICSharpCode.TextEditor.Util.Win32Util.ToPoint(ToLParam(x, y));

            ClassicAssert.That(point.X, Is.EqualTo(x));
            ClassicAssert.That(point.Y, Is.EqualTo(y));
        }

        private static IntPtr ToLParam(short x, short y) =>
            new(
                x & 0x0000_FFFF |
                (y & 0x0000_FFFF) << 16);
    }
}
