using Osiedle.Combat;
using Osiedle.Enemies;
using Osiedle.Weapons;
using UnityEngine;

namespace Osiedle.Editor
{
    /// <summary>Dane i prefaby walki: Sztacheta, dane manekina, śrubka Złomu, manekin treningowy.</summary>
    public static class CombatAssetsBuilder
    {
        public const string SztachetaPath = BuilderUtils.Root + "/Data/Weapons/Sztacheta.asset";
        public const string DummyDataPath = BuilderUtils.Root + "/Data/Enemies/Manekin.asset";
        public const string ScrapPrefabPath = BuilderUtils.Root + "/Prefabs/Pickups/Scrap.prefab";
        public const string DummyPrefabPath = BuilderUtils.Root + "/Prefabs/Enemies/TrainingDummy.prefab";

        // Kształt manekina (worek na kiju) — wygląd, nie balans.
        const float DummyHeight = 1.6f;
        const float DummyRadius = 0.4f;

        public static MeleeWeaponData Sztacheta() => BuilderUtils.LoadOrCreateData<MeleeWeaponData>(SztachetaPath);

        public static EnemyData DummyData() => BuilderUtils.LoadOrCreateData<EnemyData>(DummyDataPath);

        public static ScrapPickup ScrapPrefab(BuilderMaterials materials)
        {
            var root = new GameObject("Scrap");
            root.AddComponent<ScrapPickup>();

            // Śrubka: mały płaski walec (łeb) + trzpień.
            var head = BuilderUtils.Visual(PrimitiveType.Cylinder, "Head", root.transform, materials.Scrap);
            head.transform.localScale = new Vector3(0.16f, 0.03f, 0.16f);
            head.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            var shank = BuilderUtils.Visual(PrimitiveType.Cylinder, "Shank", root.transform, materials.Scrap);
            shank.transform.localScale = new Vector3(0.07f, 0.06f, 0.07f);
            shank.transform.localPosition = new Vector3(0f, 0.06f, 0f);

            return BuilderUtils.SavePrefab(root, ScrapPrefabPath).GetComponent<ScrapPickup>();
        }

        public static GameObject DummyPrefab(EnemyData data, BuilderMaterials materials)
        {
            var root = new GameObject("TrainingDummy");

            var controller = root.AddComponent<CharacterController>();
            controller.height = DummyHeight;
            controller.radius = DummyRadius;
            controller.center = new Vector3(0f, DummyHeight * 0.5f, 0f);
            controller.minMoveDistance = 0f;

            var body = BuilderUtils.Visual(PrimitiveType.Cylinder, "Worek", root.transform, materials.Dummy);
            body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            body.transform.localScale = new Vector3(DummyRadius * 2f, 0.6f, DummyRadius * 2f);
            var head = BuilderUtils.Visual(PrimitiveType.Sphere, "Glowa", root.transform, materials.Dummy);
            head.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            head.transform.localScale = Vector3.one * 0.45f;

            var health = root.AddComponent<Health>();
            var knockback = root.AddComponent<Knockback>();
            var hurtbox = root.AddComponent<Hurtbox>();
            var dummy = root.AddComponent<TrainingDummy>();

            BuilderUtils.Wire(hurtbox, ("health", health), ("knockback", knockback));
            BuilderUtils.Wire(dummy, ("data", data), ("health", health), ("knockback", knockback));
            BuilderUtils.WireArray(dummy, "flashRenderers",
                body.GetComponent<Renderer>(), head.GetComponent<Renderer>());

            return BuilderUtils.SavePrefab(root, DummyPrefabPath);
        }
    }
}
