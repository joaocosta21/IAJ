using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using System.Reflection;
using UnityEditor.MemoryProfiler;

namespace Assets.Scripts.Grid
{
    public interface IGraph

    {
        //Grid<Node> grid { get; set; }

        // An array of connections outgoing from the given node
        List<Connection> GetConnections(Node FromNode);

        //should return 0 if the coordinates coincide, and PositiveInfinity if the nodes are not adjacent
        //or one of them is not walkable
        public float GetCost(Node fromNode, Node toNode);

        public Node GetNode(int x, int y);
    }
}