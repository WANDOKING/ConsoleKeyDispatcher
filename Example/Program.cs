using ConsoleKeyUtils;

namespace Example;

internal class Program
{
    private static void Main(string[] args)
    {
        ConsoleKeyDispatcher.BindHandler(ConsoleKey.H, () =>
        {
            foreach ((ConsoleKey key, string? name) in ConsoleKeyDispatcher.HandlerNames)
            {
                Console.WriteLine($"Handler: {key} | {name ?? "No Name"}");
            }
        }, "Help");

        ConsoleKeyDispatcher.BindHandler(
            ConsoleKey.J,
            () => Console.WriteLine(ConsoleKeyDispatcher.GetJsonDescriptions(indented: true)),
            "Print Handlers As Json");

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
    }
}
