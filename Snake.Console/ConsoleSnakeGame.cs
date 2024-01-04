using System.Collections.Concurrent;
using Snake.Core;
using Snake.Core.Events;
using static System.Console;

namespace Snake.Console;

public class ConsoleSnakeGame
{
    private BlockingCollection<SnakeEventBase> _events = new();
    
    private readonly BlockingCollection<SnakeDirection> _snakeDirections = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Size _size;
    private readonly SnakeGame _snakeGame;

    public ConsoleSnakeGame(int width, int height, TimeSpan speed)
    {
        _size = new Size(width, height);
        _snakeGame = new SnakeGame(_size, speed, _snakeDirections);
        
        _snakeGame.NoPlaceForFood += SnakeGameOnNoPlaceForFood;

        CursorVisible = false;
    }

    public void Start()
    {
        DrawBorders();
        
        var (startPoints, events)  = _snakeGame.GenerateStartPosition(1);

        foreach (var startPoint in startPoints)
        {
            Print(startPoint, '*');   
        }
        
        _events = events;
        
        _snakeDirections.Add(SnakeDirection.Right);
        _snakeGame.StartListening();
        
        Task.Run(() => ListenAsync(_cancellationTokenSource.Token));

        do
        {
            var consoleKeyInfo = ReadKey();

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                break;
            }

            var direction = consoleKeyInfo.Key switch
            {
                ConsoleKey.LeftArrow => SnakeDirection.Left,
                ConsoleKey.UpArrow => SnakeDirection.Up,
                ConsoleKey.RightArrow => SnakeDirection.Right,
                ConsoleKey.DownArrow => SnakeDirection.Down,
                _ => (SnakeDirection?)null
            };

            if (direction is not null)
            {
                _snakeDirections.Add(direction.Value);
            }
        } while (true);
    }

    private void ListenAsync(CancellationToken token)
    {
        var milliseconds = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;

        while (!_events.IsCompleted)
        {
            var isTaken = _events.TryTake(out var @event, milliseconds, token);

            if (isTaken)
            {
                HandleEvent(@event!);
            }
        }

        WriteLine("End");
    }

    private void DrawBorders()
    {
        var width = _size.Width + 1;
        var height = _size.Height + 1;

        Print(0, 0, '\u2554');
        Print(width, 0, '\u2557');

        Print(width, height, '\u255D');
        Print(0, height, '\u255A');

        // vertical
        for (var i = 1; i < height; i++)
        {
            Print(0, i, '\u2551');
            Print(width, i, '\u2551');
        }

        // horizontal
        for (var i = 1; i < width; i++)
        {
            Print(i, 0, '\u2550');
            Print(i, height, '\u2550');
        }
    }

    private void Print(int x, int y, char symbol)
    {
        SetCursorPosition(x, y);
        Write(symbol);
    }

    private void Print(SnakePoint point, char symbol)
    {
        Print(point.Column + 1, point.Row + 1, symbol);
    }

    private void SnakeGameOnNoPlaceForFood()
    {
        ForegroundColor = ConsoleColor.Cyan;
        WriteLine("WIN");
    }

    private void HandleEvent(SnakeEventBase obj)
    {
        switch (obj.Type)
        {
            case SnakeEventType.Crash:
                _cancellationTokenSource.Cancel();
                break;
            
            case SnakeEventType.PositionWasChanged:
                var positionWasChangedEvent = (obj as PositionWasChangedEvent)!;
                
                Print(positionWasChangedEvent.PreviousTail, ' ');
                Print(positionWasChangedEvent.ActualHead, '*');
                break;
            
            case SnakeEventType.SpamNewFood:
                var spamNewFoodEvent = (obj as SpamNewFoodEvent)!;
                Print(spamNewFoodEvent.FoodPoint, '&');
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}