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
        public int NodesSearched { get; set; }
        public uint MaxNodes { get; set; }
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
            this.HeuristicMultiplier = 1.5f; // Initialize the multiplier
            this.NodesPerSearch = 25;//by default we process all nodes in a single request, but you should change this
            this.MaxNodes = 600000;
            this.TieBreakingWeight = tieBreakingWeight;
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
                hCost = this.HeuristicMultiplier * this.Heuristic.H(this.StartNode, this.GoalNode),
            };

            //initialize open and closed lists
            initialNode.CalculateFCost(TieBreakingWeight);
            this.Open.Clear();
            this.Open.Add(initialNode);
            AddToOpenCalls++;
            this.Closed.Clear();
            this.NodesSearched = 0;
        }

        public virtual bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false) 
        {
            if (Open.CountOpen() == 0)
            {
                solution = null;
                this.InProgress = false;
                return false;
            }

            int ProcessedNodesPerFrame = 0;
            NodeRecord currentNode;

            //While Open is not empty or if nodes havent been all processed 
            while (Open.CountOpen() > 0 && ProcessedNodesPerFrame < NodesPerSearch)
            {
                //Get the best node from the Open list
                currentNode = Open.GetBestAndRemove();
                GetBestAndRemoveCalls++;

                //If the current node is the goal node, we have found a solution
                if (currentNode.Node == GoalNode)
                {
                    solution = CalculatePath(currentNode);
                    this.InProgress = false;
                    return true;
                }
                
                //Add the current node to the Closed list
                this.Closed.Add(currentNode);
                currentNode.Node.status = VisualNodeStatus.Closed;
                AddToClosedCalls++;
                NodesSearched++;

                //Process the node's connections 
                foreach (Connection connection in gridGraph.GetConnections(currentNode.Node))
                {
                    this.TotalProcessedNodes++;
                    ProcessedNodesPerFrame++;
                    ProcessChildNode(currentNode, connection);
                }

                if (Open.CountOpen() > MaxOpenNodes) 
                {
                    MaxOpenNodes = Open.CountOpen();
                }

                if (MaxNodes == NodesSearched)
                {
                    solution = CalculatePath(currentNode);
                    this.InProgress = false;
                    return true;
                }

                if (ProcessedNodesPerFrame >= this.NodesPerSearch && returnPartialSolution)
                {
                    solution = CalculatePath(currentNode);
                    return false; // Search not finished, can continue in the next frame
                }
            }

            //Out of nodes on the openList
            solution = null;
            return false;   
        }

        protected virtual void ProcessChildNode(NodeRecord parentNode, Connection connection)
        {
            // Get the child node (neighbor)
            Node childNode = connection.ToNode;
            float newGCost = parentNode.gCost + connection.Cost;

            // Create a NodeRecord for the child node
            NodeRecord childNodeRecord = new NodeRecord(childNode)
            {
                gCost = newGCost,
                hCost = HeuristicMultiplier * Heuristic.H(childNode, GoalNode), // Adjusted here
                parent = parentNode
            };

            childNodeRecord.CalculateFCost(TieBreakingWeight);

            // Check if the child node is in the closed set
            NodeRecord closedNode = this.Closed.Find(childNodeRecord); // Use Find from IClosedSet
            SearchInClosedCalls++;

            if (closedNode != null) // If the node is found in the closed set, ignore it
            {
                return;
            }

            // Check if the child node is in the open set
            NodeRecord openNode = Open.Find(childNodeRecord); // Use Find from IOpenSet
            SearchInOpenCalls++;

            if (openNode == null)
            {
                // The child node is not in the open set, so add it
                Open.Add(childNodeRecord);
                childNodeRecord.Node.status = VisualNodeStatus.Open;
                AddToOpenCalls++;
            }
            else if (newGCost < openNode.gCost - 2 )
            {
                // The child node is in the open set, but we found a better path to it
                openNode.gCost = newGCost;
                openNode.parent = parentNode;  // Update the parent to the current node
                openNode.CalculateFCost(TieBreakingWeight);

                // Update the open set with the new values
                Open.Replace(openNode, childNodeRecord);
                ReplaceCalls++;
            }

            // Update the grid to mark the node (optional, visual purpose)
            pathfindingManager.gridGraph.grid.SetGridObject(childNode.x, childNode.y, childNode);
        }


        // Method to calculate the Path, starts from the end Node and goes up until the beggining
        //You can implement as nodes or connections, but for the visual magic to work, in this project the final path should be represented as a sequence of nodeRecords...
        public List<NodeRecord> CalculatePath(NodeRecord endNode) 
        {
            List<NodeRecord> path = new List<NodeRecord>();

            // Start from the end node and go up the parent chain until reaching the start node
            NodeRecord currentNode = endNode;

            while (currentNode != null) 
            {
                path.Add(currentNode);  // Add the current node to the path
                currentNode = currentNode.parent;  // Move to the parent node
            }

            // Reverse the path to get the order from start to goal
            path.Reverse();
            return path;
        }

    }
}
