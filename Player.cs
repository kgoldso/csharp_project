/// <summary>
/// Represents a player in the game.
/// Stores player information such as name.
/// </summary>
public class Player
{
    /// <summary>
    /// Gets the player's name. Cannot be changed after creation.
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// Initializes a new player with the specified name.
    /// </summary>
    /// <param name="name">The name of the player.</param>
    public Player(string name)
    {
        Name = name;
    }
}
