using ConsoleKeyUtils;

namespace Example;

internal class Program
{
    static void Main(string[] args)
    {
        ConsoleKeyDispatcher dispatcher = new ConsoleKeyDispatcher();

        // 핸들러 정보를 출력하는 도움말 핸들러를 등록합니다.
        dispatcher.BindHandler(ConsoleKey.H, () =>
        {
            foreach ((ConsoleKey key, string? name) in dispatcher.HandlerNames)
            {
                Console.WriteLine($"Handler: {key} | {name ?? "No Name"}");
            }
        }, "Help");

        // 등록되지 않은 키가 입력되었을 때의 이벤트 핸들러를 등록합니다.
        dispatcher.KeyNotRegistered += (sender, e) =>
        {
            Console.WriteLine($"Key '{e.Key}' is not registered. Press 'H' for help.");
        };

        dispatcher.BindExitHandler();

        dispatcher.BindHandler(ConsoleKey.A, () => Console.WriteLine("Hello, World!"), "Print Hello, World!");
        
        dispatcher.BindHandler(ConsoleKey.B, () => Console.WriteLine(DateTime.Now), "Print DateTime.Now");
        
        dispatcher.BindAsyncHandler(ConsoleKey.C, async () =>
        {
            await Task.Delay(1000);
            Console.WriteLine("Async operation completed after 1 second.");
        }, "Async Operation");

        dispatcher.KeepDispatching();
    }
}
