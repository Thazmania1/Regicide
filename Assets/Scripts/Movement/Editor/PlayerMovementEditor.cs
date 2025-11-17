using UnityEditor;

[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovementEditor : PieceMovementEditor
{
    public override void OnInspectorGUI()
    {
        PlayerMovement playerMovement = target as PlayerMovement;

        serializedObject.Update();

        SerializedProperty
            currentPattern = serializedObject.FindProperty(playerMovement.CurrentPatternReference);

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(currentPattern);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }
}