using Osiedle.Combat;
using Osiedle.Player;
using Osiedle.UI;
using Osiedle.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Osiedle.Editor
{
    /// <summary>Prefab gracza (Kuba): ruch, celowanie, dash, walka wręcz, zasoby. Wspólny dla wszystkich scen testowych.</summary>
    public static class PlayerPrefabBuilder
    {
        public const string Path = BuilderUtils.Root + "/Prefabs/Player/Player.prefab";

        public static GameObject Build(PlayerData data, InputActionAsset controls, BuilderMaterials materials,
            MeleeWeaponData weapon, ScrapPickup scrapPrefab)
        {
            var root = new GameObject("Player");

            // Wymiary ciała z PlayerData (sekcja Ciało).
            float height = data.bodyHeight;
            float radius = data.bodyRadius;

            var controller = root.AddComponent<CharacterController>();
            controller.height = height;
            controller.radius = radius;
            controller.center = new Vector3(0f, height * 0.5f, 0f);
            controller.stepOffset = Mathf.Min(data.stepOffset, height);
            // 0 = kontroler nie ignoruje małych ruchów (przy wysokim FPS docisk do ziemi jest bardzo mały).
            controller.minMoveDistance = 0f;

            // Wygląd: kapsuła + "twarz", żeby było widać, gdzie postać patrzy.
            var body = BuilderUtils.Visual(PrimitiveType.Capsule, "Body", root.transform, materials.Player);
            body.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            body.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);

            var face = BuilderUtils.Visual(PrimitiveType.Cube, "Face", root.transform, materials.PlayerFace);
            face.transform.localPosition = new Vector3(0f, height * 0.75f, radius);
            face.transform.localScale = new Vector3(0.3f, 0.12f, 0.12f);

            // Strzałka nad krawędzią obiektu do wskoczenia (włączana przez VaultPrompt).
            var arrow = new GameObject("VaultArrow");
            arrow.transform.SetParent(root.transform, false);
            var diamond = BuilderUtils.Visual(PrimitiveType.Cube, "Diamond", arrow.transform, materials.Arrow);
            diamond.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            diamond.transform.localScale = new Vector3(0.28f, 0.28f, 0.06f);
            arrow.SetActive(false);

            // Hitbox ciosu na wysokości biodra, skierowany tam, gdzie patrzy postać.
            var hitboxGo = new GameObject("MeleeHitbox");
            hitboxGo.transform.SetParent(root.transform, false);
            hitboxGo.transform.localPosition = new Vector3(0f, data.hipHeight, 0f);
            var hitbox = hitboxGo.AddComponent<Hitbox>();

            var input = root.AddComponent<PlayerInputReader>();
            var motor = root.AddComponent<PlayerMotor>();
            var aim = root.AddComponent<PlayerAim>();
            var dash = root.AddComponent<PlayerDash>();
            var prompt = root.AddComponent<VaultPrompt>();
            var health = root.AddComponent<Health>();
            var hurtbox = root.AddComponent<Hurtbox>();
            var resources = root.AddComponent<PlayerResources>();
            var scrapSpawner = root.AddComponent<ScrapSpawner>();
            var melee = root.AddComponent<PlayerMelee>();
            var hud = root.AddComponent<DebugHud>();

            BuilderUtils.Wire(input, ("actions", controls));
            BuilderUtils.Wire(motor, ("data", data), ("input", input));
            BuilderUtils.Wire(aim, ("data", data), ("input", input));
            BuilderUtils.Wire(dash, ("data", data), ("input", input), ("motor", motor), ("aim", aim));
            BuilderUtils.Wire(prompt, ("data", data), ("dash", dash), ("arrow", arrow.transform));
            BuilderUtils.Wire(hitbox, ("owner", root));
            BuilderUtils.Wire(hurtbox, ("health", health));
            BuilderUtils.Wire(resources, ("data", data), ("health", health));
            BuilderUtils.Wire(scrapSpawner, ("data", data), ("resources", resources), ("pickupPrefab", scrapPrefab));
            BuilderUtils.Wire(melee, ("playerData", data), ("weapon", weapon), ("input", input), ("motor", motor),
                ("dash", dash), ("resources", resources), ("hitbox", hitbox), ("scrapSpawner", scrapSpawner));
            BuilderUtils.Wire(hud, ("resources", resources), ("dash", dash), ("melee", melee));

            return BuilderUtils.SavePrefab(root, Path);
        }
    }
}
