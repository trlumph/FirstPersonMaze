using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BacktrackingMaze
{
    [Flags]
    enum Directions
    {
        N = 1,
        S = 2,
        E = 4,
        W = 8
    }
}
