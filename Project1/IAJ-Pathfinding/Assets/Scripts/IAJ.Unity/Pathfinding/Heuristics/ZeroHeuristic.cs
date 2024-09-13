using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public class ZeroHeuristic : IHeuristic
    {
        public float H(Node node, Node goalNode)
        {
            return 0;
        }
    }
}
