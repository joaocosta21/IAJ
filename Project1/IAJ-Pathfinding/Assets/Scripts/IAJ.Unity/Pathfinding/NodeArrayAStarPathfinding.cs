﻿using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using System.Runtime.CompilerServices;
using System;
using UnityEditor.Experimental.GraphView;
using Node = Assets.Scripts.Grid.Node;
using UnityEditor.MemoryProfiler;
using Connection = Assets.Scripts.Grid.Connection;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class NodeArrayAStarPathfinding : AStarPathfinding
    {
        private NodeRecordArray nodeRecordArray;
        PathfindingManager pathfindingManager;

        public NodeArrayAStarPathfinding(IGraph grid, IHeuristic heuristic, float tieBreakingWeight = 0.0f)
            : base(grid, null, null, heuristic)
        {
            this.nodeRecordArray = new NodeRecordArray(grid.AllNodes());
            this.Open = this.nodeRecordArray;
            this.Closed = this.nodeRecordArray;
            this.pathfindingManager = GameObject.FindObjectOfType<PathfindingManager>(); 
            base.TieBreakingWeight = tieBreakingWeight;
        }

        protected override void ProcessChildNode(NodeRecord parentNode, Connection connection)
        {
            Node childNode = connection.ToNode;
            float newGCost = parentNode.gCost + connection.Cost;

            NodeRecord childNodeRecord = nodeRecordArray.GetNodeRecordByIndex(childNode.Index);

            if (childNodeRecord.Category == NodeCategory.Closed)
            {
                return;
            }

            if (childNodeRecord.Category == NodeCategory.Unvisited || newGCost < childNodeRecord.gCost)
            {
                childNodeRecord.gCost = newGCost;
                childNodeRecord.hCost = this.Heuristic.H(childNode, this.GoalNode);
                childNodeRecord.parent = parentNode;
                childNodeRecord.CalculateFCost(TieBreakingWeight);
                
                if (childNodeRecord.Category == NodeCategory.Unvisited)
                {
                    ((IOpenSet)nodeRecordArray).Add(childNodeRecord);
                }
                else
                {
                    nodeRecordArray.Replace(childNodeRecord, childNodeRecord);
                }
            }
            pathfindingManager.gridGraph.grid.SetGridObject(childNode.x, childNode.y, childNode);
        }
    }
}
