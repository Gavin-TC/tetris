namespace Tetris;

public class Piece
{
    public enum Pieces
    {
        O,
        I,
        S,
        Z,
        L,
        J,
        T,
        PieceCount
    }
    private Pieces _pieceType;
    
    private int[,] _shape;

    public Piece(Pieces pieceType)
    {
        _pieceType = pieceType;
        _shape = new int[1, 1]
        {
            {1}
        };
        
        InitializePiece();
    }

    public Piece()
    {
        _pieceType = (Pieces) new Random().Next(0, (int)Pieces.PieceCount);
        _shape = new int[1, 1]
        {
            {1}
        };
        InitializePiece();
    }

    private void InitializePiece()
    {
        switch (_pieceType)
        {
            case Pieces.O:
                _shape = new int[2, 2]
                {
                    {1, 1},
                    {1, 1}
                };
                break;
            case Pieces.I:
                _shape = new int[4, 1]
                {
                    {1},
                    {1},
                    {1},
                    {1}
                };
                break;
            case Pieces.S:
                _shape = new int[3, 2]
                {
                    {1, 0},
                    {1, 1},
                    {0, 1}
                };
                break;
            case Pieces.Z:
                _shape = new int[3, 2]
                {
                    {0, 1},
                    {1, 1},
                    {1, 0}
                };
                break;
            case Pieces.L:
                _shape = new int[3, 2]
                {
                    {1, 1},
                    {0, 1},
                    {0, 1}
                };
                break;
            case Pieces.J:
                _shape = new int[3, 2]
                {
                    {0, 1},
                    {0, 1},
                    {1, 1}
                };
                break;
            case Pieces.T:
                _shape = new int[3, 2]
                {
                    {0, 1},
                    {1, 1},
                    {0, 1}
                };
                break;
        }
    }

    // Clockwise rotation
    public void RotateCw()
    {
        int rows = _shape.GetLength(0);
        int cols = _shape.GetLength(1);
        int[,] newShape = new int[cols, rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                newShape[j, rows - 1 - i] = _shape[i, j];
            }
        }

        _shape = newShape;
    }

    // Counter-clockwise rotation
    public void RotateCcw()
    {
        int rows = _shape.GetLength(0);
        int cols = _shape.GetLength(1);
        int[,] newShape = new int[cols, rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                newShape[cols - 1 - j, i] = _shape[i, j];
            }
        }

        _shape = newShape;
    }
    
    public Pieces PieceType => _pieceType;
    public int[,]? Shape
    {
        get => _shape;
        set => _shape = value;
    }
}