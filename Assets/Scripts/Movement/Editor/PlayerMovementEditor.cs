using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovementEditor : PieceMovementEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space();

        PlayerMovement playerMovement = target as PlayerMovement;

        serializedObject.Update();

        SerializedProperty
            currentPattern = serializedObject.FindProperty(playerMovement.CurrentPatternReference),
            UIIconSelectionAnimation = serializedObject.FindProperty(playerMovement.UIIconSelectionAnimationReference),
            playerPatternsPanel = serializedObject.FindProperty(playerMovement.PlayerPatternsPanelReference);

        EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(currentPattern);
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(UIIconSelectionAnimation);
            EditorGUILayout.PropertyField(playerPatternsPanel);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        serializedObject.ApplyModifiedProperties();

        // Scales icons accordingly to the chosen pattern
        foreach(RectTransform patternIcon in playerMovement.PlayerPatternsPanel.transform)
        {
            if(patternIcon == null) continue;
            bool isSelected = patternIcon.name == playerMovement.CurrentPattern.ToString();

            Vector3 targetScale = isSelected ? Vector3.one * 1.1f : Vector3.one * 1.0f;
            patternIcon.localScale = targetScale;
        }
    }
}