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

ConsoleUI ui = new ConsoleUI();
HashSet<string> dictionary;
Dictionary<string, string> language = [];
List<string> attempts = [];

ChooseLanguage();

string originalWord = GetOriginalWord(dictionary, language);
Dictionary<char, int> originalWordDictionary = GetLetterCounts(originalWord);

while (true)
{
    if (!PlayerTurn("1", dictionary, language, originalWordDictionary, attempts)) break;
    if (!PlayerTurn("2", dictionary, language, originalWordDictionary, attempts)) break;
}

ShowResults();

string? ReadLineWithTimeOut(int time) {
    string? result = null;
    var task = Task.Run(() => result = ui.ReadLine());
    if (task.Wait(time)) return result;
    else return null;
}

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

Dictionary<char, int> GetLetterCounts(string word) {
    var counts = new Dictionary<char, int>();

    foreach (char ch in word) {
        if (counts.TryGetValue(ch, out int c)) counts[ch] = c + 1;
        else counts[ch] = 1;
    }
    return counts;
}

bool PlayerTurn(
    string playerName,
    HashSet<string> dict,
    Dictionary<string, string> language,
    Dictionary<char, int> original_word_dictionary,
    List<string> attempts
    )
{
    const int InputTimeLimit = 10;
    DateTime start = DateTime.Now;
    while (true) {
        if (!IsTimeOver(start, InputTimeLimit, playerName, language)) {
            return false;
        }

        string? input = AskWord(dict, language, GetRemainingMs(start, InputTimeLimit));
        if (input == null) {
            return false;
        }
        if (input == "") continue;

        string playerWord = input.ToLower();
        if (attempts.Contains(playerWord)) {
            ui.PrintError(language["reuse_word_error"]);
            continue;
        }
        if (playerWord == "-1") return false;

        if (IsWordValid(playerWord, playerName, original_word_dictionary, language)) {
           attempts.Add(playerWord);
           break;
        }
    }
    return true;
}

void SetLanguageRussian() {
    language = new Dictionary<string, string>()
    {
        {"choise_original_word", "Выберите слово размером от 8 до 30 букв:"},
        {"ask_word", "\nВведите слово:"},
        {"input_error", "\nОшибка ввода."},
        {"word_not_in_dictionary", "\nТакого слова нет в словаре."},
        {"size_error", "\nДанное слово не соответствует требованию размеров."},
        {"first_player_timeOut", "\nВремя пользователя №1 вышло."},
        {"second_player_timeOut", "\nВремя пользователя №2 вышло."},
        {"time_left", "\nПридумайте слово за {0} секунд. Или же напишите -1 для завершения игры."},
        {"loose_time", "\nВремя на попытку вышло."},
        {"reuse_word_error", "Данное слово уже было использованно."},
        {"reuse_letters_error", "Данное слово не подходит, проверьте количество повторно использованных букв."},
        {"availability_letters_error", "Данное слово не подходит, проверьте наличие букв."},
        {"first_player_win", "\n🏆Победил пользователь №1."},
        {"second_player_win", "\n🏆Победил пользователь №2."},
        {"used_words", "Слова раунда - [ "}
    };
}

void SetLanguageEnglish() {
    language = new Dictionary<string, string>()
    {
        {"choise_original_word", "Select a word between 8 and 30 letters:"},
        {"ask_word", "\nEnter the word:"},
        {"input_error", "\nInput Error."},
        {"word_not_in_dictionary", "\nThis word is not in the dictionary."},
        {"size_error", "\nThis word does not meet the size requirement."},
        {"first_player_timeOut", "\nUser #1's time has expired."},
        {"second_player_timeOut", "\nUser #2's time has expired."},
        {"time_left", "\nCome up with a word in {0} seconds. Or type -1 to end the game."},
        {"loose_time", "\nTime to try is up."},
        {"reuse_word_error", "This word has already been used."},
        {"reuse_letters_error", "This word is not suitable, check the number of reused letters."},
        {"availability_letters_error", "This word is not suitable, check the presence of letters."},
        {"first_player_win", "\n🏆User #1 wins."},
        {"second_player_win", "\n🏆User #2 wins."},
        {"used_words", "Words used in the round - [ "}
    };
}

string? AskWord(HashSet<string> dict, Dictionary<string, string> language, int remainingMs) {
    ui.PrintLine(language["ask_word"]);
    string? input = ReadLineWithTimeOut(remainingMs)?.ToLower()?.Trim();

    if (input == null) {
        ui.PrintLine(language["loose_time"]);
        return null;
    }

    if (input == "-1") {
        return null;
    }

    if (!dict.Contains(input)) {
        ui.PrintError(language["word_not_in_dictionary"]);
        return "";
    }
    return input;
}

bool IsTimeOver(DateTime start, int limit, string playerName, Dictionary<string, string> language) {
    double elapsedMs = (DateTime.Now - start).TotalMilliseconds;
    int remainingMs = (int)(limit * 1000 - elapsedMs);
    int remainingSec = remainingMs > 0 ? (remainingMs + 999) / 1000 : 0;

    if (remainingMs <= 0) {
        ui.PrintLine(language[$"{(playerName == "1" ? "first" : "second")}_player_timeOut"]);
        return false;
    }
    ui.PrintLine(string.Format(language["time_left"], remainingSec));
    return true;
}

int GetRemainingMs(DateTime start, int limit) {
    double elapsedMs = (DateTime.Now - start).TotalMilliseconds;
    int remainingMs = (int)(limit * 1000 - elapsedMs);
    return remainingMs;
}

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

void ShowResults() {
    if (attempts.Count % 2 == 0) {
    ui.PrintLine(language["second_player_win"]);
    }
    else {
    ui.PrintLine(language["first_player_win"]);
    }

    ui.Print(language["used_words"]);
    foreach (string _ in attempts) ui.Print($"{_} ");
    ui.Print("]");
}
interface IUserInterface {
    void PrintLine(string message);
    void Print(string message);
    string? ReadLine();
    void PrintError(string message);
}

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
}