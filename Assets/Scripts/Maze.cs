public class Maze
{
    public int SizeX { get; }
    public int SizeY { get; }

    public MazeCell[,] Cells { get; }

    public Maze(int sizeX, int sizeY, MazeCell[,] cells)
    {
        SizeX = sizeX;
        SizeY = sizeY;
        Cells = cells;
    }
}
public class MazeCell
{
    private int X { get; }
    private int Y { get; }
    public WallLocations Walls { get; }

    public MazeCell(int x, int y, WallLocations walls)
    {
        X = x;
        Y = y;
        Walls = walls;
    }
}

public struct WallLocations
{
    public bool Top { get; }
    public bool Right { get; }
    public bool Bottom { get; }
    public bool Left { get; }

    public WallLocations(bool top, bool right, bool bottom, bool left)
    {
        Top = top;
        Right = right;
        Bottom = bottom;
        Left = left;
    }
}