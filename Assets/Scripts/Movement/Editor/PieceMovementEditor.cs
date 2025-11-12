using UnityEngine;
using UnityEditor;
using static PieceMovement;
using static ChunkBehaviour;

[CustomEditor(typeof(PieceMovement), true)]
public class PieceMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PieceMovement pieceMovement = target as PieceMovement;
        PieceCoordinates serializationReference = new PieceCoordinates();


        serializedObject.Update();

        SerializedProperty
            currentPosition = serializedObject.FindProperty(pieceMovement.CurrentPositionReference);

        SerializedProperty
            currentChunk = currentPosition.FindPropertyRelative(serializationReference.ChunkReference),
            currentLayer = currentPosition.FindPropertyRelative(serializationReference.LayerReference),
            currentBlock = currentPosition.FindPropertyRelative(serializationReference.BlockReference);


        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Chunk");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(currentChunk.FindPropertyRelative("x"), new GUIContent("X"));
        EditorGUILayout.PropertyField(currentChunk.FindPropertyRelative("y"), new GUIContent("Z"));
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.PropertyField(currentLayer, new GUIContent("Layer"));
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Block");
        EditorGUI.indentLevel++;

        // Enforces correct block coordinates
        SerializedProperty blockX = currentBlock.FindPropertyRelative("x");
        SerializedProperty blockZ = currentBlock.FindPropertyRelative("y");

        int newXCoordinate = EditorGUILayout.IntField("X", blockX.intValue);
        blockX.intValue = (newXCoordinate % GRID_SIZE + GRID_SIZE) % GRID_SIZE;

        int newZCoordinate = EditorGUILayout.IntField("Z", blockZ.intValue);
        blockZ.intValue = (newZCoordinate % GRID_SIZE + GRID_SIZE) % GRID_SIZE;

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();


        serializedObject.ApplyModifiedProperties();
        if(!pieceMovement.TranslatePiecePosition()) EditorGUILayout.HelpBox("This position doesn't exist!", MessageType.Warning);
    }
}