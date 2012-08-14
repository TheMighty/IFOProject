using System;
using System.Collections.Generic;
using System.Text;

namespace IFOProject
{
    public struct RowInfo
    {
        /// <summary>
        /// Row index in pattern
        /// </summary>
        public uint number;
        /// <summary>
        /// Initial approximation
        /// </summary>
        public Coefficients approximation;
        /// <summary>
        /// Final coefficients
        /// </summary>
        public Coefficients final;
    }
}
