using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Grid
{
    //For visualization purposes. Should NOT be used for the algorithms.
    public enum VisualNodeStatus
    {
        Unvisited,
        Open,
        Closed
    }

    public class Node
    {
        //private Grid<Node> grid;
        public int x;
        public int y;
        // Only for visualization purposes. Do NOT use in your algorithms
        public VisualNodeStatus status;


        public bool isWalkable;

        public Node( int x, int y)
        {
            this.x = x;
            this.y = y;
            isWalkable = true;
            status = VisualNodeStatus.Unvisited;
        }

        public override string ToString()
        {
            return x + "," + y;
        }
    }
}
