namespace Minesweeper;

public struct Cell()
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsMine { get; set; }
    public State State { get; set; }
    public ConsoleColor Color { get; set; }
    public char Symbol { get; set; }
    public int MinesAround { get; set; }
}

public enum State
{
    Default,
    Digged,
    Flag
}