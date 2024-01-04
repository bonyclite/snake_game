using Snake.Core.CoreGrid;

namespace Snake.Core;

public record struct SnakePoint(int Row, int Column)
{
    internal SnakePoint(Cell cell) : this(cell.Row, cell.Column)
    {
    }
}