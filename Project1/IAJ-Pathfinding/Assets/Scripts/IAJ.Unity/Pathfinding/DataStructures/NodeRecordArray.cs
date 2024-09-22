using System.Collections.Generic;
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using UnityEngine;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using System.Runtime.CompilerServices;
using System;
using UnityEditor.Experimental.GraphView;
using Node = Assets.Scripts.Grid.Node;
using UnityEditor.MemoryProfiler;
using Connection = Assets.Scripts.Grid.Connection;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures
{
    public class NodeRecordArray : IOpenSet, IClosedSet
    {
        private NodeRecord[] NodeRecords { get; set; }
        private NodePriorityHeap openHeap { get; set; }

        public NodeRecordArray(List<Node> nodes)
        {
            var totalNodes = nodes.Count;
            NodeRecords = new NodeRecord[totalNodes];
            for (int i = 0; i < totalNodes; i++)
            {
                nodes[i].Index = i;
                NodeRecords[i] = new NodeRecord(nodes[i]);
                NodeRecords[i].Reset();
            }
            this.openHeap = new NodePriorityHeap();
        }

        void IOpenSet.Clear()
        {
            this.openHeap.Clear();
            for (int i = 0; i < this.NodeRecords.Length; i++)
            {
                if(NodeRecords[i].Node.isWalkable)
                    this.NodeRecords[i].Category = NodeCategory.Unvisited;
            }
        }

        void IClosedSet.Clear()
        {
            return;
        }

        public NodeRecord GetNodeRecordByIndex(int index)
        {
            return NodeRecords[index];
        }

        void IOpenSet.Add(NodeRecord nodeRecord)
        {
            nodeRecord.Category = NodeCategory.Open;
            openHeap.Add(nodeRecord);
        }

        public NodeRecord GetBestAndRemove()
        {
            var bestNode = openHeap.GetBestAndRemove();
            bestNode.Category = NodeCategory.Closed;
            return bestNode;
        }

        NodeRecord IOpenSet.Find(NodeRecord nodeRecord)
        {   
            return GetNodeRecordByIndex(nodeRecord.Node.Index);
        }

        public void Replace(NodeRecord nodeToBeReplaced, NodeRecord nodeToReplace)
        {
            openHeap.Replace(nodeToBeReplaced, nodeToReplace);
        }

        public int CountOpen()
        {
            return openHeap.CountOpen();
        }

        void IClosedSet.Add(NodeRecord nodeRecord)
        {
            nodeRecord.Category = NodeCategory.Closed;
        }

        NodeRecord IClosedSet.Find(NodeRecord nodeRecord)
        {
            return nodeRecord.Category == NodeCategory.Closed ? nodeRecord : null;
        }

        public NodeRecord PeekBest()
        {
            return openHeap.PeekBest();
        }

        public void Remove(NodeRecord nodeRecord)
        {
            openHeap.Remove(nodeRecord);
        }

        ICollection<NodeRecord> IOpenSet.All()
        {
            return openHeap.All();
        }

        ICollection<NodeRecord> IClosedSet.All()
        {
            return null;
        }
    }
}
