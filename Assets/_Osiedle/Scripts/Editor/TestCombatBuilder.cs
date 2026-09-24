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

        [MenuItem("Osiedle/Build/Test_Combat")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            SharedAssets assets = SharedAssets.Build();
            var scene = SceneKit.NewScene();
            SceneKit.Arena(ArenaSize, assets.Materials);

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
