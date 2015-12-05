using Solver.Engine;
using Solver.Engine.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Solver.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// Quick and simple GUI for the API (so did not use MVVM... WIP)
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            GenerateGrid();
        }

        private void GenerateGrid()
        {
            for (int i = 0; i < 9; i++)
            {
                var cd = new ColumnDefinition() { Width = GridLength.Auto };

                InputGrid.ColumnDefinitions.Add(cd);

                var rd = new RowDefinition() { Height = GridLength.Auto };
                InputGrid.RowDefinitions.Add(rd);


            }

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox tx = MakeTextBox(r, c);

                    InputGrid.Children.Add(tx);

                    Grid.SetColumn(tx, c);
                    Grid.SetRow(tx, r);

                }
            }
        }


        private TextBox MakeTextBox(int r, int c)
        {

            TextBox tx = new TextBox();

            //Setting these overrides the ResourceDictionary values so did not use the GenericDictionary in this App.

            tx.MaxLength = 1;
            tx.HorizontalContentAlignment = HorizontalAlignment.Center;
            tx.Width = tx.Height = 30;
            tx.Tag = r + "-" + c;
            tx.TabIndex = GetIndex(r, c);
            tx.Padding = new Thickness(5);

            tx.GotFocus += TextBox_GotFocus;
            tx.TextChanged += TextBox_TextChanged;
            tx.HorizontalAlignment = HorizontalAlignment.Right;
            tx.VerticalAlignment = VerticalAlignment.Bottom;
            tx.Margin = new Thickness(5);

            if (c % 3 == 0)
                tx.Margin = new Thickness(tx.Margin.Left + 10, tx.Margin.Top, tx.Margin.Right, tx.Margin.Bottom);

            if (r % 3 == 0)
                tx.Margin = new Thickness(tx.Margin.Left, tx.Margin.Top + 10, tx.Margin.Right, tx.Margin.Bottom);

            return tx;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;

            if (tb.Text.Length == 1)
            { 
                int next = InputGrid.Children.IndexOf(tb) + 1;
                
                if (next < 81)
                {
                    InputGrid.Children[next].Focus();
                }
            }
        }

        
        private static int GetIndex(int r, int c)
        {
            return r * 9 + c;
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void ResetBoardButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (TextBox tx in InputGrid.Children)
            {
                tx.Text = "";
            }

            InputGrid.IsEnabled = true;
        }

        private void SolveButton_Click(object sender, RoutedEventArgs e)
        {
            Board board = new Board();

            //Just iterate through the children

            int ctr = 0;

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox tb = InputGrid.Children[ctr] as TextBox;
                    if (tb == null)
                        throw new Exception("Not possible?");

                    if (!String.IsNullOrWhiteSpace(tb.Text))
                    {
                        try
                        {
                            int i = Convert.ToInt32(tb.Text);

                            if (i < 1 || i > 9)
                                throw new Exception("Must be between 1-9");

                            board.SetCellValue(r, c, (Numbers)(i - 1));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(tb.Tag + ": " + ex.Message);
                        }
                    }

                    ctr++;
                }
            }

            BruteForceSolver solver = new BruteForceSolver(board);

            Board solvedBoard = solver.Solve();

            ctr = 0;

            
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox tb = InputGrid.Children[ctr] as TextBox;
                    if (tb == null)
                        throw new Exception("Not possible?");

                    tb.Text = ((int)solvedBoard.GetCell(r, c).Value + 1).ToString();

                    ctr++;
                }
            }

            InputGrid.IsEnabled = false;
        }
    }
}
