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
            LeastMoves,
            MostMoves
        }

        private readonly int maxIterations;

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

        public BruteForceSolver(Board board, Mode mode = Mode.LeastMoves, int maxIterations = 1000)
        {
            originalBoard = board;

            this.solverMode = mode;
            this.maxIterations = maxIterations;

            if (mode == Mode.Random)
                rand = new Random();


            //Initialize Snapshots

            Board bd = board.Clone();
            BoardCleaner.CleanBoard(bd);

            var cell = board.GetAllChangeableCells().First();
            var snapshot = new Snapshot(bd, cell.Row, cell.Column, cell.GetPossibleNumbers(), 0);

            snapshots.Push(snapshot);

        }

        public int Iterations { get; private set; }

        public Board Solve()
        {
            Iterations = 0;
            Snapshot snapshot;
            Board board;

            while (true)
            {
                Iterations++;

                if (Iterations >= maxIterations)
                    throw new Exception("Board is most likely unsolvable, exceeded iteration limit: " + maxIterations);

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
                    IEnumerable<ChangeableCell> cells = board.GetAllChangeableCells();


                    switch (this.solverMode)
                    {
                        case Mode.LeastMoves:

                            cellToChange = cells.OrderBy(x => x.GetPossibleNumbers().Count).First();
                            
                            //if (cellToChange == null) not possible
                            break;
                        case Mode.MostMoves: //Worst
                            cellToChange = cells.OrderByDescending(x => x.GetPossibleNumbers().Count).First();

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
