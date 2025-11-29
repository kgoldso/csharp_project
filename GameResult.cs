/// <summary>
/// Represents the result of a completed game.
/// Used for saving and loading game history.
/// </summary>
public class GameResult
{
    /// <summary>
    /// Gets or sets the name of the first player.
    /// </summary>
    public required string Player1 { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the second player.
    /// </summary>
    public required string Player2 { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the winner.
    /// </summary>
    public required string Winner { get; set; }
}
