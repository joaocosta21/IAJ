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
        private Node meetingNode;
        
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

            // Check if there's a meeting node between the two searches
            Node meetingNode = FindMeetingNode();
            
            // If a meeting node is found, combine the paths
            if (meetingNode != null)
            {
                solution = CombinePaths(forwardSolution, backwardSolution, meetingNode);
                this.InProgress = false;
                return true;
            }

            // If both forward and backward searches have finished (no meeting node found earlier)
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

        private Node FindMeetingNode()
        {
            foreach (var nodeRecord in this.forwardSearch.Closed.All())
            {
                if (this.backwardSearch.Closed.Find(nodeRecord) != null)
                {
                    return nodeRecord.Node; 
                }
            }

            return null;
        }

        public void Preprocess()
        {
            // No preprocessing for BiDirectional A*, but it could be added if necessary
        }

        private List<NodeRecord> CombinePaths(List<NodeRecord> forwardPath, List<NodeRecord> backwardPath, Node meetingNode)
        {
            List<NodeRecord> path = new List<NodeRecord>();

            foreach (var nodeRecord in forwardPath)
            {
                path.Add(nodeRecord);
                if (nodeRecord.Node == meetingNode) break;
            }

            for (int i = backwardPath.Count - 1; i >= 0; i--)
            {
                if (backwardPath[i].Node == meetingNode) continue;
                path.Add(backwardPath[i]);
            }

            return path;
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
