using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using System;
using UnityEngine.UI;
using System.Linq;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using System.Reflection;

namespace Assets.Scripts.Grid
{
    public class Grid<TGridObject>
    {

        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;
        public class OnGridValueChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        private TGridObject[,] gridArray;
        private float cellSize;

        public TGridObject this[int x, int y]
        {
            get => gridArray[x, y];
            set => gridArray[x, y] = value;
        }

        public Grid(Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
        {            
            this.Width = PathfindingManager.width;
            this.Height = PathfindingManager.height;
            this.cellSize = PathfindingManager.cellSize;

            gridArray = new TGridObject[Width, Height];
            for (int x = 0; x < gridArray.GetLength(0); x++)
                for (int y = 0; y < gridArray.GetLength(1); y++) {

                    gridArray[x, y] = createGridObject(this, x, y);
                  
                }
        }

        // Converts grid coordinates to world coordinates
        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, 0, y) * cellSize;
        }

        // Get from World Position to grid coordinate
        public void GetXY(Vector3 WorldPosition, out int x, out int y)
        {

            x = Mathf.FloorToInt(WorldPosition.x / cellSize);
            //Take into account the reference of the world y->z
            y = Mathf.FloorToInt(WorldPosition.z / cellSize);

        }


        //public int getWidth()
        //{
        //    return this.width;
        //}
        //public int getHeight()
        //{
        //    return this.height;
        //}
        public void SetGridObject(int x, int y, TGridObject value)
        {
            //What happens when the value is unaceptable let's igone them for now
            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                gridArray[x, y] = value;
                OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
            }

        }

      
        public void SetGridObject(Vector3 worldPosition, TGridObject value)
        {
            int x, y;
            GetXY(worldPosition, out x, out y);
            SetGridObject(x, y, value);
        }

        public TGridObject GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < Width && y < Height)
                return gridArray[x, y];
            else return default(TGridObject);
        }

    
        public TGridObject GetGridObject(Vector3 worldPosition)
        {
            int x, y;
            GetXY(worldPosition, out x, out y);
            return GetGridObject(x, y);
        }

        public bool WithinGridLimits(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }


        public List<TGridObject> GetAll()
        {
            return gridArray.Cast<TGridObject>().ToList();
        }
     
    }

}