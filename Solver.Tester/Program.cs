using Solver.Engine;
using Solver.Engine.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver.Tester
{
    class Program
    {
        /// <summary>
        /// Reads a board from a text file and solves it.
        /// 
        /// x denotes empty spaces on the board.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Board board = new Board();

            //StreamReader sr = new StreamReader("Easy.txt");
            StreamReader sr = new StreamReader("Expert.txt");

            string line;

            for (int r = 0; r < 9; r++)
            {
                line = sr.ReadLine();

                for (int c = 0; c < 9; c++)
                {
                    char val = line[c];

                    if (val == 'x')
                        continue;

                    int i = Convert.ToInt32(val.ToString()) - 1; //Must convert to string otherwise, it returns the int value of the char
                    Numbers n = (Numbers)i;

                    board.SetCellValue(r, c, n);
                }
            }

            BruteForceSolver solver = new BruteForceSolver(board, BruteForceSolver.Mode.Random);
            board = solver.Solve();

            List<BruteForceSolver.Snapshot> steps = solver.GetSteps();
            
            PrintSolution(solver, board);

            Console.ReadLine();
        }

        private static void PrintSolution(BruteForceSolver solver, Board board)
        {
            for (int r = 0; r < 9; r++)
            {
                var cells = board.GetRow(r).ToList();

                for (int c = 0; c < 9; c++)
                {
                    Console.Write(((int)cells[c].Value + 1) + " ");

                    if (c / 3 != (c + 1) / 3)
                    {
                        Console.Write("| ");
                    }
                }

                if (r / 3 != (r + 1) / 3)
                {
                    Console.WriteLine();
                }

                Console.WriteLine();
            }

            Console.WriteLine();
            Console.WriteLine("Number of iterations: " + solver.Iterations);
            Console.WriteLine("Steps");


            foreach (var s in solver.GetSteps())
            {
                string tmp = String.Format("Row (1-based): {0}, Column: {1}, Value: {2}", s.RowChanged + 1, s.ColumnChanged + 1, s.PossibleNumbers[s.IndexToTry]);
                Console.WriteLine(tmp);
            }
        }
    }   
}
