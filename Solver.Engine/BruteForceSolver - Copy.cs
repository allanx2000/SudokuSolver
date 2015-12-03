using Solver.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver.Engine
{
    /// <summary>
    /// The uses brute force/trial and error.
    /// 
    /// If a solution ends up failing, it will back track and try something else.
    /// </summary>
    public class BruteForceSolver
    {
        /// <summary>
        /// Holds the Snapshot state for restoring. This also contains the moves used to solve the Board
        /// </summary>
        class Snapshot
        {
            public bool IsOriginal
            {
                get
                {
                    return RowChanged == -1;
                }
            }
            public int IndexTried { get; private set; }
            public IList<Numbers> PossibleNumbers { get; private set; }

            public int RowChanged { get; private set; }
            public int ColumnChanged { get; private set; }

            public Board Board { get; private set; }

            public static Snapshot SnapshotForOriginalClone(Board board)
            {
                return new Snapshot(board, -1, -1, new List<Numbers>(), -1);
            }

            public Snapshot(Board board, int rowChanged, int columnChanged, List<Numbers> possibleValues, int tried)
            {
                Board = board;
                RowChanged = rowChanged;
                ColumnChanged = columnChanged;
                IndexTried = tried;
                PossibleNumbers = possibleValues.AsReadOnly();
            }

            public void IncrementIndexTried()
            {
                IndexTried++;
            }
        }

        private Stack<Snapshot> snapshots = new Stack<Snapshot>();
        private Board originalBoard;
        public BruteForceSolver(Board board)
        {
            originalBoard = board;

            var cell = board.GetAllChangeableCells().First();

            var snapshot = new Snapshot(board.Clone(), cell.Row, cell.Column, cell.GetPossibleNumbers(), -1);

            //Initialize Snapshot
            snapshots.Push(snapshot);

        }

        public Board Solve()
        {
            Snapshot snapshot;
            Board board;

            while (true)
            {
                snapshot = snapshots.Peek();

                if (snapshot.Board.Solved) //If already solved then return it
                    return snapshot.Board;
                

                board = snapshot.Board.Clone(); //Make a copy for trial and error
                int tried = snapshot.IndexTried;
                                
                if (tried < snapshot.PossibleNumbers.Count) //Get next Number to try and try it
                {
                    IEnumerable<ChangeableCell> cells = null;

                    for (int i = 1; i < 9; i++)
                    {
                        cells = board.GetAllChangeableCells().Where(x => x.GetPossibleNumbers().Count == i);

                        //cells = board.GetAllChangeableCells().Where(x => x.GetPossibleNumbers().Count == 10 - i);

                        if (cells.Count() > 0)
                            break;
                    }

                    ChangeableCell cellToChange = cells.First();

                    bool found = false;
                    var pv = cellToChange.GetPossibleNumbers();

                    //Try to find a valid one
                    for (int i = 0; i < pv.Count; i++)
                    {
                        var b2 = board.Clone();
                        b2.SetCellValue(cellToChange.Row, cellToChange.Column, pv[i]);
                        BoardCleaner.CleanBoard(b2);

                        if (b2.IsValid) //Note: Valid but may not be the correct one depending on previous choices
                        {
                            Snapshot sn = new Snapshot(b2, cellToChange.Row, cellToChange.Column, pv, i);
                            snapshots.Push(sn);
                            board = b2;
                            found = true;
                            break;
                        }
                    }

                    if (!found) //Try the next value next time
                    {
                        snapshots.Peek().IncrementIndexTried();
                        
                    }
                }                
                else //reached end so something went wrong in the last state so go back
                {
                    snapshots.Pop();

                    if (snapshots.Count == 1)
                    {
                        throw new Exception("Something wrong");
                    }

                    snapshots.Peek().IncrementIndexTried();
                }
            }
        }
    }
}
