using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static ChunkBehaviour;

[CustomEditor(typeof(LayerBehaviour))]
public class LayerBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Sets up serliazed fields
        LayerBehaviour layerBehaviour = (LayerBehaviour) target;

        serializedObject.Update();

        SerializedProperty
            height = serializedObject.FindProperty(layerBehaviour.GetSerializedFieldName("Height")),
            layerGrid = serializedObject.FindProperty(layerBehaviour.GetSerializedFieldName("LayerGrid"));

        EditorGUILayout.PropertyField(height);

        serializedObject.ApplyModifiedProperties();
        layerBehaviour.TranslateHeightPosition();
    }

    private void OnSceneGUI()
    {
        LayerBehaviour layerBehaviour = (LayerBehaviour) target;
        Transform targetTransform = layerBehaviour.transform;

        // Calculates the center point of the layer, and spawns in an interactable grid.
        IReadOnlyList<bool> rawLayerGrid = layerBehaviour.LayerGrid;
        int gridCenter = GRID_SIZE / 2;
        float gridOffset = rawLayerGrid.Count % 2 != 0 ? 0.0f : 0.5f;
        float gridXPositionStart = targetTransform.position.x - gridCenter + gridOffset;
        float gridZPositionStart = targetTransform.position.z - gridCenter + gridOffset;
        for (int row = 0; row < GRID_SIZE; row++)
        {
            int unwrappedRow = row * GRID_SIZE;

            for(int col = 0; col < GRID_SIZE; col++)
            {
                Handles.CubeHandleCap
                (
                    unwrappedRow + col,
                    new Vector3
                    (
                        gridXPositionStart + col,
                        0,
                        gridZPositionStart + row
                    ),
                    Quaternion.identity,
                    1f,
                    EventType.Repaint
                );
            }
        }
    }
}