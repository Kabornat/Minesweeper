namespace Minesweeper;

public class Difficulty(int mapWidth, int mapHeight, int minesCount, int preset)
{
    public int MapWidth { get; set; } = mapWidth;
    public int MapHeight { get; set; } = mapHeight;
    public int MinesCount { get; set; } = minesCount;
    public int Preset { get; set; } = preset;
}
