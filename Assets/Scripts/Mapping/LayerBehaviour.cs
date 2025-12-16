using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VirtualSpaceManager;

public class LayerBehaviour : MonoBehaviour
{
    // Transform Yposition is based on the height.
    [SerializeField] private int _height = 0;
    public void TranslateHeightPosition()
    {
        gameObject.name = $"Layer {_height}";
        transform.localPosition = new Vector3Int(0, _height, 0);
    }

    // Tracks the occupied positions of the layer's grid
    [SerializeField] private bool[] _grid = new bool[GRID_SIZE * GRID_SIZE];

    // Draws its grid to represent an explorable area or a match board (also with IEnumerator for animation optimization)
    public void RedrawGridMaterials(bool isMatchBoard)
    {
        Material white = isMatchBoard ? GridMaterialsUtility.WhiteRedMaterial : GridMaterialsUtility.WhiteMaterial;
        Material black = isMatchBoard ? GridMaterialsUtility.BlackRedMaterial : GridMaterialsUtility.BlackMaterial;

        int index = 0;
        for(int row = 0; row < GRID_SIZE; row++)
        {
            for(int col = 0; col < GRID_SIZE; col++, index++)
            {
                if(index >= transform.childCount) return;

                Transform block = transform.GetChild(index);
                Renderer renderer = block.GetComponent<Renderer>();

                bool isWhite = (index + row) % 2 == 0;
                renderer.sharedMaterial = isWhite ? white : black;
            }
        }
    }
    public IEnumerator AnimatedRedrawGridMaterials(bool isMatchBoard)
    {
        Material white = isMatchBoard ? GridMaterialsUtility.WhiteRedMaterial : GridMaterialsUtility.WhiteMaterial;
        Material black = isMatchBoard ? GridMaterialsUtility.BlackRedMaterial : GridMaterialsUtility.BlackMaterial;

        int index = 0;
        for(int row = 0; row < GRID_SIZE; row++)
        {
            for(int col = 0; col < GRID_SIZE; col++, index++)
            {
                if(index >= transform.childCount) yield break;

                Transform block = transform.GetChild(index);
                Renderer renderer = block.GetComponent<Renderer>();

                bool isWhite = (index + row) % 2 == 0;
                renderer.sharedMaterial = isWhite ? white : black;
            }
        }
    }

    // Getters
    public int Height => _height;
    public IReadOnlyList<bool> Grid => System.Array.AsReadOnly(_grid);

    // Serialization getters
    public string HeightReference => nameof(_height);
    public string GridReference => nameof(_grid);
}