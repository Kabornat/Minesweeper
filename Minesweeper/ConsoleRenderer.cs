using static System.Console;

namespace Minesweeper;

public struct ConsoleRenderer
{
    public const int MAP_ROW = 4;
    public const int INFO_ROW = 2;

    public const int FOOTER_ROW = 5;

    public void ClearConsole()
    {
        SetCursorPosition(0, 0);
        Write(new string(' ', GameInfo.SCREEN_WIDTH * GameInfo.SCREEN_HEIGHT));
    }

    public void WriteHeader(in int width)
    {
        WriteMessage($"{new string('-', (width / 2) - 3)} САПЁР {new string('-', (width / 2) - 3)}");
    }

    public void WriteControlHelpLine(int row, in Control control)
    {
        row += FOOTER_ROW;
        WriteMessage("Управление:", row);

        WriteMessage($"{string.Join('/', control.DigKeys)} - Копать", row + 2);
        WriteMessage($"{string.Join('/', control.FlagKeys)} - Установить флаг", row + 3);

        WriteMessage($"{string.Join('/', control.MoveUpKeys)} - Наверх", row + 5);
        WriteMessage($"{string.Join('/', control.MoveLeftKeys)} - Влево", row + 6);
        WriteMessage($"{string.Join('/', control.MoveDownKeys)} - Вниз", row + 7);
        WriteMessage($"{string.Join('/', control.MoveRightKeys)} - Вправо", row + 8);

        WriteMessage($"{string.Join('/', control.EasyDifficultKeys)} - Легкая сложность", row + 10);
        WriteMessage($"{string.Join('/', control.MediumDifficultKeys)} - Средняя сложность", row + 11);
        WriteMessage($"{string.Join('/', control.HardDifficultKeys)} - Высокая сложность", row + 12);

        WriteMessage($"{string.Join('/', control.LeaveKeys)} - Выход", row + 14);
    }

    public void WriteMessage(in string message, in int row = 0)
    {
        SetCursorPosition(0, row);
        ForegroundColor = GameInfo.TEXT_COLOR;
        Write(message);
    }

    public void WriteInfo(in int minesCount, in int flagsCount, in int width, in int height, in Cell[,] Map)
    {
        WriteInfoLine(minesCount, flagsCount);
        WriteMap(width, height, Map);
    }

    public void WriteInfoLine(in int minesCount, in int flagsCount)
    {
        WriteMessage($"Мин: {minesCount}, Флажков: {flagsCount} ", INFO_ROW);
    }

    public void WriteMap(in int mapWidth, in int mapHeight, in Cell[,] map)
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                ForegroundColor = map[x, y].Color;
                SetCursorPosition(x, y + MAP_ROW);
                Write(map[x, y].Symbol);
            }
        }
    }

    public void WriteMapWithMines(in int mapWidth, in int mapHeight, in Cell[,] map, in int playerX, in int playerY)
    {
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (map[x, y].IsMine)
                {
                    if (playerX == x && playerY == y)
                        ForegroundColor = GameInfo.EXPLORED_MINE_COLOR;
                    else
                        ForegroundColor = GameInfo.MINE_COLOR;

                    SetCursorPosition(x, y + MAP_ROW);
                    Write(GameInfo.MINE_SYMBOL);
                }
                else
                {
                    SetCursorPosition(x, y + MAP_ROW);
                    ForegroundColor = map[x, y].Color;
                    Write(map[x, y].Symbol);
                }
            }
        }
    }

    public void WriteGameOver(bool isWin, int mapWidth, int mapHeight, in Cell[,] map, in int playerX, in int playerY, TimeSpan timeDifference)
    {
        ClearConsole();
        WriteHeader(mapWidth);
        WriteMapWithMines(mapWidth, mapHeight, map, playerX, playerY);

        if (isWin)
            WriteMessage($"Победа! Время: {timeDifference}", INFO_ROW);
        else
            WriteMessage($"Взрыв! Время: {timeDifference}", INFO_ROW);

        WriteMessage("Нажмите Enter чтобы выйти, или введите R чтобы начать заново: ", mapHeight + FOOTER_ROW);
    }
}
