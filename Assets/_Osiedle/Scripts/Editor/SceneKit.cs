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

        /// <summary>Kwadratowa arena: podłoga o boku <paramref name="size"/> m i ściany dookoła.</summary>
        public static Transform Arena(float size, BuilderMaterials materials)
        {
            var level = new GameObject("Level").transform;
            float half = size * 0.5f;
            float wallCenter = half + WallThickness * 0.5f;
            float wallY = WallHeight * 0.5f;

            BuilderUtils.Block("Floor", new Vector3(0f, -0.5f, 0f), new Vector3(size, 1f, size), materials.Floor, level);
            BuilderUtils.Block("Wall_N", new Vector3(0f, wallY, wallCenter), new Vector3(size + 2f * WallThickness, WallHeight, WallThickness), materials.Wall, level);
            BuilderUtils.Block("Wall_S", new Vector3(0f, wallY, -wallCenter), new Vector3(size + 2f * WallThickness, WallHeight, WallThickness), materials.Wall, level);
            BuilderUtils.Block("Wall_E", new Vector3(wallCenter, wallY, 0f), new Vector3(WallThickness, WallHeight, size), materials.Wall, level);
            BuilderUtils.Block("Wall_W", new Vector3(-wallCenter, wallY, 0f), new Vector3(WallThickness, WallHeight, size), materials.Wall, level);
            return level;
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
            vcam.Follow = cameraTarget.transform;
            TopDownCamera.Apply(data, vcam, follow);
            vcamGo.transform.position = cameraTarget.transform.position + follow.FollowOffset;
            mainCameraGo.transform.SetPositionAndRotation(vcamGo.transform.position, vcamGo.transform.rotation);
        }

        public static void HitStop() => new GameObject("HitStop").AddComponent<HitStop>();
    }
}
