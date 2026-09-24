using Osiedle.Combat;
using Osiedle.Core;
using Osiedle.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Osiedle.Editor
{
    /// <summary>Wspólne elementy scen testowych: światło, podłoga ze ścianami, gracz, kamera i hit-stop.</summary>
    public static class SceneKit
    {
        const float WallHeight = 1.5f;
        const float WallThickness = 1f;

        public static Scene NewScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = AmbientMode.Skybox;
            return scene;
        }

        /// <summary>
        /// Kwadratowa arena: podłoga o boku <paramref name="size"/> m i ściany dookoła.
        /// Wysokość każdej ściany można podać osobno (0 lub mniej = domyślna, niska).
        /// Kamera patrzy na północ (+Z), więc wysokie ściany N/E/W pokazują fasady, a wysoka ściana S zasłoniłaby gracza.
        /// </summary>
        public static Transform Arena(float size, BuilderMaterials materials,
            float north = 0f, float east = 0f, float south = 0f, float west = 0f)
        {
            var level = new GameObject("Level").transform;
            float half = size * 0.5f;
            float wallCenter = half + WallThickness * 0.5f;
            float span = size + 2f * WallThickness;

            BuilderUtils.Block("Floor", new Vector3(0f, -0.5f, 0f), new Vector3(size, 1f, size), materials.Floor, level);
            Wall(level, materials, "Wall_N", new Vector3(0f, 0f, wallCenter), new Vector2(span, WallThickness), north);
            Wall(level, materials, "Wall_S", new Vector3(0f, 0f, -wallCenter), new Vector2(span, WallThickness), south);
            Wall(level, materials, "Wall_E", new Vector3(wallCenter, 0f, 0f), new Vector2(WallThickness, size), east);
            Wall(level, materials, "Wall_W", new Vector3(-wallCenter, 0f, 0f), new Vector2(WallThickness, size), west);
            return level;
        }

        static void Wall(Transform level, BuilderMaterials materials, string name, Vector3 foot, Vector2 footprint, float height)
        {
            if (height <= 0f) height = WallHeight;
            var center = new Vector3(foot.x, height * 0.5f, foot.z);
            BuilderUtils.Block(name, center, new Vector3(footprint.x, height, footprint.y), materials.Wall, level);
        }

        public static GameObject Player(Scene scene, SharedAssets assets, Vector3 position)
        {
            var player = (GameObject)PrefabUtility.InstantiatePrefab(assets.PlayerPrefab, scene);
            player.transform.SetPositionAndRotation(position, Quaternion.identity);
            return player;
        }

        /// <summary>Main Camera z mózgiem Cinemachine + kamera z góry jadąca za punktem wyprzedzenia.</summary>
        public static void CameraRig(PlayerData data, GameObject player)
        {
            var cameraTarget = new GameObject("CameraTarget");
            cameraTarget.transform.position = player.transform.position;
            var lookAhead = cameraTarget.AddComponent<CameraLookAhead>();
            BuilderUtils.Wire(lookAhead, ("data", data), ("aim", player.GetComponent<PlayerAim>()));

            var mainCameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            mainCameraGo.AddComponent<Camera>();
            mainCameraGo.AddComponent<AudioListener>();
            mainCameraGo.AddComponent<CinemachineBrain>();

            var vcamGo = new GameObject("CM_TopDown");
            var vcam = vcamGo.AddComponent<CinemachineCamera>();
            var follow = vcamGo.AddComponent<CinemachineFollow>();
            var topDown = vcamGo.AddComponent<TopDownCamera>();
            BuilderUtils.Wire(topDown, ("data", data));

            // Odbiornik wstrząsów ekranu (CameraShake na graczu wysyła impulsy).
            var listener = vcamGo.AddComponent<CinemachineImpulseListener>();
            listener.ApplyAfter = CinemachineCore.Stage.Noise;
            listener.ChannelMask = 1;
            listener.Gain = 1f;
            listener.UseCameraSpace = true;
            listener.ReactionSettings = new CinemachineImpulseListener.ImpulseReaction
            {
                AmplitudeGain = 1f,
                FrequencyGain = 1f,
                Duration = 1f,
            };
            vcam.Follow = cameraTarget.transform;
            TopDownCamera.Apply(data, vcam, follow);
            vcamGo.transform.position = cameraTarget.transform.position + follow.FollowOffset;
            mainCameraGo.transform.SetPositionAndRotation(vcamGo.transform.position, vcamGo.transform.rotation);
        }

        public static void HitStop() => new GameObject("HitStop").AddComponent<HitStop>();

        // Grubość jasnej krawędzi na obiektach do wskoczenia (m).
        const float EdgeThickness = 0.06f;

        /// <summary>Bryła, na którą da się wskoczyć dashem, z jasną krawędzią na górze.</summary>
        public static GameObject VaultBlock(string name, Vector3 center, Vector3 size, BuilderMaterials materials, Transform parent)
        {
            var go = BuilderUtils.Block(name, center, size, materials.Vaultable, parent);
            go.AddComponent<Vaultable>();

            // Jasna krawędź na górze: gracz od razu widzi, że da się tu wskoczyć.
            float edgeScaleY = EdgeThickness / size.y;
            var edge = BuilderUtils.Visual(PrimitiveType.Cube, "JasnaKrawedz", go.transform, materials.VaultEdge);
            edge.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            edge.transform.localScale = new Vector3(1.02f, edgeScaleY, 1.02f);
            edge.isStatic = true;
            return go;
        }
    }
}
