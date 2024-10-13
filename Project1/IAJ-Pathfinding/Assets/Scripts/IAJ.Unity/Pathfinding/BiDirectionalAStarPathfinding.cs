using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using Assets.Scripts.Grid;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class BiDirectionalAStarPathfinding : IPathfinding
    {
        public AStarPathfinding forwardSearch;
        public AStarPathfinding backwardSearch;
        private NodeRecord meetingNode;
        
        public bool InProgress { get; set; }
        public float TotalProcessingTime { get; set; }
        public uint TotalProcessedNodes { get; protected set; }
        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }
        public IOpenSet Open2 { get; protected set; }
        public IClosedSet Closed2 { get; protected set; }
        public int MaxOpenNodes { get; protected set; }

        public BiDirectionalAStarPathfinding(IGraph grid, IHeuristic heuristic, float tieBreakingWeight = 0.0f)
        {
            // Initialize both forward and backward A* searches
            this.Open = new SimpleUnorderedNodeList();
            this.Closed = new SimpleUnorderedNodeList();
            this.Open2 = new SimpleUnorderedNodeList();
            this.Closed2 = new SimpleUnorderedNodeList();
            this.forwardSearch = new AStarPathfinding(grid, this.Open, this.Closed, heuristic, tieBreakingWeight);
            this.backwardSearch = new AStarPathfinding(grid, this.Open2, this.Closed2, heuristic, tieBreakingWeight);
            this.InProgress = false;
        }

        // Initializes both forward and backward searches
        public void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY)
        {
            // Initialize both searches
            this.forwardSearch.InitializePathfindingSearch(startX, startY, goalX, goalY);
            this.backwardSearch.InitializePathfindingSearch(goalX, goalY, startX, startY);
            this.TotalProcessingTime = 0;
            this.TotalProcessedNodes = 0;
            this.MaxOpenNodes = 0;
            this.InProgress = true;
            this.meetingNode = null;
        }

        // Perform search in both forward and backward directions
        public bool Search(out List<NodeRecord> solution, bool partialPath)
        {
            solution = null;

            if (!this.InProgress)
            {
                return false;
            }

            List<NodeRecord> forwardSolution = null;
            List<NodeRecord> backwardSolution = null;

            // Step through forward and backward searches
            bool forwardFinished = forwardSearch.Search(out forwardSolution, partialPath);
            bool backwardFinished = backwardSearch.Search(out backwardSolution, partialPath);
            if (forwardSearch.Open.CountOpen() + backwardSearch.Open.CountOpen() > MaxOpenNodes)
            {
                MaxOpenNodes = forwardSearch.Open.CountOpen() + backwardSearch.Open.CountOpen();
            }
            // Check if there's a meeting node between the two searches
            this.meetingNode = FindMeetingNode();
            
            // If a meeting node is found, combine the paths
            if (meetingNode != null)
            {
                var forwardPath = forwardSearch.CalculatePath(this.forwardSearch.Closed.Find(meetingNode));
                var backwardPath = backwardSearch.CalculatePath(this.backwardSearch.Closed.Find(meetingNode));
                TotalProcessedNodes = forwardSearch.TotalProcessedNodes + backwardSearch.TotalProcessedNodes;
                solution = CombinePaths(forwardPath, backwardPath, meetingNode);
                this.InProgress = false;
                CleanPaths(solution);
                return true;
            }

            // If both forward and backward searches have finished
            if (forwardFinished && backwardFinished)
            {
                // We should attempt to combine paths at the point where both searches have finished
                solution = CombinePaths(forwardSolution, backwardSolution, null);
                this.InProgress = false;
                return solution != null;
            }

            // If partialPath is true and either search finishes, return partial solution
            if (partialPath && (forwardFinished || backwardFinished))
            {
                solution = CombinePaths(forwardSolution, backwardSolution, meetingNode);
                this.InProgress = false;
                
                return true;
            }

            // If no path is found yet and the search is still ongoing
            return false;
        }

        private NodeRecord FindMeetingNode()
        {
            foreach (var nodeRecord in this.forwardSearch.Closed.All())
            {
                if (this.backwardSearch.Closed.Find(nodeRecord) != null)
                {
                    return nodeRecord; 
                }
            }

            return null;
        }

        public void Preprocess()
        {
            // No preprocessing for BiDirectional A*, but it could be added if necessary
        }

        private List<NodeRecord> CombinePaths(List<NodeRecord> forwardPath, List<NodeRecord> backwardPath, NodeRecord meetingNode)
        {
            List<NodeRecord> path = new List<NodeRecord>();

            foreach (var nodeRecord in forwardPath)
            {
                path.Add(nodeRecord);
                //if (nodeRecord.Node.Equals(meetingNode)) break;
            }

            foreach (var nodeRecord in backwardPath)
            {
                path.Add(nodeRecord);
                //if (nodeRecord.Node.Equals(meetingNode)) break;
            }
            return path;
        }

        public void CleanPaths(List<NodeRecord> path)
        {
            foreach (var nodeRecord in path)
            {
                Closed.Remove(nodeRecord);
                Closed2.Remove(nodeRecord);
            }
        }

        public List<NodeRecord> GetFowardMeetingPath()
        {
            return this.forwardSearch.CalculatePath(meetingNode);
        }

        public List<NodeRecord> GetBackwardMeetingPath()
        {
            return this.backwardSearch.CalculatePath(meetingNode);
        }

        public List<NodeRecord> GetForwardPartialSolution()
        {
            return this.forwardSearch.GetPartialSolution();
        }

        public List<NodeRecord> GetBackwardPartialSolution()
        {
            return this.backwardSearch.GetPartialSolution();
        }
    }
}
