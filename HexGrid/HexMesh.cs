using System.Collections.Generic;
using UnityEngine;

namespace exp.grid._3d
{
    public enum HexDirection {
        NE, E, SE, SW, W, NW
    }
    public static class HexMetrics {
        public const float outerRadius = 4f;
        public const float innerRadius = outerRadius * 0.866025404f;
        
        static Vector3[] corners = { // 六边形的6个顶点坐标，从最上方开始顺时针
            new Vector3(0f, 0f, outerRadius),
            new Vector3(innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius),
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
            // 为了方便遍历，复制第1个点到第7个点
            new Vector3(0f, 0f, outerRadius)
        };
        
        public const float HorizontalSpacing = innerRadius * 2f;
        public const float VerticalSpacing = outerRadius * 1.5f;
        
        // 边的前一个顶点
        public static Vector3 GetFirstCorner (HexDirection direction) {
            return corners[(int)direction];
        }

        // 边的后一个顶点
        public static Vector3 GetSecondCorner (HexDirection direction) {
            return corners[(int)direction + 1];
        }
        
        public static Dictionary<HexDirection,Color> DirectionColors = new Dictionary<HexDirection, Color> {
            {HexDirection.E, Color.red},
            {HexDirection.NE, Color.green},
            {HexDirection.NW, Color.blue},//0 SE
            {HexDirection.W, Color.yellow},
            {HexDirection.SW, Color.cyan},//0 NE
            {HexDirection.SE, Color.gray},//0 NW
        };
        
        // 纯色占比
        public const float solidFactor = 0.75f;
        // 混合占比
        public const float blendFactor = 1f - solidFactor;  
        
        public static Vector3 GetFirstSolidCorner (HexDirection direction) {
            // 外侧六边形顶点的坐标 * 内侧占比
            return corners[(int)direction] * solidFactor;
        }

        public static Vector3 GetSecondSolidCorner (HexDirection direction) {
            return corners[(int)direction + 1] * solidFactor;
        }
        
        public static Vector3 GetBridge (HexDirection direction) {
            return (corners[(int)direction] + corners[(int)direction + 1])*blendFactor;//The bridges now form direct connections between hexagons. 
        }
        
        public const float elevationStep = 5f; //5f的高度差建造一个step
    }
    
    public static class HexDirectionExtensions {

        // 通过扩展方法，来扩展枚举的功能
        // 拿到一条边的对边方向
        public static HexDirection Opposite (this HexDirection direction) {
            return (int)direction < 3 ? (direction + 3) : (direction - 3);
        }
        /// <summary>
        /// 这个方向的前一个方向，按照顺时针方向排列，NE的前一个是NW，其他的前一个就是枚举值-1
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static HexDirection Previous (this HexDirection direction) {
            return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
        }
        /// <summary>
        /// 这个方向的后一个方向，按照顺时针方向排列，NW的后一个是NE，其他的后一个就是枚举值+1
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static HexDirection Next (this HexDirection direction) {
            return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
        }
    }
    
    [System.Serializable]
    public struct HexCoordinates
    {
        // 目的是为了改变纵坐标狗啃状的排布，使得每两行之间的横坐标差为1
        [SerializeField]
        private int x, z;

        public int X { get { return x; } }
        public int Z { get { return z; } }
        // 引入Y分量的目的是为了让逻辑关系的计算上更清晰，虽然在实际使用中并不需要存储Y分量
        // 由于x+y+z=0，所以y可以通过x和z计算得到
        public int Y { get { return -X - Z; } }

        public HexCoordinates(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        // 传参z、x表示实际在第几行、第几列
        // 成员变量z、x表示逻辑层面（显示的坐标）的第几行、第几列
        public static HexCoordinates FromOffsetCoordinates(int x, int z)
        {
            // 处理偏移的核心逻辑，z分量每增加2，x分量减少1
            return new HexCoordinates(x - z / 2, z);
        }
    }

    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HexMesh : MonoBehaviour
    {
        Mesh hexMesh;

        // 存储所有顶点
        List<Vector3> vertices;

        // 存储所有三角面片，每3个为1组三角形，int表示对应第几个顶点
        List<int> triangles;
        private List<Color> colors;


        void Awake()
        {
            GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
            hexMesh.name = "Hex Mesh";
            vertices = new List<Vector3>();
            triangles = new List<int>();
            colors = new List<Color>();
        }

        public void Triangulate(HexCell[] cells)
        {
            hexMesh.Clear();
            vertices.Clear();
            triangles.Clear();
            colors.Clear();
            for (int i = 0; i < cells.Length; i++)
            {
                // 处理单个六边形的绘制
                Triangulate(cells[i]);
            }

            hexMesh.vertices = vertices.ToArray(); // 设置顶点坐标
            hexMesh.triangles = triangles.ToArray(); // 装配三角形
            hexMesh.colors = colors.ToArray(); // 设置每个顶点的颜色 
            hexMesh.RecalculateNormals();
        }

        void Triangulate(HexCell cell)
        {
            for (HexDirection dir = HexDirection.NE; dir <= HexDirection.NW; dir++)
            {
                Triangulate(dir, cell);
            }
        }

        void Triangulate(HexDirection direction, HexCell cell)
        {
            Vector3 center = cell.transform.localPosition;
            Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
            Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);
            // 划分方法：两个顶点与六边形中心构建三角形
            AddTriangle(center, v1, v2);
            AddTriangleColor(cell.color);
            if (direction <= HexDirection.SE) // 裁剪边界，避免重复绘制
                TriangulateConnection(direction, cell,v1, v2);
            

        }
        
        private void TriangulateConnection (
            HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2
        ) {
            HexCell neighbor = cell.GetNeighbor(direction);
            if (neighbor == null) return;
		
            Vector3 bridge = HexMetrics.GetBridge(direction);
            Vector3 v3 = v1 + bridge;
            Vector3 v4 = v2 + bridge;
            v4.y = neighbor.Elevation * HexMetrics.elevationStep;
            v3.y = v4.y;

            // 添加Quad Bridge，一下连接自己和邻居，只用画一次，于是不用每个方向都画一遍
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(cell.color, neighbor.color);
            
            //补角
            HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
            if (direction <= HexDirection.E && nextNeighbor != null)
            {
                Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
                v5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;
                AddTriangle(v2, v4, v5);
                AddTriangleColor(cell.color, neighbor.color, nextNeighbor.color);
            }
        }

        private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }
        
        private void AddTriangleColor(Color color)
        {
            colors.Add(color);
            colors.Add(color);
            colors.Add(color);
        }
        
        private void AddTriangleColor(Color color1, Color color2, Color color3)
        {
            colors.Add(color1);
            colors.Add(color2);
            colors.Add(color3);
        }

        private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            vertices.Add(v4);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }

        void AddQuadColor (Color c1, Color c2) {
            colors.Add(c1);
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c2);
        }
    }
}