using Solver.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver.Tester
{
    public class BruteForceSolver
    {
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
            snapshots.Push(Snapshot.SnapshotForOriginalClone(board.Clone()));

        }

        public Board Solve()
        {
            Snapshot snapshot; //= snapshots.Peek();
            Board board; // = snapshot.Board.Clone();


            /*if (twoValues.Count() == 0)
                throw new Exception("Cannot solve"); //Ideally just pick a random and keep working
            */

            //First Try
            board = originalBoard.Clone();

            int tried = 0;

            do
            {

                snapshot = snapshots.Peek();
                board = snapshot.Board.Clone();
                tried = snapshot.IndexTried;

                ChangeableCell cellToChange;

                if (snapshot.IsOriginal || tried < snapshot.PossibleNumbers.Count) //Get Next
                {
                    IEnumerable<ChangeableCell> cells = null;

                    for (int i = 0; i < 9; i++)
                    {
                        cells = board.GetAllChangeableCells().Where(x => x.GetPossibleNumbers().Count == i);

                        if (cells.Count() > 0)
                            break;
                    }

                    cellToChange = cells.First();


                    bool found = false;
                    var pv = cellToChange.GetPossibleNumbers();

                    for (int i = 0; i < pv.Count; i++)
                    {
                        var b2 = board.Clone();
                        b2.SetCellValue(cellToChange.Row, cellToChange.Column, pv[i]);
                        BoardCleaner.CleanBoard(b2);
                        if (b2.IsValid) //Valid but may not be final solution
                        {
                            Snapshot sn = new Snapshot(b2, cellToChange.Row, cellToChange.Column, pv, i);
                            snapshots.Push(sn);
                            board = b2;
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        snapshots.Peek().IncrementIndexTried();
                    }

                    continue;
                }
                else //Not original and reached end
                {
                    snapshots.Pop();
                    snapshots.Peek().IncrementIndexTried();
                }
            
            }
            while (!board.Solved);

            return snapshots.Peek().Board;
        }
    }
}
