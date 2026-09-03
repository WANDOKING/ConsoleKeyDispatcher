using System.Text.Encodings.Web;
using System.Text.Json;

namespace ConsoleKeyUtils;

/// <summary>
/// 등록되지 않은 키가 입력되었을 때 발생하는 이벤트의 인자입니다.
/// </summary>
public class KeyNotRegisteredEventArgs : EventArgs
{
    /// <summary>
    /// 생성자입니다.
    /// </summary>
    /// <param name="key">입력된 키 입니다.</param>
    public KeyNotRegisteredEventArgs(ConsoleKey key)
    {
        Key = key;
    }

    /// <summary>
    /// 입력된 키 입니다.
    /// </summary>
    public ConsoleKey Key { get; }
}

/// <summary>
/// 핸들러 실행 중 예외가 발생했을 때 전달되는 이벤트 인자입니다.
/// </summary>
public class HandlerExceptionEventArgs : EventArgs
{
    /// <summary>
    /// 생성자입니다.
    /// </summary>
    /// <param name="key">예외가 발생한 핸들러에 대응하는 키입니다.</param>
    /// <param name="exception">발생한 예외입니다.</param>
    public HandlerExceptionEventArgs(ConsoleKey key, Exception exception)
    {
        Key = key;
        Exception = exception;
    }

    /// <summary>
    /// 예외가 발생한 핸들러에 대응하는 키입니다.
    /// </summary>
    public ConsoleKey Key { get; }

    /// <summary>
    /// 발생한 예외입니다.
    /// </summary>
    public Exception Exception { get; }
}

/// <summary>
/// 콘솔 키 입력에 따른 핸들러를 등록하고 실행하는 디스패처입니다.
/// </summary>
public static class ConsoleKeyDispatcher
{
    private static readonly Dictionary<ConsoleKey, string?> HandlerNamesByKey = new Dictionary<ConsoleKey, string?>();
    private static readonly Dictionary<ConsoleKey, Func<Task>> AsyncHandlersByKey = new Dictionary<ConsoleKey, Func<Task>>();
    private static readonly Dictionary<ConsoleKey, Action> SyncHandlersByKey = new Dictionary<ConsoleKey, Action>();
    private static readonly JsonSerializerOptions CompactJsonOptions = new JsonSerializerOptions
    {
        WriteIndented = false,

        // 한글 등 비 ASCII 핸들러 이름이 \uXXXX로 이스케이프되지 않고 그대로 출력되도록 합니다.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly JsonSerializerOptions IndentedJsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static bool isRequestedToExitDispatching;

    /// <summary>
    /// 등록되지 않은 키가 입력되었을 때 발생하는 이벤트입니다.
    /// </summary>
    public static event EventHandler<KeyNotRegisteredEventArgs>? KeyNotRegistered;

    /// <summary>
    /// 핸들러 실행 중 예외가 발생했을 때 발생하는 이벤트입니다.
    /// </summary>
    public static event EventHandler<HandlerExceptionEventArgs>? HandlerException;

    /// <summary>
    /// 등록된 핸들러들입니다.
    /// </summary>
    public static IEnumerable<(ConsoleKey key, string? name)> HandlerNames => HandlerNamesByKey.Select(kv => (kv.Key, kv.Value));

    /// <summary>
    /// 등록된 핸들러들의 키와 이름을 JSON 문자열로 반환합니다.
    /// 키 이름을 프로퍼티로, 핸들러 이름을 값으로 가지는 JSON 객체이며, 이름이 없는 핸들러의 값은 null입니다.
    /// </summary>
    /// <param name="indented">들여쓰기 여부입니다.</param>
    /// <returns>등록된 핸들러들의 키와 이름을 담은 JSON 문자열입니다.</returns>
    public static string GetJsonDescriptions(bool indented = false)
    {
        Dictionary<string, string?> descriptionsByKeyName = HandlerNamesByKey.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);

        return JsonSerializer.Serialize(descriptionsByKeyName, indented ? IndentedJsonOptions : CompactJsonOptions);
    }

    /// <summary>
    /// 비동기 핸들러를 등록합니다.
    /// </summary>
    /// <param name="key">입력 키입니다.</param>
    /// <param name="handler">키 입력 시 실행할 핸들러입니다.</param>
    /// <param name="name">핸들러의 이름입니다.</param>
    /// <exception cref="ArgumentException">이미 등록된 키를 등록하고자 할할 경우 발생합니다.</exception>
    public static void BindAsyncHandler(ConsoleKey key, Func<Task> handler, string? name = default)
    {
        if (HandlerNamesByKey.ContainsKey(key))
        {
            throw new ArgumentException($"A handler for the key '{key}' is already registered.", nameof(key));
        }

        AsyncHandlersByKey[key] = handler;
        HandlerNamesByKey[key] = name;
    }

    /// <summary>
    /// 핸들러를 등록합니다.
    /// </summary>
    /// <param name="key">입력 키입니다.</param>
    /// <param name="handler">키 입력 시 실행할 핸들러입니다.</param>
    /// <param name="name">핸들러의 이름입니다.</param>
    /// <exception cref="ArgumentException">이미 등록된 키를 등록하고자 할할 경우 발생합니다.</exception>
    public static void BindHandler(ConsoleKey key, Action handler, string? name = default)
    {
        if (HandlerNamesByKey.ContainsKey(key))
        {
            throw new ArgumentException($"A handler for the key '{key}' is already registered.", nameof(key));
        }

        SyncHandlersByKey[key] = handler;
        HandlerNamesByKey[key] = name;
    }

    /// <summary>
    /// <see cref="KeepDispatching"/>을 반환하게 하는 핸들러를 등록합니다.
    /// </summary>
    /// <param name="key">입력 키입니다.</param>
    public static void BindExitHandler(ConsoleKey key = ConsoleKey.Escape)
    {
        BindHandler(key, () => isRequestedToExitDispatching = true, "Exit Handler");
    }

    /// <summary>
    /// 핸들러를 제거합니다.
    /// </summary>
    /// <param name="key">제거할 입력 키입니다.</param>
    /// <returns>핸들러를 제거했을 경우 true, 그렇지 않으면 false입니다.</returns>
    public static bool RemoveHandler(ConsoleKey key)
    {
        if (HandlerNamesByKey.Remove(key))
        {
            if (SyncHandlersByKey.Remove(key) is false)
            {
                AsyncHandlersByKey.Remove(key);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 모든 핸들러와 이벤트 구독을 제거하고 디스패처를 초기 상태로 리셋합니다.
    /// </summary>
    public static void Reset()
    {
        HandlerNamesByKey.Clear();
        AsyncHandlersByKey.Clear();
        SyncHandlersByKey.Clear();
        isRequestedToExitDispatching = false;
        KeyNotRegistered = null;
        HandlerException = null;
    }

    /// <summary>
    /// 키 입력에 따른 핸들러를 실행합니다.
    /// </summary>
    /// <exception cref="InvalidOperationException">등록되지 않은 키가 입력되었을 경우 발생합니다.</exception>
    public static void Dispatch()
    {
        if (TryDispatch() is false)
        {
            throw new InvalidOperationException("No handler found for the pressed key.");
        }
    }

    /// <summary>
    /// 비동기로 키 입력에 따른 핸들러를 실행합니다.
    /// </summary>
    /// <returns>비동기 핸들러 실행 작업입니다.</returns>
    /// <exception cref="InvalidOperationException">등록되지 않은 키가 입력되었을 경우 발생합니다.</exception>
    public static async Task DispatchAsync()
    {
        if (await TryDispatchAsync() is false)
        {
            throw new InvalidOperationException("No handler found for the pressed key.");
        }
    }

    /// <summary>
    /// 키 입력에 따른 핸들러를 실행합니다.
    /// </summary>
    /// <returns>핸들러를 실행했을 경우 true, 해당하는 키에 대한 핸들러가 입력되지 않았을 경우 false입니다.</returns>
    public static bool TryDispatch()
    {
        return TryDispatchAsync().Result;
    }

    /// <summary>
    /// 비동기로 키 입력에 따른 핸들러를 실행합니다.
    /// </summary>
    /// <returns>핸들러를 실행했을 경우 true, 해당하는 키에 대한 핸들러가 입력되지 않았을 경우 false입니다.</returns>
    public static async Task<bool> TryDispatchAsync()
    {
        var keyInfo = Console.ReadKey(intercept: true);

        if (SyncHandlersByKey.TryGetValue(keyInfo.Key, out var syncHandler))
        {
            try
            {
                syncHandler.Invoke();
            }
            catch (Exception ex)
            {
                HandlerException?.Invoke(null, new HandlerExceptionEventArgs(keyInfo.Key, ex));
            }

            return true;
        }
        else if (AsyncHandlersByKey.TryGetValue(keyInfo.Key, out var asyncHandler))
        {
            try
            {
                await asyncHandler.Invoke();
            }
            catch (Exception ex)
            {
                HandlerException?.Invoke(null, new HandlerExceptionEventArgs(keyInfo.Key, ex));
            }

            return true;
        }
        else
        {
            KeyNotRegistered?.Invoke(null, new KeyNotRegisteredEventArgs(keyInfo.Key));
            return false;
        }
    }

    /// <summary>
    /// 키가 입력된 경우에만 핸들러를 실행합니다.
    /// 그렇지 않은 경우 바로 반환됩니다.
    /// </summary>
    public static void DispatchIfKeyAvailable()
    {
        if (Console.KeyAvailable)
        {
            TryDispatch();
        }
    }

    /// <summary>
    /// 키가 입력된 경우에만 핸들러를 비동기로 실행합니다.
    /// 그렇지 않은 경우 바로 반환됩니다.
    /// </summary>
    /// <returns>비동기 핸들러 실행 작업입니다.</returns>
    public static Task DispatchIfKeyAvailableAsync()
    {
        if (Console.KeyAvailable is false)
        {
            return Task.CompletedTask;
        }

        return TryDispatchAsync();
    }

    /// <summary>
    /// 디스패칭을 실패할 때 까지 무한 반복합니다.
    /// </summary>
    public static void KeepDispatchingUntilFails()
    {
        while (TryDispatch())
        {
        }
    }

    /// <summary>
    /// 디스패칭을 무한 반복합니다.
    /// 한 번 실행하면 <see cref="BindExitHandler(ConsoleKey)"/>로 등록한 키 입력 전까지 절대 반환되지 않습니다.
    /// </summary>
    public static void KeepDispatching()
    {
        isRequestedToExitDispatching = false;

        while (!isRequestedToExitDispatching)
        {
            TryDispatch();
        }
    }

    /// <summary>
    /// 백그라운드 스레드에서 디스패칭을 시작합니다.
    /// </summary>
    public static void StartBackgroundDispatching()
    {
        Thread dispatchThread = new Thread(() =>
        {
            KeepDispatching();
        });

        dispatchThread.Start();
    }
}
