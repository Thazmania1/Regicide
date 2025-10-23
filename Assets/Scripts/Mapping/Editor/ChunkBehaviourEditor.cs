using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChunkBehaviour))]
public class ChunkBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Sets up serliazed fields
        ChunkBehaviour chunkManager = (ChunkBehaviour) target;

        serializedObject.Update();

        SerializedProperty
            concatenatingPosition = serializedObject.FindProperty(chunkManager.GetSerializedFieldName("ConcatenatingPosition"));

        EditorGUILayout.LabelField("Concatenating position");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(concatenatingPosition.FindPropertyRelative("x"), new GUIContent("X"));
        EditorGUILayout.PropertyField(concatenatingPosition.FindPropertyRelative("y"), new GUIContent("Z"));
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate new layer"))
        {
            chunkManager.GenerateNewLayer();
        }

        serializedObject.ApplyModifiedProperties();
        chunkManager.TranslateConcatenatingPosition();
    }
}