using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWindowsIot
{
    public static class Extensions
    {
        public static bool AllEqual(this bool firstValue, params bool[] bools)
        {
            return bools.All(thisBool => thisBool == firstValue);
        }
    }
}
