namespace Minesweeper;

public class Difficulty(in int mapWidth, in int mapHeight, in int minesCount, in int preset)
{
    public int MapWidth { get; init; } = mapWidth;
    public int MapHeight { get; init; } = mapHeight;
    public int MinesCount { get; init; } = minesCount;
    public int Preset { get; init; } = preset;
}
