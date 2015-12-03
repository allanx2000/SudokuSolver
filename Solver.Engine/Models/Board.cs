using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver.Engine.Models
{
    public class Board
    {
        public static ICollection<ChangeableCell> GetChangeableCells(ICollection<Cell> cells)
        {
            List<ChangeableCell> cc = new List<ChangeableCell>();
            foreach (var c in cells)
            {
                if (c is ChangeableCell)
                {
                    cc.Add((ChangeableCell)c);
                }
            }

            return cc;
        }
        
        private Cell[] cells;

        public bool IsValid { get
            {
                //Could just return from solver while setting? If changeable runs out of possibles, throw error?
                var noPossibles = from i in GetAllChangeableCells()
                                  where i.GetPossibleNumbers().Count == 0
                                  select i;

                return noPossibles.Count() == 0;
            }
        }

        public Board Clone()
        {
            Board newBoard = new Board();

            for (int i = 0; i < cells.Length; i++)
            {
                newBoard.cells[i] = this.cells[i].Clone();
            }

            return newBoard;
        }

        public int ChangeableCellsLeft
        {
            get
            {
                return cells.Count(x => x.CellType == CellType.Changeable);
            }
        }


        public bool Solved {
            get
            {
                return ChangeableCellsLeft == 0;
            }
        }

        
        public ICollection<ChangeableCell> GetAllChangeableCells()
        {
            return GetChangeableCells(cells);
        }
        
        public Board()
        {
            cells = new Cell[81];
            
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null)
                    cells[i] = MakeEmptyCell(i/9, i % 9);
            }
        }
        
        private Cell MakeEmptyCell(int row, int column)
        {
            var emptyCell = new ChangeableCell(row, column);
            return emptyCell;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <param name="value">One Based</param>
        public void SetCellValue(int row, int col, Numbers value)
        {
            int idx = GetCellIndex(row, col);

            cells[idx] = new StaticCell(value, row, col);

            List<Cell> update = new List<Cell>();
            update.AddRange(GetRow(row));
            update.AddRange(GetColumn(col));
            update.AddRange(GetSection(row, col));

            foreach (Cell c in update)
            {
                ChangeableCell cc = c as ChangeableCell;
                if (cc != null)
                    cc.RemoveValue(value);
            }
        }

        public ICollection<Cell> GetSection(int row, int col)
        {
            //Round down
            int rstart = (row / 3) * 3;
            int cstart = (col / 3) * 3;

            List<Cell> results = new List<Cell>();
            
            for (int r = rstart; r < rstart + 3; r++)
            {
                for (int c = cstart; c < cstart + 3; c++)
                {
                    results.Add(GetCell(r, c));
                }
            }

            return results;
        }
              


        public ICollection<Cell> GetRow(int row)
        {
            
            Cell[] result = new Cell[9];

            for (int i = 0; i < 9; i++)
            {
                result[i] = GetCell(row, i);
            }

            return result;
        }

        public ICollection<Cell> GetColumn(int column)
        {
            Cell[] result = new Cell[9];

            for (int i = 0; i < 9; i++)
            {
                result[i] = GetCell(i, column);
            }

            return result;
        }

        /// <summary>
        /// Gets the index
        /// row, column are 0 based
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private int GetCellIndex(int row, int col)
        {
            int idx = row * 9 + col;

            return idx;
        }
        
        /// <summary>
        /// Gets the cell at the index
        /// </summary>
        /// <param name="row"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public Cell GetCell(int row, int col)
        {

            int idx = GetCellIndex(row, col);

            return cells[idx];
        }
    }
}
