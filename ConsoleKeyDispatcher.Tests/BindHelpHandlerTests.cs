namespace ConsoleKeyUtils.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class BindHelpHandlerTests
{
    [TestInitialize]
    public void TestInitialize()
    {
        ConsoleKeyDispatcher.Reset();
    }

    [TestMethod]
    public void BindHelpHandler_DefaultKey_RegistersOnH()
    {
        ConsoleKeyDispatcher.BindHelpHandler();

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.H, handlers[0].key);
        Assert.AreEqual("Help Handler", handlers[0].name);
    }

    [TestMethod]
    public void BindHelpHandler_CustomKey_RegistersOnGivenKey()
    {
        ConsoleKeyDispatcher.BindHelpHandler(ConsoleKey.F1);

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.F1, handlers[0].key);
    }

    [TestMethod]
    public void BindHelpHandler_DuplicateKey_ThrowsArgumentException()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.H, () => { });

        Assert.ThrowsException<ArgumentException>(() => ConsoleKeyDispatcher.BindHelpHandler());
    }

    [TestMethod]
    public void BindHelpHandler_IsIncludedInJsonDescriptions()
    {
        ConsoleKeyDispatcher.BindHelpHandler();

        StringAssert.Contains(ConsoleKeyDispatcher.GetJsonDescriptions(indented: false), "\"Key\":\"H\",\"Name\":\"Help Handler\"");
    }

    [TestMethod]
    public void BindHelpHandler_AfterRemove_CanBeRegisteredAgain()
    {
        ConsoleKeyDispatcher.BindHelpHandler();
        ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.H);

        ConsoleKeyDispatcher.BindHelpHandler(ConsoleKey.F1);

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.F1, handlers[0].key);
    }
}
