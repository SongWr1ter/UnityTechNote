using UnityEngine;

namespace exp.grid._3d
{
    public class HexCell : MonoBehaviour
    {
        public HexCoordinates coordinates;
        public Color color;
        public RectTransform uiRect;
        // 存储六边形6个方向上相邻的其他六边形
        [SerializeField]
        HexCell[] neighbors = new HexCell[6];

        public HexCell GetNeighbor(HexDirection direction)
        {
            return neighbors[(int)direction];
        }

        public void SetNeighbor(HexDirection direction, HexCell cell)
        {
            // 以边的枚举值作为下标
            neighbors[(int)direction] = cell;
            // 同时，让相邻格子将自己存储为邻居，方向取反
            cell.neighbors[(int)direction.Opposite()] = this;
        }
        
        public int Elevation {
            get {
                return elevation;
            }
            set {
                elevation = value;
                Vector3 position = transform.localPosition;
                position.y = value * HexMetrics.elevationStep;
                transform.localPosition = position;
                //UI
                Vector3 uiPosition = uiRect.localPosition;
                uiPosition.y += elevation * HexMetrics.elevationStep;
                uiRect.localPosition = uiPosition;
            }
        }
	
        int elevation;
        
    }
}