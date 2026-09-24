using UnityEngine;

namespace Osiedle.Combat
{
    /// <summary>
    /// Krótki błysk brył po przyjętym trafieniu (np. na biało). Czas i kolor ustawia właściciel ze swoich danych.
    /// </summary>
    public class HitFlash : MonoBehaviour
    {
        const string BaseColorProperty = "_BaseColor";

        [SerializeField] Health health;
        [Tooltip("Bryły, które błyskają.")]
        [SerializeField] Renderer[] renderers;

        MaterialPropertyBlock block;
        Color[] baseColors;
        float duration;
        Color color = Color.white;
        float flashUntil;
        bool flashing;

        void Awake()
        {
            if (health == null) health = GetComponentInParent<Health>();
            block = new MaterialPropertyBlock();
            baseColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                baseColors[i] = renderers[i].sharedMaterial.GetColor(BaseColorProperty);
        }

        public void Configure(float flashDuration, Color flashColor)
        {
            duration = flashDuration;
            color = flashColor;
        }

        void OnEnable()
        {
            if (health != null) health.Damaged += HandleDamaged;
        }

        void OnDisable()
        {
            if (health != null) health.Damaged -= HandleDamaged;
            if (flashing) Set(false);
        }

        void HandleDamaged(DamageInfo info, float dealt)
        {
            flashUntil = Time.time + duration;
            Set(true);
        }

        void Update()
        {
            if (flashing && Time.time >= flashUntil) Set(false);
        }

        void Set(bool on)
        {
            flashing = on;
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].GetPropertyBlock(block);
                block.SetColor(BaseColorProperty, on ? color : baseColors[i]);
                renderers[i].SetPropertyBlock(block);
            }
        }
    }
}
