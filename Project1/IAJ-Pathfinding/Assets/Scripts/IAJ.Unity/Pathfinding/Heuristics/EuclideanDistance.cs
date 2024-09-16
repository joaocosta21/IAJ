using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using UnityEngine;


using System;
using Assets.Scripts.Grid;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class EuclideanDistance : IHeuristic
    {
        public float H(Node node, Node goalNode)
        {
            float dx = node.x - goalNode.x;
            float dy = node.y - goalNode.y;
            
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
    }
}
