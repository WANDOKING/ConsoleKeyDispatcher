# ConsoleKeyDispatcher

ConsoleKeyDispatcher는 콘솔 애플리케이션에서 키 입력에 따라 다양한 액션(동기/비동기 핸들러)을 손쉽게 등록하고 실행할 수 있도록 도와주는 도구입니다.

## 주요 기능

- 특정 키에 동기/비동기 핸들러 등록
- 등록된 핸들러 이름 및 키 목록 조회
- ESC(기본) 또는 지정한 키로 종료
- 키 입력이 있을 때만 핸들러 실행 또는 무한 디스패칭 루프 지원
- 등록되지 않은 키 입력 및 핸들러 실행 중 발생한 예외를 이벤트로 통지

## 사용 예시

아래는 Example 프로젝트의 간단한 사용 예시입니다.

### 시작하기

1. `ConsoleKeyDispatcher.BindHandler` 또는 `ConsoleKeyDispatcher.BindAsyncHandler`로 키와 핸들러를 등록합니다.
2. `ConsoleKeyDispatcher.BindExitHandler`로 종료 키를 등록합니다(기본 ESC).
3. `ConsoleKeyDispatcher.KeepDispatching()`을 호출하여 입력을 처리합니다.

### 예제 코드
```csharp
ConsoleKeyDispatcher.BindHandler(ConsoleKey.H, () =>
{
    foreach ((ConsoleKey key, string? name) in ConsoleKeyDispatcher.HandlerNames)
    {
        Console.WriteLine($"Handler: {key} | {name ?? "No Name"}");
    }
}, "Help");

ConsoleKeyDispatcher.KeyNotRegistered += (sender, e) =>
{
    Console.WriteLine($"Key '{e.Key}' is not registered. Press 'H' for help.");
};

ConsoleKeyDispatcher.BindExitHandler();

ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => Console.WriteLine("Hello, World!"), "Print Hello, World!");

ConsoleKeyDispatcher.BindHandler(ConsoleKey.B, () => Console.WriteLine(DateTime.Now), "Print DateTime.Now");

ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.C, async () =>
{
    await Task.Delay(1000);
    Console.WriteLine("Async operation completed after 1 second.");
}, "Async Operation");

Console.WriteLine("Press 'H' for help. Press 'Escape' to exit.");
ConsoleKeyDispatcher.KeepDispatching();
```

## 핸들러 예외 처리

핸들러 내부에서 발생한 예외는 디스패처가 잡아서 `HandlerException` 이벤트로 통지하며, `Dispatch()`나 `KeepDispatching()`을 호출한 쪽으로는 전파되지 않습니다.
덕분에 핸들러 하나가 실패해도 디스패칭 루프는 중단되지 않지만, **`HandlerException`을 구독하지 않으면 예외가 아무 흔적 없이 사라지므로 반드시 구독하는 것을 권장합니다.**

```csharp
ConsoleKeyDispatcher.HandlerException += (sender, e) =>
{
    // e.Key: 예외가 발생한 핸들러에 대응하는 키
    // e.Exception: 발생한 예외
    Console.WriteLine($"Handler for key '{e.Key}' failed: {e.Exception}");
};

ConsoleKeyDispatcher.BindHandler(ConsoleKey.D, () =>
{
    throw new InvalidOperationException("Something went wrong.");
}, "Throw Exception");

ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.E, async () =>
{
    await Task.Delay(100);
    throw new TimeoutException("Async operation timed out.");
}, "Throw Async Exception");
```

`D` 키를 누르면 핸들러가 던진 `InvalidOperationException`이, `E` 키를 누르면 비동기 핸들러가 던진 `TimeoutException`이 위 이벤트로 전달되고, 디스패칭 루프는 그대로 계속 동작합니다.

> [!NOTE]
> `HandlerException` 이벤트 구독자 자체에서 발생한 예외는 디스패처가 잡지 않고 호출자에게 전파됩니다.
> 특히 `StartBackgroundDispatching()`으로 실행 중인 경우 이 예외를 받아줄 곳이 없어 프로세스가 종료될 수 있으므로, 구독자 안에서는 예외가 발생하지 않도록 주의하세요.

## 라이선스

MIT License
