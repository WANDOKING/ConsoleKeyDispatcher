using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleKeyUtils.Tests;

[TestClass]
public class ConsoleKeyDispatcherTests
{
    [TestInitialize]
    public void TestInitialize()
    {
        ConsoleKeyDispatcher.Reset();
    }

    [TestMethod]
    public void HandlerNames_Initially_IsEmpty()
    {
        Assert.IsFalse(ConsoleKeyDispatcher.HandlerNames.Any());
    }

    [TestMethod]
    public void BindHandler_RegistersSyncHandler()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "TestHandler");

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.A, handlers[0].key);
        Assert.AreEqual("TestHandler", handlers[0].name);
    }

    [TestMethod]
    public void BindHandler_WithoutName_RegistersWithNullName()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { });

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.A, handlers[0].key);
        Assert.IsNull(handlers[0].name);
    }

    [TestMethod]
    public void BindHandler_MultipleKeys_RegistersAll()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.B, () => { }, "HandlerB");
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.C, () => { }, "HandlerC");

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(3, handlers.Count);
    }

    [TestMethod]
    public void BindHandler_DuplicateKey_ThrowsArgumentException()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { });

        Assert.ThrowsException<ArgumentException>(
            () => ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }));
    }

    [TestMethod]
    public void BindHandler_DuplicateKey_ExceptionMessageContainsKeyName()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { });

        var ex = Assert.ThrowsException<ArgumentException>(
            () => ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }));

        StringAssert.Contains(ex.Message, "A");
    }

    [TestMethod]
    public void BindAsyncHandler_RegistersAsyncHandler()
    {
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask, "AsyncHandler");

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.A, handlers[0].key);
        Assert.AreEqual("AsyncHandler", handlers[0].name);
    }

    [TestMethod]
    public void BindAsyncHandler_WithoutName_RegistersWithNullName()
    {
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask);

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.IsNull(handlers[0].name);
    }

    [TestMethod]
    public void BindAsyncHandler_DuplicateKey_ThrowsArgumentException()
    {
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask);

        Assert.ThrowsException<ArgumentException>(
            () => ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask));
    }

    [TestMethod]
    public void BindAsyncHandler_AfterSyncHandler_SameKey_ThrowsArgumentException()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { });

        Assert.ThrowsException<ArgumentException>(
            () => ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask));
    }

    [TestMethod]
    public void BindHandler_AfterAsyncHandler_SameKey_ThrowsArgumentException()
    {
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask);

        Assert.ThrowsException<ArgumentException>(
            () => ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }));
    }

    [TestMethod]
    public void BindExitHandler_RegistersWithDefaultEscapeKey()
    {
        ConsoleKeyDispatcher.BindExitHandler();

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.Escape, handlers[0].key);
        Assert.AreEqual("Exit Handler", handlers[0].name);
    }

    [TestMethod]
    public void BindExitHandler_WithCustomKey_RegistersWithSpecifiedKey()
    {
        ConsoleKeyDispatcher.BindExitHandler(ConsoleKey.Q);

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.Q, handlers[0].key);
    }

    [TestMethod]
    public void BindExitHandler_DuplicateKey_ThrowsArgumentException()
    {
        ConsoleKeyDispatcher.BindExitHandler(ConsoleKey.Escape);

        Assert.ThrowsException<ArgumentException>(
            () => ConsoleKeyDispatcher.BindExitHandler(ConsoleKey.Escape));
    }

    [TestMethod]
    public void RemoveHandler_ExistingSyncHandler_ReturnsTrue()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { });

        bool result = ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void RemoveHandler_ExistingSyncHandler_RemovesFromHandlerNames()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { });

        ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsFalse(ConsoleKeyDispatcher.HandlerNames.Any());
    }

    [TestMethod]
    public void RemoveHandler_ExistingAsyncHandler_ReturnsTrue()
    {
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask);

        bool result = ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void RemoveHandler_ExistingAsyncHandler_RemovesFromHandlerNames()
    {
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask);

        ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsFalse(ConsoleKeyDispatcher.HandlerNames.Any());
    }

    [TestMethod]
    public void RemoveHandler_NonExistentKey_ReturnsFalse()
    {
        bool result = ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void RemoveHandler_AfterRemoval_CanReRegisterSameKey()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "First");

        ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "Second");

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual("Second", handlers[0].name);
    }

    [TestMethod]
    public void RemoveHandler_OnlyRemovesSpecifiedKey()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.B, () => { }, "HandlerB");

        ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual(ConsoleKey.B, handlers[0].key);
    }

    [TestMethod]
    public void RemoveHandler_ExitHandler_ReturnsTrue()
    {
        ConsoleKeyDispatcher.BindExitHandler(ConsoleKey.Escape);

        bool result = ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.Escape);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void HandlerNames_ReturnsAllRegisteredHandlers()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "SyncHandler");
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.B, () => Task.CompletedTask, "AsyncHandler");
        ConsoleKeyDispatcher.BindExitHandler(ConsoleKey.Escape);

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();

        Assert.AreEqual(3, handlers.Count);

        var keys = handlers.Select(h => h.key).ToHashSet();
        Assert.IsTrue(keys.Contains(ConsoleKey.A));
        Assert.IsTrue(keys.Contains(ConsoleKey.B));
        Assert.IsTrue(keys.Contains(ConsoleKey.Escape));
    }

    [TestMethod]
    public void HandlerNames_AfterRemoval_DoesNotIncludeRemovedKey()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "HandlerA");
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.B, () => { }, "HandlerB");

        ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.IsFalse(handlers.Any(h => h.key == ConsoleKey.A));
    }

    [TestMethod]
    public void MixedHandlers_SyncAndAsync_RegisterCorrectly()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "Sync");
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.B, () => Task.CompletedTask, "Async");

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(2, handlers.Count);
    }

    [TestMethod]
    public void RemoveHandler_SyncThenReAddAsAsync_Works()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "Sync");

        ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask, "Async");

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual("Async", handlers[0].name);
    }

    [TestMethod]
    public void RemoveHandler_AsyncThenReAddAsSync_Works()
    {
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.A, () => Task.CompletedTask, "Async");

        ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "Sync");

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual("Sync", handlers[0].name);
    }

    [TestMethod]
    public void BindHandler_AllFunctionKeys_CanRegister()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.F1, () => { }, "F1");
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.F2, () => { }, "F2");
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.F3, () => { }, "F3");

        Assert.AreEqual(3, ConsoleKeyDispatcher.HandlerNames.Count());
    }

    [TestMethod]
    public void RemoveHandler_CalledTwice_SecondCallReturnsFalse()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { });

        ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);
        bool secondResult = ConsoleKeyDispatcher.RemoveHandler(ConsoleKey.A);

        Assert.IsFalse(secondResult);
    }

    [TestMethod]
    public void HandlerException_CanSubscribeEvent()
    {
        bool eventSubscribed = false;

        ConsoleKeyDispatcher.HandlerException += (sender, args) => { eventSubscribed = true; };

        Assert.IsFalse(eventSubscribed);
    }

    [TestMethod]
    public void HandlerException_CanSubscribeAndUnsubscribe()
    {
        EventHandler<HandlerExceptionEventArgs> handler = (sender, args) => { };

        ConsoleKeyDispatcher.HandlerException += handler;
        ConsoleKeyDispatcher.HandlerException -= handler;
    }

    [TestMethod]
    public void HandlerException_MultipleSubscribers_CanSubscribe()
    {
        int subscriberCount = 0;

        ConsoleKeyDispatcher.HandlerException += (sender, args) => { subscriberCount++; };
        ConsoleKeyDispatcher.HandlerException += (sender, args) => { subscriberCount++; };
        ConsoleKeyDispatcher.HandlerException += (sender, args) => { subscriberCount++; };
    }

    [TestMethod]
    public void Reset_ClearsAllHandlers()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { });
        ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.B, () => Task.CompletedTask);

        ConsoleKeyDispatcher.Reset();

        Assert.IsFalse(ConsoleKeyDispatcher.HandlerNames.Any());
    }

    [TestMethod]
    public void Reset_AllowsReRegistration()
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "Before");

        ConsoleKeyDispatcher.Reset();
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => { }, "After");

        var handlers = ConsoleKeyDispatcher.HandlerNames.ToList();
        Assert.AreEqual(1, handlers.Count);
        Assert.AreEqual("After", handlers[0].name);
    }
}
