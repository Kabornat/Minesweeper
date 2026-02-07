namespace Minesweeper;

public class GameInfo(Difficulty difficulty)
{
    public const int EASY_PRESET = 1;
    public const int MEDIUM_PRESET = 2;
    public const int HARD_PRESET = 3;

    public const ConsoleColor TEXT_COLOR = ConsoleColor.White;

    public const ConsoleColor DEFAULT_COLOR_EVEN = ConsoleColor.Green;
    public const ConsoleColor DEFAULT_COLOR_NOT_EVEN = ConsoleColor.DarkGreen;
    public const char DEFAULT_SYMBOL = '█';

    public const ConsoleColor BORDER_COLOR = ConsoleColor.DarkGray;
    public const char BORDER_SYMBOL = '█';

    public const ConsoleColor DIGGED_COLOR = ConsoleColor.DarkYellow;
    public const char DIGGED_SYMBOL = ' ';

    public readonly Dictionary<int, ConsoleColor> NumberColors = new()
    {
        {1, ConsoleColor.Blue},
        {2, ConsoleColor.Green},
        {3, ConsoleColor.Red},
        {4, ConsoleColor.Magenta},
        {5, ConsoleColor.Yellow},
        {6, ConsoleColor.DarkBlue},
        {7, ConsoleColor.DarkMagenta},
        {8, ConsoleColor.DarkGray},
    };

    public const ConsoleColor FLAG_COLOR = ConsoleColor.DarkRed;
    public const char FLAG_SYMBOL = '█';

    public const ConsoleColor MINE_COLOR = ConsoleColor.White;
    public const ConsoleColor EXPLORED_MINE_COLOR = ConsoleColor.Red;
    public const char MINE_SYMBOL = '*';


    public const ConsoleColor CURSOR_COLOR = ConsoleColor.White;
    public const char CURSOR_SYMBOL = '█';

    public const int SCREEN_WIDTH = 100;
    public const int SCREEN_HEIGHT = 50;

    public int MapWidth { get; set; } = difficulty.MapWidth + 2; // Увеличиваем для границ
    public int MapHeight { get; set; } = difficulty.MapHeight + 2;

    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public int Preset { get; set; } = difficulty.Preset;

    public int DiggedCount { get; set; } = 0;
    public int MinesCount { get; set; } = difficulty.MinesCount;
    public int FlagsCount { get; set; } = difficulty.MinesCount;

    public int PlayerX { get; set; } = (difficulty.MapWidth + 2) / 2; // Инициализация игрока по центру карты
    public int PlayerY { get; set; } = (difficulty.MapHeight + 2) / 2;

    public bool ItsFirstMove { get; set; } = true;
    public bool GameIsRunning { get; set; } = true;


    public ConsoleColor GetColorForDefault(in int x, in int y)
    {
        return (x + y) % 2 is 0 ? DEFAULT_COLOR_EVEN : DEFAULT_COLOR_NOT_EVEN;
    }

    public TimeSpan GetTimeDifference()
    {
        return DateTime.UtcNow.Subtract(StartDate);
    }
}
