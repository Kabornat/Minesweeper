namespace Minesweeper;

public struct Cell()
{
    public bool IsMine { get; set; }
    public State State { get; set; }
    public char Symbol { get; set; }
}

public enum State
{
    Default,
    Border,
    Digged,
    Flag
}