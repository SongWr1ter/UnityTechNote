using UnityEditor;
using UnityEngine;
using StudyMat.ASTAR;

namespace StudyMat.ASTAR.Editor
{
    [CustomEditor(typeof(GridModeling))]
    public class GridModelingEditor : UnityEditor.Editor
    {
        private bool isMousePickerActive = false;
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
            EditorGUILayout.Space();
            GUILayout.BeginVertical();
            if (GUILayout.Button("Add Path Shower"))
            {
                Undo.RecordObject(gridModeling, "Add Path Shower");
                gridModeling.AddPathFinderShower();
                EditorUtility.SetDirty(gridModeling);
                SceneView.RepaintAll();
            }
            
            GUILayout.EndVertical();
        }
    }
}
