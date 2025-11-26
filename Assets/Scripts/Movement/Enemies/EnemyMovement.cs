using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyMovement : PieceMovement
{
    // Tracks which blocks the piece can move to
    protected List<Coordinates> _calculatedMoves = new List<Coordinates>();

    // PawnMovement and BishopMovement use this variable
    protected Coordinates _lastCoordinates = new Coordinates();

    private void Start()
    {
        _gridManager = transform.root.GetComponent<GridManager>();
        TranslatePiecePosition();
        _preMatchCoordinates = _currentCoordinates.Clone();
        _lastCoordinates = _currentCoordinates.Clone();
    }

    // Enemies always make the move that gets them the closest to the player
    protected void PlayMove()
    {
        // Filters out non-match board chunks
        IReadOnlyList<ChunkBehaviour> matchBoard = _gridManager.CurrentMatchBoardChunks;
        for(int i = 0; i < _calculatedMoves.Count; i++)
        {
            bool isInMatchBoard = false;
            foreach(ChunkBehaviour chunk in matchBoard)
            {
                if(chunk.ConcatenatingPosition == _calculatedMoves[i].Chunk)
                {
                    isInMatchBoard = true;
                    break;
                }
            }
            
            if(!isInMatchBoard)
            {
                _calculatedMoves.RemoveAt(i);
                i--;
            }
        }

        Coordinates playerPieceCoordinates = _gridManager.CurrentMatchPlayerPiece.CurrentCoordinates;

        Coordinates bestMove = null;
        float bestDistance = float.MaxValue;
        foreach(Coordinates calculatedMove in _calculatedMoves)
        {
            float distance = Vector3.Distance(calculatedMove.ToWorldPosition3D(), playerPieceCoordinates.ToWorldPosition3D());
            Debug.Log($"{calculatedMove.Chunk} | {calculatedMove.Layer} | {calculatedMove.WrappedIndexBlock} (Unwrapped: {calculatedMove.UnwrappedIndexBlock})\nDistance from player: {distance}");
            if(distance < bestDistance)
            {
                bestMove = calculatedMove;
                bestDistance = distance;
                Debug.Log("New best move!");
            }
        }
        _calculatedMoves = new List<Coordinates>();
        if(bestMove == null) return;
        Debug.Log($"Best move: {bestMove.Chunk} | {bestMove.Layer} | {bestMove.WrappedIndexBlock} (Unwrapped: {bestMove.UnwrappedIndexBlock})");
        _currentCoordinates = bestMove.Clone();
        TranslatePiecePosition();

        // Tries to take the player piece
        TakePiece(_gridManager.CurrentMatchPlayerPiece);
    }
}