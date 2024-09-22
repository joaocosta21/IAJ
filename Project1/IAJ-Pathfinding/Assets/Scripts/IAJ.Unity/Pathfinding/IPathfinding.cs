using System.Collections.Generic;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;

namespace Assets.Scripts.IAJ.Unity.Pathfinding
{
    public interface IPathfinding
    {
        bool InProgress { get; set; }
        float TotalProcessingTime { get; set; }
        IOpenSet Open { get; }
        IClosedSet Closed { get; }
        IOpenSet Open2 { get; }
        IClosedSet Closed2 { get; }
        int MaxOpenNodes { get; }
        uint TotalProcessedNodes { get; }
        void InitializePathfindingSearch(int startX, int startY, int goalX, int goalY);
        bool Search(out List<NodeRecord> solution, bool partialPath);
        void Preprocess();
    }
}
