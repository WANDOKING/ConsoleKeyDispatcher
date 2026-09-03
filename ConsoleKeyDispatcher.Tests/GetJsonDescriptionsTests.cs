using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleKeyUtils.Tests;

[TestClass]
public class GetJsonDescriptionsTests
{
    [TestInitialize]
    public void TestInitialize()
    {
        ConsoleKeyDispatcher.Reset();
    }

    [TestMethod]
    public void GetJsonDescriptions_NoHandlers_ReturnsEmptyJsonObject()
    {
        Assert.AreEqual("{}", ConsoleKeyDispatcher.GetJsonDescriptions());
    }

    [TestMethod]
    public void GetJsonDescriptions_SingleHandler_ReturnsKeyNameAndHandlerName()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");

        Assert.AreEqual("{\"A\":\"HandlerA\"}", ConsoleKeyDispatcher.GetJsonDescriptions());
    }

    [TestMethod]
    public void GetJsonDescriptions_HandlerWithoutName_ReturnsNullValue()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { });

        Assert.AreEqual("{\"A\":null}", ConsoleKeyDispatcher.GetJsonDescriptions());
    }

    [TestMethod]
    public void GetJsonDescriptions_AsyncHandler_IsIncluded()
    {
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.B, () => Task.CompletedTask, "AsyncB");

        Assert.AreEqual("{\"B\":\"AsyncB\"}", ConsoleKeyDispatcher.GetJsonDescriptions());
    }

    [TestMethod]
    public void GetJsonDescriptions_MultipleHandlers_ContainsAll()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.B, () => Task.CompletedTask, "HandlerB");
        ConsoleKeyDispatcher.BindExitHandler();

        var descriptions = JsonSerializer.Deserialize<Dictionary<string, string?>>(ConsoleKeyDispatcher.GetJsonDescriptions());

        Assert.IsNotNull(descriptions);
        Assert.AreEqual(3, descriptions.Count);
        Assert.AreEqual("HandlerA", descriptions["A"]);
        Assert.AreEqual("HandlerB", descriptions["B"]);
        Assert.AreEqual("Exit Handler", descriptions[nameof(ConsoleKey.Escape)]);
    }

    [TestMethod]
    public void GetJsonDescriptions_AfterRemoveHandler_ExcludesRemovedKey()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.B, () => { }, "HandlerB");

        ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);

        Assert.AreEqual("{\"B\":\"HandlerB\"}", ConsoleKeyDispatcher.GetJsonDescriptions());
    }

    [TestMethod]
    public void GetJsonDescriptions_NonAsciiName_IsNotEscaped()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "도움말");

        Assert.AreEqual("{\"A\":\"도움말\"}", ConsoleKeyDispatcher.GetJsonDescriptions());
    }

    [TestMethod]
    public void GetJsonDescriptions_NameWithQuote_IsEscaped()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "Print \"Hello\"");

        var descriptions = JsonSerializer.Deserialize<Dictionary<string, string?>>(ConsoleKeyDispatcher.GetJsonDescriptions());

        Assert.IsNotNull(descriptions);
        Assert.AreEqual("Print \"Hello\"", descriptions["A"]);
    }

    [TestMethod]
    public void GetJsonDescriptions_Indented_ContainsNewLine()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");

        var json = ConsoleKeyDispatcher.GetJsonDescriptions(indented: true);

        StringAssert.Contains(json, Environment.NewLine);
        StringAssert.Contains(json, "\"A\"");
        StringAssert.Contains(json, "\"HandlerA\"");
    }

    [TestMethod]
    public void GetJsonDescriptions_NotIndented_IsSingleLine()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.B, () => { }, "HandlerB");

        var json = ConsoleKeyDispatcher.GetJsonDescriptions();

        Assert.IsFalse(json.Contains('\n'));
    }
}
