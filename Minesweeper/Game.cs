using static System.Console;

namespace Minesweeper;

public class Game
{
    private readonly Difficulty EasyDifficulty = new(mapWidth: 9, mapHeight: 9, minesCount: 10, preset: 1);
    private readonly Difficulty MediumDifficulty = new(mapWidth: 17, mapHeight: 17, minesCount: 40, preset: 2);
    private readonly Difficulty HighDifficulty = new(mapWidth: 31, mapHeight: 16, minesCount: 99, preset: 3);

    private GameInfo GameInfo;
    private Cell[,] _map;


    private void InitEasy()
    {
        Init(EasyDifficulty);
    }

    private void InitMedium()
    {
        Init(MediumDifficulty);
    }

    private void InitHigh()
    {
        Init(HighDifficulty);
    }

    private void Init(Difficulty difficulty)
    {
        ClearConsole(); // Очистка консоли и сбросы // Увеличиваем для границ
        GameInfo = new(difficulty);

        var grid = new Cell[GameInfo.MapWidth, GameInfo.MapHeight];

        for (int yI = 0; yI < GameInfo.MapHeight; yI++)
        {
            for (int xI = 0; xI < GameInfo.MapWidth; xI++)
            {
                var cell = new Cell();

                if (IsBorder(xI, yI))
                {
                    cell.Symbol = GameInfo.BORDER_SYMBOL;
                    cell.Color = GameInfo.BORDER_COLOR;
                }
                else
                {
                    cell.Symbol = GameInfo.DEFAULT_SYMBOL;
                    cell.Color = GetColorForDefault(xI, yI);
                }

                cell.X = xI;
                cell.Y = yI;

                grid[xI, yI] = cell;
            }
        }

        _map = grid;

        GameInfo.PlayerX = GameInfo.MapWidth / 2; // Инициализация игрока
        GameInfo.PlayerY = GameInfo.MapHeight / 2;

        _map[GameInfo.PlayerX, GameInfo.PlayerY].Color = GameInfo.CURSOR_COLOR;
        _map[GameInfo.PlayerX, GameInfo.PlayerY].Symbol = GameInfo.CURSOR_SYMBOL;

        var localMinesCount = 0;
        while (true)
        {
            if (!TryGetRandomPositionWithoutBorderAndMine(out var x, out var y))
                continue;

            if (localMinesCount < GameInfo.MinesCount)
            {
                _map[x, y].IsMine = true;

                localMinesCount++;
            }
            else
            {
                break;
            }
        }

        SetMinesCountAround();

        SetHeaderLine();
        SetControlHelpLine(GameInfo.MapHeight + GameInfo.FOOTER_ROW);
    }

    private void ClearConsole()
    {
        SetCursorPosition(0, 0);
        Write(new string(' ', GameInfo.SCREEN_WIDTH * GameInfo.SCREEN_HEIGHT));
    }

    private ConsoleColor GetColorForDefault(int x, int y)
    {
        return (x + y) % 2 is 0 ? GameInfo.DEFAULT_COLOR_EVEN : GameInfo.DEFAULT_COLOR_NOT_EVEN;
    }

    private bool TryGetRandomPositionWithoutBorderAndMine(out int x, out int y)
    {
        x = Random.Shared.Next(GameInfo.MapWidth);
        y = Random.Shared.Next(GameInfo.MapHeight);

        if (IsBorder(x, y) || _map[x, y].IsMine)
            return false;

        return true;
    }

    private void SetMinesCountAround()
    {
        for (int y = 0; y < GameInfo.MapHeight; y++)
        {
            for (int x = 0; x < GameInfo.MapWidth; x++)
            {
                if (IsBorder(x, y))
                    continue;

                var cell1 = _map[x - 1, y - 1];
                var cell2 = _map[x, y - 1];
                var cell3 = _map[x + 1, y - 1];

                var cell4 = _map[x - 1, y];
                var cell6 = _map[x + 1, y];

                var cell7 = _map[x - 1, y + 1];
                var cell8 = _map[x, y + 1];
                var cell9 = _map[x + 1, y + 1];

                int minesAround = 0;

                if (cell1.IsMine)
                    minesAround++;
                if (cell2.IsMine)
                    minesAround++;
                if (cell3.IsMine)
                    minesAround++;

                if (cell4.IsMine)
                    minesAround++;
                if (cell6.IsMine)
                    minesAround++;

                if (cell7.IsMine)
                    minesAround++;
                if (cell8.IsMine)
                    minesAround++;
                if (cell9.IsMine)
                    minesAround++;

                _map[x, y].MinesAround = minesAround;
            }
        }
    }

    private void SetHeaderLine()
    {
        SetMessageToLine($"{new string('-', (GameInfo.MapWidth / 2) - 3)} САПЁР {new string('-', (GameInfo.MapWidth / 2) - 3)}", 0);
    }

    private void SetMessageToLine(string message, int row)
    {
        SetCursorPosition(0, row);
        ForegroundColor = GameInfo.TEXT_COLOR;
        Write(message);
    }


    public void Run()
    {
        SetWindowSize(GameInfo.SCREEN_WIDTH, GameInfo.SCREEN_HEIGHT);
        CursorVisible = false;

        InitMedium();

        try
        {
            while (GameInfo.GameIsRunning)
            {
                WriteInfo();
                HandleInput();
            }
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ClearConsole();
            SetCursorPosition(0, 0);
            Write(ex.Message);
        }
    }


    private void WriteInfo()
    {
        SetInfoLine();
        SetMap();
    }

    private void SetInfoLine()
    {
        SetMessageToLine($"Мин: {GameInfo.MinesCount}, Флажков: {GameInfo.FlagsCount} ", GameInfo.INFO_ROW);
    }

    private void SetMap()
    {
        for (int y = 0; y < GameInfo.MapHeight; y++)
        {
            for (int x = 0; x < GameInfo.MapWidth; x++)
            {
                ForegroundColor = _map[x, y].Color;
                SetCursorPosition(x, y + GameInfo.MAP_ROW);
                Write(_map[x, y].Symbol);
            }
        }
    }


    private void HandleInput()
    {
        if (!KeyAvailable)
            return;

        var input = ReadKey(true).Key;

        switch (input)
        {
            case ConsoleKey.E:
                var diff = DateTime.UtcNow.Subtract(GameInfo.StartDate);

                DigAtPlayerPositionAndUpdateDiggedStat();

                if (AllWihoutMinesHasDigged())
                {
                    ClearConsole();
                    SetHeaderLine();
                    SetInfoLine();
                    SetMapWidhoutDiggedCells();
                    SetMessageToLine($"Победа! Время: {diff}", GameInfo.INFO_ROW);
                }
                else if (GameInfo.GameIsRunning)
                {
                    break;
                }

                SetMessageToLine("Нажмите Enter чтобы выйти, или введите R чтобы начать заново: ", GameInfo.MapHeight + GameInfo.FOOTER_ROW);
                var strInput = ReadLine()?.ToUpper();

                if (strInput == "R")
                {
                    switch (GameInfo.Preset)
                    {
                        case 1:
                            InitEasy();
                            return;

                        case 2:
                            InitMedium();
                            return;

                        case 3:
                            InitHigh();
                            return;
                    }
                    return;
                }
                GameInfo.GameIsRunning = false;
                return;

            case ConsoleKey.Q:
                SetFlagAtPlayerPositionAndUpdateFlagsInStat();
                return;

            case ConsoleKey.W:
            case ConsoleKey.UpArrow:
                MoveUp();
                return;

            case ConsoleKey.A:
            case ConsoleKey.LeftArrow:
                MoveLeft();
                return;

            case ConsoleKey.S:
            case ConsoleKey.DownArrow:
                MoveDown();
                return;

            case ConsoleKey.D:
            case ConsoleKey.RightArrow:
                MoveRight();
                return;


            case ConsoleKey.D1:
                InitEasy();
                return;

            case ConsoleKey.D2:
                InitMedium();
                return;

            case ConsoleKey.D3:
                InitHigh();
                return;


            case ConsoleKey.Escape:
            case ConsoleKey.X:
                GameInfo.GameIsRunning = false;
                return;
        }
    }

    private bool AllWihoutMinesHasDigged()
    {
        return (((GameInfo.MapWidth - 2) * (GameInfo.MapHeight - 2)) - GameInfo.MinesCount) == GameInfo.DiggedCount;
    }

    private void DigAtPlayerPositionAndUpdateDiggedStat()
    {
        if (_map[GameInfo.PlayerX, GameInfo.PlayerY].State is State.Flag)
            return;

        if (GameInfo.ItsFirstMove && _map[GameInfo.PlayerX, GameInfo.PlayerY].IsMine)
        {
            while (_map[GameInfo.PlayerX, GameInfo.PlayerY].IsMine)
            {
                _map[GameInfo.PlayerX, GameInfo.PlayerY].IsMine = false;

                if (!TryGetRandomPositionWithoutBorderAndMine(out var x, out var y))
                    continue;

                _map[x, y].IsMine = true;
            }

            SetMinesCountAround();

            BfsDig();
            SetMinesAroundAndUpdateDiggedStat(GameInfo.PlayerX, GameInfo.PlayerY);

            SetCursorPosition(GameInfo.PlayerX, GameInfo.PlayerY);
            ForegroundColor = _map[GameInfo.PlayerX, GameInfo.PlayerY].Color;
            Write(_map[GameInfo.PlayerX, GameInfo.PlayerY].Symbol);

            GameInfo.ItsFirstMove = false;
            return;
        }

        if (!_map[GameInfo.PlayerX, GameInfo.PlayerY].IsMine)
        {
            GameInfo.ItsFirstMove = false;
            BfsDig();
            SetMinesAroundAndUpdateDiggedStat(GameInfo.PlayerX, GameInfo.PlayerY);
            return;
        }

        GameInfo.GameIsRunning = false;
        var diff = DateTime.UtcNow.Subtract(GameInfo.StartDate);

        ClearConsole();
        SetHeaderLine();
        SetMessageToLine($"Взрыв! Время: {diff}", GameInfo.INFO_ROW);

        SetMapWidhoutDiggedCells();
    }

    private void MoveUp()
    {
        Move(GameInfo.PlayerX, GameInfo.PlayerY - 1);
    }

    private void MoveDown()
    {
        Move(GameInfo.PlayerX, GameInfo.PlayerY + 1);
    }

    private void MoveRight()
    {
        Move(GameInfo.PlayerX + 1, GameInfo.PlayerY);
    }

    private void MoveLeft()
    {
        Move(GameInfo.PlayerX - 1, GameInfo.PlayerY);
    }

    private void Move(int x, int y)
    {
        if (IsBorder(x, y))
            return;

        _map[x, y].Symbol = GameInfo.CURSOR_SYMBOL;
        _map[x, y].Color = GameInfo.CURSOR_COLOR;

        if (_map[GameInfo.PlayerX, GameInfo.PlayerY].State is State.Default)
            SetDefault();
        else if (_map[GameInfo.PlayerX, GameInfo.PlayerY].State is State.Flag)
            SetFlag();
        else if (_map[GameInfo.PlayerX, GameInfo.PlayerY].State is State.Digged)
            SetMinesAroundAndUpdateDiggedStat(GameInfo.PlayerX, GameInfo.PlayerY);

        GameInfo.PlayerX = x;
        GameInfo.PlayerY = y;
    }

    private void SetDefault()
    {
        _map[GameInfo.PlayerX, GameInfo.PlayerY].State = State.Default;
        _map[GameInfo.PlayerX, GameInfo.PlayerY].Symbol = GameInfo.DEFAULT_SYMBOL;
        _map[GameInfo.PlayerX, GameInfo.PlayerY].Color = GetColorForDefault(GameInfo.PlayerX, GameInfo.PlayerY);
    }

    private void SetFlag()
    {
        _map[GameInfo.PlayerX, GameInfo.PlayerY].State = State.Flag;
        _map[GameInfo.PlayerX, GameInfo.PlayerY].Symbol = GameInfo.FLAG_SYMBOL;
        _map[GameInfo.PlayerX, GameInfo.PlayerY].Color = GameInfo.FLAG_COLOR;
    }

    private void SetFlagAtPlayerPositionAndUpdateFlagsInStat()
    {
        if (_map[GameInfo.PlayerX, GameInfo.PlayerY].State is State.Default)
        {
            SetFlag();
            GameInfo.FlagsCount--;
        }
        else if (_map[GameInfo.PlayerX, GameInfo.PlayerY].State is State.Flag)
        {
            SetDefault();
            GameInfo.FlagsCount++;
        }
    }

    private void SetMapWidhoutDiggedCells()
    {
        for (int y = 0; y < GameInfo.MapHeight; y++)
        {
            for (int x = 0; x < GameInfo.MapWidth; x++)
            {
                if (_map[x, y].IsMine)
                {
                    if (GameInfo.PlayerX == x && GameInfo.PlayerY == y)
                        ForegroundColor = GameInfo.EXPLORED_MINE_COLOR;
                    else
                        ForegroundColor = GameInfo.MINE_COLOR;

                    SetCursorPosition(x, y + GameInfo.MAP_ROW);
                    Write(GameInfo.MINE_SYMBOL);
                }
                else
                {
                    SetCursorPosition(x, y + GameInfo.MAP_ROW);
                    ForegroundColor = _map[x, y].Color;
                    Write(_map[x, y].Symbol);
                }
            }
        }
    }


    private void SetControlHelpLine(int row)
    {
        SetMessageToLine("Управление:", row);
        SetMessageToLine($"{ConsoleKey.E} - Копать", row + 1);
        SetMessageToLine($"{ConsoleKey.Q} - Установить флаг", row + 2);

        SetMessageToLine($"{ConsoleKey.W}/{ConsoleKey.UpArrow} - Наверх", row + 4);
        SetMessageToLine($"{ConsoleKey.A}/{ConsoleKey.LeftArrow} - Влево", row + 5);
        SetMessageToLine($"{ConsoleKey.S}/{ConsoleKey.DownArrow} - Вниз", row + 6);
        SetMessageToLine($"{ConsoleKey.D}/{ConsoleKey.RightArrow} - Вправо", row + 7);

        SetMessageToLine($"{1} - Легкая сложность", row + 9);
        SetMessageToLine($"{2} - Средняя сложность", row + 10);
        SetMessageToLine($"{3} - Высокая сложность", row + 11);

        SetMessageToLine($"{ConsoleKey.Escape}/{ConsoleKey.X} - Выход", row + 13);
    }

    private bool IsBorder(int x, int y)
    {
        return
            x == 0 || y == 0 || // Верхняя и левая граница
            x == GameInfo.MapWidth - 1 || y == GameInfo.MapHeight - 1; // Нижняя и правая
    }


    private void BfsDig()
    {
        var queue = new Queue<Cell>();
        queue.Enqueue(_map[GameInfo.PlayerX, GameInfo.PlayerY]);

        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();

            if (cell.MinesAround is not 0)
                continue;

            foreach (var item in GetCellsAround(cell.X, cell.Y))
            {
                if (IsBorder(item.X, item.Y) || item.State is State.Digged)
                    continue;

                queue.Enqueue(item);
                SetMinesAroundAndUpdateDiggedStat(item.X, item.Y);
            }
        }
    }

    private Cell[] GetCellsAround(int x, int y)
    {
        var cell1 = _map[x - 1, y - 1];
        var cell2 = _map[x, y - 1];
        var cell3 = _map[x + 1, y - 1];

        var cell4 = _map[x - 1, y];
        var cell6 = _map[x + 1, y];

        var cell7 = _map[x - 1, y + 1];
        var cell8 = _map[x, y + 1];
        var cell9 = _map[x + 1, y + 1];

        return [cell1, cell2, cell3, cell4, cell6, cell7, cell8, cell9];
    }

    private void SetMinesAroundAndUpdateDiggedStat(int x, int y)
    {
        if (_map[x, y].State is not State.Digged && !IsBorder(x, y))
        {
            _map[x, y].State = State.Digged;
            _map[x, y].Color = GameInfo.DIGGED_COLOR;
            GameInfo.DiggedCount++;
        }

        if (_map[x, y].MinesAround is not 0)
        {
            _map[x, y].Symbol = _map[x, y].MinesAround
                .ToString()
                .First();

            _map[x, y].Color = GameInfo.NumberColors[_map[x, y].Symbol];
        }
        else
        {
            _map[x, y].Symbol = GameInfo.DIGGED_SYMBOL;
            _map[x, y].Color = GameInfo.DIGGED_COLOR;
        }
    }
}