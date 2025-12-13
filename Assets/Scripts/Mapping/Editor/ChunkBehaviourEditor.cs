using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ChunkBehaviour))]
[CanEditMultipleObjects]
public class ChunkBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ChunkBehaviour chunkBehaviour = target as ChunkBehaviour; // Only used in single selection cases
        
        // Dynamic change between single and multiple selection
        if(targets.Length > 1)
        {
            EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Multi-chunk match board state setters");
                EditorGUILayout.BeginHorizontal("box");
                    if(GUILayout.Button("Match board ON")) SetMultiChunkBoardMatchState(true);
                    if(GUILayout.Button("Match board OFF")) SetMultiChunkBoardMatchState(false);
                EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.HelpBox("Multi-chunk position editing and layer generation is not allowed.", MessageType.Info);

            EditorGUILayout.BeginVertical("box");
        }
        else
        {
            serializedObject.Update();

            SerializedProperty
                isMatchBoard = serializedObject.FindProperty(chunkBehaviour.IsMatchBoardReference),
                concatenatingPosition = serializedObject.FindProperty(chunkBehaviour.ConcatenatingPositionReference);

            EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(isMatchBoard, new GUIContent("Is a match board?"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Concatenating position");
                EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(concatenatingPosition.FindPropertyRelative("x"), new GUIContent("X"));
                    EditorGUILayout.PropertyField(concatenatingPosition.FindPropertyRelative("y"), new GUIContent("Z"));
                EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();

            // Layer creation logic
            EditorGUILayout.BeginVertical("box");
                if(GUILayout.Button("Generate new layer"))
                {
                    GameObject newLayer = new GameObject();
                    Undo.RegisterCreatedObjectUndo(newLayer, "Generated new layer");
                    newLayer.transform.parent = chunkBehaviour.transform;
                    newLayer.isStatic = true;
                    newLayer.AddComponent<LayerBehaviour>().TranslateHeightPosition();
                }

            serializedObject.ApplyModifiedProperties();
            chunkBehaviour.TranslateConcatenatingPosition();
        }

            // Layer sort logic
            if(GUILayout.Button("Sort layers ascendingly"))
            {
                foreach(var gameObject in targets)
                {
                    ChunkBehaviour isolatedChunkBehaviour = gameObject as ChunkBehaviour;
                    if (isolatedChunkBehaviour == null) continue;

                    // Collects valid LayerBehaviour children
                    List<LayerBehaviour> layers = new List<LayerBehaviour>();
                    HashSet<int> seenLayers = new HashSet<int>();

                    // Tracks duplicate layer heights
                    List<LayerBehaviour> duplicatedLayers = new List<LayerBehaviour>();

                    foreach(Transform child in isolatedChunkBehaviour.transform)
                    {
                        LayerBehaviour layer = child.GetComponent<LayerBehaviour>();
                        if(layer == null) continue;

                        if(seenLayers.Contains(layer.Height))
                        {
                            duplicatedLayers.Add(layer);
                            continue;
                        }

                        seenLayers.Add(layer.Height);
                        layers.Add(layer);
                    }

                    // Destroys all duplicate layers
                    foreach(LayerBehaviour duplicatedLayer in duplicatedLayers) DestroyImmediate(duplicatedLayer.gameObject);

                    // Sort layers in the hierarchy
                    layers.Sort((a, b) => a.Height.CompareTo(b.Height));
                    for(int i = 0; i < layers.Count; i++) layers[i].transform.SetSiblingIndex(i);
                }
            }
        EditorGUILayout.EndVertical();

        // Match board visual cue
        foreach(var gameObject in targets)
        {
            ChunkBehaviour isolatedChunkBehaviour = gameObject as ChunkBehaviour;
            if(isolatedChunkBehaviour == null) continue;

            isolatedChunkBehaviour.RedrawGridMaterials();
        }
    }

    public void SetMultiChunkBoardMatchState(bool state)
    {
        foreach(var gameObject in targets)
        {
            ChunkBehaviour isolatedChunkBehaviour = gameObject as ChunkBehaviour;
            if(isolatedChunkBehaviour == null) continue;

            SerializedObject isolatedSerializedObject = new SerializedObject(isolatedChunkBehaviour);

            isolatedSerializedObject.Update();

            SerializedProperty
                isolatedIsMatchBoard = isolatedSerializedObject.FindProperty(isolatedChunkBehaviour.IsMatchBoardReference);

            isolatedIsMatchBoard.boolValue = state;

            isolatedSerializedObject.ApplyModifiedProperties();
        }
    }
}