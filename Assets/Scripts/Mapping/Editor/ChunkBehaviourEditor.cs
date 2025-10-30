using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static ChunkBehaviour;

[CustomEditor(typeof(ChunkBehaviour))]
public class ChunkBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Sets up serliazed fields
        ChunkBehaviour chunkManager = (ChunkBehaviour)target;

        serializedObject.Update();

        SerializedProperty
            concatenatingPosition = serializedObject.FindProperty(chunkManager.ConcatenatingPositionReference);

        EditorGUILayout.LabelField("Concatenating position");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(concatenatingPosition.FindPropertyRelative("x"), new GUIContent("X"));
        EditorGUILayout.PropertyField(concatenatingPosition.FindPropertyRelative("y"), new GUIContent("Z"));
        EditorGUI.indentLevel--;
        EditorGUILayout.Space();

        // Layer creation logic
        if(GUILayout.Button("Generate new layer"))
        {
            // Creates the empty layer object
            GameObject newLayer = new GameObject();
            Undo.RegisterCreatedObjectUndo(newLayer, "Generated new layer");
            newLayer.transform.parent = chunkManager.transform;
            newLayer.isStatic = true;
            newLayer.AddComponent<LayerBehaviour>().TranslateHeightPosition();
        }

        serializedObject.ApplyModifiedProperties();
        chunkManager.TranslateConcatenatingPosition();
    }
}