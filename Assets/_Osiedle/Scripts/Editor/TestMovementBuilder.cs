using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Osiedle.Core;
using Osiedle.Player;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Osiedle.Editor
{
    /// <summary>
    /// Budowniczy etapu M0: dane gracza, mapa klawiszy, szare materiały, prefab gracza i scena Test_Movement.
    /// Można go uruchamiać wielokrotnie — za każdym razem odtwarza wynik od zera.
    /// Jedyny wyjątek: istniejący plik PlayerData nie jest nadpisywany, żeby nie skasować Twojego strojenia.
    /// </summary>
    public static class TestMovementBuilder
    {
        const string Root = "Assets/_Osiedle";
        const string PlayerDataPath = Root + "/Data/Player/PlayerData.asset";
        const string ControlsPath = Root + "/Data/Input/OsiedleControls.inputactions";
        const string MaterialsFolder = Root + "/Art/Materials";
        const string PlayerPrefabPath = Root + "/Prefabs/Player/Player.prefab";
        const string ScenePath = Root + "/Scenes/Test_Movement.unity";

        // Wymiary ciała Kuby (12 lat) — kształt postaci, nie balans.
        const float PlayerHeight = 1.4f;
        const float PlayerRadius = 0.35f;
        const float PlayerStepOffset = 0.3f;

        // Grubość jasnej krawędzi na obiektach do wskoczenia (m).
        const float EdgeThickness = 0.06f;

        const string BaseColorProperty = "_BaseColor";

        [MenuItem("Osiedle/Build/Test_Movement")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            PlayerData data = LoadOrCreatePlayerData();
            InputActionAsset controls = BuildControls();
            Materials materials = BuildMaterials();
            GameObject playerPrefab = BuildPlayerPrefab(data, controls, materials);
            BuildScene(data, playerPrefab, materials);

            AssetDatabase.SaveAssets();
            OsiedleLog.Info("Gotowe: scena " + ScenePath + " zbudowana. Wciśnij Play.");
        }

        // ---------- Dane ----------

        static PlayerData LoadOrCreatePlayerData()
        {
            var data = AssetDatabase.LoadAssetAtPath<PlayerData>(PlayerDataPath);
            if (data != null) return data;

            EnsureFolder(Path.GetDirectoryName(PlayerDataPath));
            data = ScriptableObject.CreateInstance<PlayerData>();
            AssetDatabase.CreateAsset(data, PlayerDataPath);
            return data;
        }

        // ---------- Sterowanie ----------

        static InputActionAsset BuildControls()
        {
            EnsureFolder(Path.GetDirectoryName(ControlsPath));
            string json = BuildControlsJson();
            string fullPath = Path.GetFullPath(ControlsPath);
            if (!File.Exists(fullPath) || File.ReadAllText(fullPath) != json)
            {
                File.WriteAllText(fullPath, json, new UTF8Encoding(false));
                AssetDatabase.ImportAsset(ControlsPath, ImportAssetOptions.ForceSynchronousImport);
            }

            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            // Ustawiamy nasz plik jako globalne akcje projektu (zastępuje usunięty plik z szablonu Unity).
            InputSystem.actions = asset;
            return asset;
        }

        static string BuildControlsJson()
        {
            const string map = PlayerInputReader.MapName;
            var actions = new[]
            {
                Action(PlayerInputReader.MoveAction, "Value", "Vector2"),
                Action(PlayerInputReader.AimAction, "Value", "Vector2"),
                Action(PlayerInputReader.DashAction, "Button", "Button"),
                Action(PlayerInputReader.MeleeAction, "Button", "Button"),
                Action(PlayerInputReader.RangedAction, "Button", "Button"),
                Action(PlayerInputReader.SpellAction, "Button", "Button"),
                Action(PlayerInputReader.InteractAction, "Button", "Button"),
                Action(PlayerInputReader.ThrowAction, "Button", "Button"),
            };

            var bindings = new[]
            {
                Binding("WASD", "2DVector", PlayerInputReader.MoveAction, composite: true),
                Binding("up", "<Keyboard>/w", PlayerInputReader.MoveAction, part: true),
                Binding("down", "<Keyboard>/s", PlayerInputReader.MoveAction, part: true),
                Binding("left", "<Keyboard>/a", PlayerInputReader.MoveAction, part: true),
                Binding("right", "<Keyboard>/d", PlayerInputReader.MoveAction, part: true),
                Binding("", "<Pointer>/position", PlayerInputReader.AimAction),
                Binding("", "<Keyboard>/space", PlayerInputReader.DashAction),
                Binding("", "<Mouse>/rightButton", PlayerInputReader.MeleeAction),
                Binding("", "<Mouse>/leftButton", PlayerInputReader.RangedAction),
                Binding("", "<Keyboard>/q", PlayerInputReader.SpellAction),
                Binding("", "<Keyboard>/e", PlayerInputReader.InteractAction),
                Binding("", "<Keyboard>/f", PlayerInputReader.ThrowAction),
            };

            var sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("    \"name\": \"OsiedleControls\",\n");
            sb.Append("    \"maps\": [\n");
            sb.Append("        {\n");
            sb.Append($"            \"name\": \"{map}\",\n");
            sb.Append($"            \"id\": \"{StableId("map/" + map)}\",\n");
            sb.Append("            \"actions\": [\n");
            sb.Append(string.Join(",\n", actions));
            sb.Append("\n            ],\n");
            sb.Append("            \"bindings\": [\n");
            sb.Append(string.Join(",\n", bindings));
            sb.Append("\n            ]\n");
            sb.Append("        }\n");
            sb.Append("    ],\n");
            sb.Append("    \"controlSchemes\": []\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        static string Action(string name, string type, string controlType)
        {
            return "                {\n" +
                   $"                    \"name\": \"{name}\",\n" +
                   $"                    \"type\": \"{type}\",\n" +
                   $"                    \"id\": \"{StableId("action/" + name)}\",\n" +
                   $"                    \"expectedControlType\": \"{controlType}\",\n" +
                   "                    \"processors\": \"\",\n" +
                   "                    \"interactions\": \"\",\n" +
                   $"                    \"initialStateCheck\": {(type == "Value" ? "true" : "false")}\n" +
                   "                }";
        }

        static string Binding(string name, string path, string action, bool composite = false, bool part = false)
        {
            return "                {\n" +
                   $"                    \"name\": \"{name}\",\n" +
                   $"                    \"id\": \"{StableId("binding/" + action + "/" + name + "/" + path)}\",\n" +
                   $"                    \"path\": \"{path}\",\n" +
                   "                    \"interactions\": \"\",\n" +
                   "                    \"processors\": \"\",\n" +
                   "                    \"groups\": \"\",\n" +
                   $"                    \"action\": \"{action}\",\n" +
                   $"                    \"isComposite\": {(composite ? "true" : "false")},\n" +
                   $"                    \"isPartOfComposite\": {(part ? "true" : "false")}\n" +
                   "                }";
        }

        // Stałe identyfikatory (z nazwy), żeby ponowne budowanie nie zmieniało pliku bez potrzeby.
        static string StableId(string key)
        {
            using var md5 = MD5.Create();
            return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes("OsiedleControls/" + key))).ToString();
        }

        // ---------- Materiały (szare bryły do M10) ----------

        class Materials
        {
            public Material Floor, Wall, Vaultable, VaultEdge, Player, PlayerFace, Arrow;
        }

        static Materials BuildMaterials()
        {
            EnsureFolder(MaterialsFolder);
            // Tylko szarości i beże o niskim nasyceniu: czerwień i kolory magii są zarezerwowane.
            return new Materials
            {
                Floor = GreyMaterial("M_Floor", new Color(0.42f, 0.42f, 0.40f)),
                Wall = GreyMaterial("M_Wall", new Color(0.30f, 0.30f, 0.31f)),
                Vaultable = GreyMaterial("M_Vaultable", new Color(0.52f, 0.50f, 0.46f)),
                VaultEdge = GreyMaterial("M_VaultEdge", new Color(0.86f, 0.85f, 0.80f)),
                Player = GreyMaterial("M_Player", new Color(0.93f, 0.90f, 0.82f)),
                PlayerFace = GreyMaterial("M_PlayerFace", new Color(0.18f, 0.18f, 0.20f)),
                Arrow = GreyMaterial("M_VaultArrow", new Color(0.97f, 0.97f, 0.94f)),
            };
        }

        static Material GreyMaterial(string name, Color color)
        {
            string path = MaterialsFolder + "/" + name + ".mat";
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

        // ---------- Prefab gracza ----------

        static GameObject BuildPlayerPrefab(PlayerData data, InputActionAsset controls, Materials materials)
        {
            EnsureFolder(Path.GetDirectoryName(PlayerPrefabPath));

            var root = new GameObject("Player");
            try
            {
                var controller = root.AddComponent<CharacterController>();
                controller.height = PlayerHeight;
                controller.radius = PlayerRadius;
                controller.center = new Vector3(0f, PlayerHeight * 0.5f, 0f);
                controller.stepOffset = PlayerStepOffset;
                // 0 = kontroler nie ignoruje małych ruchów (przy wysokim FPS docisk do ziemi jest bardzo mały).
                controller.minMoveDistance = 0f;

                // Wygląd: kapsuła + "twarz", żeby było widać, gdzie postać patrzy.
                var body = Visual(PrimitiveType.Capsule, "Body", root.transform, materials.Player);
                body.transform.localPosition = new Vector3(0f, PlayerHeight * 0.5f, 0f);
                body.transform.localScale = new Vector3(PlayerRadius * 2f, PlayerHeight * 0.5f, PlayerRadius * 2f);

                var face = Visual(PrimitiveType.Cube, "Face", root.transform, materials.PlayerFace);
                face.transform.localPosition = new Vector3(0f, PlayerHeight * 0.75f, PlayerRadius);
                face.transform.localScale = new Vector3(0.3f, 0.12f, 0.12f);

                // Strzałka nad krawędzią obiektu do wskoczenia (włączana przez VaultPrompt).
                var arrow = new GameObject("VaultArrow");
                arrow.transform.SetParent(root.transform, false);
                var diamond = Visual(PrimitiveType.Cube, "Diamond", arrow.transform, materials.Arrow);
                diamond.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                diamond.transform.localScale = new Vector3(0.28f, 0.28f, 0.06f);
                arrow.SetActive(false);

                var input = root.AddComponent<PlayerInputReader>();
                var motor = root.AddComponent<PlayerMotor>();
                var aim = root.AddComponent<PlayerAim>();
                var dash = root.AddComponent<PlayerDash>();
                var prompt = root.AddComponent<VaultPrompt>();

                Wire(input, ("actions", controls));
                Wire(motor, ("data", data), ("input", input));
                Wire(aim, ("data", data), ("input", input));
                Wire(dash, ("data", data), ("input", input), ("motor", motor), ("aim", aim));
                Wire(prompt, ("data", data), ("dash", dash), ("arrow", arrow.transform));

                return PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        // ---------- Scena testowa ----------

        static void BuildScene(PlayerData data, GameObject playerPrefab, Materials materials)
        {
            EnsureFolder(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientMode = AmbientMode.Skybox;

            var level = new GameObject("Level").transform;

            Block("Floor", new Vector3(0f, -0.5f, 0f), new Vector3(30f, 1f, 30f), materials.Floor, level);
            Block("Wall_N", new Vector3(0f, 0.75f, 15.5f), new Vector3(32f, 1.5f, 1f), materials.Wall, level);
            Block("Wall_S", new Vector3(0f, 0.75f, -15.5f), new Vector3(32f, 1.5f, 1f), materials.Wall, level);
            Block("Wall_E", new Vector3(15.5f, 0.75f, 0f), new Vector3(1f, 1.5f, 30f), materials.Wall, level);
            Block("Wall_W", new Vector3(-15.5f, 0.75f, 0f), new Vector3(1f, 1.5f, 30f), materials.Wall, level);

            // Obiekty do wskoczenia (dash przy nich = skok).
            VaultBlock("Murek", new Vector3(-5f, 0.4f, 4f), new Vector3(4f, 0.8f, 0.6f), materials, level);
            VaultBlock("Skrzynia_A", new Vector3(4f, 0.5f, 3f), new Vector3(1f, 1f, 1f), materials, level);
            VaultBlock("Skrzynia_B", new Vector3(5.5f, 0.6f, 3.5f), new Vector3(1.2f, 1.2f, 1.2f), materials, level);
            VaultBlock("Maska_Kurdupla", new Vector3(-5f, 0.35f, -4f), new Vector3(1.6f, 0.7f, 2.6f), materials, level);
            VaultBlock("Dach_Garazu", new Vector3(8f, 0.9f, -6f), new Vector3(4f, 1.8f, 6f), materials, level);

            // Kontrolne: za wysoki słup (bez skoku) i niski krawężnik (przechodzi się normalnie).
            Block("Slup_Za_Wysoki", new Vector3(0f, 1.5f, 8f), new Vector3(0.8f, 3f, 0.8f), materials.Wall, level);
            Block("Kraweznik", new Vector3(0f, 0.075f, -3f), new Vector3(6f, 0.15f, 0.4f), materials.Wall, level);

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            player.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Kamera: Main Camera z mózgiem Cinemachine + wirtualna kamera z góry jadąca za punktem wyprzedzenia.
            var cameraTarget = new GameObject("CameraTarget");
            var lookAhead = cameraTarget.AddComponent<CameraLookAhead>();
            Wire(lookAhead, ("data", data), ("aim", player.GetComponent<PlayerAim>()));

            var mainCameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            mainCameraGo.AddComponent<Camera>();
            mainCameraGo.AddComponent<AudioListener>();
            mainCameraGo.AddComponent<CinemachineBrain>();

            var vcamGo = new GameObject("CM_TopDown");
            var vcam = vcamGo.AddComponent<CinemachineCamera>();
            var follow = vcamGo.AddComponent<CinemachineFollow>();
            var topDown = vcamGo.AddComponent<TopDownCamera>();
            Wire(topDown, ("data", data));
            vcam.Follow = cameraTarget.transform;
            TopDownCamera.Apply(data, vcam, follow);
            vcamGo.transform.position = cameraTarget.transform.position + follow.FollowOffset;
            mainCameraGo.transform.SetPositionAndRotation(vcamGo.transform.position, vcamGo.transform.rotation);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuild(ScenePath);
        }

        static void AddSceneToBuild(string path)
        {
            var scenes = EditorBuildSettings.scenes
                .Where(s => File.Exists(s.path) && s.path != path)
                .ToList();
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        // ---------- Pomocnicze ----------

        static GameObject Block(string name, Vector3 center, Vector3 size, Material material, Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            go.isStatic = true;
            return go;
        }

        static GameObject VaultBlock(string name, Vector3 center, Vector3 size, Materials materials, Transform parent)
        {
            var go = Block(name, center, size, materials.Vaultable, parent);
            go.AddComponent<Vaultable>();

            // Jasna krawędź na górze: gracz od razu widzi, że da się tu wskoczyć.
            float edgeScaleY = EdgeThickness / size.y;
            var edge = Visual(PrimitiveType.Cube, "JasnaKrawedz", go.transform, materials.VaultEdge);
            edge.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            edge.transform.localScale = new Vector3(1.02f, edgeScaleY, 1.02f);
            edge.isStatic = true;
            return go;
        }

        /// <summary>Bryła tylko do oglądania: bez collidera.</summary>
        static GameObject Visual(PrimitiveType type, string name, Transform parent, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        /// <summary>Podpina prywatne pola [SerializeField] komponentu.</summary>
        static void Wire(Object target, params (string field, Object value)[] references)
        {
            var serialized = new SerializedObject(target);
            foreach (var (field, value) in references)
            {
                SerializedProperty property = serialized.FindProperty(field);
                if (property == null)
                {
                    OsiedleLog.Error($"Brak pola '{field}' w {target.GetType().Name}.");
                    continue;
                }
                property.objectReferenceValue = value;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
