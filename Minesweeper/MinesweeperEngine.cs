namespace Minesweeper;

public class MinesweeperEngine
{
    public MinesweeperEngine()
    {
        StartMediumGame();
    }

    public const int EASY_X_PRESET = 11;
    public const int EASY_Y_PRESET = 5;

    public const int MEDIUM_X_PRESET = 15;
    public const int MEDIUM_Y_PRESET = 10;

    public const int HARD_X_PRESET = 31;
    public const int HARD_Y_PRESET = 20;

    public const char DEFAULT_SYMBOL = '.';
    public const char DIGGED_SYMBOL = ' ';
    public const char FLAG_SYMBOL = '|';
    public const char MINE_SYMBOL = 'X';

    public const char BORDER_SYMBOL = '#';
    public const char CURSOR_SYMBOL = '@';

    public int GridX, GridY;
    private int _playerX, _playerY;

    private Cell[,] Grid;

    public void StartEasyGame()
    {
        StartGame(EASY_X_PRESET, EASY_Y_PRESET);
    }

    public void StartMediumGame()
    {
        StartGame(MEDIUM_X_PRESET, MEDIUM_Y_PRESET);
    }

    public void StartHardGame()
    {
        StartGame(HARD_X_PRESET, HARD_Y_PRESET);
    }

    public void StartGame(int x, int y)
    {
        GridX = x + 2; // Увеличиваем для границ
        GridY = y + 2;

        _playerX = GridX - 2; // Ставим игрока в нижнем правом краю
        _playerY = GridY - 2;

        var grid = new Cell[GridX, GridY];

        for (int yI = 0; yI < GridY; yI++)
        {
            for (int xI = 0; xI < GridX; xI++)
            {
                if (IsBorder(xI, yI))
                {
                    grid[xI, yI].State = State.Border;
                    grid[xI, yI].Symbol = BORDER_SYMBOL;
                }
                else
                {
                    if (Random.Shared.Next(5) == 0)
                        grid[xI, yI].IsMine = true;

                    grid[xI, yI].Symbol = DEFAULT_SYMBOL;
                }
            }
        }

        grid[_playerX, _playerY].Symbol = CURSOR_SYMBOL;

        Grid = grid;
    }


    public bool IsBorder(int nextX, int nextY)
    {
        return
            nextX == 0 || nextY == 0 || // Верхняя и левая граница
            nextX == GridX - 1 || nextY == GridY - 1; // Нижняя и правая
    }

    public void PrintGrid()
    {
        for (int y = 0; y < GridY; y++)
        {
            for (int x = 0; x < GridX; x++)
                Console.Write(Grid[x, y].Symbol);

            Console.WriteLine();
        }
    }

    public void ShowGrid()
    {
        for (int y = 0; y < GridY; y++)
        {
            for (int x = 0; x < GridX; x++)
                if (Grid[x, y].IsMine)
                    Console.Write(MINE_SYMBOL);
                else if(Grid[x, y].State is State.Border)
                    Console.Write(BORDER_SYMBOL);
                else
                    Console.Write(DIGGED_SYMBOL);

            Console.WriteLine();
        }
    }


    public bool Dig()
    {
        if (Grid[_playerX, _playerY].State is State.Flag)
            return true;

        Grid[_playerX, _playerY].Symbol = DIGGED_SYMBOL;
        Grid[_playerX, _playerY].State = State.Digged;

        SetSymbol();

        if (Grid[_playerX, _playerY].IsMine)
            return false;

        return true;
    }

    public bool AllWihoutMinesHasDigged()
    {
        var minesCount = 0;
        var diggedCount = 0;

        for (int y = 0; y < GridY; y++)
            for (int x = 0; x < GridX; x++)
                if (Grid[x, y].IsMine)
                    minesCount++;
                else if (Grid[x, y].State is State.Digged)
                    diggedCount++;

        if ((((GridX - 2) * (GridY - 2)) - minesCount) == diggedCount)
            return true;

        return false;
    }

    public int GetMinesCountAround()
    {
        var cell1 = Grid[_playerX - 1, _playerY - 1];
        var cell2 = Grid[_playerX, _playerY - 1];
        var cell3 = Grid[_playerX + 1, _playerY - 1];

        var cell4 = Grid[_playerX - 1, _playerY];
        var cell5 = Grid[_playerX, _playerY];
        var cell6 = Grid[_playerX + 1, _playerY];

        var cell7 = Grid[_playerX - 1, _playerY + 1];
        var cell8 = Grid[_playerX, _playerY + 1];
        var cell9 = Grid[_playerX + 1, _playerY + 1];

        int minesCount = 0;

        if (cell1.IsMine)
            minesCount++;
        if (cell2.IsMine)
            minesCount++;
        if (cell3.IsMine)
            minesCount++;

        if (cell4.IsMine)
            minesCount++;
        if (cell5.IsMine)
            minesCount++;
        if (cell6.IsMine)
            minesCount++;

        if (cell7.IsMine)
            minesCount++;
        if (cell8.IsMine)
            minesCount++;
        if (cell9.IsMine)
            minesCount++;

        return minesCount;
    }


    public void SetFlag()
    {
        if (Grid[_playerX, _playerY].State is State.Flag)
            Grid[_playerX, _playerY].State = State.Default;
        else if (Grid[_playerX, _playerY].State is State.Default)
            Grid[_playerX, _playerY].State = State.Flag;

        SetSymbol();
    }

    public void SetSymbol()
    {
        if (Grid[_playerX, _playerY].State is State.Default)
        {
            Grid[_playerX, _playerY].Symbol = DEFAULT_SYMBOL;
        }
        else if(Grid[_playerX, _playerY].State is State.Flag)
        {
            Grid[_playerX, _playerY].Symbol = FLAG_SYMBOL;
        }
        else if (Grid[_playerX, _playerY].State is State.Digged)
        {
            var minesCount = GetMinesCountAround();

            if (minesCount is not 0)
                Grid[_playerX, _playerY].Symbol = minesCount
                    .ToString()
                    .First();
            else
                Grid[_playerX, _playerY].Symbol = DIGGED_SYMBOL;
        }
    }


    public void MoveUp()
    {
        var newY = _playerY - 1;

        if (IsBorder(_playerX, newY))
            return;

        Grid[_playerX, newY].Symbol = CURSOR_SYMBOL;
        SetSymbol();

        _playerY = newY;
    }

    public void MoveDown()
    {
        var newY = _playerY + 1;

        if (IsBorder(_playerX, newY))
            return;

        Grid[_playerX, newY].Symbol = CURSOR_SYMBOL;
        SetSymbol();

        _playerY = newY;
    }

    public void MoveRight()
    {
        var newX = _playerX + 1;

        if (IsBorder(newX, _playerY))
            return;

        Grid[newX, _playerY].Symbol = CURSOR_SYMBOL;
        SetSymbol();

        _playerX = newX;
    }

    public void MoveLeft()
    {
        var newX = _playerX - 1;

        if (IsBorder(newX, _playerY))
            return;

        Grid[newX, _playerY].Symbol = CURSOR_SYMBOL;
        SetSymbol();

        _playerX = newX;
    }
}