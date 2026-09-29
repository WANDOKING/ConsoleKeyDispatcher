# ConsoleKeyDispatcher

ConsoleKeyDispatcher는 콘솔 애플리케이션에서 키 입력에 따라 다양한 액션(동기/비동기 핸들러)을 손쉽게 등록하고 실행할 수 있도록 도와주는 도구입니다.

## 주요 기능

- 특정 키에 동기/비동기 핸들러 등록
- 등록된 핸들러 이름 및 키 목록 조회 (열거 또는 JSON)
- 등록된 핸들러 목록을 JSON으로 출력하는 도움말 키 등록 (H 기본)
- ESC(기본) 또는 지정한 키로 종료
- 키 입력이 있을 때만 핸들러 실행 또는 무한 디스패칭 루프 지원
- 등록되지 않은 키 입력 및 핸들러 실행 중 발생한 예외를 이벤트로 통지

## 사용 예시

아래는 ConsoleKeyDispatcher.Example 프로젝트의 간단한 사용 예시입니다.

### 시작하기

1. `ConsoleKeyDispatcher.BindHandler` 또는 `ConsoleKeyDispatcher.BindAsyncHandler`로 키와 핸들러를 등록합니다.
2. `ConsoleKeyDispatcher.BindExitHandler`로 종료 키를 등록합니다(기본 ESC).
3. 필요하다면 `ConsoleKeyDispatcher.BindHelpHandler`로 도움말 키를 등록합니다(기본 H).
4. `ConsoleKeyDispatcher.KeepDispatching()`을 호출하여 입력을 처리합니다. 비동기 핸들러가 많다면 `await ConsoleKeyDispatcher.KeepDispatchingAsync()`를 사용할 수 있습니다.

### 예제 코드
```csharp
ConsoleKeyDispatcher.BindHelpHandler();

ConsoleKeyDispatcher.KeyNotRegistered += (sender, e) =>
{
    Console.WriteLine($"Key '{e.Key}' is not registered. Press 'H' for help.");
};

ConsoleKeyDispatcher.HandlerException += (sender, e) =>
{
    Console.WriteLine($"Handler for key '{e.Key}' failed: {e.Exception}");
};

ConsoleKeyDispatcher.BindExitHandler();

ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => Console.WriteLine("Hello, World!"), "Print Hello, World!");

ConsoleKeyDispatcher.BindHandler(ConsoleKey.B, () => Console.WriteLine(DateTime.Now), "Print DateTime.Now");

ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.C, async () =>
{
    await Task.Delay(1000);
    Console.WriteLine("Async operation completed after 1 second.");
}, "Async Operation");

ConsoleKeyDispatcher.BindHandler(
    ConsoleKey.D,
    () => throw new InvalidOperationException("Something went wrong."),
    "Throw Exception");

ConsoleKeyDispatcher.BindAsyncHandler(
    ConsoleKey.E,
    async () =>
    {
        await Task.Delay(100);
        throw new TimeoutException("Async operation timed out.");
    },
    "Throw Async Exception");

Console.WriteLine("Press 'H' for help. Press 'Escape' to exit.");
ConsoleKeyDispatcher.KeepDispatching();
```

## 등록된 핸들러 목록 조회

`HandlerNames`는 `(ConsoleKey key, string? name)` 튜플을 열거하며, `GetJsonDescriptions()`는 같은 정보를 JSON 문자열로 반환합니다.
`Handlers` 배열 안에 핸들러마다 `Key`(키 이름)와 `Name`(핸들러 이름)을 가지는 객체가 들어가며, 이름 없이 등록한 핸들러의 `Name`은 `null`입니다.

```csharp
ConsoleKeyDispatcher.BindHandler(ConsoleKey.A, () => Console.WriteLine("Hello, World!"), "Print Hello, World!");
ConsoleKeyDispatcher.BindAsyncHandler(ConsoleKey.C, () => Task.CompletedTask, "Async Operation");
ConsoleKeyDispatcher.BindExitHandler();

// 기본값은 들여쓰기 출력입니다.
Console.WriteLine(ConsoleKeyDispatcher.GetJsonDescriptions());

// 한 줄로 받고 싶다면 indented: false를 사용합니다.
Console.WriteLine(ConsoleKeyDispatcher.GetJsonDescriptions(indented: false));
```

기본 호출의 출력은 다음과 같습니다.

```json
{
  "Handlers": [
    {
      "Key": "A",
      "Name": "Print Hello, World!"
    },
    {
      "Key": "C",
      "Name": "Async Operation"
    },
    {
      "Key": "Escape",
      "Name": "Exit Handler"
    }
  ]
}
```

`indented: false`로 호출하면 한 줄로 반환됩니다.

```json
{"Handlers":[{"Key":"A","Name":"Print Hello, World!"},{"Key":"C","Name":"Async Operation"},{"Key":"Escape","Name":"Exit Handler"}]}
```

한글처럼 ASCII가 아닌 핸들러 이름도 `\uXXXX`로 이스케이프되지 않고 그대로 출력됩니다.
다만 배열의 순서는 등록 순서를 보장하지 않으므로, 정렬된 결과가 필요하다면 역직렬화 후 직접 정렬해서 사용하세요.

### 도움말 키 등록

`BindHelpHandler`는 위 JSON을 콘솔에 출력하는 핸들러를 등록합니다. `BindExitHandler`처럼 키만 지정하면 되고, 기본 키는 `H`입니다.

```csharp
// H 키를 누르면 등록된 핸들러 목록이 JSON으로 출력됩니다.
ConsoleKeyDispatcher.BindHelpHandler();

// 키와 들여쓰기 여부를 바꿀 수도 있습니다.
ConsoleKeyDispatcher.BindHelpHandler(ConsoleKey.F1, indented: false);
```

핸들러 실행 시점에 목록을 조회하므로, `BindHelpHandler`를 먼저 호출하고 나중에 등록한 핸들러도 출력에 포함됩니다.

## 핸들러 예외 처리

핸들러 내부에서 발생한 예외는 디스패처가 잡아서 `HandlerException` 이벤트로 통지하며, `Dispatch()`, `KeepDispatching()`, `KeepDispatchingAsync()`를 호출한 쪽으로는 전파되지 않습니다.
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
