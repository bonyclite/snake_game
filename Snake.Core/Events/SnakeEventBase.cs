namespace Snake.Core.Events;

public abstract record SnakeEventBase
{
    public abstract SnakeEventType Type { get; }
};