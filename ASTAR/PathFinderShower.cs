using System;
using UnityEngine;

namespace StudyMat.ASTAR
{
    public class PathFinderShower : MonoBehaviour
    {
        private GridModeling gridModeling;
        private BKBPathFinder pathFinder;

        private GridNode startNode;
        private GridNode endNode;
        private GridModeling.GridCell startGridCell;
        private GridModeling.GridCell endGridCell;
        private int pickupStatus = 0; // 0: pick start, 1: pick start, 2: pick end

        [SerializeField, Min(1)]
        private int guiButtonFontSize = 28;

        private void OnGUI()
        {
            // 在屏幕左侧显示一个按钮，功能是打印hello world
            GUILayout.BeginVertical();
            GUILayout.Space(30f);

            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = guiButtonFontSize
            };

            if (GUILayout.Button("Print Hello World", buttonStyle, GUILayout.Width(300), GUILayout.Height(90)))
            {
                Debug.Log("Hello World");
            }
            GUILayout.Space(10f);
            if (pickupStatus == 0)
            {
                if (GUILayout.Button("Pick Start Node", buttonStyle, GUILayout.Width(300), GUILayout.Height(90)))
                {
                    ResetStartNode();
                    pickupStatus = 1;
                }
                GUILayout.Space(10f);
                if (GUILayout.Button("Pick End Node", buttonStyle, GUILayout.Width(300), GUILayout.Height(90)))
                {
                    ResetEndNode();
                    pickupStatus = 2;
                }
                if (GUILayout.Button("Reset Node", buttonStyle, GUILayout.Width(300), GUILayout.Height(90)))
                {
                    ResetNode();
                }
            }
            else
            {
                if (GUILayout.Button(" Exit Pick Node", buttonStyle, GUILayout.Width(300), GUILayout.Height(90)))
                {
                    pickupStatus = 0;
                }
            }
            
            GUILayout.Space(10f);
            if(startNode != null && endNode != null)
            {
                if (GUILayout.Button("Find Path", buttonStyle, GUILayout.Width(300), GUILayout.Height(90)))
                {
                    var path = pathFinder.FindPath(startNode, endNode);
                    if (path != null)
                    {
                        Debug.Log("Path found:");
                        GeneratePath(path);
                    }
                    else
                    {
                        Debug.Log("No path found.");
                    }
                }
            }
            
            GUILayout.EndVertical();
        }

        public void OnAdd(GridModeling gridModeling)
        {
            this.gridModeling = gridModeling;
            if(gridModeling.Cells.Length <= 0)
            {
                Debug.LogError("请先构建网格");
                return;
            }
            else
            {
                pathFinder = new BKBPathFinder(gridModeling.Cells);
            }
        }
        
        public void ResetPathFinder()
        {
            if (pathFinder != null)
            {
                pathFinder = null;
            }
            pathFinder = new BKBPathFinder(gridModeling.Cells);
        }

        private void Start()
        {
            if (gridModeling is null)
            {
                gridModeling = GetComponent<GridModeling>();
            }
        }

        private void Update()
        {
            if (pickupStatus != 0)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    var cell = gridModeling.MouseInputUpdateFunction();
                    if(cell is null) return;
                    if (pickupStatus == 1)
                    {
                        startGridCell = cell;
                        startNode = pathFinder.GetNodeByCell(cell);
                        cell.manualColor = Color.green;
                    }
                    else
                    {
                        endGridCell = cell;
                        endNode = pathFinder.GetNodeByCell(cell);
                        cell.manualColor = Color.magenta;
                    }
                    cell.isDrawedByManual = true;
                    pickupStatus = 0;
                }
            }
            
        }

        private void GeneratePath(GridNode node)
        {
            var path = pathFinder.GetPath(node);
            foreach (var n in path)
            {
                if (gridModeling.SetGridCellByCoordinate(n.x, n.y, true, Color.cyan))
                {
                    
                }
                else
                {
                    return;
                }
            }
        }

        private void ResetNode()
        {
            ResetStartNode();
            ResetEndNode();
        }
        
        private void ResetStartNode()
        {
            startNode = null;
            if (startGridCell != null)
            {
                startGridCell.isDrawedByManual = false;
            }
        }

        private void ResetEndNode()
        {
            endNode = null;
            if (endGridCell != null)
            {
                endGridCell.isDrawedByManual = false;
            }
        }
    }
}