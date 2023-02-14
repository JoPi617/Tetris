using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Accessibility;

namespace Tetris;
/// <summary>
/// Inherits from Image, adding properties and methods for pieces
/// </summary>
internal class Piece : Image
{
    #region Properties
    /// <summary>
    /// Gets the default, un-rotated layout of the piece
    /// </summary>
    public bool[,] Layout { get; }
    /// <summary>
    /// Gets the name (letter) of the piece
    /// </summary>
    public string PieceName { get; }
    /// <summary>
    /// Gets/ sets a int representing rotation 90 degrees clockwise
    /// </summary>
    public int Rotation { get; private set; }
    /// <summary>
    /// Gets the size of the piece in blocks
    /// </summary>
    public int BlockSize => Layout.GetLength(0);
    /// <summary>
    /// Gets the layout of the block in it's rotation
    /// </summary>
    public bool[,] ActualLayout
    {
        get
        {
            var rotate = Rotation;
            var output = Layout.Clone() as bool[,];
            if (PieceName == "o") return Layout;
            if (PieceName == "i")
                switch (rotate)
                {
                    case 0:
                        break;
                    case 1:
                        rotate += 2;
                        break;
                    case 2:
                        break;
                    case 3:
                        rotate -= 2;
                        break;
                    default:
                        throw new ArgumentException("Invalid rotation");
                }
            bool[,] Transpose(bool[,] input)
            {
                var output = new bool[BlockSize,BlockSize];
                for (int row = 0; row < BlockSize; row++)
                {
                    for (int column = 0; column < BlockSize; column++)
                    {
                        output[row,column] = input[column, row];
                    }
                }
                return output; 
            }

            bool[,] Flip(bool[,] input)
            {
                var output = new bool[BlockSize,BlockSize];
                for (int row = 0; row < BlockSize; row++)
                {
                    for (int column = 0; column < BlockSize; column++)
                    {
                        output[row,column] = input[row, BlockSize-column-1];
                    }
                }
                return output;
            }
            for (int i = 0; i < rotate; i++)
            {
                output = Transpose(output ?? throw new InvalidOperationException());
                output = Flip(output);
            }

            return output;
        }
    }

    /// <summary>
    /// The X of the centre of rotation for the piece, in pixels
    /// </summary>
    public double CentreX { get; }
    /// <summary>
    /// The Y of the centre of rotation for the piece, in pixels
    /// </summary>
    public double CentreY { get; }

    /// <summary>
    /// The Column position of the piece in the grid
    /// </summary>
    public int Column
    {
        get=> Grid.GetColumn(this);
        set=> Grid.SetColumn(this, value);
    }
    /// <summary>
    /// The Row position of the piece in the grid
    /// </summary>
    public int Row
    {
        get => Grid.GetRow(this);
        set
        {
            Grid.SetRow(this, value);
            Stopped = 0;
        }
    }
    /// <summary>
    /// The number of times the piece has stopped
    /// </summary>
    public int Stopped { get; set; }
    /// <summary>
    /// The number of cells high the rotated piece occupies
    /// </summary>
    public int ActualBlockHeight => Rotation % 2 == 0 ? height : width;

    /// <summary>
    /// The number of cells wide the piece occupies
    /// </summary>
    public int ActualBlockWidth => Rotation % 2 == 0 ? width : height;
    #endregion
    #region Private fields

    private readonly int width;
    private readonly int height;
    #endregion
    #region Constructors
    public Piece(string pieceName)
    {
        Rotation = 0;
        PieceName = pieceName;
        switch (pieceName.ToLower())
        {
            case "j":
                Layout = new[,]
                {
                    { true, false, false },
                    { true, true, true },
                    { false, false, false }
                };
                break;
            case "l":
                Layout = new[,]
                {
                    { false, false, true },
                    { true, true, true },
                    { false, false, false }
                };
                break;
            case "t":
                Layout = new[,]
                {
                    { false, true, false },
                    { true, true, true },
                    { false, false, false }
                };
                break;
            case "i":
                Layout = new[,]
                {
                    { false, false, false, false },
                    { true, true, true, true },
                    { false, false, false, false },
                    { false, false, false, false }
                };
                break;
            case "o":
                Layout = new[,]
                {
                    { false, true, true, false },
                    { false, true, true, false },
                    { false, false, false, false },
                    { false, false, false, false }
                };
                break;
            case "s":
                Layout = new[,]
                {
                    { false, true, true },
                    { true, true, false },
                    { false, false, false }
                };
                break;
            case "z":
                Layout = new[,]
                {
                    { true, true, false },
                    { false, true, true },
                    { false, false, false }
                };
                break;
            default:
                throw new ArgumentException("Invalid Piece");
        }

        if (pieceName is "s" or "z" or "j" or "l" or "t")
        {
            CentreX = CentreY = _minoSize * 1.5;
            width = 3;
            height = 2;
        }
        else if (pieceName == "o")
        {
            CentreX = _minoSize * 2;
            CentreY = _minoSize;
            width = height = 2;
        }
        else
        {
            CentreY = CentreX = 2 * _minoSize;
            width = 4;
            height = 1;
        }

        Source = new BitmapImage(new Uri(@"C:\Users\jhp33\source\repos\School\Tetris\WpfApp1\WpfApp1\Resources\Tetrominoes\" + PieceName + ".png"));
    }
    #endregion
    
    #region Public methods
    public void RotateRight()
    {
        Rotation++;
        if (Rotation == 4) Rotation = 0;

        var transform = new TransformGroup();
        transform.Children.Add(new RotateTransform(90,CentreX,CentreY));
        transform.Children.Add(RenderTransform);
        RenderTransform = transform;
        Grid.SetRowSpan(this,ActualBlockWidth);
        Grid.SetColumnSpan(this,ActualBlockHeight);
    }

    public void RotateLeft()
    {
        Rotation--;
        if (Rotation == -1) Rotation = 3;

        var transform = new TransformGroup();
        transform.Children.Add(new RotateTransform(-90,CentreX,CentreY));
        transform.Children.Add(RenderTransform);
        RenderTransform = transform;
        Grid.SetRowSpan(this,ActualBlockWidth);
        Grid.SetColumnSpan(this,ActualBlockHeight);
    }

    #endregion

    #region Public static methods
    public static bool Collision(bool[,] board, Piece piece)
    {
        var x = piece.Column;
        var y = piece.Row;
        for (int row = 0; row < piece.BlockSize; row++)
        {
            for (int column = 0; column < piece.BlockSize; column++)
            {
                if (piece.ActualLayout[row, column] && board[row + y, column + x+1])
                    return true;
            }
        }
        return false;
    }
    #endregion
}