using System.Linq;
using UnityEngine;
using static GridManager;

// Used for individual piece movement patterns
public abstract class PieceMovement : MonoBehaviour
{
    // Tracks a chunk position, a layer height, and a block position
    [System.Serializable] public class Coordinates
    {
        [SerializeField] private Vector2Int _chunk;
        [SerializeField] private int _layer;
        [SerializeField] private Vector2Int _block;

        public Coordinates(Vector2Int chunk, int layer, Vector2Int wrappedIndexBlock)
        {
            _chunk = chunk;
            _layer = layer;
            _block = wrappedIndexBlock;
        }

        public Coordinates(Vector2Int chunk, int layer, int unwrappedIndexBlock)
        {
            _chunk = chunk;
            _layer = layer;
            _block = new Vector2Int(unwrappedIndexBlock % GRID_SIZE, unwrappedIndexBlock / GRID_SIZE);
        }

        public Coordinates() { }

        public Coordinates Clone()
        {
            return new Coordinates(_chunk, _layer, _block);
        }

        // For coordinate distance comparisions
        public Vector3Int ToWorldPosition3D()
        {
            return new Vector3Int
            (
                _chunk.x * GRID_SIZE + _block.x,
                _layer,
                _chunk.y * GRID_SIZE + _block.y
            );
        }
        public Vector2Int ToWorldPosition2D()
        {
            return new Vector2Int
            (
                _chunk.x * GRID_SIZE + _block.x,
                _chunk.y * GRID_SIZE + _block.y
            );
        }

        // Getters and setters
        public Vector2Int Chunk {get => _chunk; set => _chunk = value;}
        public int Layer {get => _layer; set => _layer = value;}
        public Vector2Int WrappedIndexBlock {get => _block; set => _block = value;}
        public int UnwrappedIndexBlock => _block.x + _block.y * GRID_SIZE;

        // Serialization getters
        public string ChunkReference => nameof(_chunk);
        public string LayerReference => nameof(_layer);
        public string BlockReference => nameof(_block);
    }

    // Tracks the piece's current position
    [SerializeField] protected Coordinates _currentCoordinates = new Coordinates();

    // Tracks the last position the piece was on before a match
    protected Coordinates _preMatchCoordinates = new Coordinates();

    // Tracks what position is being checked
    protected Coordinates _checkingCoordinates = new Coordinates();

    // Reference to the piece's grid manager
    protected GridManager _gridManager;


    private void Start()
    {
        _gridManager = transform.root.GetComponent<GridManager>();
        TranslatePiecePosition();
        CalculatePieceMoves();
    }

    // Block parent is based on the piece's current coordinates
    public virtual bool TranslatePiecePosition()
    {
        Transform queriedBlock = FindBlock(_currentCoordinates.Chunk, _currentCoordinates.Layer, _currentCoordinates.UnwrappedIndexBlock);
        if(queriedBlock == null) return false;

        transform.parent = queriedBlock;
        transform.localPosition = new Vector3(0, 1, 0);

        // Resets the pathfinder
        _checkingCoordinates = _currentCoordinates.Clone();
        return true;
    }

    // Queries
    public Transform FindChunk(Vector2Int concatenatingPositionQuery)
    {
        var queriedChunk = transform.root
            .GetComponentsInChildren<ChunkBehaviour>()
            .FirstOrDefault(chunk => chunk != null && chunk.ConcatenatingPosition == concatenatingPositionQuery);

        return queriedChunk != null ? queriedChunk.transform : null;
    }

    public Transform FindLayer(Transform chunk, int heightQuery)
    {
        var queriedLayer = chunk
            .GetComponentsInChildren<LayerBehaviour>()
            .FirstOrDefault(layer => layer != null && layer.Height == heightQuery);

        return queriedLayer != null ? queriedLayer.transform : null;
    }

    public Transform FindBlock(Transform layer, int unwrappedIndexQuery, bool isBlockActiveStateRelevant = true)
    {
        Transform queriedBlock = layer.GetChild(unwrappedIndexQuery);
        if(!isBlockActiveStateRelevant) return queriedBlock;
        LayerBehaviour layerBehaviour = layer.GetComponent<LayerBehaviour>();
        return layerBehaviour.Grid[unwrappedIndexQuery] ? queriedBlock : null;
    }

    public Transform FindBlock(Vector2Int concatenatingPositionQuery, int heightQuery, int unwrappedIndexQuery, bool isBlockActiveStateRelevant = true)
    {
        Transform queriedChunk = FindChunk(concatenatingPositionQuery);
        if(queriedChunk == null) return null;

        Transform queriedLayer = FindLayer(queriedChunk, heightQuery);
        if(queriedLayer == null) return null;

        Transform queriedBlock = FindBlock(queriedLayer, unwrappedIndexQuery, isBlockActiveStateRelevant);
        if(queriedBlock == null) return null;

        return queriedBlock;
    }

    public Transform FindBlock(Coordinates blockCoordinates, bool isBlockActiveStateRelevant = true)
    {
        Transform queriedChunk = FindChunk(blockCoordinates.Chunk);
        if(queriedChunk == null) return null;

        Transform queriedLayer = FindLayer(queriedChunk, blockCoordinates.Layer);
        if(queriedLayer == null) return null;

        Transform queriedBlock = FindBlock(queriedLayer, blockCoordinates.UnwrappedIndexBlock, isBlockActiveStateRelevant);
        if(queriedBlock == null) return null;

        return queriedBlock;
    }

    // The piece's movement possibilities
    public abstract void CalculatePieceMoves();

    // The invoker of this method will destroy the provided piece if the two pieces are on the same coordinates
    public void TakePiece(PieceMovement piece)
    {
        Coordinates pieceCoordinates = piece.CurrentCoordinates;
        if(_currentCoordinates.Chunk == pieceCoordinates.Chunk && _currentCoordinates.Layer == pieceCoordinates.Layer && _currentCoordinates.UnwrappedIndexBlock == pieceCoordinates.UnwrappedIndexBlock) piece.gameObject.SetActive(false);
    }

    // When the player loses a match, this method is executed
    public abstract void ResetPiece();

    // Getters and setters
    public Coordinates CurrentCoordinates {get => _currentCoordinates.Clone(); set { _currentCoordinates = value; TranslatePiecePosition(); }}
    public Transform GetBlock => transform.parent;
    public Transform GetLayer => transform.parent.parent;
    public Transform GetChunk => transform.parent.parent.parent;

    // Serialization getters
    public string CurrentCoordinatesReference => nameof(_currentCoordinates);
}