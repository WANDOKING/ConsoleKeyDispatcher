using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleKeyUtils.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class KeyNotRegisteredEventArgsTests
{
    [TestMethod]
    public void Constructor_SetsKeyProperty()
    {
        var args = new KeyNotRegisteredEventArgs(ConsoleKey.A);

        Assert.AreEqual(ConsoleKey.A, args.Key);
    }

    [TestMethod]
    public void Constructor_WithEscapeKey_SetsKeyProperty()
    {
        var args = new KeyNotRegisteredEventArgs(ConsoleKey.Escape);

        Assert.AreEqual(ConsoleKey.Escape, args.Key);
    }

    [TestMethod]
    public void InheritsFromEventArgs()
    {
        var args = new KeyNotRegisteredEventArgs(ConsoleKey.A);

        Assert.IsInstanceOfType(args, typeof(EventArgs));
    }
}
