using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using CodeMonkey.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    //Pathfinding Manager reference
    [HideInInspector]
    public PathfindingManager manager;

    //Debug Components you can add your own here
    Text debugCoordinates;
    Text debugG;
    Text debugF;
    Text debugH;
    Text debugWalkable;
    Text debugtotalProcessedNodes;
    Text debugtotalProcessingTime;
    Text debugMaxNodes;
    Text debugDArray;
    Text debugBounds;

    bool useGoal;
    
    private int currentX, currentY;
    VisualGridManager visualGrid;

    // Start is called before the first frame update
    void Start()
    {

        // Simple way of getting the manager's reference
        manager = GameObject.FindObjectOfType<PathfindingManager>();
        visualGrid = GameObject.FindObjectOfType<VisualGridManager>();

        // Retrieving the Debug Components
        var debugTexts = this.transform.GetComponentsInChildren<Text>();
        debugCoordinates = debugTexts[0];
        debugH = debugTexts[1];
        debugG = debugTexts[2];
        debugF = debugTexts[3];
        debugtotalProcessedNodes = debugTexts[4];
        debugtotalProcessingTime = debugTexts[5];
        debugMaxNodes = debugTexts[6];
        debugWalkable = debugTexts[7];
        debugDArray = debugTexts[8];
        useGoal = manager.aStarType==PathfindingManager.AStarType.NodeArrayGoalBounding;
        currentX = -2;
        currentY = -2;
    }

    // Update is called once per frame
    void Update()
    {
        // A Long way of printing useful information regarding the algorithm
        var currentPosition = UtilsClass.GetMouseWorldPosition();
        if (currentPosition != null)
        {
            int x, y;
            if (manager.gridGraph.grid != null)
            {
                manager.gridGraph.grid.GetXY(currentPosition, out x, out y);

                currentX = x;
                currentY = y;
                if (x != -1 && y != -1)
                {
                    var node = manager.gridGraph.grid.GetGridObject(x, y);
                    if (node != null)
                    {
                        debugCoordinates.text = " x:" + x + "; y:" + y;
                        var nodeRecord = new NodeRecord(node);
                        var openNode = manager.pathfinding.Open.Find(nodeRecord);
                        if (openNode != null)
                        {
                            debugG.text = "G:" + openNode.gCost;
                            debugF.text = "F:" + openNode.fCost;
                            debugH.text = "In Open \n H:" + openNode.hCost;
                        }
                        var closedNode = manager.pathfinding.Closed.Find(nodeRecord);
                        if (closedNode != null)
                        {
                            debugG.text = "G:" + closedNode.gCost;
                            debugF.text = "F:" + closedNode.fCost;
                            debugH.text = "In Closed \n H:" + closedNode.hCost;
                        }
                        if (closedNode != null && openNode != null)
                        {
                            debugG.text = "G:" + closedNode.gCost;
                            debugF.text = "F:" + closedNode.fCost;
                            debugH.text = "In Closed and Open!!!! \n H:" + closedNode.hCost;
                        }
                        if (closedNode == null && openNode == null)
                        {
                            debugG.text = "G: ?";
                            debugF.text = "F: ?";
                            debugH.text = "Neither in Closed nor Open!!!! \n H: ?";
                        }

                        debugWalkable.text = "IsWalkable:" + node.isWalkable;
                        debugDArray.text = String.Empty;
                        /*if (node.isWalkable)
                        {
                           if (useGoal)
                            {
                                var ss = new StringBuilder();
                                var goalBoundingPathfinder = (GoalBoundAStarPathfinding)manager.pathfinding;
                                var bounds = goalBoundingPathfinder.goalBounds[x, y];
                                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                                {
                                    var bound = bounds[(int)dir];
                                    if(bound.init)
                                    {
                                        ss.Append(Enum.GetName(typeof(Direction), dir));
                                        ss.Append(": ");
                                        ss.Append(" x1=");
                                        ss.Append(bound.x1);
                                        ss.Append(" y1=");
                                        ss.Append(bound.y1);

                                        ss.Append(" x2=");
                                        ss.Append(bound.x2);
                                        ss.Append(" y2=");
                                        ss.Append(bound.y2);

                                        ss.AppendLine();

                                        debugDArray.text = ss.ToString();
                                    }
                                }

                            }

                        }*/
                    }
                }

            }
        }

        // if manager finished search display statistics from the search
        // if (!this.manager.pathfinding.InProgress)
        {
                debugMaxNodes.text = "MaxOpenNodes:" + manager.pathfinding.MaxOpenNodes;
                debugtotalProcessedNodes.text = "TotalPNodes:" + manager.pathfinding.TotalProcessedNodes;
                debugtotalProcessingTime.text = "TotalPTime:" + manager.pathfinding.TotalProcessingTime;
            }
        }
}
