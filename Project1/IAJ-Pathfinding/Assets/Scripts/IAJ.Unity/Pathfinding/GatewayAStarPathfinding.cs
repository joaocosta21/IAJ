using System.Collections.Generic;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public class GatewayAStarPathfinding : AStarPathfinding
    {
        private PathfindingManager pathfindingManager;
        private Dictionary<(Node, Node), float> gatewayDistances; // Store precomputed distances between gateways
        private List<Node> gateways; // List of gateways
        private int[,] zones; // Store zone information for each node in the grid
        private int currentZoneID;

        // Optimization: Precomputed nearest gateway for each node
        private Dictionary<Node, Node> nearestGatewayMap;
        
        // Optimization: Gateway-to-gateway distance matrix
        private float[,] gatewayDistanceMatrix;

        public GatewayAStarPathfinding(IGraph grid, IHeuristic heuristic, float tieBreakingWeight = 0.0f)
            : base(grid, null, null, heuristic, tieBreakingWeight)
        {
            this.Open = new NodePriorityHeap();
            this.Closed = new ClosedDictionary();
            this.gateways = new List<Node>();
            this.gatewayDistances = new Dictionary<(Node, Node), float>();
            this.TieBreakingWeight = tieBreakingWeight;
            this.pathfindingManager = GameObject.FindObjectOfType<PathfindingManager>();
        }

        // Preprocess the grid to identify zones, gateways, and precompute distances
        public override void Preprocess()
        {
            DecomposeMapIntoZones();
            IdentifyGateways();
            PrecomputeGatewayDistances();
            PrecomputeNearestGateways();
            PrintZones();
        }

        private void DecomposeMapIntoZones()
        {
            zones = new int[gridGraph.grid.Width, gridGraph.grid.Height];
            int currZoneID = 1;

            while (true)
            {
                Node topLeftFreeNode = GetTopLeftFreeNode();
                if (topLeftFreeNode == null) break;

                int xLeft = topLeftFreeNode.x;
                int y = topLeftFreeNode.y;
                bool shrunkR = false, shrunkL = false;

                while (true)
                {
                    int x = xLeft;
                    if (gridGraph.GetNode(x, y).isWalkable) zones[x, y] = currZoneID;

                    while (x + 1 < gridGraph.grid.Width && y + 1 < gridGraph.grid.Height &&
                           gridGraph.GetNode(x + 1, y).isWalkable &&
                           (!gridGraph.GetNode(x + 1, y + 1).isWalkable || zones[x + 1, y + 1] != 0))
                    {
                        x = x + 1;
                        zones[x, y] = currZoneID;
                    }

                    if (y + 1 < gridGraph.grid.Height && x + 1 < gridGraph.grid.Width && zones[x + 1, y + 1] == currZoneID)
                    {
                        shrunkR = true;
                    }
                    else if (y + 1 < gridGraph.grid.Height && zones[x, y + 1] != currZoneID && shrunkR)
                    {
                        while (zones[x, y] == currZoneID)
                        {
                            zones[x, y] = 0;
                            x = x - 1;
                        }
                        break;
                    }

                    x = xLeft;
                    y = y - 1;

                    if (y < 0) break;

                    while (x < gridGraph.grid.Width - 1 && (!gridGraph.GetNode(x, y).isWalkable && zones[x, y + 1] == currZoneID))
                    {
                        x = x + 1;
                    }

                    while (x > 0 && y + 1 < gridGraph.grid.Height && gridGraph.GetNode(x - 1, y).isWalkable &&
                           (!gridGraph.GetNode(x - 1, y + 1).isWalkable || zones[x - 1, y + 1] != 0))
                    {
                        x = x - 1;
                    }

                    if (y + 1 < gridGraph.grid.Height && x > 0 && zones[x - 1, y + 1] == currZoneID)
                    {
                        shrunkL = true;
                    }
                    else if (y + 1 < gridGraph.grid.Height && zones[x, y + 1] != currZoneID && shrunkL)
                    {
                        break;
                    }
                }

                currZoneID++;
            }

            PrintZones();
        }

        private Node GetTopLeftFreeNode()
        {
            for (int y = gridGraph.grid.Height - 1; y >= 0; y--)
            {
                for (int x = 0; x < gridGraph.grid.Width; x++)
                {
                    if (zones[x, y] == 0 && gridGraph.GetNode(x, y).isWalkable)
                    {
                        return gridGraph.GetNode(x, y);
                    }
                }
            }
            return null;
        }

        private void PrintZones()
        {
            for (int y = gridGraph.grid.Height - 1; y >= 0; y--)
            {
                string line = "";
                for (int x = 0; x < gridGraph.grid.Width; x++)
                {
                    line += zones[x, y].ToString("D2") + " ";
                }
                Debug.Log(line);
            }
        }

        private void IdentifyGateways()
        {
            for (int x = 1; x < gridGraph.grid.Width - 1; x++)
            {
                for (int y = 1; y < gridGraph.grid.Height - 1; y++)
                {
                    Node node = gridGraph.GetNode(x, y);
                    if (!node.isWalkable) continue;

                    if (IsGateway(node))
                    {
                        node.status = VisualNodeStatus.Open;
                        gateways.Add(node);
                    }
                }
            }
        }

        private bool IsGateway(Node node)
        {
            int x = node.x;
            int y = node.y;
            int currentZone = zones[x, y];
            HashSet<int> neighborZones = new HashSet<int>();

            foreach (var connection in gridGraph.GetConnections(node))
            {
                Node neighbor = connection.ToNode;
                if (neighbor.isWalkable && zones[neighbor.x, neighbor.y] != currentZone)
                {
                    neighborZones.Add(zones[neighbor.x, neighbor.y]);
                }
            }

            return neighborZones.Count > 0;
        }

        // Optimization: Precompute nearest gateway for each node
        private void PrecomputeNearestGateways()
        {
            nearestGatewayMap = new Dictionary<Node, Node>();
            foreach (var node in gridGraph.AllNodes())
            {
                if (!node.isWalkable) continue;

                Node nearestGateway = FindNearestGateway(node);
                nearestGatewayMap[node] = nearestGateway;
            }
        }

        // Optimization: Precompute gateway-to-gateway distances using Floyd-Warshall or Dijkstra
        private void PrecomputeGatewayDistances()
        {
            int gatewayCount = gateways.Count;
            gatewayDistanceMatrix = new float[gatewayCount, gatewayCount];

            for (int i = 0; i < gatewayCount; i++)
            {
                for (int j = i + 1; j < gatewayCount; j++)
                {
                    float distance = CalculateDistanceBetweenGateways(gateways[i], gateways[j]);
                    gatewayDistanceMatrix[i, j] = distance;
                    gatewayDistanceMatrix[j, i] = distance;
                }
            }
        }

        private float CalculateDistanceBetweenGateways(Node start, Node end)
        {
            float dx = start.x - end.x;
            float dy = start.y - end.y;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        private Node FindNearestGateway(Node node)
        {
            Node nearestGateway = null;
            float minDistance = float.MaxValue;

            foreach (var gateway in gateways)
            {
                float distance = this.Heuristic.H(node, gateway);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestGateway = gateway;
                }
            }

            return nearestGateway;
        }

        // Optimized heuristic calculation
        private float CalculateHeuristic(Node currentNode, Node goalNode)
        {
            Node nearestGatewayToStart = nearestGatewayMap[currentNode];
            Node nearestGatewayToGoal = nearestGatewayMap[goalNode];

            int startGatewayIndex = gateways.IndexOf(nearestGatewayToStart);
            int goalGatewayIndex = gateways.IndexOf(nearestGatewayToGoal);

            float gatewayDistance = gatewayDistanceMatrix[startGatewayIndex, goalGatewayIndex];
            float startToGatewayDistance = this.Heuristic.H(currentNode, nearestGatewayToStart);
            float goalToGatewayDistance = this.Heuristic.H(goalNode, nearestGatewayToGoal);

            return startToGatewayDistance + gatewayDistance + goalToGatewayDistance;
        }

        public override bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false)
        {
            return base.Search(out solution, returnPartialSolution);
        }

        // Override the ProcessChildNode function to include optimized heuristic
        protected override void ProcessChildNode(NodeRecord parentNode, Connection connection)
        {
            Node childNode = connection.ToNode;
            float newGCost = parentNode.gCost + connection.Cost;

            // Use the optimized heuristic calculation
            float hCost = CalculateHeuristic(childNode, this.GoalNode);

            // Proceed with regular A* logic for node processing
            NodeRecord childNodeRecord = new NodeRecord(childNode)
            {
                gCost = newGCost,
                hCost = hCost,
                parent = parentNode
            };

            childNodeRecord.CalculateFCost(TieBreakingWeight);

            NodeRecord closedNode = this.Closed.Find(childNodeRecord);
            if (closedNode != null) return;

            NodeRecord openNode = Open.Find(childNodeRecord);
            if (openNode == null)
            {
                Open.Add(childNodeRecord);
            }
            else if (newGCost < openNode.gCost)
            {
                openNode.gCost = newGCost;
                openNode.parent = parentNode;
                openNode.CalculateFCost(TieBreakingWeight);
                Open.Replace(openNode, childNodeRecord);
            }
        }
    }
}
