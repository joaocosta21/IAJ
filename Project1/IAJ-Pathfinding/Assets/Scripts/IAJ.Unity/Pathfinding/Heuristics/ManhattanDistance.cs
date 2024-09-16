using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using UnityEngine;


using System;
using Assets.Scripts.Grid;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class ManhattanDistance : IHeuristic
    {
        public float H(Node node, Node goalNode)
        {
            float dx = Mathf.Abs(node.x - goalNode.x);
            float dy = Mathf.Abs(node.y - goalNode.y);
            return dx + dy;
        }
    }
}
