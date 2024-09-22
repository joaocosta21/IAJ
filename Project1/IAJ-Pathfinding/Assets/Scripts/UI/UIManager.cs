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

                        if (manager.pathfinding is BiDirectionalAStarPathfinding biDirectionalAStar)
                        {
                            var forwardOpenNode = manager.pathfinding.Open.Find(nodeRecord);
                            var backwardOpenNode = manager.pathfinding.Open2.Find(nodeRecord);
                            var forwardClosedNode = manager.pathfinding.Closed.Find(nodeRecord);
                            var backwardClosedNode = manager.pathfinding.Closed2.Find(nodeRecord);

                            // Show forward search data
                            if (forwardOpenNode != null)
                            {
                                debugG.text = "Forward G:" + forwardOpenNode.gCost;
                                debugF.text = "Forward F:" + forwardOpenNode.fCost;
                                debugH.text = "Forward In Open \n H:" + forwardOpenNode.hCost;
                            }
                            if (forwardClosedNode != null)
                            {
                                debugG.text = "Forward G:" + forwardClosedNode.gCost;
                                debugF.text = "Forward F:" + forwardClosedNode.fCost;
                                debugH.text = "Forward In Closed \n H:" + forwardClosedNode.hCost;
                            }

                            // Show backward search data
                            if (backwardOpenNode != null)
                            {
                                debugG.text += "\nBackward G:" + backwardOpenNode.gCost;
                                debugF.text += "\nBackward F:" + backwardOpenNode.fCost;
                                debugH.text += "\nBackward In Open \n H:" + backwardOpenNode.hCost;
                            }
                            if (backwardClosedNode != null)
                            {
                                debugG.text += "\nBackward G:" + backwardClosedNode.gCost;
                                debugF.text += "\nBackward F:" + backwardClosedNode.fCost;
                                debugH.text += "\nBackward In Closed \n H:" + backwardClosedNode.hCost;
                            }

                            if (forwardOpenNode == null && forwardClosedNode == null && backwardOpenNode == null && backwardClosedNode == null)
                            {
                                debugG.text = "G: ? (Not in Forward or Backward)";
                                debugF.text = "F: ? (Not in Forward or Backward)";
                                debugH.text = "H: ? (Not in Forward or Backward)";
                            }
                        }
                        else
                        {
                            // Handle single A* case
                            var openNode = manager.pathfinding.Open.Find(nodeRecord);
                            var closedNode = manager.pathfinding.Closed.Find(nodeRecord);

                            if (openNode != null)
                            {
                                debugG.text = "G:" + openNode.gCost;
                                debugF.text = "F:" + openNode.fCost;
                                debugH.text = "In Open \n H:" + openNode.hCost;
                            }
                            if (closedNode != null)
                            {
                                debugG.text = "G:" + closedNode.gCost;
                                debugF.text = "F:" + closedNode.fCost;
                                debugH.text = "In Closed \n H:" + closedNode.hCost;
                            }
                            if (closedNode == null && openNode == null)
                            {
                                debugG.text = "G: ? (Not in Open or Closed)";
                                debugF.text = "F: ? (Not in Open or Closed)";
                                debugH.text = "H: ? (Not in Open or Closed)";
                            }
                        }

                        debugWalkable.text = "IsWalkable:" + node.isWalkable;
                        debugDArray.text = String.Empty;
                    }
                }
            }
        }

        // Show pathfinding statistics
        debugMaxNodes.text = "MaxOpenNodes: " + manager.pathfinding.MaxOpenNodes;
        debugtotalProcessedNodes.text = "TotalPNodes: " + manager.pathfinding.TotalProcessedNodes;
        debugtotalProcessingTime.text = "TotalPTime: " + manager.pathfinding.TotalProcessingTime;
    }

}
