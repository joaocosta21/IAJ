using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
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
    [Serializable]
    public class AStarPathfinding : IPathfinding
    {
        PathfindingManager pathfindingManager;
        public IGraph gridGraph { get; set; }
        public bool InProgress { get; set; }
        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }
        public IOpenSet Open2 { get; protected set; }
        public IClosedSet Closed2 { get; protected set; }

        public IHeuristic Heuristic { get; protected set; }

        public Node GoalNode { get; set; }
        public Node StartNode { get; set; }
        public int StartPositionX { get; set; }
        public int StartPositionY { get; set; }
        public int GoalPositionX { get; set; }
        public int GoalPositionY { get; set; }
        protected float TieBreakingWeight;

        // variables for analysis purposes
        public uint NodesPerSearch { get; set; }
        public uint NodesSearched { get; set; }
        public uint TotalProcessedNodes { get; protected set; }
        public int MaxOpenNodes { get; protected set; }
        public float TotalProcessingTime { get; set; }
        public int AStarPathfindingSearchCalls { get; set; } = 0;
        public int GetBestAndRemoveCalls { get; set; } = 0;
        public int AddToOpenCalls { get; set; } = 0;
        public int SearchInOpenCalls { get; set; } = 0;
        public int RemoveFromOpenCalls { get; set; } = 0;
        public int ReplaceCalls { get; set; } = 0;
        public int AddToClosedCalls { get; set; } = 0;
        public int SearchInClosedCalls { get; set; } = 0;
        public int RemoveFromClosedCalls { get; set; } = 0;
        public int MaxNodes { get; set; } = 0;


        public AStarPathfinding(IGraph grid, IOpenSet open, IClosedSet closed, IHeuristic heuristic, float tieBreakingWeight = 0.0f)
        {

            this.pathfindingManager = GameObject.FindObjectOfType<PathfindingManager>();
            this.gridGraph = grid;
            this.Open = open;
            this.Closed = closed;
            this.Open2 = null;
            this.Closed2 = null;
            this.InProgress = false;
            this.Heuristic = heuristic;
            this.NodesPerSearch = 30;
            this.TieBreakingWeight = tieBreakingWeight;
            this.MaxNodes = 20000;
            this.NodesSearched = 0;
        }
        public virtual void Preprocess()
        {
            //No preprocessing needed for basic A*
        }
        public virtual void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY)
        {
            this.StartPositionX = startX;
            this.StartPositionY = startY;
            this.GoalPositionX = goalX;
            this.GoalPositionY = goalY;
            this.StartNode = gridGraph.GetNode(StartPositionX, StartPositionY);
            this.GoalNode = gridGraph.GetNode(GoalPositionX, GoalPositionY);
            
            if (this.StartNode == null || this.GoalNode == null) return;

            this.InProgress = true;
            this.TotalProcessedNodes = 0;
            this.TotalProcessingTime = 0.0f;
            this.MaxOpenNodes = 0;

            var initialNode = new NodeRecord(StartNode)
            {
                gCost = 0,
                hCost = this.Heuristic.H(this.StartNode, this.GoalNode),
            };

            initialNode.CalculateFCost(TieBreakingWeight);
            this.Open.Clear();
            this.Open.Add(initialNode);
            AddToOpenCalls++;
            this.Closed.Clear();
        }

        public virtual bool Search(out List<NodeRecord> solution, bool returnPartialSolution = true)
        {
            int ProcessedNodesPerFrame = 0;
            NodeRecord currentNode;

            while (Open.CountOpen() > 0)
            {
                currentNode = Open.GetBestAndRemove();
                if (currentNode.Node == GoalNode)
                {
                    solution = CalculatePath(currentNode);
                    return true;
                }

                Closed.Add(currentNode);
                currentNode.Node.status = VisualNodeStatus.Closed;
                NodesSearched++;

                foreach (Connection connection in gridGraph.GetConnections(currentNode.Node))
                {
                    this.TotalProcessedNodes++;
                    ProcessedNodesPerFrame++;
                    ProcessChildNode(currentNode, connection);
                }

                if (MaxNodes == this.NodesSearched)
                {
                    solution = null;
                    if (returnPartialSolution) { 
                        solution = CalculatePath(currentNode); 
                    }
                    return true;
                }

                if (ProcessedNodesPerFrame >= this.NodesPerSearch)
                {
                    solution = null;
                    if (returnPartialSolution) { 
                        solution = CalculatePath(currentNode); 
                    }
                    return false;
                }

                if(Open.CountOpen() > MaxOpenNodes)
                {
                    MaxOpenNodes = Open.CountOpen();
                }

            }

            solution = null;
            return false;

        }

        protected virtual void ProcessChildNode(NodeRecord parentNode, Connection connection)
        {
            Node childNode = connection.ToNode;
            float newCost = parentNode.gCost + connection.Cost;

            // Calculate newCost: parent cost + Calculate Distance Cont 
            // float newCost = parentNode.gCost + CalculateDistanceCost(parentNode, node) + this.Heuristic.H(node, this.GoalNode);
            // Calculate newCost: parent cost + Calculate Distance Cont 

            NodeRecord closedNode = this.Closed.Find(new NodeRecord(childNode));
            
            if (closedNode != null) return;

            NodeRecord openNode = Open.Find(new NodeRecord(childNode));

            if (openNode == null)
            {
                NodeRecord childNodeRecord = new NodeRecord(childNode)
                {
                    gCost = newCost,
                    hCost = this.Heuristic.H(childNode, GoalNode),
                    parent = parentNode
                };
                childNodeRecord.CalculateFCost(TieBreakingWeight);

                Open.Add(childNodeRecord);
                childNodeRecord.Node.status = VisualNodeStatus.Open;
                AddToOpenCalls++;
            }
            else if (newCost < openNode.gCost)
            {
                openNode.gCost = newCost;
                openNode.parent = parentNode;
                openNode.CalculateFCost(TieBreakingWeight);

                Open.Replace(openNode, openNode);
                ReplaceCalls++;
            }

            pathfindingManager.gridGraph.grid.SetGridObject(childNode.x, childNode.y, childNode);
        }


        public List<NodeRecord> CalculatePath(NodeRecord endNode)
        {
            List<NodeRecord> path = new List<NodeRecord>();
            path.Add(endNode);

            NodeRecord currentNode = endNode;
            while (currentNode.parent != null)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }

            path.Reverse();
            return path;
        }

        public List<NodeRecord> GetPartialSolution()
        {
            // Return the current best path to the node with the lowest fCost in the Open set
            NodeRecord currentBestNode = this.Open.PeekBest();
            
            if (currentBestNode == null)
            {
                return new List<NodeRecord>(); // No valid partial path
            }

            // Calculate and return the path from the current best node
            return this.CalculatePath(currentBestNode);
        }
    }
}