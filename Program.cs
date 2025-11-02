using System.Globalization;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.Marshalling;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

HashSet<string> dict;

Dictionary<string, string> language = [];
string choose_language;

while (true) {
    Console.WriteLine("Выберите язык / Choose language: Русский(1), English(2)");
    choose_language = Console.ReadLine();
    if (choose_language == "1") {
        SetLanguageRussian();
        dict = new(File.ReadAllLines("russian.txt"), StringComparer.OrdinalIgnoreCase);
        break;
    }
    else if (choose_language == "2") {
        SetLanguageEnglish();
        dict = new(File.ReadAllLines("english.txt"), StringComparer.OrdinalIgnoreCase);
        break;
    }
    else Console.WriteLine("\nОшибка ввода / Input Error");
}

Console.WriteLine(language["choise_original_word"]);

string? original_word;
while (true) {
    original_word = Console.ReadLine().ToLower();
    if (dict.Contains(original_word)) break;
    else {
        Console.WriteLine(language["word_not_in_dictionary"]);
        continue;
    }
}
bool flag = true;
while(flag) {    
    if (original_word.Length < 8 || original_word.Length > 30) {
        Console.WriteLine(language["size_error"]);
        original_word = Console.ReadLine().ToLower();
    }
    else break;
}

List<char> original_word_letters = [.. original_word];
Dictionary<char, int> original_word_dictionary = [];
int count;

for(int i = 0; i < original_word.Length; i++)
{
    count = 1;
    for (int j = i; j < original_word.Length; j++)
    {
        if (i == j) continue;
        if (original_word_letters[i] == original_word_letters[j]) count += 1;
    }
    original_word_dictionary.TryAdd(original_word_letters[i], count);
}

List<string> attempts = [];
string first_player_try;
string second_player_try;
bool att;
flag = true;

while(flag) {
    if(flag) att = true;
    else att = false;
    DateTime start = DateTime.Now;
    int limit = 10;

    while (att)
    {
        double elapsedMs = (DateTime.Now - start).TotalMilliseconds;
        int remainingMs = (int)(limit * 1000 - elapsedMs);
        int remainingSec = remainingMs > 0 ? (remainingMs + 999) / 1000 : 0;

        if (remainingMs <= 0) {
            Console.WriteLine(language["first_player_timeOut"]);
            flag = false;
            break;
        }

        Console.WriteLine(string.Format(language["time_left"], remainingSec));

        string? input = AskWord(dict, language, remainingMs);
        if (input == null) {
            flag = false;
            break;
        }
        if (input == "") continue;

        first_player_try = input.ToLower();
        Dictionary<char, int> first_player_try_dictionary = [];
        List<char> first_player_try_letters = [.. first_player_try];

        if(attempts.Contains(first_player_try)) {Console.WriteLine(language["reuse_word_error"]); continue;}
        if (first_player_try == "-1") {flag = false; break;}

        for (int i = 0; i < first_player_try.Length; i++)
        {
            count = 1;
            for (int j = i; j < first_player_try.Length; j++)
            {
                if (i == j) continue;
                if (first_player_try_letters[i] == first_player_try_letters[j]) count += 1;
            }
            first_player_try_dictionary.TryAdd(first_player_try_letters[i], count);
        }

        int num = 0;
        foreach (var pair in first_player_try_dictionary)
        {
            char key = pair.Key;
            int value1 = pair.Value;

            if (original_word_dictionary.TryGetValue(key, out int value2))
            {
                if (value1 > value2) { Console.WriteLine(language["reuse_letters_error"]); break; }
                else num += value1;
            }
            else { Console.WriteLine(language["availability_letters_error"]); break; }
        }

        if (num == first_player_try.Length)
        {
            attempts.Add(first_player_try);
            break;
        }
    }

    if (flag) att = true;
    else att = false;
    start = DateTime.Now;
    limit = 10;

    while(att) {
        double elapsedMs = (DateTime.Now - start).TotalMilliseconds;
        int remainingMs = (int)(limit * 1000 - elapsedMs);
        int remainingSec = remainingMs > 0 ? (remainingMs + 999) / 1000 : 0;

        if (remainingMs <= 0) {
            Console.WriteLine(language["second_player_timeOut"]);
            flag = false;
            break;
        }

        Console.WriteLine(string.Format(language["time_left"], remainingSec));

        string? input = AskWord(dict, language, remainingMs);
        if (input == null) {
            flag = false;
            break;
        }
        if (input == "") continue;

        second_player_try = input.ToLower();
        Dictionary<char, int> second_player_try_dictionary = [];
        List<char> second_player_try_letters = [.. second_player_try];

        if (attempts.Contains(second_player_try)) {Console.WriteLine(language["reuse_word_error"]); continue;}
        if (second_player_try == "-1") {flag = false; break;}

        for (int i = 0; i < second_player_try.Length; i++) {
            count = 1;
            for (int j = i; j < second_player_try.Length; j++) {
                if (i == j) continue;
                if (second_player_try[i] == second_player_try[j]) count += 1;
            }
            second_player_try_dictionary.TryAdd(second_player_try_letters[i], count);
        }

        int num = 0;
        foreach(var pair in second_player_try_dictionary) {
            char key = pair.Key;
            int value1 = pair.Value;

            if(original_word_dictionary.TryGetValue(key, out int value2)) {
                if (value1 > value2) {Console.WriteLine(language["reuse_letters_error"]); break;}
                else num += value1;
            }
            else { Console.WriteLine(language["availability_letters_error"]); break; }
        }

        if (num == second_player_try.Length)
        {
            attempts.Add(second_player_try);
            break;
        }
    }
}

if (attempts.Count % 2 == 0) {
    Console.WriteLine(language["second_player_win"]);
}
else {
    Console.WriteLine(language["first_player_win"]);
}

Console.Write(language["used_words"]);
foreach (string _ in attempts) Console.Write($"{_} ");
Console.WriteLine("]");

string? ReadLineWithTimeOut(int time) {
    string? result = null;
    var task = Task.Run(() => result = Console.ReadLine());
    if (task.Wait(time)) return result;
    else return null;
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
    Console.WriteLine(language["ask_word"]);

    string? input = ReadLineWithTimeOut(remainingMs)?.ToLower();

    if (input == null) {
        Console.WriteLine(language["loose_time"]);
        return null;
    }

    if (input == "-1") {
        return null;
    }

    if (!dict.Contains(input)) {
        Console.WriteLine(language["word_not_in_dictionary"]);
        return "";
    }
    return input;
}

