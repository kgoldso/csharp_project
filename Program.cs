using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

ConsoleUI ui = new();
var (repository, language) = ChooseLanguage(ui);

Player player1 = new Player(GetPlayerName("1", language, ui));
Player player2 = new Player(GetPlayerName("2", language, ui));

string originalWord = GetOriginalWord(repository, language, ui);

Game game = new Game(ui, repository, language);
game.Start(player1, player2, originalWord);

/// <summary>
/// Prompts the user to select a language and initializes the corresponding dictionary and localization.
/// </summary>
/// <param name="ui">The user interface for input/output.</param>
/// <returns>Tuple containing the DataRepository and language dictionary.</returns>
(DataRepository, Dictionary<string, string>) ChooseLanguage(ConsoleUI ui) 
{
    while (true) 
    {
        ui.PrintLine("Выберите язык / Choose language: Русский(1), English(2)");
        string? choice = ui.ReadLine();

        if (choice == "1") 
        {
            var language = SetLanguageRussian();
            var repository = new DataRepository("russian.txt");
            return (repository, language);
        }
        else if (choice == "2") 
        {
            var language = SetLanguageEnglish();
            var repository = new DataRepository("english.txt");
            return (repository, language);
        }
        else 
        {
            ui.PrintError("\nОшибка ввода / Input Error");
        }
    }
}

/// <summary>
/// Prompts for and validates a player's name.
/// Name must not be empty or contain spaces.
/// </summary>
/// <param name="playerNumber">The player number for display purposes.</param>
/// <param name="language">Localization dictionary.</param>
/// <param name="ui">User interface for input/output.</param>
/// <returns>Valid player name.</returns>
string GetPlayerName(string playerNumber, Dictionary<string, string> language, ConsoleUI ui) 
{
    while (true) 
    {
        ui.PrintLine($"{language["select_name"]}{playerNumber}: ");

        string? name = ui.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(name)) 
        {
            ui.PrintError(language["input_error"]);
            continue;
        }
        if (name.Contains(' '))
        {
            ui.PrintError(language["space_error"]);
        }
        else return name;
    }
}

/// <summary>
/// Prompts for and validates the original word for the game.
/// Word must exist in dictionary and be between 8-30 characters.
/// </summary>
/// <param name="repository">Data repository containing the word dictionary.</param>
/// <param name="language">Localization dictionary.</param>
/// <param name="ui">User interface for input/output.</param>
/// <returns>Valid original word.</returns>
string GetOriginalWord(DataRepository repository, Dictionary<string, string> language, ConsoleUI ui) 
{
    const int MinWordLength = 8;
    const int MaxWordLength = 30;

    ui.PrintLine(language["choise_original_word"]);
    while (true) 
    {
        string? word = ui.ReadLine()?.ToLower() ?? "";

        if (!repository.Dictionary.Contains(word)) 
        {
            ui.PrintError(language["word_not_in_dictionary"]);
            continue;
        }
        if (word.Length < MinWordLength || word.Length > MaxWordLength) 
        {
            ui.PrintError(language["size_error"]);
            continue;
        }
        return word;
    }
}

/// <summary>
/// Creates and returns the Russian localization dictionary.
/// Contains all UI strings and messages in Russian.
/// </summary>
/// <returns>Dictionary with Russian localization strings.</returns>
Dictionary<string, string> SetLanguageRussian() {
    return new Dictionary<string, string>() {
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
/// Creates and returns the English localization dictionary.
/// Contains all UI strings and messages in English.
/// </summary>
/// <returns>Dictionary with English localization strings.</returns>
Dictionary<string, string> SetLanguageEnglish() {
    return new Dictionary<string, string>() {
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