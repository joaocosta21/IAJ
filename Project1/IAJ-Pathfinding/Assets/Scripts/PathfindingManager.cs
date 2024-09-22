using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using Assets.Scripts.Grid;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.IO;
using System;
using Assets.Scripts.IAJ.Unity.Pathfinding;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.Heuristics;
using UnityEngine.Networking;
using UnityEditor.Experimental.GraphView;
using Node = Assets.Scripts.Grid.Node;

public class PathfindingManager : MonoBehaviour
{

    //Struct for default positions
    [Serializable]
    public struct SearchPos
    {
        public Vector2 startingPos;
        public Vector2 goalPos;
    }

    //Struct to store default positions by grid
    [Serializable]
    public struct SearchPosPerGrid
    {
        public string gridName;
        public List<SearchPos> searchPos;
    }

    // "Default Positions are quite useful for testing"
    public List<SearchPosPerGrid> defaultPositions;


    [Serializable]
    private enum GridType
    {
        tinyGrid,
        smallGrid,
        mediumGrid,
        giantGrid
    };

    [Header("Grid Settings")]
    [Tooltip("Choose grid name to change grid")]
    [SerializeField]
    private GridType gridName;
    [Tooltip("Von Neumann means 4 neighbours, Moore 8")]
    public NeighbourhoodType neighbourhoodType;

    public enum AStarType
    {
        Vanilla,
        NodeArray,
        GatewayAstar,
        NodeArrayGoalBounding,
        BiDirectionalAStarPathfinding
    }

    public enum OpenSetType
    {
        SimpleUnordered,
        PriorityHeap
    }
    public enum ClosedSetType
    {
        SimpleUnordered,
        Dictionary
    }
    public enum Heuristics
    {
        ZeroHeuristic,
        EuclideanDistance,
        ManhattanDistance,
    }

    [Header("Pahfinding Settings")]
    [Tooltip("Add settings to your liking, useful for faster testing")]
    //public properties useful for testing, you can add other booleans here such as which heuristic to use
    public bool partialPath = false;
    public bool tieBreaking = true;
    public AStarType aStarType = AStarType.GatewayAstar;
    public Heuristics heuristics = Heuristics.EuclideanDistance;
    public OpenSetType openSetType = OpenSetType.SimpleUnordered;
    public ClosedSetType closedSetType = ClosedSetType.Dictionary;

    //Grid configuration
    public GridGraph gridGraph;

    public static int width;
    public static int height;
    public static float cellSize;
 
    //Essential Pathfind classes 
    public IPathfinding pathfinding { get; set; }

    //The Visual Grid
    private VisualGridManager visualGrid;
    private string[,] textLines;

    //Private fields for internal use only
    public static int startingX = -1;
    public static int startingY = -1;
    public static int goalX = -1;
    public static int goalY = -1;

    //Path
    List<NodeRecord> solution;

    private void Start()
    {
        // Finding reference of Visual Grid Manager
        visualGrid = GameObject.FindObjectOfType<VisualGridManager>();

        // Creating the Path for the Grid, reading the file and Creating it
        var gridPath = "Assets/Resources/Grid/" + gridName + ".txt";
        this.LoadGrid(gridPath);

        this.gridGraph = new GridGraph(neighbourhoodType);
        

        // Creating and Initializing the Pathfinding class, you can change the open, closed and heuristic sets here

        // SELECT VARIATION

        float tieBreakingWeight = tieBreaking ? 0.5f : 0f;

        IHeuristic heuristics;
        switch(this.heuristics)
        {
            case Heuristics.EuclideanDistance:
                heuristics = new EuclideanDistance();
                break;
            case Heuristics.ZeroHeuristic:
                heuristics = new ZeroHeuristic();
                break;
            case Heuristics.ManhattanDistance:
                heuristics = new ManhattanDistance();
                break;
            default:
                throw new Exception();
        }


        switch (this.aStarType)
        {
            case AStarType.Vanilla:
                IOpenSet openSet;
                switch (this.openSetType)
                {
                    case OpenSetType.SimpleUnordered:
                        openSet = new SimpleUnorderedNodeList();
                        break;
                    case OpenSetType.PriorityHeap:
                        openSet = new NodePriorityHeap();
                        break;
                    default:
                        throw new Exception();
                }
                IClosedSet closedSet;
                switch (this.closedSetType)
                {
                    case ClosedSetType.SimpleUnordered:
                        closedSet = new SimpleUnorderedNodeList();
                        break;
                    case ClosedSetType.Dictionary:
                        closedSet = new ClosedDictionary();
                        break;
                    default:
                        throw new Exception();
                }
                this.pathfinding = new AStarPathfinding(gridGraph, openSet, closedSet, heuristics, tieBreakingWeight);
                break;
            case AStarType.NodeArray:
                this.pathfinding = new NodeArrayAStarPathfinding(gridGraph, heuristics, tieBreakingWeight);
                break;
            case AStarType.GatewayAstar:
                // this.pathfinding = new GatewayAStarPathfinding(gridGraph, heuristics, tieBreakingWeight);
                break;
            case AStarType.BiDirectionalAStarPathfinding:
                this.pathfinding = new BiDirectionalAStarPathfinding(gridGraph, heuristics, tieBreakingWeight);
                break;
            default:
                break;
        }      

        visualGrid.GridMapVisual(width, height, cellSize, textLines, gridGraph.grid);

        this.pathfinding.Preprocess();

        gridGraph.grid.OnGridValueChanged += visualGrid.Grid_OnGridValueChange;
    }

    // Update is called once per frame
    void Update()
    {

        // The first mouse click goes here, it defines the starting position;
        if (Input.GetMouseButtonDown(0))
        {

            //Retrieving clicked position
            var clickedPosition = UtilsClass.GetMouseWorldPosition();

            int positionX, positionY = 0;

            // Retrieving the grid's corresponding X and Y from the clicked position
            gridGraph.grid.GetXY(clickedPosition, out positionX, out positionY);

            // Getting the corresponding Node 
            var node = gridGraph.grid.GetGridObject(positionX, positionY);

            if (node != null && node.isWalkable)
            {

                if (startingX == -1)
                {
                    startingX = positionX;
                    startingY = positionY;
                    this.visualGrid.SetObjectColor(startingX, startingY, Color.cyan);

                }
                else if (goalX == -1)
                {
                    goalX = positionX;
                    goalY = positionY;
                    this.visualGrid.SetObjectColor(goalX, goalY, Color.green);
                    //We can now start the search
                    InitializeSearch(startingX, startingY, goalX, goalY);
                }
                else
                {
                    goalY = -1;
                    goalX = -1;
                    this.visualGrid.ClearGrid();
                    startingX = positionX;
                    startingY = positionY;
                    this.visualGrid.SetObjectColor(startingX, startingY, Color.cyan);
                }
            }
        }

        // We will use the right mouse to clean the selection and the grid
        if (Input.GetMouseButtonDown(1))
        {
            startingX = -1;
            startingY = -1;
            goalY = -1;
            goalX = -1;
            this.visualGrid.ClearGrid();
        }

        // Input Handler: deals with most keyboard inputs
        InputHandler();
        // Make sure you tell the pathfinding algorithm to keep searching
        if (this.pathfinding.InProgress)
        {
            var finished = this.pathfinding.Search(out this.solution, partialPath);
            if (finished)
            {

                this.pathfinding.InProgress = false;
                this.visualGrid.DrawPath(this.solution);
            }
            else if (partialPath){
                if (this.pathfinding is BiDirectionalAStarPathfinding biDirectionalAStar){
                    var forwardPath = ((BiDirectionalAStarPathfinding)this.pathfinding).forwardSearch.GetPartialSolution();
                    var backwardPath = ((BiDirectionalAStarPathfinding)this.pathfinding).backwardSearch.GetPartialSolution();
                    this.visualGrid.DrawPathTwice(forwardPath,backwardPath,Color.yellow);
                }
                else{
                    this.visualGrid.DrawPartialPath(this.solution);
                }
            }
            // Debug.Log(pathfinding.AddToOpenCalls+" open");

            this.pathfinding.TotalProcessingTime += Time.deltaTime;
        }
    }


    void InputHandler()
    {
        // Space clears the grid
        if (Input.GetKeyDown(KeyCode.Space))
            this.visualGrid.ClearGrid();

        // If you press 1-6 keys you pathfinding will use default positions
        int index = 0;
        if (Input.GetKeyDown(KeyCode.Alpha1))
            index = 1;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            index = 2;
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            index = 3;
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            index = 4;
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            index = 5;
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            index = 6;
        if (index != 0)
        {
            this.visualGrid.ClearGrid();
            var positions = defaultPositions.Find(x => x.gridName == this.gridName.ToString()).searchPos;

            if (index - 1 <= positions.Count && index - 1 >= 0)
            {
                var actualPositions = positions[index - 1];

                startingX = (int)actualPositions.startingPos.x;
                startingY = (int)actualPositions.startingPos.y;
                // Getting the corresponding Node 
                var node = gridGraph.grid.GetGridObject(startingX, startingY);
                if (node != null && node.isWalkable)
                {
                    goalX = (int)actualPositions.goalPos.x;
                    goalY = (int)actualPositions.goalPos.y;

                    node = gridGraph.grid.GetGridObject(goalX, goalY);

                    if (node != null && node.isWalkable)
                    {
                        InitializeSearch(startingX, startingY, goalX, goalY);
                    }
                }
            }
        }

    }


    public void InitializeSearch(int _startingX, int _startingY, int _goalX, int _goalY)
    {
        this.visualGrid.SetObjectColor(startingX, startingY, Color.cyan);
        this.visualGrid.SetObjectColor(goalX, goalY, Color.green);
        this.pathfinding.InitializePathfindingSearch(_startingX, _startingY, _goalX, _goalY);
    }

    // Reads the text file that where the grid "definition" is stored, I don't recomend changing this ^^ 
    public void LoadGrid(string gridPath)
    {

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(gridPath);
        var fileContent = reader.ReadToEnd();
        reader.Close();
        var lines = fileContent.Split("\n"[0]);

        //Calculating Height and Width from text file
        height = lines.Length;
        width = lines[0].Length - 1;

        // CellSize Formula 
         cellSize = 700.0f / (width + 2);
      
        textLines = new string[height, width];
        int i = 0;
        foreach (var l in lines)
        {
            var words = l.Split();
            var j = 0;

            var w = words[0];

            foreach (var letter in w)
            {
                textLines[i, j] = letter.ToString();
                j++;

                if (j == textLines.GetLength(1))
                    break;
            }

            i++;
            if (i == textLines.GetLength(0))
                break;
        }

    }

    public void UpdateColor(Node node, Color color)
    {
        gridGraph.grid.SetGridObject(node.x, node.y, node);
    }

}
