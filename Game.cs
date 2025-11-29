/// <summary>
/// Manages the core game logic and round execution.
/// Handles player turns, word validation, commands, and result tracking.
/// </summary>
public class Game 
{
    private readonly IUserInterface _ui;
    private readonly DataRepository _repository;
    private readonly Dictionary<string, string> _language;
    
    private string _originalWord = null!;
    private Dictionary<char, int> _originalWordLetters = null!;
    private List<string> _attempts;

    /// <summary>
    /// Initializes a new game with required dependencies.
    /// </summary>
    /// <param name="ui">User interface for input/output operations.</param>
    /// <param name="repository">Data repository for dictionary and results.</param>
    /// <param name="language">Localization dictionary for messages.</param>
    public Game(IUserInterface ui, DataRepository repository, Dictionary<string, string> language) 
    {
        _ui = ui;
        _repository = repository;
        _language = language;
        _attempts = [];
    }
    
    /// <summary>
    /// Starts and manages the complete game round between two players.
    /// Players take turns until one loses, then saves and displays results.
    /// </summary>
    /// <param name="player1">The first player.</param>
    /// <param name="player2">The second player.</param>
    /// <param name="originalWord">The base word from which players must form words.</param>
    public void Start(Player player1, Player player2, string originalWord) 
    {
        _originalWord = originalWord;
        _originalWordLetters = GetLetterCounts(originalWord);
        
        while (true)
        {
            if (!PlayerTurn(player1, player1, player2)) break;
            if (!PlayerTurn(player2, player1, player2)) break;
        }
        
        ShowResults(player1, player2);
    }

    /// <summary>
    /// Executes one complete turn for a player.
    /// Handles time limits, word input, validation, and command processing.
    /// </summary>
    /// <param name="player">The player taking the turn.</param>
    /// <param name="player1">First player (needed for score command).</param>
    /// <param name="player2">Second player (needed for score command).</param>
    /// <returns>True if turn completed successfully, false if player loses or exits.</returns>
    private bool PlayerTurn(Player player, Player player1, Player player2) 
    {
        const int InputTimeLimit = 20;
        DateTime start = DateTime.Now;
        
        while (true) 
        {
            if (!IsTimeOver(start, InputTimeLimit, player.Name)) 
                return false;

            string? input = AskWord(GetRemainingMs(start, InputTimeLimit));
            string? cmd = HandleCommand(input, player1, player2);
            
            if (input == null) 
                return false;

            if (input == "") continue;

            if (cmd == "executed") 
                continue;

            if (cmd == "exit") 
                return false;

            string playerWord = input.ToLower();
            if (_attempts.Contains(playerWord)) 
            {
                _ui.PrintError(_language["reuse_word_error"]);
                continue;
            }

            if (IsWordValid(playerWord, player.Name)) 
            {
                _attempts.Add(playerWord);
                break;
            }
        }
        return true;
    }

    /// <summary>
    /// Validates that a word can be formed from the original word's letters.
    /// Checks letter availability and frequency constraints.
    /// </summary>
    /// <param name="playerWord">The word to validate.</param>
    /// <param name="playerName">The player's name (for error messages).</param>
    /// <returns>True if word is valid, false otherwise.</returns>
    private bool IsWordValid(string playerWord, string playerName) 
    {
        Dictionary<char, int> playerWordDictionary = GetLetterCounts(playerWord);
        int validLetters = 0;
        
        foreach (var pair in playerWordDictionary) 
        {
            char key = pair.Key;
            int value1 = pair.Value;

            if (_originalWordLetters.TryGetValue(key, out int value2)) 
            {
                if (value1 > value2) 
                {
                    _ui.PrintError(_language["reuse_letters_error"]);
                    return false;
                }
                else validLetters += value1;
            }
            else 
            {
                _ui.PrintError(_language["availability_letters_error"]);
                validLetters = -1;
                break;
            }
        }
        return validLetters == playerWord.Length;
    }

    /// <summary>
    /// Counts the frequency of each character in a word.
    /// </summary>
    /// <param name="word">The word to analyze.</param>
    /// <returns>Dictionary mapping each character to its count.</returns>
    private Dictionary<char, int> GetLetterCounts(string word) 
    {
        var counts = new Dictionary<char, int>();

        foreach (char ch in word) 
        {
            if (counts.TryGetValue(ch, out int c)) 
                counts[ch] = c + 1;
            else 
                counts[ch] = 1;
        }
        return counts;
    }

    /// <summary>
    /// Prompts the user for a word with timeout validation.
    /// Checks if input exists in dictionary.
    /// </summary>
    /// <param name="remainingMs">Milliseconds remaining before timeout.</param>
    /// <returns>The input word, empty string if invalid, or null if timeout.</returns>
    private string? AskWord(int remainingMs) 
    {
        _ui.PrintLine(_language["ask_word"]);
        string? input = ReadLineWithTimeOut(remainingMs)?.ToLower()?.Trim();

        if (input == null) 
        {
            _ui.PrintLine(_language["loose_time"]);
            return null;
        }

        if (input.StartsWith("/")) return input;

        if (!_repository.Dictionary.Contains(input)) 
        {
            _ui.PrintError(_language["word_not_in_dictionary"]);
            return "";
        }
        return input;
    }

    /// <summary>
    /// Reads user input with a specified timeout.
    /// </summary>
    /// <param name="time">Timeout in milliseconds.</param>
    /// <returns>User input or null if timeout exceeded.</returns>
    private string? ReadLineWithTimeOut(int time) 
    {
        string? result = null;
        var task = Task.Run(() => result = _ui.ReadLine());
        if (task.Wait(time)) return result;
        else return null;
    }

    /// <summary>
    /// Checks if the player's time limit has been exceeded.
    /// Displays remaining time and turn information.
    /// </summary>
    /// <param name="start">The time when the turn started.</param>
    /// <param name="limit">Time limit in seconds.</param>
    /// <param name="playerName">Name of the current player.</param>
    /// <returns>True if time remaining, false if expired.</returns>
    private bool IsTimeOver(DateTime start, int limit, string playerName) 
    {
        double elapsedMs = (DateTime.Now - start).TotalMilliseconds;
        int remainingMs = (int)(limit * 1000 - elapsedMs);
        int remainingSec = remainingMs > 0 ? (remainingMs + 999) / 1000 : 0;

        if (remainingMs <= 0) 
        {
            _ui.PrintLine($"{playerName} {_language["player_timeOut"]}");
            return false;
        }
        _ui.PrintOrange($"\n{playerName} {_language["player_move"]}");
        _ui.Print(string.Format(_language["time_left"], remainingSec));
        return true;
    }

    /// <summary>
    /// Calculates remaining time in milliseconds.
    /// </summary>
    /// <param name="start">The time when the turn started.</param>
    /// <param name="limit">Time limit in seconds.</param>
    /// <returns>Remaining milliseconds.</returns>
    private int GetRemainingMs(DateTime start, int limit) 
    {
        double elapsedMs = (DateTime.Now - start).TotalMilliseconds;
        int remainingMs = (int)(limit * 1000 - elapsedMs);
        return remainingMs;
    }

    /// <summary>
    /// Processes player commands like /help, /score, /exit, etc.
    /// </summary>
    /// <param name="input">The user input string.</param>
    /// <param name="player1">First player (for score command).</param>
    /// <param name="player2">Second player (for score command).</param>
    /// <returns>"executed" if command processed, "exit" to end game, "none" if not a command.</returns>
    private string HandleCommand(string input, Player player1, Player player2) 
    {
        if (!input.StartsWith("/"))
            return "none";

        switch (input) 
        {
            case "/help":
                _ui.PrintLine($"\n{_language["available_commands"]}");
                _ui.PrintLine($"/show-words - {_language["show_words"]}");
                _ui.PrintLine($"/score - {_language["score"]}");
                _ui.PrintLine($"/total-score - {_language["total_score"]}");
                _ui.PrintLine($"/exit - {_language["exit"]}");
                return "executed";

            case "/show-words":
                ShowWords();
                return "executed";

            case "/score":
                ShowHeadToHeadScore(player1, player2);
                return "executed";

            case "/total-score":
                ShowTotalScore();
                return "executed";

            case "/exit":
                return "exit";

            default:
                _ui.PrintError(_language["wrong_command"]);
                return "executed";
        }
    }

    /// <summary>
    /// Displays all words used in the current round.
    /// </summary>
    private void ShowWords() 
    {
        _ui.Print(_language["used_words"]);
        foreach (string word in _attempts) 
            _ui.Print($"{word} ");
        _ui.Print("]");
    }

    /// <summary>
    /// Displays win statistics between two specific players.
    /// </summary>
    /// <param name="player1">First player.</param>
    /// <param name="player2">Second player.</param>
    private void ShowHeadToHeadScore(Player player1, Player player2) 
    {
        Dictionary<string, int> scores = _repository.GetHeadToHeadScores(player1.Name, player2.Name);
        
        if (scores.Count == 0) 
        {
            _ui.PrintError(_language["file_data_error"]);
            return;
        }
        
        ShowTableOfBestPlayers(scores);
    }

    /// <summary>
    /// Displays global leaderboard with all players' total wins.
    /// </summary>
    private void ShowTotalScore() 
    {
        Dictionary<string, int> scores = _repository.GetTotalScores();
        
        if (scores.Count == 0) 
        {
            _ui.PrintError(_language["file_data_error"]);
            return;
        }
        
        ShowTableOfBestPlayers(scores);
    }

    /// <summary>
    /// Displays a formatted leaderboard sorted by wins.
    /// </summary>
    /// <param name="scores">Dictionary of player names and their win counts.</param>
    private void ShowTableOfBestPlayers(Dictionary<string, int> scores) 
    {
        var sortedScore = scores.OrderByDescending(s => s.Value);
        _ui.PrintGreen($"\n{_language["best_players"]}");
        foreach (var player in sortedScore) 
        {
            _ui.PrintOrange($"\t{player.Key} - {player.Value}");
        }
    }

    /// <summary>
    /// Determines the winner, saves the result, and displays game summary.
    /// Winner is determined by the parity of attempts count.
    /// </summary>
    /// <param name="player1">First player.</param>
    /// <param name="player2">Second player.</param>
    private void ShowResults(Player player1, Player player2) 
    {
        string winner;
        if (_attempts.Count % 2 == 0) 
        {
            _ui.PrintGreen($"{player2.Name} {_language["player_win"]}");
            winner = player2.Name;
        }
        else 
        {
            _ui.PrintGreen($"{player1.Name} {_language["player_win"]}");
            winner = player1.Name;
        }
        
        var gameResult = new GameResult 
        {
            Player1 = player1.Name,
            Player2 = player2.Name,
            Winner = winner
        };
        
        _repository.SaveResult(gameResult);
        ShowWords();
    }
}
