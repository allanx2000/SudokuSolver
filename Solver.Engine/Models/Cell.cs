using System;

namespace Solver.Engine.Models
{
    public abstract class Cell
    {
        public abstract CellType CellType { get; }
        public abstract Numbers Value { get; }

        public int Row { get; private set; }
        public int Column { get; private set; }

        public int Section {
            get
            {
                return (Row/3) * 3 + (Column/3);
            }
        }

        public Cell(int row, int column)
        {
            this.Row = row;
            this.Column = column;
        }

        internal abstract Cell Clone();
    }
}