using UnityEditor;
using UnityEngine;
using StudyMat.ASTAR;

namespace StudyMat.ASTAR.Editor
{
    [CustomEditor(typeof(GridModeling))]
    public class GridModelingEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            GridModeling gridModeling = (GridModeling)target;

            if (GUILayout.Button("Build Grid And Scan Obstacles"))
            {
                Undo.RecordObject(gridModeling, "Build Grid And Scan Obstacles");
                gridModeling.BuildGridAndScan();
                EditorUtility.SetDirty(gridModeling);
                SceneView.RepaintAll();
            }
        }
    }
}
