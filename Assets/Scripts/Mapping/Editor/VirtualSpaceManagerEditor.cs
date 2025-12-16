using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VirtualSpaceManager))]
public class VirtualSpaceManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // UI references
        EditorGUILayout.BeginVertical("box");
            base.OnInspectorGUI();
        EditorGUILayout.EndVertical();

        VirtualSpaceManager virtualSpaceManager = target as VirtualSpaceManager;
        Transform virtualSpaceManagerTransform = virtualSpaceManager.transform;

        serializedObject.Update();

        EditorGUILayout.BeginVertical("box");
            if(GUILayout.Button("Generate new chunk"))
            {
                GameObject newChunk = new GameObject();
                Undo.RegisterCreatedObjectUndo(newChunk, "Generated new chunk");
                newChunk.transform.parent = virtualSpaceManagerTransform;
                newChunk.isStatic = true;
                newChunk.AddComponent<ChunkBehaviour>().TranslateConcatenatingPosition();
            }
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

        // The grid layout must never be manipulated
        virtualSpaceManagerTransform.position = new Vector3(0, 0, 0);
        virtualSpaceManagerTransform.rotation = Quaternion.identity;
        virtualSpaceManagerTransform.localScale = new Vector3(1, 1, 1);
    }
}