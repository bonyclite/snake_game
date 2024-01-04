namespace Snake.Core.Events;

public record SnakeCrashedEvent : SnakeEventBase
{
    public override SnakeEventType Type => SnakeEventType.Crash;
}