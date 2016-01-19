using Innouvous.Utils;
using Innouvous.Utils.MVVM;
using Solver.Engine;
using Solver.Engine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Solver.GUI
{
    public class MainWindowViewModel : ViewModel
    {
        private readonly Grid inputGrid;

        //Solution State
        private Board originalBoard;
        private BruteForceSolver currentSolver;
        private Board solvedBoard;

        private const char Empty = 'x'; //For Load/Save Serialization 

        public MainWindowViewModel(Grid inputGrid)
        {
            this.inputGrid = inputGrid;

            GenerateGrid();

        }

        #region Properties

        private bool gridEnabled = true;

        public bool GridEnabled
        {
            get
            {
                return gridEnabled;
            }
            set
            {
                gridEnabled = value;

                RaisePropertyChanged("GridEnabled");
                RaisePropertyChanged("SolveEnabled");
                RaisePropertyChanged("RevertEnabled");
            }
        }

        public bool SolveEnabled
        {
            get
            {
                return gridEnabled;
            }
        }

        public bool RevertEnabled
        {
            get
            {
                return !gridEnabled;
            }
        }


        public int? Iterations
        {
            get
            {
                if (currentSolver == null)
                    return null;
                else
                    return currentSolver.Iterations;
            }
        }

        #endregion

        #region Commands

        public ICommand SaveCommand
        {
            get
            {
                return new CommandHelper(Save);
            }
        }

        private void Save()
        {
            try
            {
                var sfd = DialogsUtility.CreateSaveFileDialog("Save Board");
                DialogsUtility.AddExtension(sfd, "Sudoku Solver Board", "*.ssb");

                sfd.ShowDialog();

                if (String.IsNullOrEmpty(sfd.FileName))
                    return;

                StreamWriter sw = new StreamWriter(sfd.FileName);

                foreach (TextBox tb in inputGrid.Children)
                {
                    if (String.IsNullOrEmpty(tb.Text))
                        sw.Write(Empty);
                    else
                        sw.Write(tb.Text);
                }

                sw.Close();
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e, "Problem Saving");
            }
        }

        public ICommand LoadCommand
        {
            get
            {
                return new CommandHelper(Load);
            }
        }

        private void Load()
        {
            try
            {
                var ofd = DialogsUtility.CreateOpenFileDialog("Load Board");
                DialogsUtility.AddExtension(ofd, "Sudoku Solver Board", "*.ssb");

                ofd.ShowDialog();

                if (String.IsNullOrEmpty(ofd.FileName))
                    return;

                StreamReader sr = new StreamReader(ofd.FileName);
                string data = sr.ReadLine();
                sr.Close();

                int ctr = 0;

                foreach (TextBox tb in inputGrid.Children)
                {
                    char c = data[ctr++];

                    if (c != Empty)
                        tb.Text = c.ToString();
                    else
                        tb.Text = null;
                }
            }
            catch (Exception e)
            {
                MessageBoxFactory.ShowError(e, "Problem Loading");
            }
        }

        public ICommand SolveCommand
        {
            get
            {
                return new CommandHelper(Solve);
            }
        }


        private void Solve()
        {
            try
            {
                originalBoard = CreateBoard();

                currentSolver = new BruteForceSolver(originalBoard, BruteForceSolver.Mode.LeastMoves);

                var sb = currentSolver.Solve();
                SetSolvedBoard(sb);
                OutputToInputGrid(sb);

                GridEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBoxFactory.ShowError(ex.Message);
            }
        }

        public ICommand RevertCommand
        {
            get
            {
                return new CommandHelper(Revert);
            }
        }

        private void Revert()
        {
            if (originalBoard != null)
            {
                SetSolvedBoard(null);
             
                OutputToInputGrid(originalBoard);
                GridEnabled = true;
            }
        }

        public ICommand ResetCommand
        {
            get
            {
                return new CommandHelper(Reset);
            }
        }
        private void Reset()
        {
            foreach (TextBox tx in inputGrid.Children)
            {
                tx.Text = "";
            }

            GridEnabled = true;
            originalBoard = null;

            SetSolvedBoard(null);
        }


        #endregion

        #region Generate Grid
        private void GenerateGrid()
        {
            for (int i = 0; i < 9; i++)
            {
                var cd = new ColumnDefinition() { Width = GridLength.Auto };

                inputGrid.ColumnDefinitions.Add(cd);

                var rd = new RowDefinition() { Height = GridLength.Auto };
                inputGrid.RowDefinitions.Add(rd);


            }

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox tx = MakeTextBox(r, c);

                    inputGrid.Children.Add(tx);

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
                int next = inputGrid.Children.IndexOf(tb) + 1;

                if (next < 81)
                {
                    inputGrid.Children[next].Focus();
                }
            }
        }


        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        #endregion

        private static int GetIndex(int r, int c)
        {
            return r * 9 + c;
        }


        private void OutputToInputGrid(Board board)
        {
            int ctr = 0;

            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox tb = inputGrid.Children[ctr] as TextBox;
                    if (tb == null)
                        throw new Exception("Not possible?");

                    var cell = board.GetCell(r, c);
                    if (cell.Value == Numbers.None)
                        tb.Text = null;
                    else
                        tb.Text = ((int)cell.Value + 1).ToString();

                    ctr++;
                }
            }
        }


        private Board CreateBoard()
        {
            Board board = new Board();

            int ctr = 0;
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    TextBox tb = inputGrid.Children[ctr] as TextBox;
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
                        catch (Exception e)
                        {
                            throw new Exception(tb.Tag + ": " + e.Message, e);
                        }
                    }

                    ctr++;
                }
            }

            return board;
        }
        
        private void SetSolvedBoard(Board board)
        {
            solvedBoard = board;

            if (solvedBoard == null)
                currentSolver = null;

            RaisePropertyChanged("Iterations");
        }

    }
}
