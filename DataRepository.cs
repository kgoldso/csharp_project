/// <summary>
/// Manages all data operations including dictionary loading and game results persistence.
/// Responsible for file I/O operations and data transformations.
/// </summary>
public class DataRepository 
{
    /// <summary>
    /// Gets the dictionary of valid words loaded from file.
    /// </summary>
    public HashSet<string> Dictionary { get; private set; }
    
    private const string ResultFilePath = "result.json";

    /// <summary>
    /// Initializes a new DataRepository and loads the dictionary from file.
    /// </summary>
    /// <param name="dictionaryFilePath">Path to the dictionary file.</param>
    /// <exception cref="FileNotFoundException">Thrown when dictionary file is not found.</exception>
    public DataRepository(string dictionaryFilePath) 
    {
        if (!File.Exists(dictionaryFilePath)) 
        {
            throw new FileNotFoundException($"Файл {dictionaryFilePath} не найден.");
        }

        Dictionary = new HashSet<string>(
            File.ReadAllLines(dictionaryFilePath), 
            StringComparer.OrdinalIgnoreCase
        );
    }

    /// <summary>
    /// Saves a game result to the results file.
    /// Appends to existing results or creates new file if doesn't exist.
    /// </summary>
    /// <param name="result">The game result to save.</param>
    public void SaveResult(GameResult result) 
    {
        List<GameResult> results = [];

        if (File.Exists(ResultFilePath)) 
        {
            string json = File.ReadAllText(ResultFilePath);
            results = System.Text.Json.JsonSerializer.Deserialize<List<GameResult>>(json) ?? [];
        }
        
        results.Add(result);

        string updatedJson = System.Text.Json.JsonSerializer.Serialize(results, 
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ResultFilePath, updatedJson);
    }

    /// <summary>
    /// Loads all game results from the results file.
    /// </summary>
    /// <returns>List of all saved game results. Empty list if file doesn't exist.</returns>
    public List<GameResult> LoadResults()
    {
        if (!File.Exists(ResultFilePath))
            return new List<GameResult>();
        
        string json = File.ReadAllText(ResultFilePath);
        return System.Text.Json.JsonSerializer.Deserialize<List<GameResult>>(json) 
               ?? new List<GameResult>();
    }

    /// <summary>
    /// Calculates total wins for all players across all games.
    /// </summary>
    /// <returns>Dictionary mapping player names to their total win counts.</returns>
    public Dictionary<string, int> GetTotalScores()
    {
        List<GameResult> results = LoadResults();
        
        if (results.Count == 0)
        {
            return new Dictionary<string, int>();
        }
        
        Dictionary<string, int> scores = new Dictionary<string, int>();
        
        foreach (var result in results)
        {
            if (scores.TryGetValue(result.Winner, out int currentScore))
            {
                scores[result.Winner] = currentScore + 1;
            }
            else
            {
                scores[result.Winner] = 1;
            }
        }
        
        return scores;
    }

    /// <summary>
    /// Calculates win statistics between two specific players.
    /// </summary>
    /// <param name="player1Name">Name of the first player.</param>
    /// <param name="player2Name">Name of the second player.</param>
    /// <returns>Dictionary mapping player names to win counts in their head-to-head matches.</returns>
    public Dictionary<string, int> GetHeadToHeadScores(string player1Name, string player2Name)
    {
        List<GameResult> results = LoadResults();
        
        if (results.Count == 0)
        {
            return new Dictionary<string, int>();
        }
        
        Dictionary<string, int> scores = new Dictionary<string, int>();
        
        foreach (var result in results)
        {
            bool isMatchBetweenPlayers = 
                (result.Player1 == player1Name && result.Player2 == player2Name) ||
                (result.Player1 == player2Name && result.Player2 == player1Name);
            
            if (isMatchBetweenPlayers)
            {
                if (scores.TryGetValue(result.Winner, out int currentScore))
                {
                    scores[result.Winner] = currentScore + 1;
                }
                else
                {
                    scores[result.Winner] = 1;
                }
            }
        }
        
        return scores;
    }
}
