using Solver.Engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver.Engine
{
    public static class BoardCleaner
    {
        public static bool CleanBoard(Board board)
        {
            bool changed;

            int iterations = 0;

            do
            {
                changed = false;

                bool tmp = SetSingles(board);
                if (tmp)
                    changed = tmp;

                if (board.Solved)
                    break;

                tmp = AnalyzeForOutliers(board);
                if (tmp)
                    changed = tmp;

                iterations++;
            }
            while (changed && !board.Solved);

            return changed || board.Solved;
        }

        private static bool SetSingles(Board board)
        {
            bool changed;

            int iterations = 0;

            do
            {
                changed = false;

                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        var cell = board.GetCell(r, c);

                        if (cell is ChangeableCell)
                        {
                            var possible = ((ChangeableCell)cell).GetPossibleNumbers();
                            if (possible.Count == 1)
                            {
                                board.SetCellValue(r, c, possible[0]);
                                changed = true;
                            }
                        }
                    }
                }

                iterations++;

            } while (changed == true);

            return iterations > 1;
        }

        #region Analyze for Outliers
        private static bool AnalyzeForOutliers(Board board)
        {
            int changed = 0;

            Dictionary<Numbers, List<Cell>> values = new Dictionary<Numbers, List<Cell>>();
            ICollection<Cell> cells;
            int tmp;

            for (int i = 0; i < 9; i++)
            {
                //Rows
                values.Clear();
                cells = board.GetRow(i);

                tmp = FindAndSetOutliers(board, values, cells);
                changed += tmp;

                //Columns
                values.Clear();
                cells = board.GetColumn(i);

                tmp = FindAndSetOutliers(board, values, cells);
                changed += tmp;

                //Sections
                for (int r = 0; r < 3; r++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        cells = board.GetSection(r, c);
                        values.Clear();

                        tmp = FindAndSetOutliers(board, values, cells);
                        changed += tmp;
                    }
                }
            }

            return changed > 0;
        }

        private static int FindAndSetOutliers(Board board, Dictionary<Numbers, List<Cell>> rowColumnSection, ICollection<Cell> cells)
        {
            int tmp;

            var ccs = Board.GetChangeableCells(cells);

            foreach (var c in cells)
            {
                if (c is ChangeableCell)
                {
                    foreach (var n in ((ChangeableCell)c).GetPossibleNumbers())
                    {
                        if (!rowColumnSection.ContainsKey(n))
                            rowColumnSection[n] = new List<Cell>();

                        rowColumnSection[n].Add(c);
                    }
                }
            }

            tmp = SetOutliers(board, rowColumnSection);

            return tmp;
        }

        private static int SetOutliers(Board board, Dictionary<Numbers, List<Cell>> values)
        {
            int changed = 0;

            foreach (var kv in values.Where(x => x.Value.Count == 1))
            {
                var cell = kv.Value[0];
                board.SetCellValue(cell.Row, cell.Column, kv.Key);
                changed++;
            }

            return changed;
        }


        #endregion

        
        /*
        //TODO: Allow for more than 2 (3,4, ...)
        private static bool RemovePairValues(ICollection<ChangeableCell> cells)
        {
            List<Cell> possiblePairs = new List<Cell>();
            List<List<Numbers>> possibles = new List<List<Numbers>>();

            bool changed = false;

            foreach (var c in cells)
            {
                var p = c.GetPossibleNumbers();
                if (p.Count == 2)
                {
                    possiblePairs.Add(c);
                    possibles.Add(p);
                }
            }

            for (int a = 0; a < possiblePairs.Count; a++)
            {
                for (int b = a + 1; b < possiblePairs.Count; b++)
                {
                    bool isPair = true;

                    for (int i = 0; i < possibles[a].Count; i++)
                    {
                        if (possibles[a][i] != possibles[b][i])
                        {
                            isPair = false;
                            break;
                        }
                    }

                    if (isPair)
                    {

                        foreach (ChangeableCell c in cells)
                        {
                            if (c != possiblePairs[a] && c != possiblePairs[b])
                            {
                                c.RemoveValue(possibles[a][0]);
                                c.RemoveValue(possibles[a][1]);
                            }

                            changed = true;
                        }
                    }
                }
            }

            return changed;
        }

        */

    }
}
