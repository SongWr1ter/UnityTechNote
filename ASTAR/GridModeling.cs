using UnityEngine;

namespace StudyMat.ASTAR
{
    [ExecuteAlways]
    public class GridModeling : MonoBehaviour
    {
        public enum GridPlaneMode
        {
            XyxThenY,
            XzxThenZ
        }

        public enum RayAxisMode
        {
            PositiveY,
            NegativeY,
            PositiveZ,
            NegativeZ
        }

        public enum PhysicsDetectionMode
        {
            Physics3D,
            Physics2D
        }

        [System.Serializable]
        public class GridCell
        {
            public int x;
            public int y;
            public Vector3 center;
            public bool isBlocked;

            public GridCell(int x, int y, Vector3 center)
            {
                this.x = x;
                this.y = y;
                this.center = center;
                isBlocked = false;
            }
        }

        [Header("Grid")]
        public GridPlaneMode planeMode = GridPlaneMode.XzxThenZ;
        public float cellSize = 1f;
        public int columns = 8;
        public int rows = 8;
        public Vector3 startPosition = Vector3.zero;
        public Color gridLineColor = Color.white;

        [Header("Detection")]
        public LayerMask obstacleMask;
        public PhysicsDetectionMode physicsDetectionMode = PhysicsDetectionMode.Physics3D;
        public RayAxisMode rayAxisMode = RayAxisMode.NegativeY;
        public float rayDistance = 10f;
        public float rayStartOffset = 5f;
        [Range(0.05f, 1f)]
        public float blockedCubeScale = 0.25f;
        public bool drawDetectionResult = true;
        public bool drawDetectArrow = true;

        private GridCell[,] _cells;
        public GridCell[,] Cells => _cells;
        [SerializeField,Range(0,1f)]
        private float physics2DDetectPrecision = 0.9f;

        public void BuildGridAndScan()
        {
            BuildGrid();
            ScanObstacles();
        }

        public void BuildGrid()
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            cellSize = Mathf.Max(0.01f, cellSize);

            _cells = new GridCell[columns, rows];

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    Vector3 center = GetCellCenter(x, y);
                    _cells[x, y] = new GridCell(x, y, center);
                }
            }
        }

        public void ScanObstacles()
        {
            EnsureGridExists();

            if (physicsDetectionMode == PhysicsDetectionMode.Physics2D)
            {
                ScanObstacles2D();
                return;
            }

            ScanObstacles3D();
        }

        public void ScanObstacles3D()
        {
            Vector3 rayDirection = GetRayDirection();

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    GridCell cell = _cells[x, y];
                    Vector3 rayOrigin = cell.center - rayDirection * rayStartOffset;
                    bool hit = Physics.Raycast(
                        rayOrigin,
                        rayDirection,
                        rayDistance + rayStartOffset,
                        obstacleMask,
                        QueryTriggerInteraction.Ignore
                    );

                    cell.isBlocked = hit;
                }
            }
        }

        public void ScanObstacles2D()
        {
            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    GridCell cell = _cells[x, y];
                    var hit = Physics2D.OverlapCircle(cell.center, (cellSize*0.5f * physics2DDetectPrecision), obstacleMask);

                    cell.isBlocked = hit is not null;
                }
            }
        }

        private void OnValidate()
        {
            columns = Mathf.Max(1, columns);
            rows = Mathf.Max(1, rows);
            cellSize = Mathf.Max(0.01f, cellSize);
            rayDistance = Mathf.Max(0.01f, rayDistance);
            rayStartOffset = Mathf.Max(0f, rayStartOffset);
        }

        private void OnDrawGizmos()
        {
            DrawGridLines();

            if (drawDetectionResult)
            {
                DrawCellResultCubes();
            }
        }

        private void DrawGridLines()
        {
            Gizmos.color = gridLineColor;

            if (planeMode == GridPlaneMode.XyxThenY)
            {
                for (int x = 0; x <= columns; x++)
                {
                    Vector3 from = startPosition + new Vector3(x * cellSize, 0f, 0f);
                    Vector3 to = startPosition + new Vector3(x * cellSize, rows * cellSize, 0f);
                    Gizmos.DrawLine(from, to);
                }

                for (int y = 0; y <= rows; y++)
                {
                    Vector3 from = startPosition + new Vector3(0f, y * cellSize, 0f);
                    Vector3 to = startPosition + new Vector3(columns * cellSize, y * cellSize, 0f);
                    Gizmos.DrawLine(from, to);
                }

                return;
            }

            for (int x = 0; x <= columns; x++)
            {
                Vector3 from = startPosition + new Vector3(x * cellSize, 0f, 0f);
                Vector3 to = startPosition + new Vector3(x * cellSize, 0f, rows * cellSize);
                Gizmos.DrawLine(from, to);
            }

            for (int z = 0; z <= rows; z++)
            {
                Vector3 from = startPosition + new Vector3(0f, 0f, z * cellSize);
                Vector3 to = startPosition + new Vector3(columns * cellSize, 0f, z * cellSize);
                Gizmos.DrawLine(from, to);
            }
        }

        private void DrawCellResultCubes()
        {
            if (_cells == null)
            {
                return;
            }

            float blockedSize = Mathf.Clamp(cellSize * blockedCubeScale, 0.01f, cellSize);

            for (int x = 0; x < columns; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    GridCell cell = _cells[x, y];
                    Gizmos.color = cell.isBlocked ? Color.red : Color.grey;
                    Vector3 size = cell.isBlocked ? Vector3.one * blockedSize : Vector3.one * cellSize * 0.8f;
                    Gizmos.DrawCube(cell.center, size);
                    // 绘制检测射线
                    Gizmos.color = cell.isBlocked ? Color.red : Color.green;
                    Vector3 rayDirection = GetRayDirection();
                    Vector3 rayStart = cell.center - rayDirection * rayStartOffset;
                    Vector3 rayEnd = rayStart + rayDirection * (rayDistance + rayStartOffset);
                    Gizmos.DrawLine(rayStart, rayEnd);
                    if (drawDetectArrow)
                    {
                        if (physicsDetectionMode == PhysicsDetectionMode.Physics3D)
                        {
                            Vector3 arrow = rayEnd - new Vector3(cellSize * 0.5f, 0f, cellSize * 0.5f);
                            Gizmos.DrawLine(rayEnd, arrow);
                            arrow = rayEnd - new Vector3(-cellSize * 0.5f, 0f,cellSize * 0.5f);
                            Gizmos.DrawLine(rayEnd, arrow);
                        }
                        else
                        {
                            Gizmos.DrawWireSphere(cell.center,cellSize * 0.5f * physics2DDetectPrecision);
                        }
                        
                    }
                    
                }
            }
        }

        private void EnsureGridExists()
        {
            if (_cells == null || _cells.GetLength(0) != columns || _cells.GetLength(1) != rows)
            {
                BuildGrid();
            }
        }

        private Vector3 GetCellCenter(int x, int y)
        {
            float half = cellSize * 0.5f;

            if (planeMode == GridPlaneMode.XyxThenY)
            {
                return startPosition + new Vector3(x * cellSize + half, y * cellSize + half, 0f);
            }

            return startPosition + new Vector3(x * cellSize + half, 0f, y * cellSize + half);
        }

        private Vector3 GetRayDirection()
        {
            switch (rayAxisMode)
            {
                case RayAxisMode.PositiveY:
                    return Vector3.up;
                case RayAxisMode.NegativeY:
                    return Vector3.down;
                case RayAxisMode.PositiveZ:
                    return Vector3.forward;
                default:
                    return Vector3.back;
            }
        }
    }
}
