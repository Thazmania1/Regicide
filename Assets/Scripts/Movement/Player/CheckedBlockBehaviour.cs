using System.Collections;
using UnityEngine;
using static PieceMovement;

// Meant for the player's piece to become the child of the holder of this script
public class CheckedBlockBehaviour : MonoBehaviour
{
    private System.Action<Coordinates> _onBlockClicked;
    private Coordinates _blockCoordinates;

    private void OnMouseDown()
    {
        StartCoroutine(AntiClickFreeze());
    }

    private IEnumerator AntiClickFreeze()
    {
        yield return null;
        _onBlockClicked.Invoke(_blockCoordinates);
    }

    public void InitializeBlock(System.Action<Coordinates> clickEvent, Coordinates blockCoordinates)
    {
        _onBlockClicked = clickEvent;
        _blockCoordinates = blockCoordinates;
    }

    // Getters
    public Coordinates BlockCoordinates => _blockCoordinates.Clone();
}