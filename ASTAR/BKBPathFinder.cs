
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudyMat.ASTAR
{

    public class GridNode : IComparable<GridNode>
    {
        private static int idCounter = 0;

        public GridNode(int x, int y,bool isWalkable)
        {
            _id = idCounter++;
            this.x = x;
            this.y = y;
            this.isWalkable = isWalkable;
            G = 0f;
            H = 0f;
            parent = null;
        }
        public GridNode(int x, int y)
        {
            _id = idCounter++;
            this.x = x;
            this.y = y;
            G = 0f;
            H = 0f;
            parent = null;
        }

        public GridNode()
        {
            _id = idCounter++;
            x = 0;
            y = 0;
            G = 0f;
            H = 0f;
            parent = null;
        }

        private int _id;
        public int id => _id;
        public int x { get; }
        public int y { get; }
        public float G { get; set; }
        public float H { get; set; }
        public float F => G + H;
        public bool isWalkable { get; }
        public GridNode parent { get; set; }
        
        public int CompareTo(GridNode other)
        {
            if (other is null) return 1;
            if (F < other.F) return -1;
            if (F > other.F) return 1;
            
            if(!Mathf.Approximately(G, other.G)) return G.CompareTo(other.G);
            
            return id.CompareTo(other.id);//如果 F 和 G 都一样，按唯一ID排，确保绝对不重复
        }

        public override bool Equals(object obj)
        {
            if(obj is GridNode other)
            {
                return id == other.id;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override string ToString()
        {
            return id.ToString() + $":(x:{x},y:{y}) F:{F} G:{G} H:{H}";
        }
    }

    public class BKBPathFinder
    {
        private int col;
        private int row;
        private GridNode[,] grid;
        private const int MaxPathLength = 1000; // 避免死循环的最大路径长度

        public BKBPathFinder(GridModeling.GridCell[,] cells)
        {
            //Grid Map
            grid = NodeAbstractToGrid(cells);
            row = grid.GetLength(0);
            col = grid.GetLength(1);
            Debug.Log($"路径寻找器初始化完成，网格大小：{grid.GetLength(0)}x{grid.GetLength(1)}");
        }
        
        // 找得到路径才会返回路径，否则返回null
        private GridNode AStarPathFinding(GridNode startNode, GridNode endNode)
        {
            // A*算法的实现
            SortedSet<GridNode> openSet = new SortedSet<GridNode>();
            HashSet<GridNode> closedSet = new HashSet<GridNode>();
            startNode.G = 0;
            startNode.H = Heuristic(startNode, endNode);
            openSet.Add(startNode);
            int counter = 0;
            while (openSet.Count > 0)
            {
                // 避免死循环
                if (counter++ > MaxPathLength)
                {
                    Debug.LogError("路径搜索超出最大步数，可能存在死循环");
                    break;
                }
                GridNode currentNode = openSet.Min;
                if (currentNode.Equals(endNode))
                {
                    // 找到路径，构建路径列表
                    return currentNode;
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                for (int i = 0; i < Directions.GetLength(0); i++)
                {
                    int newX = currentNode.x + Directions[i, 0];
                    int newY = currentNode.y + Directions[i, 1];
                    GridNode neighbor = GetNodeByCoords(newX, newY);
                    if (neighbor == null || !neighbor.isWalkable || closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    float tentativeG = currentNode.G + 1; // 假设每步代价为1
                    if (!openSet.Contains(neighbor) || tentativeG < neighbor.G)
                    {
                        neighbor.parent = currentNode;
                        neighbor.G = tentativeG;
                        neighbor.H = Heuristic(neighbor, endNode);
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
            // 没有找到路径或者搜索超出最大步数
            return null;
        }

        #region 辅助方法
        
        private static float Heuristic(GridNode startNode, GridNode endNode)
        {
            return Math.Abs(startNode.x - endNode.x) + Math.Abs(startNode.y - endNode.y);
        }

        private GridNode GetNodeByCoords(int x, int y)
        {
            if(x < 0 || x >= row || y < 0 || y >= col)
            {
                Debug.LogError($"坐标超出范围,x:{x},y:{y}");
                return null;
            }
            return grid[x, y];
        }

        public GridNode GetNodeByCell(GridModeling.GridCell cell)
        {
            int x = cell.x;
            int y = cell.y;
            return GetNodeByCoords(x, y);
        }

        private static readonly int[,] Directions = new int[,]
        {
            { -1, 0 }, // 左
            { 1, 0 },  // 右
            { 0, -1 }, // 上
            { 0, 1 }   // 下
        };

        private static GridNode[,] NodeAbstractToGrid(GridModeling.GridCell[,] cells)
        {
            GridNode[,] grid = new GridNode[cells.GetLength(0), cells.GetLength(1)];
            for(int x = 0; x < cells.GetLength(0); x++)
            {
                for (int y = 0; y < cells.GetLength(1); y++)
                {
                    grid[x, y] = new GridNode(x, y,!cells[x, y].isBlocked);
                }
            }
            return grid;
        }
        
        #endregion

        public GridNode FindPath(int startX, int startY, int endX, int endY)
        {
            GridNode startNode = grid[startX, startY];
            GridNode endNode = grid[endX, endY];
            return AStarPathFinding(startNode, endNode);
        }
        
        public GridNode FindPath(GridNode _startNode, GridNode _endNode)
        {
            GridNode startNode = _startNode;
            GridNode endNode = _endNode;
            return AStarPathFinding(startNode, endNode);
        }

        public List<GridNode> GetPath(GridNode endNode)
        {
            // 返回路径
            List<GridNode> path = new List<GridNode>();
            GridNode currentNode = endNode;
            while (currentNode != null)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Reverse();
            return path;
        }
    }
}

