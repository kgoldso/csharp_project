/// <summary>
/// Console-based implementation of IUserInterface.
/// Handles all input/output operations through the system console.
/// </summary>
public class ConsoleUI : IUserInterface
{
    /// <summary>
    /// Reads a line of input from the console.
    /// </summary>
    /// <returns>The input string or null if no input.</returns>
    public string? ReadLine() => Console.ReadLine();

    /// <summary>
    /// Prints a message to the console without a line break.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void Print(string message) => Console.Write(message);

    /// <summary>
    /// Prints a message to the console followed by a line break.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void PrintLine(string message) => Console.WriteLine(message);

    /// <summary>
    /// Prints an error message in red color.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    public void PrintError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a message in green color.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void PrintGreen(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    /// <summary>
    /// Prints a message in yellow/orange color.
    /// </summary>
    /// <param name="message">The message to display.</param>
    public void PrintOrange(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}
