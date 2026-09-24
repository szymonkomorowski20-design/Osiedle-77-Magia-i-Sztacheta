using UnityEditor;
using UnityEngine;

namespace Osiedle.Editor
{
    /// <summary>
    /// Szare materiały prototypu (do M10). Tylko szarości i beże o niskim nasyceniu:
    /// czerwień i kolory magii są zarezerwowane.
    /// </summary>
    public class BuilderMaterials
    {
        const string Folder = BuilderUtils.Root + "/Art/Materials";
        const string BaseColorProperty = "_BaseColor";

        public Material Floor, Wall, Vaultable, VaultEdge, Player, PlayerFace, Arrow, Dummy, Scrap;

        public static BuilderMaterials Build()
        {
            BuilderUtils.EnsureFolder(Folder);
            return new BuilderMaterials
            {
                Floor = Grey("M_Floor", new Color(0.42f, 0.42f, 0.40f)),
                Wall = Grey("M_Wall", new Color(0.30f, 0.30f, 0.31f)),
                Vaultable = Grey("M_Vaultable", new Color(0.52f, 0.50f, 0.46f)),
                VaultEdge = Grey("M_VaultEdge", new Color(0.86f, 0.85f, 0.80f)),
                Player = Grey("M_Player", new Color(0.93f, 0.90f, 0.82f)),
                PlayerFace = Grey("M_PlayerFace", new Color(0.18f, 0.18f, 0.20f)),
                Arrow = Grey("M_VaultArrow", new Color(0.97f, 0.97f, 0.94f)),
                Dummy = Grey("M_Dummy", new Color(0.55f, 0.48f, 0.38f)),
                Scrap = Grey("M_Scrap", new Color(0.74f, 0.75f, 0.78f)),
            };
        }

        static Material Grey(string name, Color color)
        {
            string path = Folder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }

            material.SetColor(BaseColorProperty, color);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
