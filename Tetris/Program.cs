using System;

namespace Tetris;
        
public static class Program
{
    public static void Main(string[] args)
    {
        TetrisGame game = new TetrisGame(60);
        game.MainLoop();
        // game.TestScene();
    }
}