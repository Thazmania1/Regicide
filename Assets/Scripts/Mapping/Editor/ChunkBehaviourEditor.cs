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

        // Wouldn't make sense to allow groupal position changes or layer generation
        if (targets.Length > 1)
        {
            EditorGUILayout.HelpBox("Multi-chunk position editing and layer generation is not allowed.", MessageType.Info);
        }
        else
        {
            serializedObject.Update();

            SerializedProperty
                concatenatingPosition = serializedObject.FindProperty(chunkBehaviour.ConcatenatingPositionReference);

            EditorGUILayout.LabelField("Concatenating position");
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(concatenatingPosition.FindPropertyRelative("x"), new GUIContent("X"));
            EditorGUILayout.PropertyField(concatenatingPosition.FindPropertyRelative("y"), new GUIContent("Z"));
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Layer creation logic
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

        EditorGUILayout.Space();

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

                foreach (Transform child in isolatedChunkBehaviour.transform)
                {
                    LayerBehaviour layer = child.GetComponent<LayerBehaviour>();
                    if (layer == null) continue;

                    if (seenLayers.Contains(layer.Height))
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
    }
}