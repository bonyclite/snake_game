namespace Snake.Core.Events;

public record SpamNewFoodEvent(SnakePoint FoodPoint) : SnakeEventBase
{
    public override SnakeEventType Type => SnakeEventType.SpamNewFood;
}