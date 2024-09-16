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
            
            float X = node.x - goalNode.x;
            float Y = node.y - goalNode.y;
            return (float) Math.Sqrt(X*X + Y*Y); 

        }
    }
}
