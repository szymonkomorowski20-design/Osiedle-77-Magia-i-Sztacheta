using System.IO;
using System.Linq;
using Osiedle.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Osiedle.Editor
{
    /// <summary>Wspólne narzędzia budowniczych: foldery, podpinanie pól, bryły, zapis scen.</summary>
    public static class BuilderUtils
    {
        public const string Root = "Assets/_Osiedle";

        public static void EnsureFolder(string path)
        {
            path = path.Replace('\\', '/');
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        /// <summary>
        /// Wczytuje plik danych albo tworzy go z wartościami domyślnymi.
        /// Istniejącego pliku nie nadpisuje, żeby nie skasować strojenia.
        /// </summary>
        public static T LoadOrCreateData<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            EnsureFolder(Path.GetDirectoryName(path));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        /// <summary>Podpina prywatne pola [SerializeField] komponentu.</summary>
        public static void Wire(Object target, params (string field, Object value)[] references)
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

        /// <summary>Podpina pole-tablicę [SerializeField] komponentu.</summary>
        public static void WireArray(Object target, string field, params Object[] values)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null || !property.isArray)
            {
                OsiedleLog.Error($"Brak pola-tablicy '{field}' w {target.GetType().Name}.");
                return;
            }

            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Statyczna bryła z colliderem (podłoga, ściana, przeszkoda).</summary>
        public static GameObject Block(string name, Vector3 center, Vector3 size, Material material, Transform parent)
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

        /// <summary>Bryła tylko do oglądania: bez collidera.</summary>
        public static GameObject Visual(PrimitiveType type, string name, Transform parent, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        /// <summary>Zapisuje prefab z tymczasowego obiektu i sprząta obiekt ze sceny.</summary>
        public static GameObject SavePrefab(GameObject root, string path)
        {
            try
            {
                EnsureFolder(Path.GetDirectoryName(path));
                return PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        public static void SaveScene(Scene scene, string path)
        {
            EnsureFolder(Path.GetDirectoryName(path));
            EditorSceneManager.SaveScene(scene, path);

            var scenes = EditorBuildSettings.scenes
                .Where(s => File.Exists(s.path) && s.path != path)
                .ToList();
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
