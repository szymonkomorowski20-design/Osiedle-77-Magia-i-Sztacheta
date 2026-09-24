using Osiedle.Core;
using UnityEngine;

namespace Osiedle.Combat
{
    /// <summary>
    /// Hit-stop: po mocnym trafieniu gra zamiera na chwilę (czas z <see cref="DamageInfo.HitStop"/>).
    /// Jeden obiekt na scenę. Podpina się pod <see cref="GameEvents.OnHit"/>.
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class HitStop : MonoBehaviour
    {
        bool stopped;
        float savedTimeScale = 1f;
        float resumeAt;

        public bool IsStopped => stopped;

        void OnEnable() => GameEvents.OnHit += HandleHit;

        void OnDisable()
        {
            GameEvents.OnHit -= HandleHit;
            Resume();
        }

        void HandleHit(DamageInfo info, Hurtbox target)
        {
            if (info.HitStop > 0f) Stop(info.HitStop);
        }

        /// <summary>Zamraża grę na podany czas (kilka trafień naraz nie sumuje się — liczy się najdłuższe).</summary>
        public void Stop(float duration)
        {
            if (!stopped)
            {
                savedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                stopped = true;
            }
            resumeAt = Mathf.Max(resumeAt, Time.unscaledTime + duration);
        }

        void Update()
        {
            if (stopped && Time.unscaledTime >= resumeAt) Resume();
        }

        void Resume()
        {
            if (!stopped) return;
            Time.timeScale = savedTimeScale;
            stopped = false;
        }
    }
}
