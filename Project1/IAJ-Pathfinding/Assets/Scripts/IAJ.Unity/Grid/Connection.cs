using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Grid
{
    public class Connection : IConnection
    {
        public Node FromNode { get; private set; }
        public Node ToNode { get; private set; }
        public float Cost { get; private set; }

        public Connection(Node fromNode, Node toNode, float cost)
        {
            FromNode = fromNode;
            ToNode = toNode;
            Cost = Math.Max(0, cost);
        }

        public float GetCost()
        {
            return Cost;
        }
    }
}