using Osiedle.Enemies;
using Osiedle.Player;
using Osiedle.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Osiedle.Editor
{
    /// <summary>Wszystko, czego potrzebują sceny testowe. Budowane od zera przy każdym uruchomieniu budowniczego.</summary>
    public class SharedAssets
    {
        public const string PlayerDataPath = BuilderUtils.Root + "/Data/Player/PlayerData.asset";

        public PlayerData PlayerData;
        public InputActionAsset Controls;
        public BuilderMaterials Materials;
        public MeleeWeaponData Sztacheta;
        public EnemyData DummyData;
        public GameObject PlayerPrefab;
        public GameObject DummyPrefab;

        public static SharedAssets Build()
        {
            var assets = new SharedAssets
            {
                PlayerData = BuilderUtils.LoadOrCreateData<PlayerData>(PlayerDataPath),
                Controls = InputControlsBuilder.Build(),
                Materials = BuilderMaterials.Build(),
                Sztacheta = CombatAssetsBuilder.Sztacheta(),
                DummyData = CombatAssetsBuilder.DummyData(),
            };

            var scrapPrefab = CombatAssetsBuilder.ScrapPrefab(assets.Materials);
            assets.PlayerPrefab = PlayerPrefabBuilder.Build(assets.PlayerData, assets.Controls, assets.Materials,
                assets.Sztacheta, scrapPrefab);
            assets.DummyPrefab = CombatAssetsBuilder.DummyPrefab(assets.DummyData, assets.Materials);
            return assets;
        }
    }
}
