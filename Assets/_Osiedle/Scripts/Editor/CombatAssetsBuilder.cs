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

        public const string ProcaPath = BuilderUtils.Root + "/Data/Weapons/Proca.asset";
        public const string BoltPrefabPath = BuilderUtils.Root + "/Prefabs/Projectiles/Sruba.prefab";

        // Wygląd śruby i jej smugi — tylko czytelność, nie balans.
        const float BoltVisualSize = 0.18f;
        const float TrailTime = 0.08f;
        const float TrailWidth = 0.12f;

        public static MeleeWeaponData Sztacheta() => BuilderUtils.LoadOrCreateData<MeleeWeaponData>(SztachetaPath);

        public static RangedWeaponData Proca() => BuilderUtils.LoadOrCreateData<RangedWeaponData>(ProcaPath);

        public static Projectile BoltPrefab(BuilderMaterials materials)
        {
            var root = new GameObject("Sruba");
            root.AddComponent<Projectile>();

            var body = BuilderUtils.Visual(PrimitiveType.Sphere, "Glowka", root.transform, materials.Bolt);
            body.transform.localScale = new Vector3(BoltVisualSize, BoltVisualSize, BoltVisualSize * 1.6f);

            // Krótka smuga za pociskiem: przy 22 m/s bez niej śruby prawie nie widać.
            var trail = root.AddComponent<TrailRenderer>();
            trail.time = TrailTime;
            trail.widthMultiplier = TrailWidth;
            trail.widthCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            trail.minVertexDistance = 0.05f;
            trail.sharedMaterial = materials.BoltTrail;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;

            return BuilderUtils.SavePrefab(root, BoltPrefabPath).GetComponent<Projectile>();
        }

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

            // Wymiary ciała z EnemyData (sekcja Ciało).
            float height = data.bodyHeight;
            float radius = data.bodyRadius;

            var controller = root.AddComponent<CharacterController>();
            controller.height = height;
            controller.radius = radius;
            controller.center = new Vector3(0f, height * 0.5f, 0f);
            controller.minMoveDistance = 0f;

            // Wygląd w proporcjach do wysokości: worek na dole 3/4, głowa na górze.
            float sackHeight = height * 0.75f;
            var body = BuilderUtils.Visual(PrimitiveType.Cylinder, "Worek", root.transform, materials.Dummy);
            body.transform.localPosition = new Vector3(0f, sackHeight * 0.5f, 0f);
            body.transform.localScale = new Vector3(radius * 2f, sackHeight * 0.5f, radius * 2f);
            float headSize = height - sackHeight + radius * 0.25f;
            var head = BuilderUtils.Visual(PrimitiveType.Sphere, "Glowa", root.transform, materials.Dummy);
            head.transform.localPosition = new Vector3(0f, height - headSize * 0.5f, 0f);
            head.transform.localScale = Vector3.one * headSize;

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
