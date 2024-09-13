
using Assets.Scripts.Grid;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics
{
    public interface IHeuristic
    {
        float H(Node node, Node goalNode);
    }
}
