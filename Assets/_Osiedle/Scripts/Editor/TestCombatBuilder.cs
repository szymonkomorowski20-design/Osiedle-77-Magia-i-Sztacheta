using Osiedle.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Osiedle.Editor
{
    /// <summary>
    /// Scena Test_Combat (etap M1): arena z trzema manekinami do bicia sztachetą.
    /// Można ją budować wielokrotnie — za każdym razem powstaje od zera (pliki danych zostają nietknięte).
    /// </summary>
    public static class TestCombatBuilder
    {
        public const string ScenePath = BuilderUtils.Root + "/Scenes/Test_Combat.unity";
        const float ArenaSize = 20f;

        // Wysokie ściany do oceny kamery: czy widać fasady jak na wzorcu (Docs/Concept/wzorzec_walka.png).
        const float FacadeBlock = 4f;     // północ: ściana bloku
        const float FacadeGarages = 3f;   // wschód: rząd garaży
        const float FacadeWest = 3.5f;    // zachód: pawilon

        [MenuItem("Osiedle/Build/Test_Combat")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            SharedAssets assets = SharedAssets.Build();
            var scene = SceneKit.NewScene();
            Transform level = SceneKit.Arena(ArenaSize, assets.Materials,
                north: FacadeBlock, east: FacadeGarages, west: FacadeWest);

            // Wolnostojący garaż 3 m w środku areny: widać jego fasadę i to, czy zasłania postać za nim.
            BuilderUtils.Block("Garaz_Wolnostojacy", new Vector3(-6f, FacadeGarages * 0.5f, -3f),
                new Vector3(3f, FacadeGarages, 4f), assets.Materials.Wall, level);

            var dummies = new GameObject("Manekiny").transform;
            Dummy(assets, dummies, "Manekin_Srodek", new Vector3(0f, 0f, 3f));
            Dummy(assets, dummies, "Manekin_Lewy", new Vector3(-3.5f, 0f, 4.5f));
            Dummy(assets, dummies, "Manekin_Prawy", new Vector3(3.5f, 0f, 4.5f));

            GameObject player = SceneKit.Player(scene, assets, new Vector3(0f, 0f, -2f));
            SceneKit.CameraRig(assets.PlayerData, player);
            SceneKit.HitStop();

            BuilderUtils.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            OsiedleLog.Info("Gotowe: scena " + ScenePath + " zbudowana. Wciśnij Play i bij manekiny PPM.");
        }

        static void Dummy(SharedAssets assets, Transform parent, string name, Vector3 position)
        {
            var dummy = (GameObject)PrefabUtility.InstantiatePrefab(assets.DummyPrefab, parent);
            dummy.name = name;
            dummy.transform.position = position;
            // Manekin patrzy w stronę gracza (tylko wygląd).
            dummy.transform.rotation = Quaternion.LookRotation(Vector3.back);
        }
    }
}
