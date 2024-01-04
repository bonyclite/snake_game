namespace Snake.Core.CoreGrid;

internal record Cell(int Row, int Column, HashSet<SnakeCellContent> Content);