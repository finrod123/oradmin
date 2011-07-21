using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oradmin
{
    class SequenceGenerator
    {
        #region Members

        int min, max;
        int step;
        bool cycle;
        int current;

        #endregion

        #region Constructor

        public SequenceGenerator(int min, int max, int step, bool cycle)
        {
            if (step == 0 ||
               ((max - min) < 0 && step >= 0) ||
               ((max - min) > 0 && step <= 0))
            {
                throw new ArgumentException("Wrong sequence initialization values");
            }

            this.min = min;
            this.max = max;
            this.step = step;
            this.cycle = cycle;
            current = min;
        }

        #endregion

        #region Public interface

        public int Current
        {
            get { return current; }
        }

        public int Next
        {
            get
            {
                if (current == max && cycle)
                    current = min;
                else if(current < max)
                    ++current;

                return current;
            }
        }

        #endregion

        #region  Properties
        public int Min
        {
            get { return min; }
        }

        public int Max
        {
            get { return max; }
        }


        public bool Cycle
        {
            get { return cycle; }
        }
        #endregion
    }
}
