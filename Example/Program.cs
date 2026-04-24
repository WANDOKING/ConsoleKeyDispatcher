using ConsoleKeyUtils;

namespace Example;

internal class Program
{
    static void Main(string[] args)
    {
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

        ConsoleKeyDispatcher.KeepDispatching();
    }
}
