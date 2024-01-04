namespace Snake.Core.Events;

public record PositionWasChangedEvent(
    SnakePoint ActualHead,
    SnakePoint ActualTail,
    SnakePoint PreviousHead,
    SnakePoint PreviousTail) : SnakeEventBase
{
    public override SnakeEventType Type => SnakeEventType.PositionWasChanged;
}