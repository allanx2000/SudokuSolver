using System;

namespace Solver.Engine.Models
{
    internal class StaticCell : Cell
    {
        private Numbers value;
        
        internal override Cell Clone()
        {
            Cell c = new StaticCell(value, Row, Column);
            return c;
        }

        public override Numbers Value
        {
            get
            {
                return value;
            }
        }

        public override CellType CellType
        {
            get
            {
                return CellType.Static;
            }
        }

        public StaticCell(Numbers value, int row, int col) : base(row,col)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

    }
}