using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tetris;

namespace WpfApp1;

/// <summary>
/// Interaction logic for Game.xaml
/// </summary>
public partial class Game : Window
{
    private Piece _fallingPiece;
    private bool[,] _board;
    private int _height;
    private int _width;
    private DispatcherTimer _gravityTimer;
    private List<string> _pieces = new(){ "i","o","t","s","z","i","j" };
    private int _currentPiece = 0;

    public Game()
    {
        InitializeComponent();

        _width = 10;
        _height = 20;

        

        InitialiseBoard(_width, _height);
        ShuffleBag();
        NextPiece();
       

        _gravityTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(10000000),
            IsEnabled = true
        };
        _gravityTimer.Tick += delegate{ SoftDrop(); };
    }
    /// <summary>
    /// Set up board with columns and rows
    /// </summary>
    /// <param name="width"> Number of columns</param>
    /// <param name="height"> Number of rows </param>
    private void InitialiseBoard(int width, int height)
    {

        for (int i = 0; i < width; i++)
            GameGrid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(26)});
        

        for (int i = 0; i < height; i++)
            GameGrid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(26)});


        _board = new bool[height + 2, width + 2];
        for (int i = 0; i < width + 2; i++)
        {
            _board[height, i] = _board[height+1,i] = true;
        }

        for (int i = 0; i < height; i++)
        {
            _board[i, 0] = true;
            _board[i, width + 1] = true;
        }
    }

    /// <summary>
    /// Add image to top of grid
    /// </summary>
    /// <param name="name"> The name of the piece to add</param>
    private void PlacePiece(string name)
    {
        var piece = new Piece(name);

        Grid.SetColumn(piece,5-piece.ActualBlockWidth/2);
        Grid.SetRow(piece,0);
        Grid.SetColumnSpan(piece,piece.ActualBlockWidth);
        Grid.SetRowSpan(piece, piece.ActualBlockHeight);
        piece.Stretch = Stretch.Fill;

        GameGrid.Children.Add(piece);
        _fallingPiece = piece;
    }
    /// <summary>
    /// Allows for keyboard interaction
    /// </summary>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.A: ShiftLeft(); break;
            case Key.S: SoftDrop(); break;
            case Key.D: ShiftRight(); break;
            case Key.Q: RotateLeft(); break;
            case Key.E: RotateRight(); break;
            case Key.W: HardDrop(); break;
        }
    }
    /// <summary>
    /// Attempt to move piece 1 column right
    /// </summary>
    private void ShiftRight()
    {

        _fallingPiece.Column++;
        if (Piece.Collision(_board, _fallingPiece))
            _fallingPiece.Column--;
    }
    /// <summary>
    /// Attempt to move piece 1 column left
    /// </summary>
    private void ShiftLeft()
    {
        if (_fallingPiece.Column == 0) return;
        _fallingPiece.Column--;
        if (Piece.Collision(_board, _fallingPiece))
            _fallingPiece.Column++;

    }
    /// <summary>
    /// Attempt to rotate the piece left
    /// </summary>
    private void RotateLeft()
    {
        _fallingPiece.RotateLeft();
        
    }
    /// <summary>
    /// Attempt to rotate the piece right
    /// </summary>
    private void RotateRight()
    {
        _fallingPiece.RotateRight();
    }
    /// <summary>
    /// Attempt to drop the piece 1 row, and lock if necessary
    /// </summary>
    private void SoftDrop()
    {
        _fallingPiece.Row++;
        if (Piece.Collision(_board, _fallingPiece))
        {
            _fallingPiece.Row--;
            SetPiece();
        }
    }
    /// <summary>
    /// Update the board to show placed piece, and get next piece
    /// </summary>
    private void SetPiece()
    {
        for (int row = 0; row < _fallingPiece.BlockSize; row++)
        {
            for (int column = 0; column < _fallingPiece.BlockSize; column++)
            {
                _board[row + _fallingPiece.Row, column + _fallingPiece.Column] =
                    _board[row + _fallingPiece.Row, column + _fallingPiece.Column]
                    || _fallingPiece.ActualLayout[row, column];
            }
        }

        NextPiece();
    }
    /// <summary>
    /// Changes the order of pieces to be placed
    /// </summary>
    private void ShuffleBag()
    {
        _currentPiece = 0;
        var bag = _pieces.FindAll(x => true);
        var rand = new Random();
        var output = new List<string>();
        while (output.Count<7)
        {
            var num = rand.Next(bag.Count);
            output.Add(bag[num]);
            bag.RemoveAt(num);
        }

        _pieces = output;
    }
    /// <summary>
    /// Gets the next piece from the bag and places it
    /// </summary>
    private void NextPiece()
    {
        if (_currentPiece==7) ShuffleBag();
        var piece = _pieces[_currentPiece];
        _currentPiece++;
        PlacePiece(piece);
    }
    /// <summary>
    /// Repeatedly soft drop until piece is set
    /// </summary>
    private void HardDrop()
    {
        var temp = _currentPiece.GetHashCode();
        while (temp==_currentPiece.GetHashCode())
            SoftDrop();
    }
}