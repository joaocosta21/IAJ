using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    class ClosedDictionary : IClosedSet
    {

        //Tentative dictionary type structure, it is possible that there are better solutions...
        private Dictionary<Vector2, NodeRecord> Closed { get; set; }

        public ClosedDictionary()
        {
            this.Closed = new Dictionary<Vector2, NodeRecord>();
        }

        public void Initialize()
        {
            this.Closed.Clear();
        }

        public void Add(NodeRecord nodeRecord)
        {
            Vector2 position = new Vector2(nodeRecord.Node.x, nodeRecord.Node.y);
            if (!Closed.ContainsKey(position)) // Only add if not already present
            {
                Closed.Add(position, nodeRecord);
            }
        }

        public void Remove(NodeRecord nodeRecord)
        {
            Vector2 position = new Vector2(nodeRecord.Node.x, nodeRecord.Node.y);
            if (Closed.ContainsKey(position)) 
            {
                Closed.Remove(position);
            }
        }

        public NodeRecord Find(NodeRecord nodeRecord)
        {
            Vector2 position = new Vector2(nodeRecord.Node.x, nodeRecord.Node.y);
            if (Closed.TryGetValue(position, out NodeRecord foundNodeRecord)) 
            {
                return foundNodeRecord;
            }
            return null;
        }

        public ICollection<NodeRecord> All()
        {
            return Closed.Values;
        }
        public void Clear()
        {
            Closed.Clear();
        }
    }
}

