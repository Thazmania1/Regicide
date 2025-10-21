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

        EditorGUILayout.PropertyField(concatenatingPosition);

        serializedObject.ApplyModifiedProperties();
        chunkManager.TranslateConcatenatingPosition();
    }
}