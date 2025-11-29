/// <summary>
/// Defines the contract for user interface operations.
/// Allows different UI implementations (Console, GUI, etc.).
/// </summary>
public interface IUserInterface
{
    /// <summary>
    /// Reads a line of input from the user.
    /// </summary>
    /// <returns>The input string or null if no input.</returns>
    string? ReadLine();

    /// <summary>
    /// Prints a message to the output.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void Print(string message);

    /// <summary>
    /// Prints a message followed by a line break.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void PrintLine(string message);

    /// <summary>
    /// Prints an error message (typically in red).
    /// </summary>
    /// <param name="message">The error message to display.</param>
    void PrintError(string message);

    /// <summary>
    /// Prints a message in green color.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void PrintGreen(string message);

    /// <summary>
    /// Prints a message in orange/yellow color.
    /// </summary>
    /// <param name="message">The message to display.</param>
    void PrintOrange(string message);
}
