using Osiedle.Core;
using Osiedle.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Osiedle.Editor
{
    /// <summary>
    /// Scena Test_Movement (etap M0): ruch, dash, skoki na przeszkody, kamera.
    /// Można ją budować wielokrotnie — za każdym razem powstaje od zera.
    /// Jedyny wyjątek: istniejące pliki danych (PlayerData, broń, wróg) nie są nadpisywane, żeby nie skasować strojenia.
    /// </summary>
    public static class TestMovementBuilder
    {
        const string ScenePath = BuilderUtils.Root + "/Scenes/Test_Movement.unity";
        const float ArenaSize = 30f;

        [MenuItem("Osiedle/Build/Test_Movement")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            SharedAssets assets = SharedAssets.Build();
            BuilderMaterials materials = assets.Materials;

            var scene = SceneKit.NewScene();
            Transform level = SceneKit.Arena(ArenaSize, materials);

            // Obiekty do wskoczenia (dash przy nich = skok).
            SceneKit.VaultBlock("Murek", new Vector3(-5f, 0.4f, 4f), new Vector3(4f, 0.8f, 0.6f), materials, level);
            SceneKit.VaultBlock("Skrzynia_A", new Vector3(4f, 0.5f, 3f), new Vector3(1f, 1f, 1f), materials, level);
            SceneKit.VaultBlock("Skrzynia_B", new Vector3(5.5f, 0.6f, 3.5f), new Vector3(1.2f, 1.2f, 1.2f), materials, level);
            SceneKit.VaultBlock("Maska_Kurdupla", new Vector3(-5f, 0.35f, -4f), new Vector3(1.6f, 0.7f, 2.6f), materials, level);
            SceneKit.VaultBlock("Dach_Garazu", new Vector3(8f, 0.9f, -6f), new Vector3(4f, 1.8f, 6f), materials, level);

            // Kontrolne: za wysoki słup (bez skoku) i niski krawężnik (przechodzi się normalnie).
            BuilderUtils.Block("Slup_Za_Wysoki", new Vector3(0f, 1.5f, 8f), new Vector3(0.8f, 3f, 0.8f), materials.Wall, level);
            BuilderUtils.Block("Kraweznik", new Vector3(0f, 0.075f, -3f), new Vector3(6f, 0.15f, 0.4f), materials.Wall, level);

            GameObject player = SceneKit.Player(scene, assets, Vector3.zero);
            SceneKit.CameraRig(assets.PlayerData, player);
            SceneKit.HitStop();

            BuilderUtils.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            OsiedleLog.Info("Gotowe: scena " + ScenePath + " zbudowana. Wciśnij Play.");
        }
    }
}
