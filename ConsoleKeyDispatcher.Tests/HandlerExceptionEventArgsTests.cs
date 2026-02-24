namespace ConsoleKeyUtils.Tests;

[TestClass]
public class HandlerExceptionEventArgsTests
{
    [TestMethod]
    public void Constructor_SetsKeyProperty()
    {
        var exception = new InvalidOperationException("test");
        var args = new HandlerExceptionEventArgs(ConsoleKey.A, exception);

        Assert.AreEqual(ConsoleKey.A, args.Key);
    }

    [TestMethod]
    public void Constructor_SetsExceptionProperty()
    {
        var exception = new InvalidOperationException("test error");
        var args = new HandlerExceptionEventArgs(ConsoleKey.A, exception);

        Assert.AreSame(exception, args.Exception);
    }

    [TestMethod]
    public void Constructor_WithDifferentKey_SetsCorrectKey()
    {
        var exception = new Exception("test");
        var args = new HandlerExceptionEventArgs(ConsoleKey.Escape, exception);

        Assert.AreEqual(ConsoleKey.Escape, args.Key);
    }

    [TestMethod]
    public void Constructor_PreservesExceptionMessage()
    {
        var exception = new ArgumentException("specific error message");
        var args = new HandlerExceptionEventArgs(ConsoleKey.F1, exception);

        Assert.AreEqual("specific error message", args.Exception.Message);
    }

    [TestMethod]
    public void Constructor_PreservesExceptionType()
    {
        var exception = new NullReferenceException("null ref");
        var args = new HandlerExceptionEventArgs(ConsoleKey.B, exception);

        Assert.IsInstanceOfType(args.Exception, typeof(NullReferenceException));
    }

    [TestMethod]
    public void InheritsFromEventArgs()
    {
        var args = new HandlerExceptionEventArgs(ConsoleKey.A, new Exception());

        Assert.IsInstanceOfType(args, typeof(EventArgs));
    }
}
