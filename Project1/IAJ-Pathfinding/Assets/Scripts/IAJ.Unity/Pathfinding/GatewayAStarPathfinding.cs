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
        private Dictionary<(Node, Node), float> gatewayDistances;
        private List<Node> gateways;
        private int[,] zones;
        private int currentZoneID;

        private Dictionary<Node, Node> nearestGatewayMap;
        
        private float[,,] gatewayDistanceMatrix;

        private IHeuristic heuristic;

        public GatewayAStarPathfinding(IGraph grid, IHeuristic heuristic, float tieBreakingWeight = 0.0f)
            : base(grid, null, null, heuristic, tieBreakingWeight)
        {
            this.Open = new NodePriorityHeap();
            this.Closed = new ClosedDictionary();
            this.gateways = new List<Node>();
            this.heuristic = heuristic;
            this.nearestGatewayMap = new Dictionary<Node, Node>();
            this.gatewayDistances = new Dictionary<(Node, Node), float>();
            this.TieBreakingWeight = tieBreakingWeight;
            this.pathfindingManager = GameObject.FindObjectOfType<PathfindingManager>();
        }

        public override void Preprocess()
        {
            DecomposeMapIntoZones();
            IdentifyGateways();
            PrecomputeGatewayDistances();
            PrecomputeNearestGateways();
            // PrintZones();
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

            // PrintZones();
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

        private void PrecomputeNearestGateways()
        {
            foreach (Node node in gridGraph.AllNodes())
            {
                Node nearestGateway = null;
                float minDistance = float.MaxValue;

                foreach (Node gateway in gateways)
                {
                    float distance = OctileDistance(node, gateway);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestGateway = gateway;
                    }
                }

                if (nearestGateway != null)
                {
                    nearestGatewayMap[node] = nearestGateway;
                }
            }
        }

        private void PrecomputeGatewayDistances()
        {
            int gatewayCount = gateways.Count;
            gatewayDistanceMatrix = new float[gatewayCount, gatewayCount, 4];

            for (int i = 0; i < gatewayCount; i++)
            {
                for (int j = i + 1; j < gatewayCount; j++)
                {
                    if (zones[gateways[i].x, gateways[i].y] == zones[gateways[j].x, gateways[j].y])
                    {
                        // Calculate distances using A* or Dijkstra
                        float distanceIJ = CalculateDistanceBetweenGateways(gateways[i], gateways[j]);
                        float distanceJI = CalculateDistanceBetweenGateways(gateways[j], gateways[i]);

                        // Store four costs
                        gatewayDistanceMatrix[i, j, 0] = distanceIJ;
                        gatewayDistanceMatrix[j, i, 1] = distanceJI;
                        gatewayDistanceMatrix[i, j, 2] = distanceJI; // Reverse (G_i to G_j)
                        gatewayDistanceMatrix[j, i, 3] = distanceIJ; // Reverse (G_j to G_i)
                    }
                    else
                    {
                        // Assign a very high distance if no path exists
                        for (int k = 0; k < 4; k++)
                        {
                            gatewayDistanceMatrix[i, j, k] = float.MaxValue;
                            gatewayDistanceMatrix[j, i, k] = float.MaxValue;
                        }
                    }
                }
            }
        }


        private float CalculateDistanceBetweenGateways(Node start, Node end)
        {
            AStarPathfinding pathfinding = new AStarPathfinding(gridGraph, new NodePriorityHeap(), new ClosedDictionary(), this.heuristic);

            pathfinding.StartNode = start;
            pathfinding.GoalNode = end;

            gridGraph.GetNode(start.x, start.y).isWalkable = false;
            gridGraph.GetNode(end.x, end.y).isWalkable = false;

            List<NodeRecord> solution;
            bool pathFound = pathfinding.Search(out solution);

            gridGraph.GetNode(start.x, start.y).isWalkable = true;
            gridGraph.GetNode(end.x, end.y).isWalkable = true;

            if (pathFound && solution != null)
            {
                float totalDistance = 0;
                foreach (var nodeRecord in solution)
                {
                    totalDistance += nodeRecord.gCost;
                }
                return totalDistance;
            }

            return float.MaxValue;
        }

        private Node FindNearestGateway(Node node)
        {
            Node nearestGateway = null;
            float minDistance = float.MaxValue;

            int currentZone = zones[node.x, node.y];

            foreach (var gateway in gateways)
            {
                if (zones[gateway.x, gateway.y] == currentZone)
                {
                    float distance = this.Heuristic.H(node, gateway);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestGateway = gateway;
                    }
                }
            }
            return nearestGateway;
        }


        public float CalculateHeuristic(Node currentNode, Node goalNode)
        {
            Node nearestGatewayToStart = nearestGatewayMap.ContainsKey(currentNode) ? nearestGatewayMap[currentNode] : null;
            Node nearestGatewayToGoal = nearestGatewayMap.ContainsKey(goalNode) ? nearestGatewayMap[goalNode] : null;

            float directHeuristic = this.heuristic.H(currentNode, goalNode);

            if (nearestGatewayToStart == null || nearestGatewayToGoal == null)
            {
                return directHeuristic;
            }

            int startGatewayIndex = gateways.IndexOf(nearestGatewayToStart);
            int goalGatewayIndex = gateways.IndexOf(nearestGatewayToGoal);

            float gatewayDistance = gatewayDistanceMatrix[startGatewayIndex, goalGatewayIndex, 0]; // Default path

            // If you want to choose between multiple costs based on context (e.g., directions)
            // you can refine this logic by considering the node's direction, current area, etc.
            // For simplicity, using one of the precomputed distances here.
            float startToGatewayDistance = OctileDistance(currentNode, nearestGatewayToStart);
            float goalToGatewayDistance = OctileDistance(nearestGatewayToGoal, goalNode);

            float gatewayHeuristic = startToGatewayDistance + gatewayDistance + goalToGatewayDistance;

            return Mathf.Min(directHeuristic, gatewayHeuristic);
        }


        public override bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false)
        {
            return base.Search(out solution, returnPartialSolution);
        }

        protected override void ProcessChildNode(NodeRecord parentNode, Connection connection)
        {
            Node childNode = connection.ToNode;
            float newGCost = parentNode.gCost + connection.Cost;

            float hCost = CalculateHeuristic(childNode, this.GoalNode);

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
        private float OctileDistance(Node n, Node gateway)
        {
            float dx = Mathf.Abs(n.x - gateway.x);
            float dy = Mathf.Abs(n.y - gateway.y);
            return (dx > dy) ? dx + (dy * (Mathf.Sqrt(2) - 1)) : dy + (dx * (Mathf.Sqrt(2) - 1));
        }

    }
}