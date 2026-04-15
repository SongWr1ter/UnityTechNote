using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace exp.grid._3d
{
    public class HexMeshGenerator : MonoBehaviour
    {
        [Range(1, 20)]
        public int X;
        [Range(1, 20)]
        public int Y;
        
        public Transform generateParent;
        public GameObject textPrefab;
        public Canvas textCanvas;        
        public Material material;
        
        private List<HexCell> cells;
        public int activeElevation;
        public Color defaultColor = Color.white;

        [Header("GUI")]
        public float resetButtonWidth = 140f;
        public float resetButtonHeight = 36f;

        private HexCellMap cellMap;
        private HexMesh hexMesh;
        
        private HexCell startCell;
        private HexCell endCell;

        private void Start()
        {
            cells = new List<HexCell>();
            for (int z = 0; z < Y; ++z)
            {
                for (int x = 0; x < X; ++x)
                {
                    Vector3 targetPos = generateParent.position + new Vector3(x * HexMetrics.HorizontalSpacing
                                                                              + (z % 2 == 0
                                                                                  ? 0f
                                                                                  : HexMetrics.innerRadius) //奇数列便宜内半径
                        , 0f, z * HexMetrics.VerticalSpacing);
                    var go = new GameObject($"Hex", typeof(HexCell));
                    go.transform.SetParent(generateParent);
                    go.transform.localPosition = targetPos;
                    var cell = go.GetComponent<HexCell>();
                    cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
                    cell.color = defaultColor;
                    go.name += $"_{cell.coordinates.X}_{cell.coordinates.Z}";
                    /*
                     * 开始构建相邻关系，因为SetNeighbor函数中已经双向关联的相邻关系，我们只需要考虑格子的3个方向即可：
                     */
                    if (x > 0)
                    {
                        cell.SetNeighbor(HexDirection.W, cells[^1]);
                    }

                    if (z > 0)
                    {
                        if ((z & 1) == 0) // 偶数行
                        {
                            // 右下
                            cell.SetNeighbor(HexDirection.SE, cells[^(X)]);
                            if (x > 0)
                            {
                                // 左下
                                cell.SetNeighbor(HexDirection.SW, cells[^(X+1)]);
                            }
                        }
                        // 因为六边形x分量错位关系，再处理一遍基数行的邻居关系
                        else
                        {
                            cell.SetNeighbor(HexDirection.SW, cells[^X]);
                            if (x < X - 1)
                            {
                                cell.SetNeighbor(HexDirection.SE, cells[^(X-1)]);
                            }
                        }
                    }

                    cells.Add(cell);
                    
                    //setText
                    if (textPrefab != null && textCanvas != null)
                    {
                        var textGO = Instantiate(textPrefab, textCanvas.transform);
                        textGO.transform.localPosition = targetPos + Vector3.up;
                        textGO.GetComponent<TMP_Text>().text = $"{cell.coordinates.X}\n{cell.coordinates.Y}\n{cell.coordinates.Z}";
                        cell.uiRect = textGO.GetComponent<RectTransform>();
                    }
                    
                }
            }
            
            gameObject.AddComponent<MeshFilter>().sharedMesh = new Mesh();
            gameObject.AddComponent<MeshRenderer>().material = material;
            hexMesh = gameObject.AddComponent<HexMesh>();
            hexMesh.Triangulate(cells.ToArray());
            
            cellMap = new HexCellMap(cells);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                ScreenRayCast(out Vector3 worldPosition);
                var cell = cellMap.GetCellByWorldPos(worldPosition);
                if (cell != null)
                {
                    EditCell(cell,Color.cyan);
                    // foreach (var dir in System.Enum.GetValues(typeof(HexDirection)))
                    // {
                    //     var neighbor = cell.GetNeighbor((HexDirection)dir);
                    //     if (neighbor != null)
                    //     {
                    //         neighbor.color = HexMetrics.DirectionColors[(HexDirection)dir];
                    //     }
                    // }
                    startCell = cell;
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                ScreenRayCast(out Vector3 worldPosition);
                var cell = cellMap.GetCellByWorldPos(worldPosition);
                if (cell != null)
                {
                    cell.color = Color.yellow;
                    endCell = cell;
                    ReRender();
                }
            }
            
            if (Input.GetMouseButtonDown(2))
            {
                ScreenRayCast(out Vector3 worldPosition);
                var cell = cellMap.GetCellByWorldPos(worldPosition);
                if (cell != null)
                {
                    cell.color = Color.red;
                    ReRender();
                }
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
            GUILayout.BeginVertical();
            GUILayout.Space(10f);
            
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 30
            };

            if (GUILayout.Button("Reset",buttonStyle, GUILayout.Width(resetButtonWidth), GUILayout.Height(resetButtonHeight)))
            {
                foreach (var cell in cells)
                {
                    cell.color = defaultColor;
                }
                hexMesh.Triangulate(cells.ToArray());
            }
            
            GUILayout.Space(30f);
            

            if (GUILayout.Button("Astar",buttonStyle, GUILayout.Width(resetButtonWidth), GUILayout.Height(resetButtonHeight)))
            {
                if(startCell != null && endCell != null)
                {
                    var astar = new HexAStarMap(cells);
                    astar.PathFinding(astar.GetByHexCell(startCell), astar.GetByHexCell(endCell),out List<IHexNode> path);
                    if (path != null)
                    {
                        foreach (var node in path)
                        {
                            var cell = cellMap.GetCell(node.coordinates);
                            if (cell != null)
                            {
                                cell.color = Color.green;
                            }
                        }
                        hexMesh.Triangulate(cells.ToArray());
                    }
                }
            }
            
            GUILayout.Space(30f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+",buttonStyle, GUILayout.Width(resetButtonWidth * 0.2f), GUILayout.Height(resetButtonHeight * 0.5f)))
            {
                activeElevation++;
            }
            GUILayout.Space(10f);
            GUILayout.Label(activeElevation.ToString(), buttonStyle,GUILayout.Width(resetButtonWidth * 0.2f),
                GUILayout.Height(resetButtonHeight * 0.5f));
            GUILayout.Space(10f);
            if (GUILayout.Button("-",buttonStyle, GUILayout.Width(resetButtonWidth * 0.2f), GUILayout.Height(resetButtonHeight * 0.5f)))
            {
                activeElevation--;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void ScreenRayCast(out Vector3 worldPosition)
        {
            // 1. 创建数学平面，这里假设地面高度为 0，法线向上
            Plane plane = new Plane(Vector3.up, Vector3.zero);

            // 2. 获取射线
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // 3. 计算射线与平面的交点距离
            if (plane.Raycast(ray, out float distance))
            {
                // 根据距离获取射线上的点
                worldPosition = ray.GetPoint(distance);
            }
            else
            {
                worldPosition = Vector3.zero;
            }
        }

        void EditCell (HexCell cell,Color activeColor) {
            cell.color = activeColor;
            cell.Elevation = activeElevation;
            ReRender();
        }
        
        private void ReRender()
        {
            hexMesh.Triangulate(cells.ToArray());
        }
    }
}