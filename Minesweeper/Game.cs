using static System.Console;

namespace Minesweeper;

public class Game
{
    private const int EASY_X_PRESET = 11;
    private const int EASY_Y_PRESET = 5;
    private const int EASY_MINES_COUNT_PRESET = 10;

    private const int MEDIUM_X_PRESET = 15;
    private const int MEDIUM_Y_PRESET = 10;
    private const int MEDIUM_MINES_COUNT_PRESET = 25;

    private const int HARD_X_PRESET = 31;
    private const int HARD_Y_PRESET = 20;
    private const int HARD_MINES_COUNT_PRESET = 100;

    private const ConsoleColor TEXT_COLOR = ConsoleColor.White;

    private const char DEFAULT_SYMBOL = '█';
    private const ConsoleColor DEFAULT_COLOR_EVEN = ConsoleColor.Green;
    private const ConsoleColor DEFAULT_COLOR_NOT_EVEN = ConsoleColor.DarkGreen;

    private const ConsoleColor BORDER_COLOR = ConsoleColor.DarkGray;
    private const char BORDER_SYMBOL = '█';

    private const ConsoleColor DIGGED_COLOR = ConsoleColor.DarkYellow;
    private const char DIGGED_SYMBOL = ' ';

    private const ConsoleColor FLAG_COLOR = ConsoleColor.DarkRed;
    private const char FLAG_SYMBOL = '█';

    private const ConsoleColor MINE_COLOR = ConsoleColor.White;
    private const char MINE_SYMBOL = '*';


    private const ConsoleColor CURSOR_COLOR = ConsoleColor.White;
    private const char CURSOR_SYMBOL = '█';

    private const int SCREEN_WIDTH = 100;
    private const int SCREEN_HEIGHT = 50;

    private int _mapWidth;
    private int _mapHeight;

    private int _mapPosition = 4;
    private int _infoPosition = 2;
    private int _footerPosition = 5;

    private DateTime _startDate;
    private int _preset;
    private int _minesCount;
    private int _flagsCount;

    private int _playerX, _playerY;

    private readonly CancellationTokenSource _cts = new();
    private Cell[,] _map;

    private void AddHeaderLine()
    {
        AddMessageToLine($"{new string('-', (_mapWidth / 2) - 3)} САПЁР {new string('-', (_mapWidth / 2) - 3)}", 0);
    }

    private void AddInfoLine(int row)
    {
        AddMessageToLine($"Мин: {_minesCount}, Флажков: {_flagsCount} ", row);
    }

    private void AddControlHelpLine(int row)
    {
        AddMessageToLine("Управление:", row);
        AddMessageToLine($"{ConsoleKey.E} - Копать", row + 1);
        AddMessageToLine($"{ConsoleKey.Q} - Флаг", row + 2);

        AddMessageToLine($"{ConsoleKey.W}/{ConsoleKey.UpArrow} - Наверх", row + 4);
        AddMessageToLine($"{ConsoleKey.A}/{ConsoleKey.LeftArrow} - Влево", row + 5);
        AddMessageToLine($"{ConsoleKey.S}/{ConsoleKey.DownArrow} - Вниз", row + 6);
        AddMessageToLine($"{ConsoleKey.D}/{ConsoleKey.RightArrow} - Вправо", row + 7);

        AddMessageToLine($"{ConsoleKey.D1} - Легкая сложность", row + 9);
        AddMessageToLine($"{ConsoleKey.D2} - Средняя сложность", row + 10);
        AddMessageToLine($"{ConsoleKey.D3} - Сложная сложность", row + 11);

        AddMessageToLine($"{ConsoleKey.Escape}/{ConsoleKey.X} - Выход", row + 13);
    }

    private void AddMessageToLine(string message, int row)
    {
        SetCursorPosition(0, row);
        ForegroundColor = TEXT_COLOR;
        Write(message);
    }


    public void Run()
    {
        SetWindowSize(SCREEN_WIDTH, SCREEN_HEIGHT);
        SetBufferSize(SCREEN_WIDTH, SCREEN_HEIGHT);
        CursorVisible = false;

        InitMediumGame();

        while (!_cts.IsCancellationRequested)
        {
            WriteInfo();
            HandleInput();
        }
    }


    private void WriteInfo()
    {
        AddInfoLine(_infoPosition);
        SetMap(_mapPosition);
    }

    private void HandleInput()
    {
        if (!KeyAvailable)
            return;

        var input = ReadKey(true).Key;

        switch (input)
        {
            case ConsoleKey.E:
                var diff = DateTime.UtcNow.Subtract(_startDate);

                string message;

                if (DigAtPlayerPosition())
                    if (!AllWihoutMinesHasDigged())
                        break;
                    else
                        message = $"Победа! Время: {diff}";
                else
                    message = $"Взрыв! Время: {diff}";

                Clear();
                AddHeaderLine();
                AddMessageToLine(message, _infoPosition);
                SetDiggedMapWithMines(_mapPosition);

                AddMessageToLine("Нажмите Enter чтобы выйти, или введите R чтобы начать заново: ", _mapHeight + _footerPosition);

                var strInput = ReadLine()?
                    .ToUpper();

                _startDate = DateTime.UtcNow;

                if (strInput == "R")
                {
                    switch (_preset)
                    {
                        case 1:
                            InitEasyGame();
                            break;

                        case 2:
                            InitMediumGame();
                            break;

                        case 3:
                            InitHardGame();
                            break;
                    }
                    break;
                }
                _cts.Cancel();
                return;

            case ConsoleKey.Q:
                SetFlagAtPlayerPosition();
                break;

            case ConsoleKey.W:
            case ConsoleKey.UpArrow:
                MoveUp();
                break;

            case ConsoleKey.A:
            case ConsoleKey.LeftArrow:
                MoveLeft();
                break;

            case ConsoleKey.S:
            case ConsoleKey.DownArrow:
                MoveDown();
                break;

            case ConsoleKey.D:
            case ConsoleKey.RightArrow:
                MoveRight();
                break;


            case ConsoleKey.D1:
                InitEasyGame();
                break;

            case ConsoleKey.D2:
                InitMediumGame();
                break;

            case ConsoleKey.D3:
                InitHardGame();
                break;

            case ConsoleKey.Escape:
            case ConsoleKey.X:
                _cts.Cancel();
                return;
        }
    }


    private void InitEasyGame()
    {
        _preset = 1;
        Init(EASY_X_PRESET, EASY_Y_PRESET, EASY_MINES_COUNT_PRESET);
    }

    private void InitMediumGame()
    {
        _preset = 2;
        Init(MEDIUM_X_PRESET, MEDIUM_Y_PRESET, MEDIUM_MINES_COUNT_PRESET);
    }

    private void InitHardGame()
    {
        _preset = 3;
        Init(HARD_X_PRESET, HARD_Y_PRESET, HARD_MINES_COUNT_PRESET);
    }

    private void Init(int x, int y, int minesCount)
    {
        Clear();
        _startDate = DateTime.UtcNow;

        _mapWidth = x + 2; // Увеличиваем для границ
        _mapHeight = y + 2;

        _minesCount = minesCount;
        _flagsCount = minesCount;

        var grid = new Cell[_mapWidth, _mapHeight];

        for (int yI = 0; yI < _mapHeight; yI++)
        {
            for (int xI = 0; xI < _mapWidth; xI++)
            {
                var cell = new Cell();

                if (IsBorder(xI, yI))
                {
                    cell.Symbol = BORDER_SYMBOL;
                    cell.Color = BORDER_COLOR;
                }
                else
                {
                    cell.Symbol = DEFAULT_SYMBOL;
                    cell.Color = GetColorForDefault(xI, yI);
                }

                cell.X = xI;
                cell.Y = yI;

                grid[xI, yI] = cell;
            }
        }

        var localMinesCount = 0;

        while (true)
        {
            var randomX = Random.Shared.Next(_mapWidth);
            var randomY = Random.Shared.Next(_mapHeight);

            if (IsBorder(randomX, randomY) || grid[randomX, randomY].IsMine)
                continue;

            if (localMinesCount < _minesCount)
            {
                grid[randomX, randomY].IsMine = true;

                localMinesCount++;
            }
            else
            {
                break;
            }
        }

        _playerX = _mapWidth / 2; // Ставим игрока по центру
        _playerY = _mapHeight / 2;

        grid[_playerX, _playerY].Color = CURSOR_COLOR;

        _map = grid;
        SetMinesCountAround();

        AddHeaderLine();
        AddControlHelpLine(_mapHeight + _footerPosition);
    }


    private ConsoleColor GetColorForDefault(int x, int y)
    {
        if ((x + y) % 2 is 0)
            return DEFAULT_COLOR_EVEN;
        else
            return DEFAULT_COLOR_NOT_EVEN;
    }

    private bool IsBorder(int nextX, int nextY)
    {
        return
            nextX == 0 || nextY == 0 || // Верхняя и левая граница
            nextX == _mapWidth - 1 || nextY == _mapHeight - 1; // Нижняя и правая
    }

    private void SetMap(int row)
    {
        for (int y = 0; y < _mapHeight; y++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                ForegroundColor = _map[x, y].Color;
                SetCursorPosition(x, y + row);
                Write(_map[x, y].Symbol);
            }
        }
    }

    private void SetDiggedMapWithMines(int row)
    {
        for (int y = 0; y < _mapHeight; y++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                if (_map[x, y].IsMine)
                {
                    SetCursorPosition(x, y + row);
                    ForegroundColor = MINE_COLOR;
                    Write(MINE_SYMBOL);
                }
                else
                {
                    SetCursorPosition(x, y + row);
                    ForegroundColor = _map[x, y].Color;
                    Write(_map[x, y].Symbol);
                }
            }
        }
    }


    private bool DigAtPlayerPosition()
    {
        if (_map[_playerX, _playerY].State is State.Flag)
            return true;

        BfsDig();
        Dig(_playerX, _playerY);

        SetSymbolAtPlayerPosition();

        if (_map[_playerX, _playerY].IsMine)
            return false;

        return true;
    }

    private void Dig(int x, int y)
    {
        _map[x, y].State = State.Digged;
        SetSymbol(x, y);
    }

    private void BfsDig()
    {
        var queue = new Queue<Cell>();
        queue.Enqueue(_map[_playerX, _playerY]);

        while (queue.Count > 0)
        {
            var cell = queue.Peek();
            queue.Dequeue();

            if (cell.MinesAround is not 0)
                continue;

            foreach (var item in GetCellsAround(cell.X, cell.Y))
            {
                if (IsBorder(item.X, item.Y) || item.State is State.Digged)
                    continue;

                queue.Enqueue(item);
                Dig(item.X, item.Y);
            }
        }
    }


    private bool AllWihoutMinesHasDigged()
    {
        var minesCount = 0;
        var diggedCount = 0;

        for (int y = 0; y < _mapHeight; y++)
            for (int x = 0; x < _mapWidth; x++)
                if (_map[x, y].IsMine)
                    minesCount++;
                else if (_map[x, y].State is State.Digged)
                    diggedCount++;

        if ((((_mapWidth - 2) * (_mapHeight - 2)) - minesCount) == diggedCount)
            return true;

        return false;
    }

    private void SetMinesCountAround()
    {
        for (int y = 0; y < _mapHeight; y++)
            for (int x = 0; x < _mapWidth; x++)
                if (IsBorder(x, y))
                    continue;
                else
                    _map[x, y].MinesAround = GetMinesAroundCount(x, y);
    }

    private int GetMinesAroundCount(int x, int y)
    {
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

        return minesAround;
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



    private void SetFlagAtPlayerPosition()
    {
        if (_map[_playerX, _playerY].State is State.Flag)
        {
            _map[_playerX, _playerY].State = State.Default;
            _flagsCount++;
        }
        else if (_map[_playerX, _playerY].State is State.Default)
        {
            _map[_playerX, _playerY].State = State.Flag;
            _flagsCount--;
        }

        SetSymbolAtPlayerPosition();
    }

    private void SetSymbolAtPlayerPosition()
    {
        SetSymbol(_playerX, _playerY);
    }

    private void SetSymbol(int x, int y)
    {
        if (_map[x, y].State is State.Default)
        {
            _map[x, y].Symbol = DEFAULT_SYMBOL;
            _map[x, y].Color = GetColorForDefault(x, y);
        }
        else if (_map[x, y].State is State.Flag)
        {
            _map[x, y].Symbol = FLAG_SYMBOL;
            _map[x, y].Color = FLAG_COLOR;
        }
        else if (_map[x, y].State is State.Digged)
        {
            if (_map[x, y].MinesAround is not 0)
            {
                _map[x, y].Symbol = _map[x, y].MinesAround
                    .ToString()
                    .First();

                switch (_map[x, y].Symbol)
                {
                    case '1':
                        _map[x, y].Color = ConsoleColor.Blue;
                        break;

                    case '2':
                        _map[x, y].Color = ConsoleColor.Green;
                        break;

                    case '3':
                        _map[x, y].Color = ConsoleColor.Red;
                        break;

                    case '4':
                        _map[x, y].Color = ConsoleColor.Magenta;
                        break;

                    case '5':
                        _map[x, y].Color = ConsoleColor.Yellow;
                        break;

                    case '6':
                        _map[x, y].Color = ConsoleColor.DarkBlue;
                        break;

                    case '7':
                        _map[x, y].Color = ConsoleColor.DarkMagenta;
                        break;

                    case '8':
                        _map[x, y].Color = ConsoleColor.DarkGray;
                        break;
                }
            }
            else
            {
                _map[x, y].Symbol = DIGGED_SYMBOL;
                _map[x, y].Color = DIGGED_COLOR;
            }
        }
    }


    private void MoveUp()
    {
        var newY = _playerY - 1;

        if (IsBorder(_playerX, newY))
            return;

        _map[_playerX, newY].Symbol = CURSOR_SYMBOL;
        _map[_playerX, newY].Color = CURSOR_COLOR;
        SetSymbolAtPlayerPosition();

        _playerY = newY;
    }

    private void MoveDown()
    {
        var newY = _playerY + 1;

        if (IsBorder(_playerX, newY))
            return;

        _map[_playerX, newY].Symbol = CURSOR_SYMBOL;
        _map[_playerX, newY].Color = CURSOR_COLOR;
        SetSymbolAtPlayerPosition();

        _playerY = newY;
    }

    private void MoveRight()
    {
        var newX = _playerX + 1;

        if (IsBorder(newX, _playerY))
            return;

        _map[newX, _playerY].Symbol = CURSOR_SYMBOL;
        _map[newX, _playerY].Color = CURSOR_COLOR;
        SetSymbolAtPlayerPosition();

        _playerX = newX;
    }

    private void MoveLeft()
    {
        var newX = _playerX - 1;

        if (IsBorder(newX, _playerY))
            return;

        _map[newX, _playerY].Symbol = CURSOR_SYMBOL;
        _map[newX, _playerY].Color = CURSOR_COLOR;
        SetSymbolAtPlayerPosition();

        _playerX = newX;
    }
}