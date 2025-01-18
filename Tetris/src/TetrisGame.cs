using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Tetris;

public class TetrisGame
{
    private bool _displayFps = false; // Whether to display the fps on the screen
    private bool _gameRunning;
    private int _desiredFps;
    private float _desiredUps;

    private int _gameOver;

    private int _score;
    private int _level = 0;
    private int _linesCleared;
    private int _combo;

    private Dictionary<int, int> _linesClearedPoints = new Dictionary<int, int>();
    private Dictionary<int, int> _tSpinPoints = new Dictionary<int, int>();

    private int _consecutiveTSpin = 0;
    private int _consecutiveLines = 0; // How many lines cleared consecutively
 
    private Piece _currentPiece;
    private Piece _nextPiece;
    
    private int[,] _grid = new int[10, 20];

    private int _currentX = 0;
    private int _currentY = 0;

    private bool _updateBusy = false;

    public TetrisGame(int desiredFps)
    {
        _gameRunning = true;
        _desiredFps = desiredFps;
        _desiredUps = 1;
        
        _linesClearedPoints.Add(1, 100);
        _linesClearedPoints.Add(2, 300);
        _linesClearedPoints.Add(3, 500);
        _linesClearedPoints.Add(4, 800);
    
        _tSpinPoints.Add(1, 100);
        _tSpinPoints.Add(2, 400);
        _tSpinPoints.Add(3, 700);
        _tSpinPoints.Add(4, 1200);

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
                // Stop default updating if it's being used
                if (!_updateBusy)
                    Update();

                uDeltaTime -= uOptimalTime;
                updates += 1;
            }

            if (fDeltaTime >= fOptimalTime)
            {
                Render();

                fDeltaTime -= fOptimalTime;
                frames += 1;
            }

            Console.CursorVisible = false;
            frameCount++;
        }
    }
    
    private void HandleControls() 
    {
        while (Console.KeyAvailable)
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
                    
                    _score++;
                    _updateBusy = true;
                    Update(true); // Make a forced update to move the piece down, so pieces don't move through each other
                    _updateBusy = false;
                    break;
                
                // Hard drop
                case ConsoleKey.Spacebar:
                    int tilesMoved = 0;
                    
                    _updateBusy = true;
                    while (!IsColliding(_currentPiece, _currentX, _currentY + 1) &&
                           !IsOutOfBounds(_currentPiece, _currentX, _currentY + 1))
                    {
                        Update(true);
                        tilesMoved++;

                        if (IsColliding(_currentPiece, _currentX, _currentY + 1))
                        {
                            _updateBusy = false;
                            break;
                        }
                    }
                    _score += tilesMoved * 2;
                    _updateBusy = false;
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

        if (!IsOutOfBounds(_currentPiece, _currentX, _currentY + 1))
            _currentY++;

        // If this is not a forced update
        // if (!forced && !IsOutOfBounds(_currentPiece, _currentX, _currentY))
        //         _currentY++;
    }

    private void Render()
    {
        int offsetX = 10;
        int offsetY = 1;

        if (_gameOver)
        {
           // Print something here 
        }
        
        PrintUi(offsetX, offsetY);
        PrintGrid("#", " ", offsetX, offsetY);
        PrintShapeOutline(".", offsetX, offsetY);
        PrintPiece(_currentPiece, _currentX, _currentY, "#", offsetX, offsetY);
        
    }

    private void PrintPiece(Piece piece, int currentX, int currentY, string pieceChar, int offsetX, int offsetY)
    {
        if (piece.Shape == null)
            return;
        
        for (int y = 0; y < piece.Shape.GetLength(1); y++)
        {
            for (int x = 0; x < piece.Shape.GetLength(0); x++)
            {
                Console.SetCursorPosition(currentX + x + offsetX, currentY + y + offsetY);
                if (piece.Shape[x, y] == 1)
                    Console.Write(pieceChar);
            }
            Console.WriteLine();
        }
    }

    private void PrintGrid(string pieceChar, string emptyChar, int offsetX, int offsetY)
    {
        for (int y = 0; y < _grid.GetLength(1); y++)
        {
            for (int x = 0; x < _grid.GetLength(0); x++)
            {
                Console.SetCursorPosition(x + offsetX, y + offsetY);
                if (_grid[x, y] == 1)
                    Console.Write(pieceChar);
                else
                    Console.Write(emptyChar);
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
        int clearedThisPass = 0;
        int basePoints;
        
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
            {
                ClearLine(y);
                clearedThisPass++;
            }
        }
        if (clearedThisPass == 0)
        {
            _consecutiveLines = 0;
            return;
        }
        _consecutiveLines++;
        
        basePoints = _linesClearedPoints[clearedThisPass];
        if (_currentPiece.PieceType is Piece.Pieces.T)
            basePoints = _tSpinPoints[clearedThisPass];
        
        if (_consecutiveLines > 1)
            _score += (int) (basePoints * 1.5) * (_level + 1);
        else
            _score += basePoints * (_level + 1);
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

        _linesCleared++;
        if (_linesCleared % 10 == 0)
            _level++;
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

    private void PrintShapeOutline(string outlineChar, int offsetX, int offsetY)
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
        
        PrintPiece(tempPiece, maxX, maxY, outlineChar, offsetX, offsetY);
    }

    /// <summary>
    /// Prints thing like score, next piece, etc.
    /// </summary>
    private void PrintUi(int gameOffsetX, int gameOffsetY)
    {
        PrintGameStats(gameOffsetX, gameOffsetY);
        PrintBorders(gameOffsetX, gameOffsetY);
        PrintNextPiece(gameOffsetX, gameOffsetY);
    }

    private void PrintBorders(int gameOffsetX, int gameOffsetY)
    {
        Console.SetCursorPosition(0, 0);
        
        Console.Write("/");
        
        // Top line
        for (int i = 0; i < gameOffsetX * 3; i++)
            Console.Write("-");
        Console.Write("\\");

        // Left border
        for (int i = 1; i < 21; i++)
        {
            Console.SetCursorPosition(0, i);
            Console.Write("|");
        }
        Console.Write("\n\\");
        
        // Right border
        for (int i = 1; i < 21; i++)
        {
            Console.SetCursorPosition(gameOffsetX * 3, i);
            Console.Write("|");
        }
        
        // Left inside border
        for (int i = 1; i < 21; i++)
        {
            Console.SetCursorPosition(gameOffsetX - 1, i);
            Console.Write("|");
        }
        
        // Right inside border
        for (int i = 1; i < 21; i++)
        {
            Console.SetCursorPosition(gameOffsetX * 2, i);
            Console.Write("|");
        }
        
        Console.SetCursorPosition(1, 21);
        
        // Bottom line
        for (int i = 0; i < gameOffsetX * 3; i++)
            Console.Write("-");
        Console.Write("/");
    }

    private void PrintGameStats(int offsetX, int offsetY)
    {
        Console.SetCursorPosition(2, 2);
        Console.Write("Score:\n " + _score);
        
        Console.SetCursorPosition(2, 6);
        Console.Write("Level:\n " + _level);
        
        Console.SetCursorPosition(2, 10);
        Console.Write("Lines:\n " + _linesCleared);
    }

    private void PrintNextPiece(int gameOffsetX, int gameOffsetY)
    {
        Console.SetCursorPosition(gameOffsetX * 2 + 3, gameOffsetY * 2);
        Console.Write("Next:\n");

        if (_nextPiece.Shape == null)
        {
            Console.Write("ERROR!");
            return;
        }

        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 6; x++)
            {
                Console.SetCursorPosition(x + gameOffsetX * 2 + 3, y + gameOffsetY * 2 + 1);
                Console.Write(" ");
            }
            Console.WriteLine();
        }

        for (int y = 0; y < _nextPiece.Shape.GetLength(1); y++)
        {
            for (int x = 0; x < _nextPiece.Shape.GetLength(0); x++)
            {
                Console.SetCursorPosition(x + gameOffsetX * 2 + 3, y + gameOffsetY * 2 + 1);
                
                if (_nextPiece.Shape[x, y] == 1)
                    Console.Write(_nextPiece.Shape[x, y]);
                else
                    Console.Write(" ");
            }
            Console.WriteLine();
        }
    }
}