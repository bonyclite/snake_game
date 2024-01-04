using System.Collections;

namespace Snake.Core.CoreGrid;

internal class Grid : IEnumerator<Cell>
{
    private int _i;
    private int _j = -1;

    private readonly Size _size;
    private readonly Cell[,] _cells;

    object IEnumerator.Current => Current;

    public Cell Current => _cells[_i, _j];

    public Grid(Size size)
    {
        _size = size;
        _cells = new Cell[size.Height, size.Width];

        for (var i = 0; i < size.Height; i++)
        for (var j = 0; j < size.Width; j++)
        {
            _cells[i, j] = new Cell(i, j, []);
        }
    }

    public Cell this[SnakePoint point] => _cells[point.Row, point.Column];
    public Cell this[Cell cell] => _cells[cell.Row, cell.Column];

    public IEnumerator<Cell> GetEnumerator()
    {
        return this;
    }

    public Cell GetRandomCell()
    {
        var i = Random.Shared.Next((_size.Height - 1) * 7 / 10, _size.Height - 1);
        var j = Math.Min((_size.Width - 1) * 7 / 10, Random.Shared.Next(0, _size.Width - 1));

        return _cells[i, j];
    }

    public bool MoveNext()
    {
        var moveNext = !(_i == _size.Height - 1 && _j == _size.Width - 1);

        if (_j == _size.Width - 1)
        {
            _j = 0;
            _i++;
        }
        else
        {
            _j++;
        }

        if (_i == _size.Height)
        {
            _i = 0;
        }

        return moveNext;
    }

    public void Reset()
    {
        _i = 0;
        _j = -1;
    }

    public void Dispose()
    {
    }
}