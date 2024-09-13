
using Assets.Scripts.Grid;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public enum NodeStatus
    {
        Unvisited,
        Open,
        Closed
    }
    
    public class NodeRecord  : IComparable<NodeRecord>
    {
        //Node
        public Node Node { get; private set; }
        

        //A* Stuff
        public NodeRecord parent;
        public float gCost;
        public float hCost;
        public float fCost;
        

        public NodeRecord(Node node)
        {
            
            this.Node = node;
            gCost = int.MaxValue;
            hCost = 0;
            fCost = gCost + hCost;
            parent = null;
        }

        public void CalculateFCost(float tieBreakingWeight)
        {
            //ToDo Implement tieBreaking here
            fCost = gCost + hCost;
        }

        public int CompareTo(NodeRecord other)
        {
            return this.fCost.CompareTo(other.fCost);

        }

        //two node records are equal if they refer to the same node: Do NOT compare directly with "=="!
        public override bool Equals(object obj)
        {
            if (obj is NodeRecord target) return this.Node.Equals(target.Node);
            else if (obj is Node target2) return this.Node.Equals(target2);
            else throw new ArgumentException("NodeRecord Equals(obj) called with non Node or NodeRecord obj");
        }


        // I wonder where this might be useful...
        public void Reset()
        {
            gCost = int.MaxValue;
            hCost = 0;
            fCost = gCost + hCost;
            parent = null;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
