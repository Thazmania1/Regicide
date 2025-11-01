using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GridManager gridManager = target as GridManager;
        Transform gridManagerTransform = gridManager.transform;

        if (GUILayout.Button("Generate new chunk"))
        {
            GameObject newChunk = new GameObject();
            Undo.RegisterCreatedObjectUndo(newChunk, "Generated new chunk");
            newChunk.transform.parent = gridManagerTransform;
            newChunk.isStatic = true;
            newChunk.AddComponent<ChunkBehaviour>().TranslateConcatenatingPosition();
        }

        // The grid layout must never be manipulated
        gridManagerTransform.position = new Vector3(0, 0, 0);
        gridManagerTransform.rotation = Quaternion.identity;
        gridManagerTransform.localScale = new Vector3(1, 1, 1);
    }
}