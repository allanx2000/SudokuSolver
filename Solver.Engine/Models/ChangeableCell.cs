using System;
using System.Collections.Generic;
using System.Linq;

namespace Solver.Engine.Models
{
    public class ChangeableCell : Cell
    {

        
        public ChangeableCell(int row, int column) : base(row, column)
        {
        }

        public override CellType CellType
        {
            get
            {
                return CellType.Changeable;
            }
        }

        public override Numbers Value
        {
            get
            {
                return Numbers.None;
            }
        }

        private short[] values = new short[9];

        internal override Cell Clone()
        {
            var c = new ChangeableCell(this.Row, this.Column);
            this.values.CopyTo(c.values, 0);

            return c;
        }

        private const short REMOVED = 1;

        public void RemoveValue(Numbers number)
        {
            values[(int)number] = 1; //1 = removed
        }

        public List<Numbers> GetPossibleNumbers()
        {
            List<Numbers> results = new List<Numbers>();

            for (int i = 0; i < 9; i++)
            {
                if (values[i] != REMOVED)
                    results.Add((Numbers)i);
            }

            return results;
        }

        public override string ToString()
        {
            var possible = GetPossibleNumbers();
            var strings = from i in possible select i.ToString();

            string join = "Possible: " + String.Join(" ", strings);
            return join;
        }

    }
}