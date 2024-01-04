// See https://aka.ms/new-console-template for more information

using Snake.Console;
using static System.Console;

var snakeGame = new ConsoleSnakeGame(20, 20, TimeSpan.FromMilliseconds(200));
snakeGame.Start();

ReadLine();