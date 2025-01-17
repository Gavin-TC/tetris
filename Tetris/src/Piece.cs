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
    
    private int[,]? _shape;

    public Piece(Pieces pieceType)
    {
        _pieceType = pieceType;
        
        InitializePiece();
    }

    public Piece()
    {
        _pieceType = (Pieces) new Random().Next(0, (int)Pieces.PieceCount);
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
                _shape = new int[2, 3]
                {
                    {0, 1, 1},
                    {1, 1, 0}
                };
                break;
            case Pieces.Z:
                _shape = new int[2, 3]
                {
                    {1, 1, 0},
                    {0, 1, 1}
                };
                break;
            case Pieces.L:
                _shape = new int[3, 2]
                {
                    {1, 0},
                    {1, 0},
                    {1, 1}
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
                _shape = new int[2, 3]
                {
                    {1, 1, 1},
                    {0, 1, 0}
                };
                break;
        }
    }

    /// Clockwise rotation
    public void RotateCw()
    {
        int[,] newShape = new int[_shape.GetLength(1), _shape.GetLength(0)];

        for (int i = 0; i < _shape.GetLength(0); i++)
        {
            for (int j = 0; j < _shape.GetLength(1); j++)
            {
                newShape[j, _shape.GetLength(0) - 1 - i] = _shape[i, j];
            }
        }

        _shape = newShape;
    }

    /// Counter-clockwise rotation
    public void RotateCcw()
    {
        int[,] newShape = new int[_shape.GetLength(1), _shape.GetLength(0)];

        for (int i = 0; i < _shape.GetLength(0); i++)
        {
            for (int j = 0; j < _shape.GetLength(1); j++)
            {
                newShape[_shape.GetLength(1) - 1 - j, i] = _shape[i, j];
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