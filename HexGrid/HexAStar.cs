using System;
using System.Collections.Generic;
using UnityEngine;

namespace exp.grid._3d
{
    public abstract class IHexNode : IComparable<IHexNode>
    {
        protected static int IDCounter = 0;
        protected int ID { get; set; }
        public bool isWalkable { get; set; }
        public HexCell cell { get; set; }
        public IHexNode parent { get; set; }
        public int G { get; set; }
        public int H { get; set; }
        public int F => G + H;
        
        public int Q { get; set; }
        public int R { get; set; }
        private Vector2Int _coordinates { get; set; }
        public Vector2Int coordinates
        {
            get
            {
                if (_coordinates == Vector2.zero)
                {
                    _coordinates = new Vector2Int(Q, R);
                }
                return _coordinates;
            }
        }
        public int S => -Q - R;
        public abstract List<Vector2Int> neighbors { get; }
        
        public int CompareTo(IHexNode other)
        {
            if (other is null) return 1;
            if (F < other.F) return -1;
            if (F > other.F) return 1;
            
            if(!Mathf.Approximately(H, other.H)) return H.CompareTo(other.H);
            
            return ID.CompareTo(other.ID);
        }

        public override bool Equals(object obj)
        {
            if(obj is IHexNode other)
            {
                return ID == other.ID;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }

    public class HexNode : IHexNode
    {

        public HexNode(bool walkable)
        {
            isWalkable = walkable;
            ID = IDCounter++;
        }

        public override List<Vector2Int> neighbors
        {
            get
            {
                List<Vector2Int> result = new List<Vector2Int>();
                foreach (HexDirection direction in Enum.GetValues(typeof(HexDirection)))
                {
                    var neighborCell = cell.GetNeighbor(direction);
                    if (neighborCell != null)
                    {
                        result.Add(new Vector2Int(neighborCell.coordinates.X, neighborCell.coordinates.Z));
                    }
                }
                return result;
            }
        }
    }
    
    public class HexAStarMap
    {
        private IHexNode startNode;
        private IHexNode targetNode;
        private Dictionary<Vector2Int, IHexNode> _cellMap;
        private const int MAX_ITERATION = 1000;

        public HexAStarMap(List<HexCell> cells)
        {
            _cellMap = new Dictionary<Vector2Int, IHexNode>();
            foreach (var cell in cells)
            {
                var node = new HexNode(cell.color != Color.black) { cell = cell, Q = cell.coordinates.X, R = cell.coordinates.Z };
                _cellMap[new Vector2Int(cell.coordinates.X, cell.coordinates.Z)] = node;
            }
        }
        
        
        private IHexNode pathFinding()
        {
            int counter = 0;
            SortedSet<IHexNode> openSet = new SortedSet<IHexNode>();
            HashSet<IHexNode> closedSet = new HashSet<IHexNode>();
            openSet.Add(startNode);
            startNode.G = 0;
            while (openSet.Count > 0 && counter < MAX_ITERATION)
            {
                ++counter;
                IHexNode currentNode = openSet.Min;
                if (currentNode.Equals(targetNode))
                {
                    return currentNode;
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                foreach (var neighborVector in currentNode.neighbors)
                {
                    var neighbor = _cellMap.GetValueOrDefault(neighborVector);
                    if (neighbor == null || !neighbor.isWalkable || closedSet.Contains(neighbor)) continue;
                    
                    int tentativeG = currentNode.G + 1;
                    
                    if(!openSet.Contains(neighbor) || tentativeG < neighbor.G)
                    {
                        neighbor.G = tentativeG;
                        neighbor.H = Heruistic(neighbor, targetNode);
                        neighbor.parent = currentNode;
                        if(!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
            
            return null;
        }
        
        private List<IHexNode> reconstructPath(IHexNode node)
        {
            List<IHexNode> path = new List<IHexNode>();
            while (node != null)
            {
                path.Add(node);
                node = node.parent;
            }
            path.Reverse();
            return path;
        }
        
        public bool PathFinding(IHexNode start, IHexNode target, out List<IHexNode> path)
        {
            startNode = start;
            targetNode = target;
            IHexNode resultNode = pathFinding();
            if (resultNode != null)
            {
                path = reconstructPath(resultNode);
                return true;
            }
            else
            {
                path = null;
                return false;
            }
        }
        
        
        private static int Heruistic(IHexNode a, IHexNode b)
        {
            return (Mathf.Abs(a.Q - b.Q) + Mathf.Abs(a.R - b.R) + Mathf.Abs(a.S - b.S));
        }

        public IHexNode GetByHexCell(HexCell cell)
        {
            return _cellMap.GetValueOrDefault(new Vector2Int(cell.coordinates.X, cell.coordinates.Z));
        }
        
    }
    
    
}