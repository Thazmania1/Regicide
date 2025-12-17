using UnityEngine;

public static class GridMaterialsUtility
{
    public static Material WhiteMaterial => Resources.Load<Material>("GridWhiteMaterial");
    public static Material BlackMaterial => Resources.Load<Material>("GridBlackMaterial");
    public static Material WhiteRedMaterial => Resources.Load<Material>("GridWhiteRedMaterial");
    public static Material BlackRedMaterial => Resources.Load<Material>("GridBlackRedMaterial");
}