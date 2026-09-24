using Osiedle.Core;
using Osiedle.Player;
using UnityEditor;
using UnityEngine;

namespace Osiedle.Editor
{
    /// <summary>
    /// Narzędzia do pliku PlayerData. Budowniczy nie nadpisuje istniejących danych (żeby nie skasować strojenia),
    /// więc nowe wartości domyślne kamery trzeba wgrać świadomie — tym menu.
    /// </summary>
    public static class PlayerDataTools
    {
        [MenuItem("Osiedle/Dane/Kamera — przywróć wartości domyślne")]
        public static void ResetCamera()
        {
            var data = AssetDatabase.LoadAssetAtPath<PlayerData>(SharedAssets.PlayerDataPath);
            if (data == null)
            {
                OsiedleLog.Error("Brak " + SharedAssets.PlayerDataPath + ". Najpierw uruchom Osiedle/Build/Test_Combat.");
                return;
            }

            var defaults = ScriptableObject.CreateInstance<PlayerData>();
            try
            {
                Undo.RecordObject(data, "Kamera — wartości domyślne");
                data.cameraPitch = defaults.cameraPitch;
                data.cameraDistance = defaults.cameraDistance;
                data.cameraFieldOfView = defaults.cameraFieldOfView;
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssets();
            }
            finally
            {
                Object.DestroyImmediate(defaults);
            }

            OsiedleLog.Info($"Kamera: kąt {data.cameraPitch}°, odległość {data.cameraDistance} m, FOV {data.cameraFieldOfView}°.");
        }
    }
}
