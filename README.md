# ConsoleKeyDispatcher

ConsoleKeyDispatcher는 콘솔 애플리케이션에서 키 입력에 따라 다양한 액션(동기/비동기 핸들러)을 손쉽게 등록하고 실행할 수 있도록 도와주는 도구입니다.

## 주요 기능

- 특정 키에 동기/비동기 핸들러 등록
- 등록된 핸들러 이름 및 키 목록 조회
- ESC(기본) 또는 지정한 키로 종료
- 키 입력이 있을 때만 핸들러 실행 또는 무한 디스패칭 루프 지원

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

## 라이선스

MIT License
