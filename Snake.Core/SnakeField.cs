using Snake.Core.CoreGrid;

namespace Snake.Core;

public class SnakeField
{
    private readonly Grid _grid;
    private readonly Dictionary<int, Cell> _freeCells = [];
    private readonly LinkedList<Cell> _snakeBody = [];
    private readonly Size _size;
    
    public int FreeCellsCount => _freeCells.Count;
    public SnakePoint Head => new(_snakeBody.Last!.Value);
    public SnakePoint Tail => new(_snakeBody.First!.Value);

    public SnakeField(Size size)
    {
        _size = size;
        _grid = new Grid(size);
        
        var index = 0;

        foreach (var cell in _grid)
        {
            _freeCells.Add(index, cell);
            index++;
        }
    }

    public SnakePoint[] GenerateStartPosition(int startLength)
    {
        var points = new SnakePoint[startLength];
        
        if (startLength > 1)
        {
            for (var i = 0; i < startLength; i++)
            {
                var cell = _grid.Current;
                
                var snakePoint = new SnakePoint(cell);
                _snakeBody.AddLast(_grid[cell]);
                AddSnakePoint(snakePoint);

                _grid.MoveNext();
                
                points[i] = snakePoint;
            }
        }
        else
        {
            var cell = _grid.GetRandomCell();
            var snakePoint = new SnakePoint(cell);
            
            _snakeBody.AddFirst(_grid[cell]);
            AddSnakePoint(snakePoint);

            points[0] = snakePoint;
        }
        
        _grid.Reset();

        return points;
    }
    
    public SnakePoint SpamRandomFood()
    {
        var randomIndex = Random.Shared.Next(0, _freeCells.Count - 1);

        var cell = _freeCells[randomIndex];
        cell.Content.Add(SnakeCellContent.Food);
        
        _freeCells.Remove(randomIndex);

        return new SnakePoint(cell);
    }

    public void MoveSnakePoint(SnakePoint point)
    {
        if (point.Row < 0 || point.Row == _size.Height || point.Column < 0 || point.Column == _size.Width)
        {
            throw new SnakeCrashedToWallsException();
        }

        if (_grid[point].Content.Contains(SnakeCellContent.Body))
        {
            throw new SnakeCrashedToItselfException();
        }
        
        AddSnakePoint(point);
        
        var firstNode = _snakeBody.First!;
        
        var previousCell = _grid[firstNode.Value];
        previousCell.Content.Remove(SnakeCellContent.Body);
        
        _freeCells.TryAdd(previousCell.Row * previousCell.Column, previousCell);
        
        _snakeBody.RemoveFirst();

        var cell = _grid[point];
        _snakeBody.AddLast(cell);
    }

    public bool ContainsFood(SnakePoint point)
    {
        return _grid[point].Content.Contains(SnakeCellContent.Food);
    }
    
    public bool GrowIfFoodWasEaten(SnakePoint point)
    {
        var cell = _grid[point];

        if (cell.Content.Remove(SnakeCellContent.Food))
        {
            _snakeBody.AddFirst(cell);
            return true;
        }

        return false;
    }
    
    private void AddSnakePoint(SnakePoint point)
    {
        var cell = _grid[point];
        cell.Content.Add(SnakeCellContent.Body);
        
        _freeCells.Remove(cell.Row * cell.Column);
    }
}