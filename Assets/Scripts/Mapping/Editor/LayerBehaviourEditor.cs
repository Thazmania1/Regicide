using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static ChunkBehaviour;

[CustomEditor(typeof(LayerBehaviour))]
[CanEditMultipleObjects]
public class LayerBehaviourEditor : Editor
{
    private void OnEnable()
    {
        Tools.hidden = true;
        SceneView.duringSceneGui += OnGlobalSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        LayerBehaviour layerBehaviour = (LayerBehaviour)target; // Only used in single selection cases

        // Wouldn't make sense to allow groupal height changes
        if (targets.Length > 1)
        {
            EditorGUILayout.HelpBox("Multi-layer height editing is not allowed.", MessageType.Info);
        }
        else
        {
            serializedObject.Update();

            SerializedProperty height = serializedObject.FindProperty(layerBehaviour.GetSerializedFieldName("Height"));
            EditorGUILayout.PropertyField(height);

            serializedObject.ApplyModifiedProperties();
            layerBehaviour.TranslateHeightPosition();
        }

        EditorGUILayout.Space();

        // Toggles off the entire grid
        if (GUILayout.Button("Reset grid"))
        {
            foreach(var gameObject in targets)
            {
                LayerBehaviour isolatedLayerBehaviour = gameObject as LayerBehaviour;
                if(isolatedLayerBehaviour == null) continue;

                SerializedObject isolatedSerializedObject = new SerializedObject(isolatedLayerBehaviour);
                SerializedProperty grid = isolatedSerializedObject.FindProperty(isolatedLayerBehaviour.GetSerializedFieldName("Grid"));

                isolatedSerializedObject.Update();

                for(int i = 0; i < grid.arraySize; i++)
                {
                    grid.GetArrayElementAtIndex(i).boolValue = false;
                    Transform blockTransform = isolatedLayerBehaviour.transform.Find($"Block {i}");
                    if (blockTransform != null)
                    {
                        Undo.DestroyObjectImmediate(blockTransform.gameObject);
                    }
                }

                isolatedSerializedObject.ApplyModifiedProperties();
            }
        }
    }

    // OnSceneGUI() doesn't support multi-selection on its own so a wrapper is needed
    private void OnGlobalSceneGUI(SceneView sceneView)
    {
        // Only draws a grid on actual layers, else it would break
        foreach (var gameObject in targets)
        {
            LayerBehaviour layerBehaviour = gameObject as LayerBehaviour;
            if (layerBehaviour == null) continue;

            SerializedObject serializedObject = new SerializedObject(layerBehaviour);
            DrawLayerGrid(layerBehaviour, serializedObject);
        }
    }

    private void DrawLayerGrid(LayerBehaviour layerBehaviour, SerializedObject isolatedSerializedObject)
    {
        isolatedSerializedObject.Update();

        Transform targetTransform = layerBehaviour.transform;

        // Calculates the center point of the layer, and spawns in an interactable grid
        IReadOnlyList<bool> rawLayerGrid = layerBehaviour.LayerGrid;
        int gridCenter = GRID_SIZE / 2;
        float gridOffset = rawLayerGrid.Count % 2 != 0 ? 0.0f : 0.5f;
        float gridXPositionStart = targetTransform.position.x - gridCenter + gridOffset;
        float gridYPosition = targetTransform.position.y;
        float gridZPositionStart = targetTransform.position.z - gridCenter + gridOffset;

        float blockSize = 1f;
        for (int row = 0; row < GRID_SIZE; row++)
        {
            int unwrappedRow = row * GRID_SIZE;

            for(int col = 0; col < GRID_SIZE; col++)
            {
                int unwrappedIndex = unwrappedRow + col;
                Vector3 blockPosition = new Vector3
                (
                    gridXPositionStart + col,
                    gridYPosition,
                    gridZPositionStart + row
                );

                // Colors to indicate block states
                Color resetColor = Handles.color;
                Handles.color = rawLayerGrid[unwrappedIndex] ? new Color(0f, 1f, 0f, 0.75f) : new Color(1f, 1f, 1f, 0.25f);

                if
                (
                    Handles.Button
                    (
                        blockPosition,
                        Quaternion.identity,
                        blockSize,
                        blockSize,
                        Handles.CubeHandleCap
                    )
                )
                {
                    SerializedProperty
                        grid = isolatedSerializedObject.FindProperty(layerBehaviour.GetSerializedFieldName("Grid"));

                    SerializedProperty block = grid.GetArrayElementAtIndex(unwrappedIndex);
                    block.boolValue = !block.boolValue;
                    if (block.boolValue)
                    {
                        GameObject newBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Undo.RegisterCreatedObjectUndo(newBlock, "Materialize grid block");

                        // Simulates a chess board
                        Material blockMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        blockMaterial.color = ((unwrappedIndex + row) % 2 == 0) ? Color.white : Color.black;
                        newBlock.GetComponent<Renderer>().sharedMaterial = blockMaterial;

                        newBlock.transform.parent = layerBehaviour.transform;
                        newBlock.name = $"Block {unwrappedIndex}";
                        newBlock.transform.position = blockPosition;
                    }
                    else
                    {
                        Transform blockTransform = layerBehaviour.transform.Find($"Block {unwrappedIndex}");
                        if (blockTransform != null)
                        {
                            Undo.DestroyObjectImmediate(blockTransform.gameObject);
                        }
                    }

                    isolatedSerializedObject.ApplyModifiedProperties();
                }

                Handles.color = resetColor;
            }
        }
    }

    private void OnDisable()
    {
        Tools.hidden = false;
        SceneView.duringSceneGui -= OnGlobalSceneGUI;
    }
}