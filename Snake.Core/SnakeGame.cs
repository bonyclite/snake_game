using System.Collections.Concurrent;
using System.Diagnostics;
using Snake.Core.Events;

namespace Snake.Core;

public class SnakeGame
{
    private SnakeDirection? _previousDirection;
    
    private readonly BlockingCollection<SnakeEventBase> _events;
    private readonly SnakeField _field;
    private readonly TimeSpan _speed;
    private readonly BlockingCollection<SnakeDirection> _snakeDirections;

    public SnakeGame(Size size
        , TimeSpan speed
        , BlockingCollection<SnakeDirection> snakeDirections)
    {
        _speed = speed;
        _snakeDirections = snakeDirections;
        _field = new SnakeField(size);
        _events = new BlockingCollection<SnakeEventBase>();
    }
    
    public event Action NoPlaceForFood = null!;
    
    public (SnakePoint[] points, BlockingCollection<SnakeEventBase> events) GenerateStartPosition(int startLength)
    {
        var points = _field.GenerateStartPosition(startLength);
        _events.Add(new SpamNewFoodEvent(_field.SpamRandomFood()));
        return (points, _events);
    }
    
    public void StartListening()
    {
        _ = MoveAsync();
    }

    private async Task MoveAsync()
    {
        using var periodicTimer = new PeriodicTimer(_speed);

        var direction = _snakeDirections.Take();
        
        while (await periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                if (_snakeDirections.Count != 0)
                {
                    _previousDirection = direction;
                    direction = _snakeDirections.Take();
                }
                
                var previousHead = _field.Head;
                var previousTail = _field.Tail;

                var head = _field.Head;

                switch (direction)
                {
                    case SnakeDirection.Left:
                        if (_previousDirection is null or SnakeDirection.Up or SnakeDirection.Down)
                        {
                            head.Column--;
                        }

                        break;

                    case SnakeDirection.Right:
                        if (_previousDirection is null or SnakeDirection.Up or SnakeDirection.Down)
                        {
                            head.Column++;
                        }

                        break;

                    case SnakeDirection.Up:
                        if (_previousDirection is null or SnakeDirection.Right or SnakeDirection.Left)
                        {
                            head.Row--;
                        }

                        break;

                    case SnakeDirection.Down:
                        if (_previousDirection is null or SnakeDirection.Right or SnakeDirection.Left)
                        {
                            head.Row++;
                        }

                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
                
                _field.MoveSnakePoint(head);

                var foodWasEaten = false;

                if (_field.ContainsFood(head))
                {
                    foodWasEaten = _field.GrowIfFoodWasEaten(_field.Head);

                    if (_field.FreeCellsCount == 0)
                    {
                        OnNoPlaceForFood();
                        return;
                    }
                }
                
                var positionWasChangedEvent = new PositionWasChangedEvent(_field.Head
                    , _field.Tail
                    , previousHead
                    , previousTail);
                
                _events.Add(positionWasChangedEvent);
                
                if (foodWasEaten)
                {
                    var point = _field.SpamRandomFood();
                    _events.Add(new SpamNewFoodEvent(point));
                }
            }
            catch (SnakeCrashedToItselfException)
            {
                OnCrashed();
                break;
            }
            catch (SnakeCrashedToWallsException)
            {
                OnCrashed();
                break;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }

    private void OnNoPlaceForFood()
    {
        NoPlaceForFood.Invoke();
    }

    private void OnCrashed()
    {
        _events.Add(new SnakeCrashedEvent());
        _events.CompleteAdding();
    }
}