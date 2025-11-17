using System.Globalization;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.Marshalling;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using System.IO;
using System.Threading.Tasks;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

ConsoleUI ui = new();
HashSet<string> dictionary;
Dictionary<string, string> language = [];
List<string> attempts = [];

ChooseLanguage();
string player1Name = GetPlayerName("1", language);
string player2Name = GetPlayerName("2", language);

string originalWord = GetOriginalWord(dictionary, language);
Dictionary<char, int> originalWordDictionary = GetLetterCounts(originalWord);
HandleCommand(originalWord, attempts, language);

while (true)
{
    if (!PlayerTurn(player1Name, dictionary, language, originalWordDictionary, attempts)) break;
    if (!PlayerTurn(player2Name, dictionary, language, originalWordDictionary, attempts)) break;
}

ShowResults();

string? ReadLineWithTimeOut(int time) {
    string? result = null;
    var task = Task.Run(() => result = ui.ReadLine());
    if (task.Wait(time)) return result;
    else return null;
}
/// <summary>
/// Handles language selection at program start. 
/// Loads the appropriate dictionary file and sets the language dictionary.
/// </summary>
void ChooseLanguage() {
    while (true) {
        ui.PrintLine("Выберите язык / Choose language: Русский(1), English(2)");
        string? choice = ui.ReadLine();

        if (choice == "1") {
            if (!File.Exists("russian.txt")) {
                ui.PrintError("Файл russian.txt не найден.");
                Environment.Exit(1);
            }
            SetLanguageRussian();
            dictionary = new(File.ReadAllLines("russian.txt"), StringComparer.OrdinalIgnoreCase);
            break;
        }
        else if (choice == "2") {
            if (!File.Exists("english.txt")) {
                ui.PrintError("File english.txt not found.");
                Environment.Exit(1);
            }
            SetLanguageEnglish();
            dictionary = new(File.ReadAllLines("english.txt"), StringComparer.OrdinalIgnoreCase);
            break;
        }
        else ui.PrintError("\nОшибка ввода / Input Error");
    }
}
/// <summary>
/// Asks the user to enter a nickname for a player.
/// Validates non-empty input and absence of spaces.
/// </summary>
string GetPlayerName (string playerNumber, Dictionary<string, string> language) {
    while (true) {
        ui.PrintLine($"{language["select_name"]}{playerNumber}: ");

        string? name = ui.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(name)) {
            ui.PrintError(language["input_error"]);
            continue;
        }
        if (name.Contains(' ')){
            ui.PrintError(language["space_error"]);
        }
        else return name;
    }
}
/// <summary>
/// Requests the original word from the player.
/// Checks word existence in dictionary and validates its length.
/// </summary>
string GetOriginalWord(HashSet<string> dictionary, Dictionary<string, string> language) {
    const int MinWordLength = 8;
    const int MaxWordLength = 30;

    ui.PrintLine(language["choise_original_word"]);
    while (true) {
        string? word = ui.ReadLine()?.ToLower() ?? "";

        if (!dictionary.Contains(word)) {
            ui.PrintError(language["word_not_in_dictionary"]);
            continue;
        }
        if (word.Length < MinWordLength || word.Length > MaxWordLength) {
            ui.PrintError(language["size_error"]);
            continue;
        }
        return word;
    }
}
/// <summary>
/// Returns a dictionary of character frequencies for the given word.
/// Used to validate constructed player words.
/// </summary>
Dictionary<char, int> GetLetterCounts(string word) {
    var counts = new Dictionary<char, int>();

    foreach (char ch in word) {
        if (counts.TryGetValue(ch, out int c)) counts[ch] = c + 1;
        else counts[ch] = 1;
    }
    return counts;
}
/// <summary>
/// Executes one full turn of the given player:
/// - Checks time limits
/// - Accepts commands
/// - Validates word
/// - Adds valid word to attempts list
/// Returns false when the player loses or exits.
/// </summary>
bool PlayerTurn(
    string playerName,
    HashSet<string> dict,
    Dictionary<string, string> language,
    Dictionary<char, int> original_word_dictionary,
    List<string> attempts
    )
{
    const int InputTimeLimit = 20;
    DateTime start = DateTime.Now;
    while (true) {
        if (!IsTimeOver(start, InputTimeLimit, playerName, language)) {
            return false;
        }

        string? input = AskWord(dict, language, GetRemainingMs(start, InputTimeLimit));
        string? cmd = HandleCommand(input, attempts, language);
        if (input == null) {
            return false;
        }
        if (input == "") continue;

        if (cmd == "executed") {
            continue;
        }
        if (cmd == "exit") {
            return false;
        }

        string playerWord = input.ToLower();
        if (attempts.Contains(playerWord)) {
            ui.PrintError(language["reuse_word_error"]);
            continue;
        }

        if (IsWordValid(playerWord, playerName, original_word_dictionary, language)) {
           attempts.Add(playerWord);
           break;
        }
    }
    return true;
}
/// <summary>
/// Loads Russian localization strings into the language dictionary.
/// </summary>
void SetLanguageRussian() {
    language = new Dictionary<string, string>()
    {
        {"choise_original_word", "Выберите слово размером от 8 до 30 букв:"},
        {"ask_word", "\nВведите слово:"},
        {"input_error", "\nОшибка ввода."},
        {"word_not_in_dictionary", "\nТакого слова нет в словаре."},
        {"size_error", "\nДанное слово не соответствует требованию размеров."},
        {"player_timeOut", "не успел придумать слово."},
        {"time_left", "\nПридумайте слово за {0} секунд. Введите /help для вывода доступных команд."},
        {"loose_time", "\nВремя на попытку вышло."},
        {"reuse_word_error", "Данное слово уже было использованно."},
        {"reuse_letters_error", "Данное слово не подходит, проверьте количество повторно использованных букв."},
        {"availability_letters_error", "Данное слово не подходит, проверьте наличие букв."},
        {"player_win", "Победил 🏆"},
        {"used_words", "Слова раунда - [ "},
        {"available_commands", "Доступные команды:"},
        {"show_words", "Показать все введенные обоими пользователями слова в текущей игре;"},
        {"score", "Показать общий счет по играм для текущих игроков;"},
        {"total_score", "Показать общий счет по играм для текущих игроков;"},
        {"wrong_command", "Данной команды не существует."},
        {"exit", "Принудительно завершить раунд."},
        {"select_name", "Введите никнейм для игрока "},
        {"space_error", "Имя не должно содержать пробелы."},
        {"file_data_error", "Нет данных о прошлых играх."},
        {"best_players", "🏆 Таблица лучших игроков 🏆"},
        {"player_move", "ваш ход!"},
    };
}
/// <summary>
/// Loads English localization strings into the language dictionary.
/// </summary>
void SetLanguageEnglish() {
    language = new Dictionary<string, string>()
    {
        {"choise_original_word", "Select a word between 8 and 30 letters:"},
        {"ask_word", "\nEnter the word:"},
        {"input_error", "\nInput Error."},
        {"word_not_in_dictionary", "\nThis word is not in the dictionary."},
        {"size_error", "\nThis word does not meet the size requirement."},
        {"player_timeOut", "didn't have time to think of a word."},
        {"time_left", "\nCome up with a word in {0} seconds. Type /help to display available commands."},
        {"loose_time", "\nTime to try is up."},
        {"reuse_word_error", "This word has already been used."},
        {"reuse_letters_error", "This word is not suitable, check the number of reused letters."},
        {"availability_letters_error", "This word is not suitable, check the presence of letters."},
        {"player_win", "wins 🏆"},
        {"used_words", "Words used in the round - [ "},
        {"available_commands", "Available commands:"},
        {"show_words", "Show all words entered by both users in the current game;"},
        {"score", "Show the total score by game for current players;"},
        {"total_score", "Show the total score by game for current players;"},
        {"wrong_command", "This command does not exist."},
        {"exit", "Force end of round."},
        {"select_name", "Enter a nickname for the player "},
        {"space_error", "Name must not contain spaces."},
        {"file_data_error", "No data about past games."},
        {"best_players", "🏆 Table of best players 🏆"},
        {"player_move", "your move!"},
    };
}
/// <summary>
/// Reads a word from the user with a timeout.
/// Validates against dictionary unless it's a command.
/// Returns: null on timeout, empty string on invalid word.
/// </summary>
string? AskWord(HashSet<string> dict, Dictionary<string, string> language, int remainingMs) {
    ui.PrintLine(language["ask_word"]);
    string? input = ReadLineWithTimeOut(remainingMs)?.ToLower()?.Trim();

    if (input == null) {
        ui.PrintLine(language["loose_time"]);
        return null;
    }

    if (input.StartsWith("/")) return input;

    if (!dict.Contains(input)) {
        ui.PrintError(language["word_not_in_dictionary"]);
        return "";
    }
    return input;
}
/// <summary>
/// Checks if the player's input time has expired.
/// Displays remaining time and messages.
/// </summary>
bool IsTimeOver(DateTime start, int limit, string playerName, Dictionary<string, string> language) {
    double elapsedMs = (DateTime.Now - start).TotalMilliseconds;
    int remainingMs = (int)(limit * 1000 - elapsedMs);
    int remainingSec = remainingMs > 0 ? (remainingMs + 999) / 1000 : 0;

    if (remainingMs <= 0) {
        ui.PrintLine($"{playerName} {language["player_timeOut"]}");
        return false;
    }
    ui.PrintOrange($"\n{playerName} {language["player_move"]}");
    ui.Print(string.Format(language["time_left"], remainingSec));
    return true;
}
/// <summary>
/// Returns how many milliseconds remain before time runs out.
/// </summary>
int GetRemainingMs(DateTime start, int limit) {
    double elapsedMs = (DateTime.Now - start).TotalMilliseconds;
    int remainingMs = (int)(limit * 1000 - elapsedMs);
    return remainingMs;
}
/// <summary>
/// Validates that the player's word:
/// - Uses only available letters
/// - Respects letter frequencies
/// - Exists in dictionary (already checked earlier)
/// </summary>
bool IsWordValid(string playerWord, string playerName, Dictionary<char, int> originalWordDictionary, Dictionary<string, string> language) {
    Dictionary<char, int> playerWordDictionary = GetLetterCounts(playerWord);
    int validLetters = 0;
    foreach (var pair in playerWordDictionary) {
        char key = pair.Key;
        int value1 = pair.Value;

        if (originalWordDictionary.TryGetValue(key, out int value2)) {
            if (value1 > value2) {
                ui.PrintError(language["reuse_letters_error"]);
                return false;
                }
            else validLetters += value1;
            }
        else {
            ui.PrintError(language["availability_letters_error"]);
            validLetters = -1;
            break;
        }
    }
    return validLetters == playerWord.Length;
}
/// <summary>
/// Determines the winner based on the number of attempts.
/// Saves the game result and displays used words.
/// </summary>
void ShowResults() {
    string winner;
    if (attempts.Count % 2 == 0) {
    ui.PrintGreen($"{player2Name} {language["player_win"]}");
    winner = player2Name;
    }
    else {
    ui.PrintGreen($"{player1Name} {language["player_win"]}");
    winner = player1Name;
    }
    SaveResult(player1Name, player2Name, winner);
    ShowWords();
}
/// <summary>
/// Prints all words entered by both players in the current round.
/// </summary>
void ShowWords() {
    ui.Print(language["used_words"]);
    foreach (string _ in attempts) ui.Print($"{_} ");
    ui.Print("]");
}
/// <summary>
/// Saves the game result (player1, player2, winner) into result.json.
/// Appends to existing data using JSON serialization.
/// </summary>
void SaveResult(string player1, string player2, string winner) {
    string filePath = "result.json";
    List<GameResult> results = [];

    if(File.Exists(filePath)) {
        string json = File.ReadAllText(filePath);
        results = System.Text.Json.JsonSerializer.Deserialize<List<GameResult>>(json) ?? [];
    }
    results.Add(new GameResult {
        Player1 = player1,
        Player2 = player2,
        Winner = winner
    });

    string updatedJson = System.Text.Json.JsonSerializer.Serialize(results, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(filePath, updatedJson);
}
/// <summary>
/// Processes player commands such as:
/// /show-words, /score, /total-score, /help, /exit
/// Returns:
/// "executed", "exit", or "none".
/// </summary>
string HandleCommand(string input, List<string> attempts, Dictionary<string, string> language) {
    if (!input.StartsWith("/"))
        return "none";

    switch (input) {

        case "/help":
            ui.PrintLine($"\n{language["available_commands"]}");
            ui.PrintLine($"/show-words - {language["show_words"]}");
            ui.PrintLine($"/score - {language["score"]}");
            ui.PrintLine($"/total-score - {language["total_score"]}");
            ui.PrintLine($"/exit - {language["exit"]}");
            return "executed";

        case "/show-words":
            ShowWords();
            return "executed";

        case "/score":
            ShowHeadToHeadScore(player1Name, player2Name, language);
            return "executed";

        case "/total-score":
            ShowTotalScore(language);
            return "executed";    

        case "/exit":
            return "exit";

        default:
            ui.PrintError(language["wrong_command"]);
            return "executed";
    }
}
/// <summary>
/// Loads all results from result.json and calculates total wins per player.
/// Displays global leaderboard.
/// </summary>
void ShowTotalScore(Dictionary<string, string> language) {
    string filePath = "result.json";

    if(!File.Exists(filePath)) {
        ui.PrintError(language["file_data_error"]);
        return;
    }

    string json = File.ReadAllText(filePath);
    List <GameResult>? results = System.Text.Json.JsonSerializer.Deserialize<List<GameResult>>(json);

    if (results == null || results.Count == 0) {
        ui.PrintError(language["file_data_error"]);
        return;
    }
    Dictionary<string, int> scores = [];
    foreach (var result in results) {
        if(!scores.TryGetValue(result.Winner, out int value)) {
            scores[result.Winner] = 1;
        }
        else scores[result.Winner] = ++value;
    }
    ShowTableOfBestPlayers(scores, language);
}
/// <summary>
/// Prints a formatted leaderboard sorted by number of wins.
/// </summary>
void ShowTableOfBestPlayers(Dictionary<string, int> scores, Dictionary<string, string> language) {
    var sortedScore = scores.OrderByDescending(s => s.Value);
    ui.PrintGreen($"\n{language["best_players"]}");
    foreach (var player in sortedScore) {
        ui.PrintOrange($"\t{player.Key} - {player.Value}");
    }
}
/// <summary>
/// Shows win statistics specifically between the two current players.
/// </summary>
void ShowHeadToHeadScore(string player1, string player2, Dictionary<string, string> language) {
    string filePath = "result.json";

    if(!File.Exists(filePath)) {
        ui.PrintError(language["file_data_error"]);
    }
    
    string json = File.ReadAllText(filePath);
    List <GameResult>? results = System.Text.Json.JsonSerializer.Deserialize<List<GameResult>>(json);

    if (results == null || results.Count == 0) {
        ui.PrintError(language["file_data_error"]);
        return;
    }
    Dictionary<string, int> scores = [];
    foreach (var pair in results) {
        if((player1 == pair.Player1 && player2 == pair.Player2) || (player1 == pair.Player2 && player2 == pair.Player1)) {
            if(!scores.TryGetValue(pair.Winner, out int value)) {
                scores[pair.Winner] = 1;
            }
            else scores[pair.Winner] = ++value;
        }
    }
    if (scores.Count == 0) {
        ui.PrintError(language["file_data_error"]);
        return;
    }
    ShowTableOfBestPlayers(scores, language);
}

/// <summary>
/// Defines an abstraction for input/output.
/// Allows the game to work with Console or alternative UI.
/// </summary>
interface IUserInterface {
    void PrintLine(string message);
    void Print(string message);
    string? ReadLine();
    void PrintError(string message);
    void PrintGreen(string message);
    void PrintOrange(string message);
}
/// <summary>
/// Console implementation of IUserInterface.
/// Handles colored output and standard console I/O.
/// </summary>
class ConsoleUI : IUserInterface {
    public void PrintLine(string message) {
        Console.WriteLine(message);
    }
    public void Print(string message) {
        Console.Write(message);
    }
    public string? ReadLine() {
        return Console.ReadLine();

    }
    public void PrintError(string message) {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ForegroundColor = oldColor;
    }
    public void PrintGreen(string message) {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ForegroundColor = oldColor;
    }
    public void PrintOrange(string message) {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(message);
        Console.ForegroundColor = oldColor;
    }
}
/// <summary>
/// Represents a single recorded game:
/// Player1, Player2, Winner.
/// Used for JSON serialization.
/// </summary>
class GameResult {
    public required string Player1 {get; set;}
    public required string Player2 {get; set;}
    public required string Winner {get; set;}


}