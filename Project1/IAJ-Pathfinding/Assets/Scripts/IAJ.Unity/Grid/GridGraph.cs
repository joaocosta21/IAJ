using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.Grid.GridGraph;

namespace Assets.Scripts.Grid
{
    public enum NeighbourhoodType
    {
        VonNeumann, // 4 neighbours
        Moore      // 8 neighbours
    };
    public class GridGraph : IGraph
    {
        public NeighbourhoodType neighbourhoodType { get; set; }

        // Cost of moving through the grid
        protected const float MOVE_STRAIGHT_COST = 1;
        protected const float MOVE_DIAGONAL_COST = 1.5f;

        public Grid<Node> grid;

        public GridGraph(NeighbourhoodType neighbourhoodType)
        {
            
            this.grid = new Grid<Node>((Grid<Node> global, int x, int y) => new Node(x, y));
            this.neighbourhoodType = neighbourhoodType;
        }

        public List<Connection> GetConnections(Node fromNode)
        {
            var connections = new List<Connection>();

            int x = fromNode.x; int y = fromNode.y;

            // Check Neumann (4-directional)
            if (neighbourhoodType == NeighbourhoodType.VonNeumann)
            {
                // Up
                if (y + 1 < grid.Height && grid.GetGridObject(x, y + 1).isWalkable)
                    connections.Add(new Connection(fromNode, grid.GetGridObject(x, y+1), MOVE_STRAIGHT_COST));

                // Down
                if (y - 1 >= 0 && grid.GetGridObject(x, y - 1).isWalkable)
                    connections.Add(new Connection(fromNode, grid.GetGridObject(x, y-1), MOVE_STRAIGHT_COST));

                // Left
                if (x - 1 >= 0 && grid.GetGridObject(x-1, y).isWalkable)
                    connections.Add(new Connection(fromNode, grid.GetGridObject(x - 1, y), MOVE_STRAIGHT_COST));

                // Right
                if (x + 1 < grid.Width && grid.GetGridObject(x + 1, y).isWalkable)
                    connections.Add(new Connection(fromNode, grid.GetGridObject(x + 1, y), MOVE_STRAIGHT_COST));
            }
            else if (neighbourhoodType == NeighbourhoodType.Moore)
            {
                // Moore (8-directional)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        // Skip the center (current node)
                        if (dx == 0 && dy == 0) continue;

                        int newX = x + dx;
                        int newY = y + dy;

                        var newNode = grid.GetGridObject(newX, newY);

                        // Check bounds
                        if (newX >= 0 && newX < grid.Width && newY >= 0 && newY < grid.Height && newNode.isWalkable)
                        {
                            connections.Add(new Connection(fromNode, grid[newX, newY], GetCost(fromNode, newNode)));
                        }
                    }
                }
            }

            return connections;
        }
        //returns 0 if the coordinates coincide, and PositiveInfinity if the nodes are not adjacent
        //or one of them is not walkable
        public float GetCost(Node fromNode, Node toNode)
        {
            if (!fromNode.isWalkable || !toNode.isWalkable) return float.PositiveInfinity;

            var deltaX = Math.Abs(toNode.x - fromNode.x); 
            var deltaY = Math.Abs(toNode.y - fromNode.y);

            if (deltaX + deltaY == 0) return 0.0f;
            else if (deltaY + deltaX == 1) return MOVE_STRAIGHT_COST;
            else if (deltaX == 1 && deltaY == 1) return MOVE_DIAGONAL_COST;
            else return float.PositiveInfinity;
        }

        public Node GetNode(int x, int y) {  return grid[x, y]; }
        //GetGridObject(GoalPositionX, GoalPositionY);







    }
}