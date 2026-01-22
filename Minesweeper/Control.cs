namespace Minesweeper;

public class Control
{
    public readonly ConsoleKey[] DigKeys = [ConsoleKey.E];
    public readonly ConsoleKey[] FlagKeys = [ConsoleKey.Q];

    public readonly ConsoleKey[] MoveUpKeys = [ConsoleKey.W, ConsoleKey.UpArrow];
    public readonly ConsoleKey[] MoveLeftKeys = [ConsoleKey.A, ConsoleKey.LeftArrow];
    public readonly ConsoleKey[] MoveDownKeys = [ConsoleKey.S, ConsoleKey.DownArrow];
    public readonly ConsoleKey[] MoveRightKeys = [ConsoleKey.D, ConsoleKey.RightArrow];

    public readonly ConsoleKey[] EasyDifficultKeys = [ConsoleKey.D1];
    public readonly ConsoleKey[] MediumDifficultKeys = [ConsoleKey.D2];
    public readonly ConsoleKey[] HardDifficultKeys = [ConsoleKey.D3];


    public readonly ConsoleKey[] LeaveKeys = [ConsoleKey.X, ConsoleKey.Escape];

    public readonly Dictionary<ConsoleKey, Action> ControlActions = new();

    public void AddDigAction(Action action)
    {
        AddAction(DigKeys, action);
    }

    public void AddFlagAction(Action action)
    {
        AddAction(FlagKeys, action);
    }

    public void AddMoveUpAction(Action action)
    {
        AddAction(MoveUpKeys, action);
    }

    public void AddMoveLeftAction(Action action)
    {
        AddAction(MoveLeftKeys, action);
    }

    public void AddMoveDownAction(Action action)
    {
        AddAction(MoveDownKeys, action);
    }

    public void AddMoveRightAction(Action action)
    {
        AddAction(MoveRightKeys, action);
    }

    public void AddEasyDifficultAction(Action action)
    {
        AddAction(EasyDifficultKeys, action);
    }

    public void AddMediumDifficultAction(Action action)
    {
        AddAction(MediumDifficultKeys, action);
    }

    public void AddHardDifficultAction(Action action)
    {
        AddAction(HardDifficultKeys, action);
    }

    public void AddLeaveAction(Action action)
    {
        AddAction(LeaveKeys, action);
    }

    public void AddAction(in ConsoleKey[] keys, in Action action)
    {
        foreach (var key in keys)
            ControlActions.Add(key, action);
    }
}
