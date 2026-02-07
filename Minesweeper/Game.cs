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
    private Cell[,] Map = new Cell[10,10];

    private void InitEasy() => Init(EasyDifficulty);
    private void InitMedium() => Init(MediumDifficulty);
    private void InitHigh() => Init(HighDifficulty);
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

        var playerCell = Map[GameInfo.PlayerX, GameInfo.PlayerY];

        playerCell.Color = GameInfo.CURSOR_COLOR;
        playerCell.Symbol = GameInfo.CURSOR_SYMBOL;

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
        var playerCell = Map[GameInfo.PlayerX, GameInfo.PlayerY];

        if (playerCell.IsFlag())
            return;

        BfsDig(playerCell);

        var flaggedCellsAround = GetFlaggedCellsAround(playerCell.X, playerCell.Y);

        if (flaggedCellsAround.Count is not 0 && flaggedCellsAround.Count == playerCell.MinesAround)
        {
            foreach (var flaggedCell in flaggedCellsAround)
                if (!flaggedCell.IsMine && GameInfo.GameIsRunning)
                    StopGame();

            foreach (var cell in GetCellsAround(playerCell.X, playerCell.Y, withFlaggedCells: false))
                BfsDig(cell);
        }

        if (AllWithoutMinesHasDigged())
            GameOver(isWin: true);

        else if (!GameInfo.GameIsRunning)
            GameOver(isWin: false);
    }
    private void BfsDig(in Cell cell)
    {
        DigAndUpdateDiggedStat(cell);

        var queue = new Queue<Cell>();
        queue.Enqueue(cell);

        while (queue.Count > 0)
        {
            var currentCell = queue.Dequeue();

            if (currentCell.MinesAround is not 0)
                continue;

            foreach (var item in GetCellsAround(currentCell.X, currentCell.Y))
            {
                if (IsBorder(item.X, item.Y) || item.IsFlag() || item.IsDigged())
                    continue;

                if (!GameInfo.GameIsRunning)
                    return;

                queue.Enqueue(item);
                DigAndUpdateDiggedStat(item);
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
    private void DigAndUpdateDiggedStat(in Cell cell)
    {
        if (IsBorder(cell.X, cell.Y) || cell.IsFlag())
            return;

        if (!cell.IsDigged())
        {
            cell.State = State.Digged;
            GameInfo.DiggedCount++;
        }

        if (cell.IsMine)
        {
            if (GameInfo.ItsFirstMove)
            {
                GameInfo.ItsFirstMove = false;

                var playerCell = Map[GameInfo.PlayerX, GameInfo.PlayerY];

                while (playerCell.IsMine)
                {
                    playerCell.IsMine = false;

                    if (!TryGetRandomPositionWithoutBorderAndMine(out var randomX, out var randomY))
                        continue;

                    Map[randomX, randomY].IsMine = true;
                }

                SetMinesCountAround();
                BfsDig(playerCell);
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

        SetDigged(cell);
    }
    private void GameOver(bool isWin)
    {
        StopGame();

        ConsoleRenderer.WriteGameOver(isWin, GameInfo.MapWidth, GameInfo.MapHeight, Map, GameInfo.PlayerX, GameInfo.PlayerY, GameInfo.GetTimeDifference());

        if (Console.ReadLine()?.ToUpper() != "R")
            return;

        var preset = GameInfo.Preset switch
        {
            GameInfo.EASY_PRESET => EasyDifficulty,
            GameInfo.MEDIUM_PRESET => MediumDifficulty,
            GameInfo.HARD_PRESET => HighDifficulty,
        };

        Init(preset);
    }
    private void SetDigged(in Cell cell)
    {
        if (cell.MinesAround is not 0)
        {
            cell.Symbol = cell.MinesAround.ToString().First();
            cell.Color = GameInfo.NumberColors[cell.MinesAround];
        }
        else
        {
            cell.Symbol = GameInfo.DIGGED_SYMBOL;
            cell.Color = GameInfo.DIGGED_COLOR;
        }
    }
    private List<Cell> GetFlaggedCellsAround(in int x, in int y)
    {
        var cells = new List<Cell>();

        for (int yI = y - 1; yI <= y + 1; yI++)
            for (int xI = x - 1; xI <= x + 1; xI++)
                if (Map[xI, yI].IsFlag())
                    cells.Add(Map[xI, yI]);

        return cells;
    }
    public void StopGame()
    {
        GameInfo.GameIsRunning = false;
    }
    private bool AllWithoutMinesHasDigged()
    {
        return (((GameInfo.MapWidth - 2) * (GameInfo.MapHeight - 2)) - GameInfo.MinesCount) == GameInfo.DiggedCount;
    }


    private void MoveUp() => Move(GameInfo.PlayerX, GameInfo.PlayerY - 1);
    private void MoveDown() => Move(GameInfo.PlayerX, GameInfo.PlayerY + 1);
    private void MoveRight() => Move(GameInfo.PlayerX + 1, GameInfo.PlayerY);
    private void MoveLeft() => Move(GameInfo.PlayerX - 1, GameInfo.PlayerY);

    private void Move(in int x, in int y)
    {
        if (IsBorder(x, y))
            return;

        var cell = Map[x, y];
        var playerCell = Map[GameInfo.PlayerX, GameInfo.PlayerY];

        cell.Symbol = GameInfo.CURSOR_SYMBOL;
        cell.Color = GameInfo.CURSOR_COLOR;

        if (playerCell.IsDefault())
            SetDefault(playerCell);
        else if (playerCell.IsFlag())
            SetFlag(playerCell);
        else if (playerCell.IsDigged())
            SetDigged(playerCell);

        GameInfo.PlayerX = x;
        GameInfo.PlayerY = y;
    }
    private void SetDefault(in Cell cell)
    {
        cell.State = State.Default;
        cell.Symbol = GameInfo.DEFAULT_SYMBOL;
        cell.Color = GameInfo.GetColorForDefault(GameInfo.PlayerX, GameInfo.PlayerY);
    }
    private void SetFlag(in Cell cell)
    {
        cell.State = State.Flag;
        cell.Symbol = GameInfo.FLAG_SYMBOL;
        cell.Color = GameInfo.FLAG_COLOR;
    }
    private void SetFlagAndUpdateFlagsInStat()
    {
        if (GameInfo.ItsFirstMove)
            return;

        var playerCell = Map[GameInfo.PlayerX, GameInfo.PlayerY];

        if (playerCell.IsDefault())
        {
            SetFlag(playerCell);
            GameInfo.FlagsCount--;
        }
        else if (playerCell.IsFlag())
        {
            SetDefault(playerCell);
            GameInfo.FlagsCount++;
        }
    }
}