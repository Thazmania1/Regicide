using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GridManager gridManager = target as GridManager;
        Transform gridManagerTransform = gridManager.transform;

        serializedObject.Update();

        SerializedProperty
            matchDataPanel = serializedObject.FindProperty(gridManager.MatchDataPanelReference),
            matchTimerText = serializedObject.FindProperty(gridManager.MatchTimerTextReference),
            playerMovesText = serializedObject.FindProperty(gridManager.PlayerMovesReference);

        // UI references
        EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(matchDataPanel);
            EditorGUILayout.PropertyField(matchTimerText);
            EditorGUILayout.PropertyField(playerMovesText);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
            if(GUILayout.Button("Generate new chunk"))
            {
                GameObject newChunk = new GameObject();
                Undo.RegisterCreatedObjectUndo(newChunk, "Generated new chunk");
                newChunk.transform.parent = gridManagerTransform;
                newChunk.isStatic = true;
                newChunk.AddComponent<ChunkBehaviour>().TranslateConcatenatingPosition();
            }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

        // The grid layout must never be manipulated
        gridManagerTransform.position = new Vector3(0, 0, 0);
        gridManagerTransform.rotation = Quaternion.identity;
        gridManagerTransform.localScale = new Vector3(1, 1, 1);
    }
}