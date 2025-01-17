using System.Diagnostics;
using System.Reflection.Metadata;

namespace Tetris;

public class TetrisGame
{
    private bool _displayFps = false; // Whether to display the fps on the screen
    private bool _gameRunning;
    private int _desiredFps;
    private int _desiredUps;

    private int _score;
    private int _level;
    private int _linesCleared;
    private int _combo;

    private Piece _currentPiece;
    private Piece _nextPiece;
    
    private int[,] _grid = new int[10, 20];

    private int _currentX = 0;
    private int _currentY = 0;

    public TetrisGame(int desiredFps)
    {
        _gameRunning = true;
        _desiredFps = desiredFps;
        _desiredUps = 1;

        _currentPiece = new Piece();
        _nextPiece = new Piece();
    }

    public void MainLoop()
    {
        int frameCount = 0;

        double fOptimalTime = (double) 1_000 / _desiredFps; // Optimal time to draw (1,000ms or 1 second divided by maxFPS/UPS)
        double uOptimalTime = (double) 1_000 / _desiredUps; // Optimal time to update

        double uDeltaTime = 0, fDeltaTime = 0; // Update delta time, fps delta time
        Stopwatch stopwatch = Stopwatch.StartNew();
        long startTime = stopwatch.ElapsedMilliseconds;

        Stopwatch fpsTimer = Stopwatch.StartNew();
        int frames = 0;
        int updates = 0;

        while (_gameRunning)
        {
            // Calculate difference in time
            long currentTime = stopwatch.ElapsedMilliseconds;
            fDeltaTime += (currentTime - startTime);
            uDeltaTime += (currentTime - startTime);
            startTime = currentTime;

            HandleControls();

            // If amount of time since last frame is the optimal time to update, then update.
            if (uDeltaTime >= uOptimalTime)
            {
                Update();

                uDeltaTime -= uOptimalTime;
                updates += 1;
            }

            if (fDeltaTime >= fOptimalTime)
            {
                Render();
                Console.SetCursorPosition(0, _grid.GetLength(1));
                Console.Write("currentPos: " + _currentX + ", " + _currentY);
                Console.Write("\ncurrentShape y length: " + _currentPiece.Shape.GetLength(1));

                if (frameCount >= _desiredFps && _displayFps)
                {
                    // Calculate frame rate and updates per second.
                    double elapsedSeconds = fpsTimer.Elapsed.TotalSeconds;
                    double fps = frames / elapsedSeconds;
                    double ups = updates / elapsedSeconds;

                    fpsTimer.Restart();
                    Console.WriteLine("FPS: " + Math.Round(fps) + "\nUPS: " + (int)ups);

                    frames = 0;
                    updates = 0;
                }

                fDeltaTime -= fOptimalTime;
                frames += 1;
            }

            Console.CursorVisible = false;
            frameCount++;
        }
    }
    
    private void HandleControls() 
    {
        if (Console.KeyAvailable)
        {
            ConsoleKeyInfo key = Console.ReadKey(true);
            
            switch (key.Key)
            {
                case ConsoleKey.LeftArrow:
                    if (IsOutOfBounds(_currentPiece, _currentX - 1, _currentY) ||
                        IsColliding(_currentPiece, _currentX - 1, _currentY))
                        break;
                    
                    _currentX--;
                    break;
                
                case ConsoleKey.RightArrow:
                    if (_currentPiece.Shape == null)
                        break;

                    if (IsOutOfBounds(_currentPiece, _currentX + 1, _currentY) ||
                        IsColliding(_currentPiece, _currentX + 1, _currentY))
                        break;
                    
                    _currentX++;
                    break;
                
                // Soft drop
                case ConsoleKey.DownArrow:
                    if (IsColliding(_currentPiece, _currentX, _currentY + 1))
                        break;
                    
                    _currentY++;
                    _score++;
                    Update(forced: true); // Make a forced update to move the piece down, so pieces don't move through each other
                    break;
                
                // Hard drop
                case ConsoleKey.Spacebar:
                    int tilesMoved = 0;
                    
                    while (!IsColliding(_currentPiece, _currentX, _currentY + 1) &&
                           !IsOutOfBounds(_currentPiece, _currentX, _currentY + 1))
                    {
                        _currentY++;
                        tilesMoved++;

                        if (IsColliding(_currentPiece, _currentX, _currentY + 1))
                        {
                            PlacePiece();
                            _score += tilesMoved * 2;
                            break;
                        }
                    }
                    break;
                
                case ConsoleKey.Z:
                    Piece cwPiece = new Piece(_currentPiece.PieceType);
                    cwPiece.RotateCw();
                    
                    if (IsColliding(cwPiece, _currentX, _currentY))
                        break;
                    
                    _currentPiece.RotateCw();
                    break;
                
                case ConsoleKey.UpArrow:
                    Piece ccwPiece = new Piece(_currentPiece.PieceType);
                    ccwPiece.RotateCcw();
                    
                    if (IsColliding(ccwPiece, _currentX, _currentY))
                        break;
                    
                    _currentPiece.RotateCcw();
                    break;
                
                case ConsoleKey.Escape:
                    _gameRunning = false;
                    break;
            }
        }
    }

    private void Update(bool forced = false)
    {
        if (_currentPiece.Shape == null)
            return;
        
        // If it's colliding with a piece or at the bottom
        if (IsColliding(_currentPiece, _currentX, _currentY + 1) ||
            _currentY + _currentPiece.Shape.GetLength(1) == _grid.GetLength(1))
            PlacePiece();

        // If this is not a forced update
        if (!forced && !IsOutOfBounds(_currentPiece, _currentX, _currentY))
                _currentY++;
    }

    private void Render()
    {
        PrintGrid("#", ".");
        PrintShapeOutline("*");
        PrintPiece(_currentPiece, _currentX, _currentY, "#");
    }

    private void PrintPiece(Piece piece, int currentX, int currentY, string pieceChar)
    {
        if (piece.Shape == null)
            return;
        
        for (int y = 0; y < piece.Shape.GetLength(1); y++)
        {
            for (int x = 0; x < piece.Shape.GetLength(0); x++)
            {
                Console.SetCursorPosition(currentX + x, currentY + y);
                if (piece.Shape[x, y] == 1)
                    Console.Write(pieceChar);
            }
            Console.WriteLine();
        }
    }

    private void PrintGrid(string pieceChar, string emptyChar)
    {
        for (int y = 0; y < _grid.GetLength(1); y++)
        {
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                Console.SetCursorPosition(x, y);
                Console.Write(emptyChar);
                if (_grid[x, y] == 1)
                    Console.Write(pieceChar);
            }
            Console.WriteLine();
        }
    }

    private void PlacePiece()
    {
        if (_currentPiece.Shape == null)
            return;
        
        for (int y = 0; y < _currentPiece.Shape.GetLength(1); y++)
        {
            for (int x = 0; x < _currentPiece.Shape.GetLength(0); x++)
            {
                if (_currentPiece.Shape[x, y] == 1)
                    _grid[_currentX + x, _currentY + y] = 1;
            }
        }

        _currentX = _grid.GetLength(0) / 2 - _currentPiece.Shape.GetLength(0) / 2;
        _currentY = 0;

        CheckLines();
        
        _currentPiece = _nextPiece;
        _nextPiece = new Piece();
    }

    // Check if a line is full
    private void CheckLines()
    {
        for (int y = 0; y < _grid.GetLength(1); y++)
        {
            bool full = true;
            
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                if (_grid[x, y] == 0)
                {
                    full = false;
                    break;
                }
            }

            if (full)
                ClearLine(y);
        }
    }

    private void ClearLine(int startY)
    {
        for (int x = 0; x < _grid.GetLength(0); x++)
            _grid[x, startY] = 0;
        
        for (int y = startY; y > 0; y--)
        {
            int[] tempLine = new int[_grid.GetLength(0)];
            for (int x = 0; x < _grid.GetLength(0); x++)
                tempLine[x] = _grid[x, y - 1];

            for (int x = 0; x < _grid.GetLength(0); x++)
                _grid[x, y] = tempLine[x];
        }
    }

    /// <summary>
    ///  Returns true if a collision, false if otherwise.
    /// </summary>
    private bool IsColliding(Piece piece, int currentX, int currentY)
    {
        if (piece.Shape == null)
            return true;
        
        for (int y = 0; y < piece.Shape.GetLength(1); y++)
        {
            for (int x = 0; x < piece.Shape.GetLength(0); x++)
            {
                int gridX = currentX + x;
                int gridY = currentY + y;

                if (gridX < 0 || gridX >= _grid.GetLength(0) ||
                    gridY < 0 || gridY >= _grid.GetLength(1))
                    continue;

                if (piece.Shape[x, y] == 1 && _grid[gridX, gridY] == 1)
                    return true;
            }
        }
        
        return false;
    }

    /// <summary>
    ///  Returns true if out of bounds, false if otherwise.
    /// </summary>
    private bool IsOutOfBounds(Piece piece, int currentX, int currentY)
    {
        if (piece.Shape == null)
            return true;
        
        return currentX < 0 || currentX + piece.Shape.GetLength(0) > _grid.GetLength(0) ||
               currentY < 0 || currentY + piece.Shape.GetLength(1) > _grid.GetLength(1);
    }

    private void PrintShapeOutline(string outlineChar)
    {
        Piece tempPiece = new Piece(_currentPiece.PieceType)
        {
            Shape = _currentPiece.Shape
        };

        int maxX = _currentX;
        int maxY = _currentY;

        while (!IsColliding(tempPiece, maxX, maxY + 1) &&
               !IsOutOfBounds(tempPiece, maxX, maxY + 1))
        {
            maxY++;
        }
        
        PrintPiece(tempPiece, maxX, maxY, outlineChar);
    }
}