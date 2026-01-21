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

    private void Init(in Difficulty difficulty)
    {
        ClearConsole(); // Очистка консоли и сбросы
        GameInfo = new(difficulty);

        var grid = new Cell[GameInfo.MapWidth, GameInfo.MapHeight];

        for (int yI = 0; yI < GameInfo.MapHeight; yI++)
        {
            for (int xI = 0; xI < GameInfo.MapWidth; xI++)
            {
                var cell = new Cell(xI, yI);

                if (IsBorder(xI, yI))
                {
                    cell.Symbol = GameInfo.BORDER_SYMBOL;
                    cell.Color = GameInfo.BORDER_COLOR;
                }
                else
                {
                    cell.Symbol = GameInfo.DEFAULT_SYMBOL;
                    cell.Color = GameInfo.GetColorForDefault(xI, yI);
                }

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

        WriteHeader();
        WriteControlHelpLine(GameInfo.MapHeight + GameInfo.FOOTER_ROW);
    }

    private bool IsBorder(in int x, in int y)
    {
        return
            x == 0 || y == 0 || // Верхняя и левая граница
            x == GameInfo.MapWidth - 1 || y == GameInfo.MapHeight - 1; // Нижняя и правая
    }

    private void ClearConsole()
    {
        SetCursorPosition(0, 0);
        Write(new string(' ', GameInfo.SCREEN_WIDTH * GameInfo.SCREEN_HEIGHT));
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
        int minesAround = 0;

        for (int y = 0; y < GameInfo.MapHeight; y++)
        {
            for (int x = 0; x < GameInfo.MapWidth; x++)
            {
                if (IsBorder(x, y))
                    continue;

                for (int yI = y - 1; yI <= y + 1; yI++)
                    for (int xI = x - 1; xI <= x + 1; xI++)
                        if (_map[xI, yI].IsMine)
                            minesAround++;

                _map[x, y].MinesAround = minesAround;
                minesAround = 0;
            }
        }
    }

    private void WriteHeader()
    {
        WriteMessage($"{new string('-', (GameInfo.MapWidth / 2) - 3)} САПЁР {new string('-', (GameInfo.MapWidth / 2) - 3)}");
    }

    private void WriteControlHelpLine(in int row)
    {
        WriteMessage("Управление:", row);

        WriteMessage($"{ConsoleKey.E} - Копать", row + 2);
        WriteMessage($"{ConsoleKey.Q} - Установить флаг", row + 3);

        WriteMessage($"{ConsoleKey.W}/{ConsoleKey.UpArrow} - Наверх", row + 5);
        WriteMessage($"{ConsoleKey.A}/{ConsoleKey.LeftArrow} - Влево", row + 6);
        WriteMessage($"{ConsoleKey.S}/{ConsoleKey.DownArrow} - Вниз", row + 7);
        WriteMessage($"{ConsoleKey.D}/{ConsoleKey.RightArrow} - Вправо", row + 8);

        WriteMessage($"{1} - Легкая сложность", row + 10);
        WriteMessage($"{2} - Средняя сложность", row + 11);
        WriteMessage($"{3} - Высокая сложность", row + 12);

        WriteMessage($"{ConsoleKey.Escape}/{ConsoleKey.X} - Выход", row + 14);
    }

    private void WriteMessage(in string message, in int row = 0)
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
            WriteMessage(ex.Message);
            ReadLine();
        }
    }


    private void WriteInfo()
    {
        WriteInfoLine();
        WriteMap();
    }

    private void WriteInfoLine()
    {
        WriteMessage($"Мин: {GameInfo.MinesCount}, Флажков: {GameInfo.FlagsCount} ", GameInfo.INFO_ROW);
    }

    private void WriteMap()
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
                Dig();
                return;

            case ConsoleKey.Q:
                SetFlagAndUpdateFlagsInStat();
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

    private void Dig()
    {
        if (_map[GameInfo.PlayerX, GameInfo.PlayerY].State is State.Flag)
            return;

        BfsDig(GameInfo.PlayerX, GameInfo.PlayerY);

        var flaggedCellsAround = GetFlaggedCellsAround();

        if (flaggedCellsAround.Count is not 0 && flaggedCellsAround.Count == _map[GameInfo.PlayerX, GameInfo.PlayerY].MinesAround)
        {
            foreach (var flaggedCell in flaggedCellsAround)
                if (!flaggedCell.IsMine && GameInfo.GameIsRunning)
                    GameInfo.GameIsRunning = false;

            foreach (var cell in GetCellsAround(GameInfo.PlayerX, GameInfo.PlayerY, withFlaggedCells: false))
                BfsDig(cell.X, cell.Y);
        }

        if (AllWithoutMinesHasDigged())
            GameOver(isWin: true);

        else if (!GameInfo.GameIsRunning)
            GameOver(isWin: false);
    }

    private void BfsDig(in int x, in int y)
    {
        DigAndUpdateDiggedStat(x, y);

        var queue = new Queue<Cell>();
        queue.Enqueue(_map[x, y]);

        while (queue.Count > 0)
        {
            var cell = queue.Dequeue();

            if (cell.MinesAround is not 0)
                continue;

            foreach (var item in GetCellsAround(cell.X, cell.Y))
            {
                if (IsBorder(item.X, item.Y) || item.IsFlag() || item.IsDigged())
                    continue;

                if (!GameInfo.GameIsRunning)
                    return;

                queue.Enqueue(item);
                DigAndUpdateDiggedStat(item.X, item.Y);
            }
        }
    }

    private IEnumerable<Cell> GetCellsAround(int x, int y, bool withFlaggedCells = true)
    {
        for (int yI = y - 1; yI <= y + 1; yI++)
        {
            for (int xI = x - 1; xI <= x + 1; xI++)
            {
                if (IsBorder(xI, yI))
                    continue;

                var cell = _map[xI, yI];

                if (!withFlaggedCells && cell.IsFlag())
                    continue;

                yield return cell;
            }
        }
    }

    private void DigAndUpdateDiggedStat(in int x, in int y)
    {
        if (IsBorder(x, y) || _map[x, y].IsFlag())
            return;

        if (!_map[x, y].IsDigged())
        {
            _map[x, y].State = State.Digged;
            GameInfo.DiggedCount++;
        }

        if (_map[x, y].IsMine)
        {
            if (GameInfo.ItsFirstMove)
            {
                GameInfo.ItsFirstMove = false;

                while (_map[GameInfo.PlayerX, GameInfo.PlayerY].IsMine)
                {
                    _map[GameInfo.PlayerX, GameInfo.PlayerY].IsMine = false;

                    if (!TryGetRandomPositionWithoutBorderAndMine(out var randomX, out var randomY))
                        continue;

                    _map[randomX, randomY].IsMine = true;
                }

                SetMinesCountAround();
                BfsDig(GameInfo.PlayerX, GameInfo.PlayerY);
            }
            else
            {
                GameInfo.GameIsRunning = false;
                return;
            }
        }
        else
        {
            GameInfo.ItsFirstMove = false;
        }

        SetDigged(x, y);
    }

    private void GameOver(bool isWin)
    {
        GameInfo.GameIsRunning = false;

        ClearConsole();
        WriteHeader();
        WriteMapWithMines();

        if (isWin)
            WriteMessage($"Победа! Время: {GameInfo.GetTimeDifference()}", GameInfo.INFO_ROW);
        else
            WriteMessage($"Взрыв! Время: {GameInfo.GetTimeDifference()}", GameInfo.INFO_ROW);

        WriteMessage("Нажмите Enter чтобы выйти, или введите R чтобы начать заново: ", GameInfo.MapHeight + GameInfo.FOOTER_ROW);
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
        }
    }

    private void SetDigged(in int x, in int y)
    {
        if (_map[x, y].MinesAround is not 0)
        {
            _map[x, y].Symbol = _map[x, y].MinesAround.ToString().First();
            _map[x, y].Color = GameInfo.NumberColors[_map[x, y].MinesAround];
        }
        else
        {
            _map[x, y].Symbol = GameInfo.DIGGED_SYMBOL;
            _map[x, y].Color = GameInfo.DIGGED_COLOR;
        }
    }

    private List<Cell> GetFlaggedCellsAround()
    {
        var cells = new List<Cell>();

        for (int y = GameInfo.PlayerY - 1; y <= GameInfo.PlayerY + 1; y++)
            for (int x = GameInfo.PlayerX - 1; x <= GameInfo.PlayerX + 1; x++)
                if (_map[x, y].IsFlag())
                    cells.Add(_map[x, y]);

        return cells;
    } 
    
    private bool AllWithoutMinesHasDigged()
    {
        return (((GameInfo.MapWidth - 2) * (GameInfo.MapHeight - 2)) - GameInfo.MinesCount) == GameInfo.DiggedCount;
    }

    private void WriteMapWithMines()
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

    private void Move(in int x, in int y)
    {
        if (IsBorder(x, y))
            return;

        _map[x, y].Symbol = GameInfo.CURSOR_SYMBOL;
        _map[x, y].Color = GameInfo.CURSOR_COLOR;

        if (_map[GameInfo.PlayerX, GameInfo.PlayerY].IsDefault())
            SetDefault();
        else if (_map[GameInfo.PlayerX, GameInfo.PlayerY].IsFlag())
            SetFlag();
        else if (_map[GameInfo.PlayerX, GameInfo.PlayerY].IsDigged())
            SetDigged(GameInfo.PlayerX, GameInfo.PlayerY);

        GameInfo.PlayerX = x;
        GameInfo.PlayerY = y;
    }

    private void SetDefault()
    {
        _map[GameInfo.PlayerX, GameInfo.PlayerY].State = State.Default;
        _map[GameInfo.PlayerX, GameInfo.PlayerY].Symbol = GameInfo.DEFAULT_SYMBOL;
        _map[GameInfo.PlayerX, GameInfo.PlayerY].Color = GameInfo.GetColorForDefault(GameInfo.PlayerX, GameInfo.PlayerY);
    }

    private void SetFlag()
    {
        _map[GameInfo.PlayerX, GameInfo.PlayerY].State = State.Flag;
        _map[GameInfo.PlayerX, GameInfo.PlayerY].Symbol = GameInfo.FLAG_SYMBOL;
        _map[GameInfo.PlayerX, GameInfo.PlayerY].Color = GameInfo.FLAG_COLOR;
    }

    private void SetFlagAndUpdateFlagsInStat()
    {
        if (GameInfo.ItsFirstMove)
            return;

        if (_map[GameInfo.PlayerX, GameInfo.PlayerY].IsDefault())
        {
            SetFlag();
            GameInfo.FlagsCount--;
        }
        else if (_map[GameInfo.PlayerX, GameInfo.PlayerY].IsFlag())
        {
            SetDefault();
            GameInfo.FlagsCount++;
        }
    }
}