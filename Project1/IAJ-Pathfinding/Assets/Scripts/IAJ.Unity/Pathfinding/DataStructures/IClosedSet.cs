using Assets.Scripts.Grid;
using System.Collections.Generic;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public interface IClosedSet
    {
        void Clear();
        void Add(NodeRecord nodeRecord);
        void Remove(NodeRecord nodeRecord);
        //should return null if the node is not found
        NodeRecord Find(NodeRecord nodeRecord);
        //needed for visuals
        ICollection<NodeRecord> All();
    }
}