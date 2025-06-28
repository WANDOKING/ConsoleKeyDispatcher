namespace ConsoleKeyUtils;

/// <summary>
/// 콘솔 키 입력에 따른 핸들러를 등록하고 실행하는 디스패처입니다.
/// </summary>
public class ConsoleKeyDispatcher
{
    private readonly Dictionary<ConsoleKey, string?> handlerNames = new ();
    private readonly Dictionary<ConsoleKey, Func<Task>> asyncHandlers = new ();
    private readonly Dictionary<ConsoleKey, Action> syncHandlers = new ();
    private bool isRequestedToExitDispatching;

    /// <summary>
    /// 생성자입니다.
    /// </summary>
    /// <param name="useBackgroundDispatchThread">true로 설정하면 백그라운드에서 디스패칭하는 스레드를 수행합니다.</param>
    public ConsoleKeyDispatcher(bool useBackgroundDispatchThread = false)
    {
        if (useBackgroundDispatchThread)
        {
            Thread dispatchThread = new Thread(() =>
            {
                KeepDispatching();
            });

            dispatchThread.Start();
        }
    }

    /// <summary>
    /// 기본 디스패처 인스턴스입니다.
    /// </summary>
    public static ConsoleKeyDispatcher Default { get; } = new ConsoleKeyDispatcher();

    /// <summary>
    /// 등록된 핸들러들입니다.
    /// </summary>
    public IEnumerable<(ConsoleKey key, string? name)> HandlerNames => handlerNames.Select(kv => (kv.Key, kv.Value));

    /// <summary>
    /// 비동기 핸들러를 등록합니다.
    /// </summary>
    /// <param name="key">입력 키입니다.</param>
    /// <param name="handler">키 입력 시 실행할 핸들러입니다.</param>
    /// <param name="name">핸들러의 이름입니다.</param>
    /// <exception cref="ArgumentException">이미 등록된 키를 등록하고자 할할 경우 발생합니다.</exception>
    public void BindAsyncHandler(ConsoleKey key, Func<Task> handler, string? name = default)
    {
        if (handlerNames.ContainsKey(key))
        {
            throw new ArgumentException($"A handler for the key '{key}' is already registered.", nameof(key));
        }

        asyncHandlers[key] = handler;
        handlerNames[key] = name;
    }

    /// <summary>
    /// 핸들러를 등록합니다.
    /// </summary>
    /// <param name="key">입력 키입니다.</param>
    /// <param name="handler">키 입력 시 실행할 핸들러입니다.</param>
    /// <param name="name">핸들러의 이름입니다.</param>
    /// <exception cref="ArgumentException">이미 등록된 키를 등록하고자 할할 경우 발생합니다.</exception>
    public void BindHandler(ConsoleKey key, Action handler, string? name = default)
    {
        if (handlerNames.ContainsKey(key))
        {
            throw new ArgumentException($"A handler for the key '{key}' is already registered.", nameof(key));
        }

        syncHandlers[key] = handler;
        handlerNames[key] = name;
    }

    /// <summary>
    /// <see cref="KeepDispatching"/>을 반환하게 하는 핸들러를 등록합니다.
    /// </summary>
    /// <param name="key">입력 키입니다.</param>
    public void BindExitHandler(ConsoleKey key = ConsoleKey.Escape)
    {
        BindHandler(key, () => isRequestedToExitDispatching = true, "Exit Handler");
    }

    /// <summary>
    /// 핸들러를 제거합니다.
    /// </summary>
    /// <param name="key">제거할 입력 키입니다.</param>
    /// <returns>핸들러를 제거했을 경우 true, 그렇지 않으면 false입니다.</returns>
    public bool RemoveHandler(ConsoleKey key)
    {
        if (handlerNames.Remove(key))
        {
            if (syncHandlers.Remove(key) is false)
            {
                asyncHandlers.Remove(key);
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// 키 입력에 따른 핸들러를 실행합니다.
    /// </summary>
    /// <exception cref="InvalidOperationException">등록되지 않은 키가 입력되었을 경우 발생합니다.</exception>
    public void Dispatch()
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
    public async Task DispatchAsync()
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
    public bool TryDispatch()
    {
        return TryDispatchAsync().Result;
    }

    /// <summary>
    /// 비동기로 키 입력에 따른 핸들러를 실행합니다.
    /// </summary>
    /// <returns>핸들러를 실행했을 경우 true, 해당하는 키에 대한 핸들러가 입력되지 않았을 경우 false입니다.</returns>
    public async Task<bool> TryDispatchAsync()
    {
        var keyInfo = Console.ReadKey(intercept: true);

        if (syncHandlers.TryGetValue(keyInfo.Key, out var syncHandler))
        {
            syncHandler.Invoke();
            return true;
        }
        else if (asyncHandlers.TryGetValue(keyInfo.Key, out var asyncHandler))
        {
            await asyncHandler.Invoke();
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 키가 입력된 경우에만 핸들러를 실행합니다.
    /// 그렇지 않은 경우 바로 반환됩니다.
    /// </summary>
    public void DispatchIfKeyAvailable()
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
    public Task DispatchIfKeyAvailableAsync()
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
    public void KeepDispatchingUntilFails()
    {
        while (TryDispatch())
        {
        }
    }

    /// <summary>
    /// 디스패칭을 무한 반복합니다.
    /// 한 번 실행하면 <see cref="Dispose"/>호출 전까지 절대 반환되지 않습니다.
    /// </summary>
    public void KeepDispatching()
    {
        while (!isRequestedToExitDispatching)
        {
            TryDispatch();
        }
    }
}