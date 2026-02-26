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

1. `ConsoleKeyDispatcher` 클래스를 인스턴스화합니다.
2. `BindHandler` 또는 `BindAsyncHandler`로 키와 핸들러를 등록합니다.
3. `BindExitHandler`로 종료 키를 등록합니다(기본 ESC).
4. `KeepDispatching()`을 호출하여 입력을 처리합니다.

### 예제 코드
```csharp
ConsoleKeyDispatcher dispatcher = new ConsoleKeyDispatcher();

// 핸들러 정보를 출력하는 도움말 핸들러를 등록합니다.
dispatcher.BindHandler(ConsoleKey.H, () =>
{
    foreach ((ConsoleKey key, string? name) in dispatcher.HandlerNames)
    {
        Console.WriteLine($"Handler: {key} | {name ?? "No Name"}");
    }
}, "Help");

dispatcher.BindExitHandler();

dispatcher.BindHandler(ConsoleKey.A, () => Console.WriteLine("Hello, World!"), "Print Hello, World!");
        
dispatcher.BindHandler(ConsoleKey.B, () => Console.WriteLine(DateTime.Now), "Print DateTime.Now");
        
dispatcher.BindAsyncHandler(ConsoleKey.C, async () =>
{
    await Task.Delay(1000);
    Console.WriteLine("Async operation completed after 1 second.");
}, "Async Operation");

dispatcher.KeepDispatching();
```

## 라이선스

MIT License
