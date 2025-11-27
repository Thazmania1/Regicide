using UnityEngine;
using UnityEditor;
using static GridManager;
using static ChunkBehaviour;

// Runs every time the game recompiles in editor mode
[InitializeOnLoad]
public static class MapGizmos
{
    static MapGizmos()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    // Draws chunk corners and their names in labels (in a radius of GRID_SIZE * GRID_SIZE from the scene view camera)
    static void OnSceneGUI(SceneView sceneView)
    {
        Camera sceneCam = sceneView.camera;

        // Finds all chunks in the scene
        GameObject grid = GameObject.Find("Grid");
        foreach(Transform chunk in grid.transform)
        {
            float distanceFromChunk = Vector3.Distance(sceneCam.transform.position, chunk.position);

            if(distanceFromChunk > GRID_SIZE * GRID_SIZE) continue;

            int fontSize = Mathf.Clamp((int)(20f / (distanceFromChunk * 0.1f)), 0, 50);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = fontSize;
            style.alignment = TextAnchor.MiddleCenter;
            Vector3 labelPosition = new Vector3(chunk.position.x, sceneCam.transform.position.y, chunk.position.z);

            Handles.Label(labelPosition, chunk.name, style);

            for(int horizon = -1; horizon <= 1; horizon += 2)
            {
                for(int depth = -1; depth <= 1; depth += 2)
                {
                    Vector3 chunkCorner = CalculateChunkCorner(chunk, horizon, depth, false);

                    Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                    Handles.DrawLine
                    (
                        new Vector3
                        (
                            chunkCorner.x,
                            -100 + sceneCam.transform.position.y,
                            chunkCorner.z
                        ),
                        new Vector3
                        (
                            chunkCorner.x,
                            100 + sceneCam.transform.position.y,
                            chunkCorner.z
                        )
                    );
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                }
            }
        }
    }
}
