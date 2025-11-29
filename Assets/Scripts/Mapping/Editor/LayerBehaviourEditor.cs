using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static GridManager;
using static ChunkBehaviour;
using static PieceMovement;

[CustomEditor(typeof(LayerBehaviour))]
[CanEditMultipleObjects]
public class LayerBehaviourEditor : Editor
{
    private void OnEnable()
    {
        Tools.hidden = true;
        SceneView.duringSceneGui += OnGlobalSceneGUI;

        InitializePhysicalGrid();
    }
    private void OnDisable()
    {
        Tools.hidden = false;
        SceneView.duringSceneGui -= OnGlobalSceneGUI;
    }


    public override void OnInspectorGUI()
    {
        LayerBehaviour layerBehaviour = target as LayerBehaviour; // Only used in single selection cases

        // Wouldn't make sense to allow groupal height changes
        if(targets.Length > 1)
            EditorGUILayout.HelpBox("Multi-layer height editing is not allowed.", MessageType.Info);
        else
        {
            EditorGUILayout.BeginVertical("box");
            serializedObject.Update();

            SerializedProperty height = serializedObject.FindProperty(layerBehaviour.HeightReference);
            EditorGUILayout.PropertyField(height);

            serializedObject.ApplyModifiedProperties();
            layerBehaviour.TranslateHeightPosition();
            EditorGUILayout.EndVertical();
        }

        // Resets the selected layers' grids
        EditorGUILayout.BeginVertical("box");
        if(GUILayout.Button("Reset grid"))
        {
            foreach(var gameObject in targets)
            {
                LayerBehaviour isolatedLayerBehaviour = gameObject as LayerBehaviour;
                if(isolatedLayerBehaviour == null) continue;

                SerializedObject isolatedSerializedObject = new SerializedObject(isolatedLayerBehaviour);
                SerializedProperty grid = isolatedSerializedObject.FindProperty(isolatedLayerBehaviour.GridReference);

                isolatedSerializedObject.Update();

                for(int i = 0; i < grid.arraySize; i++)
                {
                    grid.GetArrayElementAtIndex(i).boolValue = false;
                }
                isolatedSerializedObject.ApplyModifiedProperties();
                ResetPhysicalGrid(isolatedLayerBehaviour);
            }
        }
        EditorGUILayout.EndVertical();

        // Wouldn't make sense to allow groupal piece spawns
        if(targets.Length > 1)
        {
            EditorGUILayout.HelpBox("Multi-layer piece spawns are not allowed.", MessageType.Info);
        }
        else
        {
            // Checks if there's any active block in the layer (the knight ignores this)
            IReadOnlyList<bool> grid = layerBehaviour.Grid;
            bool isLayerHabitable = false;
            int blockUnwrappedIndex = 0;
            for(; blockUnwrappedIndex < grid.Count; blockUnwrappedIndex++) if(grid[blockUnwrappedIndex]) { isLayerHabitable = true; break; }

            EditorGUILayout.BeginVertical("box");
            if(!isLayerHabitable)
            {
                EditorGUILayout.HelpBox("Only the enemy knight can reside on layers with no active blocks.", MessageType.Info);
            }
            else
            {
                if(GUILayout.Button("Spawn enemy pawn")) SpawnEnemyPiece<PawnMovement>("Pawn", layerBehaviour, blockUnwrappedIndex);
                if(GUILayout.Button("Spawn enemy rook")) SpawnEnemyPiece<RookMovement>("Rook", layerBehaviour, blockUnwrappedIndex);
                if(GUILayout.Button("Spawn enemy bishop")) SpawnEnemyPiece<BishopMovement>("Bishop", layerBehaviour, blockUnwrappedIndex);
            }
            if(GUILayout.Button("Spawn enemy knight")) SpawnEnemyPiece<KnightMovement>("Knight", layerBehaviour, blockUnwrappedIndex);
            EditorGUILayout.EndVertical();
        }
    }


    // OnSceneGUI() doesn't support multi-selection on its own so a wrapper is needed
    private void OnGlobalSceneGUI(SceneView sceneView)
    {
        // Only draws a grid on actual layers
        foreach(var gameObject in targets)
        {
            LayerBehaviour layerBehaviour = gameObject as LayerBehaviour;
            if(layerBehaviour == null) continue;

            SerializedObject serializedObject = new SerializedObject(layerBehaviour);
            DrawHandleGrid(layerBehaviour, serializedObject);
        }
    }

    // Draws an interactable grid to toggle physical blocks on and off
    private void DrawHandleGrid(LayerBehaviour layerBehaviour, SerializedObject isolatedSerializedObject)
    {
        isolatedSerializedObject.Update();

        Transform layerTransform = layerBehaviour.transform;
        IReadOnlyList<bool> rawLayerGrid = layerBehaviour.Grid;
        Vector3 gridStartPoint = CalculateChunkCorner(layerTransform);
        float blockSize = 1f;
        for(int row = 0; row < GRID_SIZE; row++)
        {
            int unwrappedRow = row * GRID_SIZE;

            for(int col = 0; col < GRID_SIZE; col++)
            {
                int unwrappedIndex = unwrappedRow + col;
                Vector3 blockPosition = new Vector3
                (
                    gridStartPoint.x + col,
                    gridStartPoint.y,
                    gridStartPoint.z + row
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
                        grid = isolatedSerializedObject.FindProperty(layerBehaviour.GridReference);

                    // Toggles the physical block on or off
                    SerializedProperty block = grid.GetArrayElementAtIndex(unwrappedIndex);
                    block.boolValue = !block.boolValue;
                    GameObject blockObject = layerTransform.GetChild(unwrappedIndex).gameObject;
                    Undo.RecordObject(blockObject, "Toggle block");
                    blockObject.SetActive(block.boolValue);

                    isolatedSerializedObject.ApplyModifiedProperties();
                }

                Handles.color = resetColor;
            }
        }
    }

    
    // Checks if the layer's physical grid is initialized
    private void InitializePhysicalGrid()
    {
        foreach (var gameObject in targets)
        {
            LayerBehaviour layerBehaviour = gameObject as LayerBehaviour;
            if (layerBehaviour == null) continue;

            if (layerBehaviour.transform.childCount != GRID_SIZE * GRID_SIZE) ResetPhysicalGrid(layerBehaviour);
        }
    }

    // Populates the layer with a grid of physical blocks
    private void ResetPhysicalGrid(LayerBehaviour layerBehaviour)
    {
        Transform layerTransform = layerBehaviour.transform;

        // Destroys the old physical grid
        // Foreach uses an augmenting index to get elements, deleting children while indexing surprisingly affects foreach
        while(layerTransform.childCount > 0)
        {
            Undo.DestroyObjectImmediate(layerTransform.GetChild(0).gameObject);
        }

        IReadOnlyList<bool> grid = layerBehaviour.Grid;
        Vector3 gridStartPoint = CalculateChunkCorner(layerTransform);
        for(int row = 0; row < GRID_SIZE; row++)
        {
            int unwrappedRow = row * GRID_SIZE;

            for(int col = 0; col < GRID_SIZE; col++)
            {
                int unwrappedIndex = unwrappedRow + col;
                Vector3 blockPosition = new Vector3
                (
                    gridStartPoint.x + col,
                    gridStartPoint.y,
                    gridStartPoint.z + row
                );

                GameObject newBlock = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Undo.RegisterCreatedObjectUndo(newBlock, "Create block");

                // Simulates a chess board
                Material blockMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                blockMaterial.color = ((unwrappedIndex + row) % 2 == 0) ? Color.white : Color.black;
                newBlock.GetComponent<Renderer>().sharedMaterial = blockMaterial;

                newBlock.isStatic = true;
                newBlock.SetActive(grid[unwrappedIndex]);
                newBlock.transform.parent = layerTransform.transform;
                newBlock.name = $"Block {unwrappedIndex}";
                newBlock.transform.position = blockPosition;
            }
        }
    }


    // Generic enemy piece spawner
    private void SpawnEnemyPiece<T>(string pieceName, LayerBehaviour layerBehaviour, int blockUnwrappedIndex) where T : EnemyMovement
    {
        GameObject newPiece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(newPiece, $"Spawn enemy {pieceName}");

        newPiece.transform.parent = layerBehaviour.transform.root;
        newPiece.name = pieceName;

        T newPieceScript = newPiece.AddComponent<T>();
        newPieceScript.CurrentCoordinates = new Coordinates
        (
            layerBehaviour.transform.parent.GetComponent<ChunkBehaviour>().ConcatenatingPosition,
            layerBehaviour.Height,
            blockUnwrappedIndex
        );
    }
}