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
        private static Random rand;

        public enum Mode
        {
            Random,
            LeastMoves
        }

        /// <summary>
        /// Holds the Snapshot state for restoring. This also contains the moves used to solve the Board
        /// </summary>
        public class Snapshot
        {
            /// <summary>
            /// The next index to try
            /// 
            /// However if it is Solved though... the board will be updated with the solution.
            /// </summary>
            public int IndexToTry { get; private set; }

            public IList<Numbers> PossibleNumbers { get; private set; }

            //Row/Col identifier
            public int RowChanged { get; private set; }
            public int ColumnChanged { get; private set; }

            /// <summary>
            /// The state before the index is tried
            /// </summary>
            public Board Board { get; private set; }
            
            public Snapshot(Board board, int rowChanged, int columnChanged, List<Numbers> possibleValues, int indexToTry)
            {
                Board = board;
                RowChanged = rowChanged;
                ColumnChanged = columnChanged;
                IndexToTry = indexToTry;
                PossibleNumbers = possibleValues.AsReadOnly();
            }

            public void IncrementIndexToTry()
            {
                IndexToTry++;
            }

            /// <summary>
            /// Sets the board; used when board becomes solved
            /// TODO: Maybe should just return the board when solved... snapshot only holds unfinished states?
            /// </summary>
            /// <param name="board"></param>
            public void SetBoard(Board board)
            {
                this.Board = board;
            }
        }

        public List<Snapshot> GetSteps()
        {
            var sn = snapshots.ToList();
            sn.Reverse();

            return sn;
        }

        private Stack<Snapshot> snapshots = new Stack<Snapshot>();
        private Board originalBoard;
        private Mode solverMode;

        public BruteForceSolver(Board board, Mode mode = Mode.LeastMoves)
        {
            originalBoard = board;
            this.solverMode = mode;

            if (mode == Mode.Random)
                rand = new Random();


            //Initialize Snapshots

            var cell = board.GetAllChangeableCells().First();
            var snapshot = new Snapshot(board.Clone(), cell.Row, cell.Column, cell.GetPossibleNumbers(), 0);
            snapshots.Push(snapshot);

        }

        public int Iterations = 0;

        public Board Solve()
        {
            Snapshot snapshot;
            Board board;

            while (true)
            {
                Iterations++;

                snapshot = snapshots.Peek();

                if (snapshot.Board.Solved) //If already solved then return it
                    return snapshot.Board;


                board = snapshot.Board.Clone(); //Make a copy for trial and error
                int toTry = snapshot.IndexToTry;

                if (toTry < snapshot.PossibleNumbers.Count) //Get next Number to try and try it
                {
                    //Evaluate the current snapshot

                    Numbers n = snapshot.PossibleNumbers[toTry];

                    board.SetCellValue(snapshot.RowChanged, snapshot.ColumnChanged, n);
                    BoardCleaner.CleanBoard(board);

                    if (!board.IsValid)
                    {
                        snapshots.Peek().IncrementIndexToTry();
                        continue; //Try again
                    }

                    if (board.Solved) //Setting it to this solved the whole board
                    {
                        return board;
                        //snapshots.Peek().SetBoard(board);
                        //snapshots.Push(new Snapshot(board, snapshot.RowChanged, snapshot.ColumnChanged, snapshot.PossibleNumbers.ToList(), toTry));
                        //break;
                    }

                    ChangeableCell cellToChange = null;

                    switch (this.solverMode)
                    {
                        case Mode.LeastMoves:
                            //Ordered
                            IEnumerable<ChangeableCell> cells = null;

                            for (int i = 1; i < 9; i++)
                            {
                                cells = board.GetAllChangeableCells().Where(x => x.GetPossibleNumbers().Count == i);

                                //cells = board.GetAllChangeableCells().Where(x => x.GetPossibleNumbers().Count == 10 - i);

                                if (cells.Count() > 0)
                                {
                                    cellToChange = cells.First();
                                    break;
                                }
                            }

                            //if (cellToChange == null) not possible
                            
                            break;
                        case Mode.Random:
                            cellToChange = board.GetAllChangeableCells().ElementAt(rand.Next(board.ChangeableCellsLeft));
                            break;
                    }
                     
                    var pv = cellToChange.GetPossibleNumbers();

                    Snapshot sn = new Snapshot(board.Clone(), cellToChange.Row, cellToChange.Column, pv, 0);
                    snapshots.Push(sn);
                }
                else //reached end so something went wrong in the last state so go back
                {
                    if (snapshots.Count == 1)
                    {
                        throw new Exception("The Board is unsolvable.");
                    }

                    snapshots.Pop();

                    snapshots.Peek().IncrementIndexToTry();
                }
            }

            return snapshots.Peek().Board;
        }

    }
}
