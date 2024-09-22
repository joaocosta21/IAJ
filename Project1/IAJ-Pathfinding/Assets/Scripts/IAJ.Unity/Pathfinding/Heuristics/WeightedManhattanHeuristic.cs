using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using UnityEngine;
using System;
using Assets.Scripts.Grid;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class WeightedManhattanHeuristic : IHeuristic
    {
        private float weight;

        public WeightedManhattanHeuristic(float weight = 1.5f)
        {
            this.weight = weight;
        }

        public float H(Node startNode, Node goalNode)
        {
            // Standard Manhattan Distance heuristic, scaled by weight
            return this.weight * (Math.Abs(startNode.x - goalNode.x) + Math.Abs(startNode.y - goalNode.y));
        }
    }
}
