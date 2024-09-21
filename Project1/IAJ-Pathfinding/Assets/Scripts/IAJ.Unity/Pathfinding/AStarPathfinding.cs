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
    public class AStarPathfinding
    {
        PathfindingManager pathfindingManager;
        public IGraph gridGraph { get; set; }
        public bool InProgress { get; set; }
        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }
        public IHeuristic Heuristic { get; protected set; }

        public Node GoalNode { get; set; }
        public Node StartNode { get; set; }
        public int StartPositionX { get; set; }
        public int StartPositionY { get; set; }
        public int GoalPositionX { get; set; }
        public int GoalPositionY { get; set; }
        protected float TieBreakingWeight;
        protected float HeuristicMultiplier;


        // variables for analysis purposes
        public uint NodesPerSearch { get; set; }
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


        public AStarPathfinding(IGraph grid, IOpenSet open, IClosedSet closed, IHeuristic heuristic, float tieBreakingWeight = 0.0f)
        {
            this.pathfindingManager = GameObject.FindObjectOfType<PathfindingManager>();
            this.gridGraph = grid;
            this.Open = open;
            this.Closed = closed;
            this.InProgress = false;
            this.Heuristic = heuristic;
            this.NodesPerSearch = 20; //by default we process all nodes in a single request, but you should change this
            this.TieBreakingWeight = tieBreakingWeight;
            this.HeuristicMultiplier = 1.5f;
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

            //if it is not possible to quantize the positions and find the corresponding nodes, then we cannot proceed
            if (this.StartNode == null || this.GoalNode == null) return;

            // Reset debug and relevat variables here
            this.InProgress = true;
            this.TotalProcessedNodes = 0;
            this.TotalProcessingTime = 0.0f;
            this.MaxOpenNodes = 0;

            //Starting with the first node
            var initialNode = new NodeRecord(StartNode)
            {
                gCost = 0,
                hCost = this.Heuristic.H(this.StartNode, this.GoalNode),
            };

            //initialize open and closed lists
            initialNode.CalculateFCost(TieBreakingWeight);
            this.Open.Clear();
            this.Open.Add(initialNode);
            AddToOpenCalls++;
            this.Closed.Clear();
        }

        public virtual bool Search(out List<NodeRecord> solution, bool returnPartialSolution = true)
        {
            NodeRecord foundNode;
            int ProcessedNodesPerFrame = 0;
            NodeRecord currentNode;

            while (Open.CountOpen() > 0)
            {
                currentNode = Open.GetBestAndRemove();

                foreach (Connection connection in gridGraph.GetConnections(currentNode.Node))
                {
                    this.TotalProcessedNodes++;
                    ProcessedNodesPerFrame++;
                    foundNode = ProcessChildNode(currentNode, connection);
                    if (foundNode != null)
                    {
                        solution = CalculatePath(foundNode);
                        return true;
                    }
                    /*else if(ProcessedNodesPerFrame == 300){
                        solution = null;
                        return false;
                    }*/
                }

                Closed.Add(currentNode);
            }

            //Out of nodes on the openList
            solution = null;
            return false;

        }

        protected virtual NodeRecord ProcessChildNode(NodeRecord parentNode, Connection connection)
        {
            Node node = connection.ToNode;
            float newCost = parentNode.gCost + connection.Cost + this.Heuristic.H(node, this.GoalNode);
            NodeRecord newNodeRecord = new NodeRecord(node);

            // Calculate newCost: parent cost + Calculate Distance Cont 
            // float newCost = parentNode.gCost + CalculateDistanceCost(parentNode, node) + this.Heuristic.H(node, this.GoalNode);
            // Calculate newCost: parent cost + Calculate Distance Cont 

            //If in Closed...
            NodeRecord currentClosedNode = Closed.Find(newNodeRecord);
            NodeRecord currentOpenNode = Open.Find(newNodeRecord);
            if (currentClosedNode != null)
            {
                if (newCost < currentClosedNode.gCost)
                {
                    currentClosedNode.parent = parentNode;
                    currentClosedNode.gCost = parentNode.gCost + connection.Cost;
                    currentClosedNode.fCost = newCost;
                    currentClosedNode.hCost = newCost - currentClosedNode.gCost;
                    Closed.Remove(currentClosedNode);
                    Open.Add(currentClosedNode);

                }
                newNodeRecord = currentClosedNode;
            }
            else if (currentOpenNode != null)
            {
                if (newCost < currentOpenNode.gCost)
                {
                    currentOpenNode.parent = parentNode;
                    currentOpenNode.gCost = parentNode.gCost + connection.Cost;
                    currentOpenNode.fCost = newCost;
                    currentOpenNode.hCost = newCost - currentOpenNode.gCost;
                }
                newNodeRecord = currentOpenNode;
            }
            else
            {
                newNodeRecord.gCost = parentNode.gCost + connection.Cost;
                newNodeRecord.fCost = newCost;
                newNodeRecord.hCost = newCost - newNodeRecord.gCost;
                newNodeRecord.parent = parentNode;
                Open.Add(newNodeRecord);
            }

            // Finally don't forget to update the actual Grid value:
            pathfindingManager.gridGraph.grid.SetGridObject(node.x, node.y, node);
            if (connection.ToNode.Equals(GoalNode))
            {
                return newNodeRecord;
            }
            else
            {
                return null;
            }
        }


        // Method to calculate the Path, starts from the end Node and goes up until the beggining
        //You can implement as nodes or connections, but for the visual magic to work, in this project the final path should be represented as a sequence of nodeRecords...
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

    }
}
