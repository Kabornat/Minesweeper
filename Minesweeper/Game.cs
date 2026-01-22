namespace Minesweeper;

public class Game
{
    public Game()
    {
        Control.AddDigAction(Dig);
        Control.AddFlagAction(SetFlagAndUpdateFlagsInStat);

        Control.AddMoveUpAction(MoveUp);
        Control.AddMoveLeftAction(MoveLeft);
        Control.AddMoveDownAction(MoveDown);
        Control.AddMoveRightAction(MoveRight);

        Control.AddEasyDifficultAction(InitEasy);
        Control.AddMediumDifficultAction(InitMedium);
        Control.AddHardDifficultAction(InitHigh);

        Control.AddLeaveAction(StopGame);
    }

    private readonly Control Control = new();


    private readonly Difficulty EasyDifficulty = new(mapWidth: 9, mapHeight: 9, minesCount: 10, preset: GameInfo.EASY_PRESET);
    private readonly Difficulty MediumDifficulty = new(mapWidth: 17, mapHeight: 17, minesCount: 40, preset: GameInfo.MEDIUM_PRESET);
    private readonly Difficulty HighDifficulty = new(mapWidth: 31, mapHeight: 16, minesCount: 99, preset: GameInfo.HARD_PRESET);

    private readonly ConsoleRenderer ConsoleRenderer;
    private GameInfo GameInfo;
    private Cell[,] Map;


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
        ConsoleRenderer.ClearConsole(); // Очистка консоли и сбросы
        GameInfo = new(difficulty);

        Map = new Cell[GameInfo.MapWidth, GameInfo.MapHeight];

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

                Map[xI, yI] = cell;
            }
        }

        Map[GameInfo.PlayerX, GameInfo.PlayerY].Color = GameInfo.CURSOR_COLOR;
        Map[GameInfo.PlayerX, GameInfo.PlayerY].Symbol = GameInfo.CURSOR_SYMBOL;

        var localMinesCount = 0;
        while (true)
        {
            if (!TryGetRandomPositionWithoutBorderAndMine(out var x, out var y))
                continue;

            if (localMinesCount < GameInfo.MinesCount)
            {
                Map[x, y].IsMine = true;
                localMinesCount++;
            }
            else
            {
                break;
            }
        }

        SetMinesCountAround();

        ConsoleRenderer.WriteHeader(GameInfo.MapWidth);
        ConsoleRenderer.WriteControlHelpLine(GameInfo.MapHeight, Control);
    }

    private bool IsBorder(in int x, in int y)
    {
        return
            x == 0 || y == 0 || // Верхняя и левая граница
            x == GameInfo.MapWidth - 1 || y == GameInfo.MapHeight - 1; // Нижняя и правая
    }


    private bool TryGetRandomPositionWithoutBorderAndMine(out int x, out int y)
    {
        x = Random.Shared.Next(GameInfo.MapWidth);
        y = Random.Shared.Next(GameInfo.MapHeight);

        if (IsBorder(x, y) || Map[x, y].IsMine)
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
                        if (Map[xI, yI].IsMine)
                            minesAround++;

                Map[x, y].MinesAround = minesAround;
                minesAround = 0;
            }
        }
    }


    public void Run()
    {
        Console.SetWindowSize(GameInfo.SCREEN_WIDTH, GameInfo.SCREEN_HEIGHT);
        Console.CursorVisible = false;

        InitMedium();

        try
        {
            while (GameInfo.GameIsRunning)
            {
                ConsoleRenderer.WriteInfo(GameInfo.MinesCount, GameInfo.FlagsCount, GameInfo.MapWidth, GameInfo.MapHeight, Map);
                HandleInput();
            }
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ConsoleRenderer.ClearConsole();
            ConsoleRenderer.WriteMessage(ex.Message);
            Console.ReadLine();
        }
    }

    private void HandleInput()
    {
        if (!Console.KeyAvailable)
            return;

        if (Control.ControlActions.TryGetValue(Console.ReadKey(true).Key, out var action))
            action();
    }

    private void Dig()
    {
        if (Map[GameInfo.PlayerX, GameInfo.PlayerY].IsFlag())
            return;

        BfsDig(GameInfo.PlayerX, GameInfo.PlayerY);

        var flaggedCellsAround = GetFlaggedCellsAround();

        if (flaggedCellsAround.Count is not 0 && flaggedCellsAround.Count == Map[GameInfo.PlayerX, GameInfo.PlayerY].MinesAround)
        {
            foreach (var flaggedCell in flaggedCellsAround)
                if (!flaggedCell.IsMine && GameInfo.GameIsRunning)
                    StopGame();

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
        queue.Enqueue(Map[x, y]);

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

                var cell = Map[xI, yI];

                if (!withFlaggedCells && cell.IsFlag())
                    continue;

                yield return cell;
            }
        }
    }

    private void DigAndUpdateDiggedStat(in int x, in int y)
    {
        if (IsBorder(x, y) || Map[x, y].IsFlag())
            return;

        if (!Map[x, y].IsDigged())
        {
            Map[x, y].State = State.Digged;
            GameInfo.DiggedCount++;
        }

        if (Map[x, y].IsMine)
        {
            if (GameInfo.ItsFirstMove)
            {
                GameInfo.ItsFirstMove = false;

                while (Map[GameInfo.PlayerX, GameInfo.PlayerY].IsMine)
                {
                    Map[GameInfo.PlayerX, GameInfo.PlayerY].IsMine = false;

                    if (!TryGetRandomPositionWithoutBorderAndMine(out var randomX, out var randomY))
                        continue;

                    Map[randomX, randomY].IsMine = true;
                }

                SetMinesCountAround();
                BfsDig(GameInfo.PlayerX, GameInfo.PlayerY);
            }
            else
            {
                StopGame();
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
        StopGame();

        ConsoleRenderer.WriteGameOver(isWin, GameInfo.MapWidth, GameInfo.MapHeight, Map, GameInfo.PlayerX, GameInfo.PlayerY, GameInfo.GetTimeDifference());

        if (Console.ReadLine()?.ToUpper() != "R")
            return;

        switch (GameInfo.Preset)
        {
            case GameInfo.EASY_PRESET:
                InitEasy();
                return;

            case GameInfo.MEDIUM_PRESET:
                InitMedium();
                return;

            case GameInfo.HARD_PRESET:
                InitHigh();
                return;
        }
    }

    private void SetDigged(in int x, in int y)
    {
        if (Map[x, y].MinesAround is not 0)
        {
            Map[x, y].Symbol = Map[x, y].MinesAround.ToString().First();
            Map[x, y].Color = GameInfo.NumberColors[Map[x, y].MinesAround];
        }
        else
        {
            Map[x, y].Symbol = GameInfo.DIGGED_SYMBOL;
            Map[x, y].Color = GameInfo.DIGGED_COLOR;
        }
    }

    private List<Cell> GetFlaggedCellsAround()
    {
        var cells = new List<Cell>();

        for (int y = GameInfo.PlayerY - 1; y <= GameInfo.PlayerY + 1; y++)
            for (int x = GameInfo.PlayerX - 1; x <= GameInfo.PlayerX + 1; x++)
                if (Map[x, y].IsFlag())
                    cells.Add(Map[x, y]);

        return cells;
    } 
    
    private bool AllWithoutMinesHasDigged()
    {
        return (((GameInfo.MapWidth - 2) * (GameInfo.MapHeight - 2)) - GameInfo.MinesCount) == GameInfo.DiggedCount;
    }

    public void StopGame()
    {
        GameInfo.GameIsRunning = false;
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

        Map[x, y].Symbol = GameInfo.CURSOR_SYMBOL;
        Map[x, y].Color = GameInfo.CURSOR_COLOR;

        if (Map[GameInfo.PlayerX, GameInfo.PlayerY].IsDefault())
            SetDefault();
        else if (Map[GameInfo.PlayerX, GameInfo.PlayerY].IsFlag())
            SetFlag();
        else if (Map[GameInfo.PlayerX, GameInfo.PlayerY].IsDigged())
            SetDigged(GameInfo.PlayerX, GameInfo.PlayerY);

        GameInfo.PlayerX = x;
        GameInfo.PlayerY = y;
    }

    private void SetDefault()
    {
        Map[GameInfo.PlayerX, GameInfo.PlayerY].State = State.Default;
        Map[GameInfo.PlayerX, GameInfo.PlayerY].Symbol = GameInfo.DEFAULT_SYMBOL;
        Map[GameInfo.PlayerX, GameInfo.PlayerY].Color = GameInfo.GetColorForDefault(GameInfo.PlayerX, GameInfo.PlayerY);
    }

    private void SetFlag()
    {
        Map[GameInfo.PlayerX, GameInfo.PlayerY].State = State.Flag;
        Map[GameInfo.PlayerX, GameInfo.PlayerY].Symbol = GameInfo.FLAG_SYMBOL;
        Map[GameInfo.PlayerX, GameInfo.PlayerY].Color = GameInfo.FLAG_COLOR;
    }

    private void SetFlagAndUpdateFlagsInStat()
    {
        if (GameInfo.ItsFirstMove)
            return;

        if (Map[GameInfo.PlayerX, GameInfo.PlayerY].IsDefault())
        {
            SetFlag();
            GameInfo.FlagsCount--;
        }
        else if (Map[GameInfo.PlayerX, GameInfo.PlayerY].IsFlag())
        {
            SetDefault();
            GameInfo.FlagsCount++;
        }
    }
}