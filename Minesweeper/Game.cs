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
    private const int HARD_MINES_COUNT_PRESET = 50;

    private const char DEFAULT_SYMBOL = '.';
    private const char DIGGED_SYMBOL = ' ';
    private const char FLAG_SYMBOL = 'P';
    private const char MINE_SYMBOL = '*';

    private const char BORDER_SYMBOL = '█';
    private const char CURSOR_SYMBOL = '@';

    private const int SCREEN_WIDTH = 100;
    private const int SCREEN_HEIGHT = 50;

    private int _mapWidth;
    private int _mapHeight;

    private int _preset;
    private DateTime _startDate;
    private int _minesCount;
    private int _flagsCount;

    private int _playerX, _playerY;

    private static readonly char[] _buffer = new char[SCREEN_WIDTH * SCREEN_HEIGHT];
    private Cell[,] _map;

    private void SetHeaderLine(int row)
    {
        var arr = $"{new string('-', (_mapWidth / 2) - 3)} САПЁР {new string('-', (_mapWidth / 2) - 3)}".ToArray();

        for (int i = 0; i < arr.Length; i++)
            _buffer[i + (SCREEN_WIDTH * (2 - 1))] = arr[i];
    }

    private void SetInfoLine(int row)
    {
        var arr = $"Мин: {_minesCount}, Флажков: {_flagsCount} ".ToArray();

        for (int i = 0; i < arr.Length; i++)
            _buffer[i + (SCREEN_WIDTH * (row - 1))] = arr[i];
    }

    private void SetControlHelpLine(int row)
    {
        var arr1 = "Управление:".ToArray();
        var arr2 = $"{ConsoleKey.E} - Копать".ToArray();
        var arr3 = $"{ConsoleKey.Q} - Флаг".ToArray();

        var arr4 = $"{ConsoleKey.W}/{ConsoleKey.UpArrow} - Наверх".ToArray();
        var arr5 = $"{ConsoleKey.A}/{ConsoleKey.LeftArrow} - Влево".ToArray();
        var arr6 = $"{ConsoleKey.S}/{ConsoleKey.DownArrow} - Вниз".ToArray();
        var arr7 = $"{ConsoleKey.D}/{ConsoleKey.RightArrow} - Вправо".ToArray();

        var arr8 = $"{ConsoleKey.D1} - Легкая сложность".ToArray();
        var arr9 = $"{ConsoleKey.D2} - Средняя сложность".ToArray();
        var arr10 = $"{ConsoleKey.D3} - Сложная сложность".ToArray();

        var arr11 = $"{ConsoleKey.Escape}/{ConsoleKey.X} - Выход".ToArray();

        AddArrayToLine(arr1, row);
        AddArrayToLine(arr2, row + 1);
        AddArrayToLine(arr3, row + 2);

        AddArrayToLine(arr4, row + 4);
        AddArrayToLine(arr5, row + 5);
        AddArrayToLine(arr6, row + 6);
        AddArrayToLine(arr7, row + 7);

        AddArrayToLine(arr8, row + 9);
        AddArrayToLine(arr9, row + 10);
        AddArrayToLine(arr10, row + 11);
        AddArrayToLine(arr11, row + 13);
    }

    private void AddArrayToLine(char[] arr, int row)
    {
        for (int i = 0; i < arr.Length; i++)
            _buffer[i + (SCREEN_WIDTH * (row - 1))] = arr[i];
    }

    public void Run()
    {
        InitMediumGame();

        Clear();
        SetWindowSize(SCREEN_WIDTH, SCREEN_HEIGHT);
        SetBufferSize(SCREEN_WIDTH, SCREEN_HEIGHT);
        CursorVisible = false;

        while (true)
        {
            SetCursorPosition(0, 0);
            WriteInfo();
            HandleInput();
        }
    }

    private void WriteInfo()
    {
        SetInfoLine(4);
        SetMap(6);
        Write(_buffer);
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

                Array.Fill(_buffer, ' ');
                SetHeaderLine(2);
                AddArrayToLine(message.ToArray(), 4);
                SetDiggedMapWithMines(6);

                var endMessage = "Нажмите Enter чтобы выйти, или введите R чтобы начать заново: ".ToArray();
                AddArrayToLine(endMessage, _mapHeight + 8);
                Write(_buffer);

                SetCursorPosition(endMessage.Length, _mapHeight + 7);
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
                    cell.Symbol = BORDER_SYMBOL;
                else
                    cell.Symbol = DEFAULT_SYMBOL;

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

            if (IsBorder(randomX, randomY))
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

        grid[_playerX, _playerY].Symbol = CURSOR_SYMBOL;

        _map = grid;
        SetMinesCountAround();

        Array.Fill(_buffer, ' ');
        SetHeaderLine(2);
        SetControlHelpLine(_mapHeight + 7);
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
            for (int x = 0; x < _mapWidth; x++)
                _buffer[((y + row - 1) * SCREEN_WIDTH) + x] = _map[x, y].Symbol;
    }

    private void SetDiggedMapWithMines(int row)
    {
        for (int y = 0; y < _mapHeight; y++)
            for (int x = 0; x < _mapWidth; x++)
                if (_map[x, y].IsMine)
                    _buffer[((y + row - 1) * SCREEN_WIDTH) + x] = MINE_SYMBOL;
                else if (IsBorder(x, y))
                    _buffer[((y + row - 1) * SCREEN_WIDTH) + x] = BORDER_SYMBOL;
                else
                    _buffer[((y + row - 1) * SCREEN_WIDTH) + x] = DIGGED_SYMBOL;
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
        }
        else if (_map[x, y].State is State.Flag)
        {
            _map[x, y].Symbol = FLAG_SYMBOL;
        }
        else if (_map[x, y].State is State.Digged)
        {
            if (_map[x, y].MinesAround is not 0)
                _map[x, y].Symbol = _map[x, y].MinesAround
                    .ToString()
                    .First();
            else
                _map[x, y].Symbol = DIGGED_SYMBOL;
        }
    }


    private void MoveUp()
    {
        var newY = _playerY - 1;

        if (IsBorder(_playerX, newY))
            return;

        _map[_playerX, newY].Symbol = CURSOR_SYMBOL;
        SetSymbolAtPlayerPosition();

        _playerY = newY;
    }

    private void MoveDown()
    {
        var newY = _playerY + 1;

        if (IsBorder(_playerX, newY))
            return;

        _map[_playerX, newY].Symbol = CURSOR_SYMBOL;
        SetSymbolAtPlayerPosition();

        _playerY = newY;
    }

    private void MoveRight()
    {
        var newX = _playerX + 1;

        if (IsBorder(newX, _playerY))
            return;

        _map[newX, _playerY].Symbol = CURSOR_SYMBOL;
        SetSymbolAtPlayerPosition();

        _playerX = newX;
    }

    private void MoveLeft()
    {
        var newX = _playerX - 1;

        if (IsBorder(newX, _playerY))
            return;

        _map[newX, _playerY].Symbol = CURSOR_SYMBOL;
        SetSymbolAtPlayerPosition();

        _playerX = newX;
    }
}