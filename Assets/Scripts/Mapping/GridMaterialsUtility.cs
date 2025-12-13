using UnityEditor;
using UnityEngine;

public static class GridMaterialsUtility
{
    private const string WhiteMatPath = "Assets/Scripts/Mapping/Resources/GridWhiteMaterial.mat";
    private const string BlackMatPath = "Assets/Scripts/Mapping/Resources/GridBlackMaterial.mat";
    private const string WhiteRedMatPath = "Assets/Scripts/Mapping/Resources/GridWhiteRedMaterial.mat";
    private const string BlackRedMatPath = "Assets/Scripts/Mapping/Resources/GridBlackRedMaterial.mat";

    private static Material _whiteMaterial;
    private static Material _blackMaterial;
    private static Material _whiteRedMaterial;
    private static Material _blackRedMaterial;

    public static Material WhiteMaterial
    {
        get { EnsureMaterials(); return _whiteMaterial; }
    }

    public static Material BlackMaterial
    {
        get { EnsureMaterials(); return _blackMaterial; }
    }

    public static Material WhiteRedMaterial
    {
        get { EnsureMaterials(); return _whiteRedMaterial; }
    }

    public static Material BlackRedMaterial
    {
        get { EnsureMaterials(); return _blackRedMaterial; }
    }

    private static void EnsureMaterials()
    {
        // Tries to load cached materials
        _whiteMaterial = _whiteMaterial ?? AssetDatabase.LoadAssetAtPath<Material>(WhiteMatPath);
        _blackMaterial = _blackMaterial ?? AssetDatabase.LoadAssetAtPath<Material>(BlackMatPath);
        _whiteRedMaterial = _whiteRedMaterial ?? AssetDatabase.LoadAssetAtPath<Material>(WhiteRedMatPath);
        _blackRedMaterial = _blackRedMaterial ?? AssetDatabase.LoadAssetAtPath<Material>(BlackRedMatPath);

        // Regenerates only missing materials
        if(_whiteMaterial == null) CreateMaterial(Color.white, "GridWhiteMaterial", WhiteMatPath, out _whiteMaterial);
        if(_blackMaterial == null) CreateMaterial(Color.black, "GridBlackMaterial", BlackMatPath, out _blackMaterial);
        if(_whiteRedMaterial == null) CreateMaterial(new Color(1f, 0.5f, 0.5f), "GridWhiteRedMaterial", WhiteRedMatPath, out _whiteRedMaterial);
        if(_blackRedMaterial == null) CreateMaterial(new Color(0.5f, 0f, 0f), "GridBlackRedMaterial", BlackRedMatPath, out _blackRedMaterial);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void CreateMaterial(Color color, string name, string path, out Material mat)
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        mat = new Material(lit)
        {
            color = color,
            name = name
        };
        AssetDatabase.CreateAsset(mat, path);
    }
}
