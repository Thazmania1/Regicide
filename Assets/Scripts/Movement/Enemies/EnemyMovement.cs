using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyMovement : PieceMovement
{
    // Tracks which blocks the piece can move to
    protected List<Coordinates> _calculatedMoves = new List<Coordinates>();

    // PawnMovement and BishopMovement use this variable
    protected Coordinates _lastCoordinates = new Coordinates();

    // RookMovement and BishopMovement use these variables
    protected bool _isExtendedPathInterrupted = false;
    protected Vector3Int _extendedPathPosition = new Vector3Int();
    protected bool _isBlockedByPiece = false;

    private void Start()
    {
        _virtualPieceManager = transform.root.GetComponent<VirtualSpaceManager>();
        TranslatePiecePosition();
        _preMatchCoordinates = _currentCoordinates.Clone();
        _lastCoordinates = _currentCoordinates.Clone();
    }

    // Enemies always make the move that gets them the closest to the player
    protected void PlayMove()
    {
        // Filters out non-match board chunks
        IReadOnlyList<ChunkBehaviour> matchBoard = _virtualPieceManager.CurrentMatchBoardChunks;
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

        Coordinates playerPieceCoordinates = _virtualPieceManager.CurrentMatchPlayerPiece.CurrentCoordinates;

        Coordinates bestMove = null;
        float bestDistance = float.MaxValue;
        foreach(Coordinates calculatedMove in _calculatedMoves)
        {
            float distance = Vector3.Distance(calculatedMove.ToWorldPosition3D(), playerPieceCoordinates.ToWorldPosition3D());
            if(distance < bestDistance)
            {
                bestMove = calculatedMove;
                bestDistance = distance;
            }
        }
        _calculatedMoves = new List<Coordinates>();
        if(bestMove == null) return;
        _currentCoordinates = bestMove.Clone();
        TranslatePiecePosition();

        // Tries to take the player piece
        TakePiece(_virtualPieceManager.CurrentMatchPlayerPiece);
    }

    public override void ResetPiece()
    {
        _currentCoordinates = _preMatchCoordinates.Clone();
        _lastCoordinates = _currentCoordinates.Clone();
        TranslatePiecePosition(true);
        gameObject.SetActive(true);
    }
}