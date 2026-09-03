namespace ConsoleKeyUtils.Tests;

using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class GetJsonDescriptionsTests
{
    [TestInitialize]
    public void TestInitialize()
    {
        ConsoleKeyDispatcher.Reset();
    }

    [TestMethod]
    public void GetJsonDescriptions_NoHandlers_ReturnsEmptyHandlerArray()
    {
        var descriptions = Deserialize(ConsoleKeyDispatcher.GetJsonDescriptions());

        Assert.AreEqual(0, descriptions.Handlers.Length);
    }

    [TestMethod]
    public void GetJsonDescriptions_SingleHandler_ReturnsKeyAndName()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");

        var descriptions = Deserialize(ConsoleKeyDispatcher.GetJsonDescriptions());

        Assert.AreEqual(1, descriptions.Handlers.Length);
        Assert.AreEqual(nameof(ConsoleKey.A), descriptions.Handlers[0].Key);
        Assert.AreEqual("HandlerA", descriptions.Handlers[0].Name);
    }

    [TestMethod]
    public void GetJsonDescriptions_HandlerWithoutName_ReturnsNullName()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { });

        var descriptions = Deserialize(ConsoleKeyDispatcher.GetJsonDescriptions());

        Assert.AreEqual(1, descriptions.Handlers.Length);
        Assert.IsNull(descriptions.Handlers[0].Name);
    }

    [TestMethod]
    public void GetJsonDescriptions_AsyncHandler_IsIncluded()
    {
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.B, () => Task.CompletedTask, "AsyncB");

        var descriptions = Deserialize(ConsoleKeyDispatcher.GetJsonDescriptions());

        Assert.AreEqual(1, descriptions.Handlers.Length);
        Assert.AreEqual(nameof(ConsoleKey.B), descriptions.Handlers[0].Key);
        Assert.AreEqual("AsyncB", descriptions.Handlers[0].Name);
    }

    [TestMethod]
    public void GetJsonDescriptions_MultipleHandlers_ContainsAll()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.B, () => Task.CompletedTask, "HandlerB");
        ConsoleKeyDispatcher.BindExitHandler();

        var descriptions = Deserialize(ConsoleKeyDispatcher.GetJsonDescriptions());

        Assert.AreEqual(3, descriptions.Handlers.Length);
        Assert.AreEqual("HandlerA", FindName(descriptions, ConsoleKey.A));
        Assert.AreEqual("HandlerB", FindName(descriptions, ConsoleKey.B));
        Assert.AreEqual("Exit Handler", FindName(descriptions, ConsoleKey.Escape));
    }

    [TestMethod]
    public void GetJsonDescriptions_AfterRemoveHandler_ExcludesRemovedKey()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.B, () => { }, "HandlerB");

        ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);

        var descriptions = Deserialize(ConsoleKeyDispatcher.GetJsonDescriptions());

        Assert.AreEqual(1, descriptions.Handlers.Length);
        Assert.AreEqual(nameof(ConsoleKey.B), descriptions.Handlers[0].Key);
    }

    [TestMethod]
    public void GetJsonDescriptions_NonAsciiName_IsNotEscaped()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "도움말");

        StringAssert.Contains(ConsoleKeyDispatcher.GetJsonDescriptions(), "도움말");
    }

    [TestMethod]
    public void GetJsonDescriptions_NameWithQuote_IsEscaped()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "Print \"Hello\"");

        var descriptions = Deserialize(ConsoleKeyDispatcher.GetJsonDescriptions());

        Assert.AreEqual("Print \"Hello\"", descriptions.Handlers[0].Name);
    }

    [TestMethod]
    public void GetJsonDescriptions_ByDefault_IsIndented()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");

        StringAssert.Contains(ConsoleKeyDispatcher.GetJsonDescriptions(), Environment.NewLine);
    }

    [TestMethod]
    public void GetJsonDescriptions_NotIndented_IsSingleLine()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.B, () => { }, "HandlerB");

        var json = ConsoleKeyDispatcher.GetJsonDescriptions(indented: false);

        Assert.IsFalse(json.Contains('\n'));
        Assert.AreEqual(
            "{\"Handlers\":[{\"Key\":\"A\",\"Name\":\"HandlerA\"},{\"Key\":\"B\",\"Name\":\"HandlerB\"}]}",
            json);
    }

    private static HandlerDescriptions Deserialize(string json)
    {
        var descriptions = JsonSerializer.Deserialize<HandlerDescriptions>(json);

        Assert.IsNotNull(descriptions);
        Assert.IsNotNull(descriptions.Handlers);

        return descriptions;
    }

    private static string? FindName(HandlerDescriptions descriptions, ConsoleKey key)
    {
        return descriptions.Handlers.Single(handler => handler.Key == key.ToString()).Name;
    }

    private sealed class HandlerDescriptions
    {
        public HandlerDescription[] Handlers { get; set; } = Array.Empty<HandlerDescription>();
    }

    private sealed class HandlerDescription
    {
        public string Key { get; set; } = string.Empty;

        public string? Name { get; set; }
    }
}
