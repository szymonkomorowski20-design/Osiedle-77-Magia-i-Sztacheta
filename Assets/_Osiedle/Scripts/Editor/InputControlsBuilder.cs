using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Osiedle.Player;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Osiedle.Editor
{
    /// <summary>
    /// Tworzy mapę klawiszy Data/Input/OsiedleControls (sterowanie z GDD, sekcja 3)
    /// i ustawia ją jako globalne akcje projektu.
    /// </summary>
    public static class InputControlsBuilder
    {
        public const string Path = BuilderUtils.Root + "/Data/Input/OsiedleControls.inputactions";

        public static InputActionAsset Build()
        {
            BuilderUtils.EnsureFolder(System.IO.Path.GetDirectoryName(Path));
            string json = BuildJson();
            string fullPath = System.IO.Path.GetFullPath(Path);
            if (!File.Exists(fullPath) || File.ReadAllText(fullPath) != json)
            {
                File.WriteAllText(fullPath, json, new UTF8Encoding(false));
                AssetDatabase.ImportAsset(Path, ImportAssetOptions.ForceSynchronousImport);
            }

            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(Path);
            // Nasz plik jako globalne akcje projektu (zastępuje usunięty plik z szablonu Unity).
            InputSystem.actions = asset;
            return asset;
        }

        static string BuildJson()
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
                Action(PlayerInputReader.RestartAction, "Button", "Button"),
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
                Binding("", "<Keyboard>/r", PlayerInputReader.RestartAction),
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
    }
}
