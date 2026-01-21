namespace Minesweeper;

public struct Cell(int x, int y)
{
    public int X { get; set; } = x;
    public int Y { get; set; } = y;
    public bool IsMine { get; set; }
    public State State { get; set; }
    public ConsoleColor Color { get; set; }
    public char Symbol { get; set; }
    public int MinesAround { get; set; }


    public bool IsDefault()
    {
        return State is State.Default;
    }

    public bool IsDigged()
    {
        return State is State.Digged;
    }

    public bool IsFlag()
    {
        return State is State.Flag;
    }
}

public enum State
{
    Default,
    Digged,
    Flag
}