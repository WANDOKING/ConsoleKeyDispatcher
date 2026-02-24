namespace ConsoleKeyUtils.Tests;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class ConsoleKeyDispatcherTests
{
    [TestMethod]
    public void Constructor_Default_CreatesInstance()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        Assert.IsNotNull(dispatcher);
    }

    [TestMethod]
    public void Default_ReturnsSingletonInstance()
    {
        var first = ConsoleKeyDispatcher.Default;
        var second = ConsoleKeyDispatcher.Default;

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void Default_IsNotNull()
    {
        Assert.IsNotNull(ConsoleKeyDispatcher.Default);
    }

    [TestMethod]
    public void HandlerNames_Initially_IsEmpty()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        Assert.IsFalse(dispatcher.HandlerNames.Any());
    }

    [TestMethod]
    public void BindHandler_RegistersSyncHandler()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        dispatcher.BindHandler(ConsoleKey.A, () => { }, "TestHandler");

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.A, handlers[0].key);
        Assert.AreEqual("TestHandler", handlers[0].name);
    }

    [TestMethod]
    public void BindHandler_WithoutName_RegistersWithNullName()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        dispatcher.BindHandler(ConsoleKey.A, () => { });

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.A, handlers[0].key);
        Assert.IsNull(handlers[0].name);
    }

    [TestMethod]
    public void BindHandler_MultipleKeys_RegistersAll()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        dispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        dispatcher.BindHandler(ConsoleKey.B, () => { }, "HandlerB");
        dispatcher.BindHandler(ConsoleKey.C, () => { }, "HandlerC");

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(3, handlers.Count);
    }

    [TestMethod]
    public void BindHandler_DuplicateKey_ThrowsArgumentException()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindHandler(ConsoleKey.A, () => { });

        Assert.ThrowsException<ArgumentException>(
            () => dispatcher.BindHandler(ConsoleKey.A, () => { }));
    }

    [TestMethod]
    public void BindHandler_DuplicateKey_ExceptionMessageContainsKeyName()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindHandler(ConsoleKey.A, () => { });

        var ex = Assert.ThrowsException<ArgumentException>(
            () => dispatcher.BindHandler(ConsoleKey.A, () => { }));

        StringAssert.Contains(ex.Message, "A");
    }

    [TestMethod]
    public void BindAsyncHandler_RegistersAsyncHandler()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        dispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask, "AsyncHandler");

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.A, handlers[0].key);
        Assert.AreEqual("AsyncHandler", handlers[0].name);
    }

    [TestMethod]
    public void BindAsyncHandler_WithoutName_RegistersWithNullName()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        dispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask);

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.IsNull(handlers[0].name);
    }

    [TestMethod]
    public void BindAsyncHandler_DuplicateKey_ThrowsArgumentException()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask);

        Assert.ThrowsException<ArgumentException>(
            () => dispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask));
    }

    [TestMethod]
    public void BindAsyncHandler_AfterSyncHandler_SameKey_ThrowsArgumentException()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindHandler(ConsoleKey.A, () => { });

        Assert.ThrowsException<ArgumentException>(
            () => dispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask));
    }

    [TestMethod]
    public void BindHandler_AfterAsyncHandler_SameKey_ThrowsArgumentException()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask);

        Assert.ThrowsException<ArgumentException>(
            () => dispatcher.BindHandler(ConsoleKey.A, () => { }));
    }

    [TestMethod]
    public void BindExitHandler_RegistersWithDefaultEscapeKey()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        dispatcher.BindExitHandler();

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.Escape, handlers[0].key);
        Assert.AreEqual("Exit Handler", handlers[0].name);
    }

    [TestMethod]
    public void BindExitHandler_WithCustomKey_RegistersWithSpecifiedKey()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        dispatcher.BindExitHandler(ConsoleKey.Q);

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.Q, handlers[0].key);
    }

    [TestMethod]
    public void BindExitHandler_DuplicateKey_ThrowsArgumentException()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindExitHandler(ConsoleKey.Escape);

        Assert.ThrowsException<ArgumentException>(
            () => dispatcher.BindExitHandler(ConsoleKey.Escape));
    }

    [TestMethod]
    public void RemoveHandler_ExistingSyncHandler_ReturnsTrue()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindHandler(ConsoleKey.A, () => { });

        bool result = dispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void RemoveHandler_ExistingSyncHandler_RemovesFromHandlerNames()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindHandler(ConsoleKey.A, () => { });

        dispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsFalse(dispatcher.HandlerNames.Any());
    }

    [TestMethod]
    public void RemoveHandler_ExistingAsyncHandler_ReturnsTrue()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask);

        bool result = dispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void RemoveHandler_ExistingAsyncHandler_RemovesFromHandlerNames()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask);

        dispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsFalse(dispatcher.HandlerNames.Any());
    }

    [TestMethod]
    public void RemoveHandler_NonExistentKey_ReturnsFalse()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        bool result = dispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void RemoveHandler_AfterRemoval_CanReRegisterSameKey()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindHandler(ConsoleKey.A, () => { }, "First");

        dispatcher.RemoveHandler(ConsoleKey.A);
        dispatcher.BindHandler(ConsoleKey.A, () => { }, "Second");

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual("Second", handlers[0].name);
    }

    [TestMethod]
    public void RemoveHandler_OnlyRemovesSpecifiedKey()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        dispatcher.BindHandler(ConsoleKey.B, () => { }, "HandlerB");

        dispatcher.RemoveHandler(ConsoleKey.A);

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.B, handlers[0].key);
    }

    [TestMethod]
    public void RemoveHandler_ExitHandler_ReturnsTrue()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindExitHandler(ConsoleKey.Escape);

        bool result = dispatcher.RemoveHandler(ConsoleKey.Escape);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HandlerNames_ReturnsAllRegisteredHandlers()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindHandler(ConsoleKey.A, () => { }, "SyncHandler");
        dispatcher.BindAsyncHandler(ConsoleKey.B, () => Task.CompletedTask, "AsyncHandler");
        dispatcher.BindExitHandler(ConsoleKey.Escape);

        var handlers = dispatcher.HandlerNames.ToList();

        Assert.AreEqual(3, handlers.Count);

        var keys = handlers.Select(h => h.key).ToHashSet();
        Assert.IsTrue(keys.Contains(ConsoleKey.A));
        Assert.IsTrue(keys.Contains(ConsoleKey.B));
        Assert.IsTrue(keys.Contains(ConsoleKey.Escape));
    }

    [TestMethod]
    public void HandlerNames_AfterRemoval_DoesNotIncludeRemovedKey()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        dispatcher.BindHandler(ConsoleKey.B, () => { }, "HandlerB");

        dispatcher.RemoveHandler(ConsoleKey.A);

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.IsFalse(handlers.Any(h => h.key == ConsoleKey.A));
    }

    [TestMethod]
    public void MixedHandlers_SyncAndAsync_RegisterCorrectly()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        dispatcher.BindHandler(ConsoleKey.A, () => { }, "Sync");
        dispatcher.BindAsyncHandler(ConsoleKey.B, () => Task.CompletedTask, "Async");

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(2, handlers.Count);
    }

    [TestMethod]
    public void RemoveHandler_SyncThenReAddAsAsync_Works()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindHandler(ConsoleKey.A, () => { }, "Sync");

        dispatcher.RemoveHandler(ConsoleKey.A);
        dispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask, "Async");

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual("Async", handlers[0].name);
    }

    [TestMethod]
    public void RemoveHandler_AsyncThenReAddAsSync_Works()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask, "Async");

        dispatcher.RemoveHandler(ConsoleKey.A);
        dispatcher.BindHandler(ConsoleKey.A, () => { }, "Sync");

        var handlers = dispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual("Sync", handlers[0].name);
    }

    [TestMethod]
    public void BindHandler_AllFunctionKeys_CanRegister()
    {
        var dispatcher = new ConsoleKeyDispatcher();

        dispatcher.BindHandler(ConsoleKey.F1, () => { }, "F1");
        dispatcher.BindHandler(ConsoleKey.F2, () => { }, "F2");
        dispatcher.BindHandler(ConsoleKey.F3, () => { }, "F3");

        Assert.AreEqual(3, dispatcher.HandlerNames.Count());
    }

    [TestMethod]
    public void RemoveHandler_CalledTwice_SecondCallReturnsFalse()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        dispatcher.BindHandler(ConsoleKey.A, () => { });

        dispatcher.RemoveHandler(ConsoleKey.A);
        bool secondResult = dispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsFalse(secondResult);
    }

    [TestMethod]
    public void HandlerException_CanSubscribeEvent()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        bool eventSubscribed = false;

        dispatcher.HandlerException += (sender, args) => { eventSubscribed = true; };

        Assert.IsFalse(eventSubscribed);
    }

    [TestMethod]
    public void HandlerException_CanSubscribeAndUnsubscribe()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        EventHandler<HandlerExceptionEventArgs> handler = (sender, args) => { };

        dispatcher.HandlerException += handler;
        dispatcher.HandlerException -= handler;
    }

    [TestMethod]
    public void HandlerException_MultipleSubscribers_CanSubscribe()
    {
        var dispatcher = new ConsoleKeyDispatcher();
        int subscriberCount = 0;

        dispatcher.HandlerException += (sender, args) => { subscriberCount++; };
        dispatcher.HandlerException += (sender, args) => { subscriberCount++; };
        dispatcher.HandlerException += (sender, args) => { subscriberCount++; };
    }
}
