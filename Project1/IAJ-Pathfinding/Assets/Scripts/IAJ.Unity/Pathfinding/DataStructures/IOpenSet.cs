using Assets.Scripts.Grid;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public interface IOpenSet
    {
        void Clear();
        void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace);
        NodeRecord GetBestAndRemove();
        NodeRecord PeekBest();
        void Add(NodeRecord nodeRecord);
        void Remove(NodeRecord nodeRecord);
        //should return null if the node is not found
        NodeRecord Find(NodeRecord nodeRecord);
        //needed for visuals
        //bool InOpenSet(Node node);
        ICollection<NodeRecord> All();
        int CountOpen();
    }
}
