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

            // Podest (dach Kurdupla / skrzynie): wskocz dashem i strzelaj z góry (+20% obrażeń procy).
            SceneKit.VaultBlock("Podest", new Vector3(5.5f, 0.6f, -4f), new Vector3(3f, 1.2f, 3f), assets.Materials, level);

            var dummies = new GameObject("Manekiny").transform;
            Dummy(assets, dummies, "Manekin_Srodek", new Vector3(0f, 0f, 3f));
            Dummy(assets, dummies, "Manekin_Lewy", new Vector3(-3.5f, 0f, 4.5f));
            Dummy(assets, dummies, "Manekin_Prawy", new Vector3(3.5f, 0f, 4.5f));
            // Daleki cel dla procy, pod ścianą bloku (strzały odbite od ściany też go trafiają).
            Dummy(assets, dummies, "Manekin_Daleki", new Vector3(-6f, 0f, 8.5f));

            GameObject player = SceneKit.Player(scene, assets, new Vector3(0f, 0f, -2f));

            // Kibic Szarżujący: startuje w rogu przy garażach, idzie na Kubę i szarżuje z telegrafem.
            var kibic = (GameObject)PrefabUtility.InstantiatePrefab(assets.KibicPrefab, scene);
            kibic.transform.SetPositionAndRotation(new Vector3(6.5f, 0f, 7f), Quaternion.LookRotation(Vector3.back));
            BuilderUtils.Wire(kibic.GetComponent<Enemies.ChargerBrain>(), ("target", player.GetComponent<Combat.Hurtbox>()));
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
