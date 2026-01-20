namespace Minesweeper;

public struct GameInfo(Difficulty difficulty)
{
    public int MapWidth { get; set; } = difficulty.MapWitdh;
    public int MapHeight { get; set; } = difficulty.MapHeight;

    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public int Preset { get; set; } = difficulty.Preset;

    public int DiggedCount { get; set; } = 0;
    public int MinesCount { get; set; } = difficulty.MinesCount;
    public int FlagsCount { get; set; } = difficulty.MinesCount;

    public int PlayerX { get; set; } = 0;
    public int PlayerY { get; set; } = 0;

    public bool ItsFirstMove { get; set; } = true;
    public bool GameIsRunning { get; set; } = true;
}
