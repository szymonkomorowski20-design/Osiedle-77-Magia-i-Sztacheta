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
        public RangedWeaponData Proca;
        public EnemyData DummyData;
        public GameObject PlayerPrefab;
        public GameObject DummyPrefab;
        public GameObject KibicPrefab;

        public static SharedAssets Build()
        {
            var assets = new SharedAssets
            {
                PlayerData = BuilderUtils.LoadOrCreateData<PlayerData>(PlayerDataPath),
                Controls = InputControlsBuilder.Build(),
                Materials = BuilderMaterials.Build(),
                Sztacheta = CombatAssetsBuilder.Sztacheta(),
                Proca = CombatAssetsBuilder.Proca(),
                DummyData = CombatAssetsBuilder.DummyData(),
            };

            var scrapPrefab = CombatAssetsBuilder.ScrapPrefab(assets.Materials);
            var boltPrefab = CombatAssetsBuilder.BoltPrefab(assets.Materials);
            assets.PlayerPrefab = PlayerPrefabBuilder.Build(assets.PlayerData, assets.Controls, assets.Materials,
                assets.Sztacheta, assets.Proca, scrapPrefab, boltPrefab);
            assets.DummyPrefab = CombatAssetsBuilder.DummyPrefab(assets.DummyData, assets.Materials);
            assets.KibicPrefab = CombatAssetsBuilder.KibicPrefab(CombatAssetsBuilder.KibicData(),
                CombatAssetsBuilder.KibicCharge(), assets.Materials);
            return assets;
        }
    }
}
