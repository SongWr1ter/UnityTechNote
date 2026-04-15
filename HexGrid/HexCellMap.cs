using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace exp.grid._3d
{
    public class HexCellMap
    {
        private Dictionary<Vector2Int, HexCell> _cellMap;
        
        public HexCellMap(List<HexCell> cells)
        {
            _cellMap = new Dictionary<Vector2Int, HexCell>();
            foreach (var cell in cells)
            {
                _cellMap[new Vector2Int(cell.coordinates.X, cell.coordinates.Z)] = cell;
            }
            
        }

        public HexCell GetCell(Vector2Int coordinates)
        {
            return _cellMap.GetValueOrDefault(coordinates);
        }
        
        // implemented based on https://www.redblobgames.com/grids/hexagons/#hex-to-pixel-offset
        private static Vector3Int HexRound(float q, float r)
        {
            int x = Mathf.RoundToInt(q);
            int z = Mathf.RoundToInt(r);
            int y = Mathf.RoundToInt(-q - r);

            float xDiff = Mathf.Abs(x - q);
            float yDiff = Mathf.Abs(y + q + r);
            float zDiff = Mathf.Abs(z - r);

            if (xDiff > yDiff && xDiff > zDiff)
                x = -y - z;
            else if (yDiff > zDiff)
                y = -x - z;

            return new Vector3Int(x, y, z);
        }
        
        public static Vector3Int WorldPos2HexPos(Vector3 worldPos)
        {
            float q = (worldPos.x * 0.577350269f - worldPos.z * 0.333333333f) / HexMetrics.outerRadius;
            float r = worldPos.z * 0.666666667f / HexMetrics.outerRadius;
            return HexRound(q, r);
        }

        public HexCell GetCellByWorldPos(Vector3 worldPos)
        {
            Vector3Int hexPos = WorldPos2HexPos(worldPos);
            return GetCell(new Vector2Int(hexPos.x, hexPos.z));
        }
    }
}