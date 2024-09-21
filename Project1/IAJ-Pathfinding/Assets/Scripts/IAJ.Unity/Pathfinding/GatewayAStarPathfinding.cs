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

        // Preprocess the grid to identify zones and gateways, and precompute distances between gateways
        public override void Preprocess()
        {
            DecomposeMapIntoZones();
            IdentifyGateways();

            foreach (var gateway in gateways)
            {
                foreach (var otherGateway in gateways)
                {
                    if (gateway != otherGateway)
                    {
                        float distance = CalculateDistanceBetweenGateways(gateway, otherGateway);
                        gatewayDistances[(gateway, otherGateway)] = distance;
                        Debug.Log($"Precomputed distance between {gateway} and {otherGateway}: {distance}");
                    }
                }
            }
        }

        private void DecomposeMapIntoZones()
        {
            zones = new int[gridGraph.grid.Width, gridGraph.grid.Height];
            currentZoneID = 1;

            for (int x = 0; x < gridGraph.grid.Width; x++)
            {
                for (int y = 0; y < gridGraph.grid.Height; y++)
                {
                    Node node = gridGraph.GetNode(x, y);

                    if (!node.isWalkable || zones[x, y] != 0) continue;

                    // Start flood-fill for a new zone
                    FloodFill(node, currentZoneID);
                    currentZoneID++;
                }
            }
            for (int x = 0; x < gridGraph.grid.Width; x++)
            {
                for (int y = 0; y < gridGraph.grid.Height; y++)
                {
                    Debug.Log($"Processing node {zones[x, y]} at ({x}, {y})");
                }
            }
            Debug.Log($"Decomposed map into {zones} zones");
        }

        private void FloodFill(Node startNode, int zoneID)
        {
            Queue<(int xLeft, int y)> openSet = new Queue<(int xLeft, int y)>();
            openSet.Enqueue((startNode.x, startNode.y));

            bool shrunkL = false, shrunkR = false;

            while (openSet.Count > 0)
            {
                (int xLeft, int y) = openSet.Dequeue();

                // Ensure y is within bounds (avoid accessing negative y values)
                if (y < 0 || y >= gridGraph.grid.Height) continue;

                // Go right until a wall or open upward space is encountered
                int x = xLeft;
                while (x < gridGraph.grid.Width && gridGraph.GetNode(x, y).isWalkable && zones[x, y] == 0 && 
                    (y == 0 || !gridGraph.GetNode(x, y - 1).isWalkable)) // Ensure y-1 is valid (upward check)
                {
                    zones[x, y] = zoneID;
                    x++;
                }
                int xRight = x - 1; // Store the farthest right point we've reached

                // Handle the shrinking on the right border (check if y - 1 is within bounds)
                if (y > 0 && x < gridGraph.grid.Width && gridGraph.GetNode(x, y - 1).isWalkable && zones[x, y - 1] == zoneID)
                {
                    shrunkR = true;
                }
                else if (shrunkR && gridGraph.GetNode(xRight, y - 1).isWalkable && zones[xRight, y - 1] != zoneID)
                {
                    // Undo the fill for this row if right shrunk
                    while (xRight >= xLeft)
                    {
                        zones[xRight, y] = 0;
                        xRight--;
                    }
                    continue;
                }

                // Move to the next row (y + 1)
                int nextY = y + 1;
                if (nextY < gridGraph.grid.Height)
                {
                    // Find the new left boundary
                    while (xLeft < gridGraph.grid.Width && (!gridGraph.GetNode(xLeft, nextY).isWalkable || zones[xLeft, nextY - 1] != zoneID))
                    {
                        xLeft++;
                    }

                    // Handle left border shrinking (check if y - 1 is within bounds)
                    if (xLeft > 0 && y > 0 && gridGraph.GetNode(xLeft - 1, nextY - 1).isWalkable && zones[xLeft - 1, nextY - 1] == zoneID)
                    {
                        shrunkL = true;
                    }
                    else if (shrunkL && y > 0 && gridGraph.GetNode(xLeft, nextY - 1).isWalkable && zones[xLeft, nextY - 1] != zoneID)
                    {
                        // Stop further filling if the left side is shrinking
                        continue;
                    }

                    openSet.Enqueue((xLeft, nextY));
                }
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
                        gateways.Add(node);
                    }
                }
            }
        }

        // Check if a node is a gateway between different zones
        private bool IsGateway(Node node)
        {
            int x = node.x;
            int y = node.y;

            int currentZone = zones[x, y];
            foreach (var connection in gridGraph.GetConnections(node))
            {
                Node neighbor = connection.ToNode;
                if (zones[neighbor.x, neighbor.y] != currentZone)
                {
                    return true;
                }
            }

            return false;
        }

        // Precompute the distance between two gateways
        private float CalculateDistanceBetweenGateways(Node start, Node end)
        {
            // You can use A* or another method to compute the distance
            return gridGraph.GetCost(start, end);
        }

        public override bool Search(out List<NodeRecord> solution, bool returnPartialSolution = false)
        {
            return base.Search(out solution, returnPartialSolution);
        }

        protected override void ProcessChildNode(NodeRecord parentNode, Connection connection)
        {
            Node childNode = connection.ToNode;

            // Use the Gateway Heuristic to calculate the heuristic cost (hCost)
            Node nearestGatewayToStart = FindNearestGateway(parentNode.Node);
            Node nearestGatewayToGoal = FindNearestGateway(GoalNode);

            float gatewayDistance = gatewayDistances[(nearestGatewayToStart, nearestGatewayToGoal)];
            float hCost = HeuristicMultiplier * (gatewayDistance + this.Heuristic.H(nearestGatewayToGoal, GoalNode));

            // Create a NodeRecord for the child node and proceed with regular A* logic
            float newGCost = parentNode.gCost + connection.Cost;
            NodeRecord childNodeRecord = new NodeRecord(childNode)
            {
                gCost = newGCost,
                hCost = hCost,
                parent = parentNode
            };

            childNodeRecord.CalculateFCost(TieBreakingWeight);

            // Handle open and closed set logic as in regular A*
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

        // Finds the nearest gateway to a given node
        private Node FindNearestGateway(Node node)
        {
            Node nearestGateway = null;
            float minDistance = float.MaxValue;

            // Loop through all gateways and find the nearest one to the given node
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
    }
}
