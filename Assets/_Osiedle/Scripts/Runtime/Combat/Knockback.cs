using UnityEngine;

namespace Osiedle.Combat
{
    /// <summary>
    /// Odrzut po trafieniu: prędkość, która wygasa ze stałym hamowaniem.
    /// Porusza przez CharacterController (jeśli jest), więc cel nie przelatuje przez ściany.
    /// Liczby ustawia właściciel ze swoich danych przez <see cref="Configure"/>.
    /// </summary>
    public class Knockback : MonoBehaviour
    {
        const float StopSpeed = 0.01f;

        CharacterController controller;
        Vector3 velocity;
        float multiplier = 1f;
        float deceleration;

        public Vector3 Velocity => velocity;
        public bool IsActive => velocity.sqrMagnitude > StopSpeed * StopSpeed;

        void Awake() => controller = GetComponent<CharacterController>();

        /// <param name="knockbackMultiplier">1 = normalnie, 0 = cel nie odlatuje (np. ciężki wróg).</param>
        /// <param name="decelerationPerSecond">Jak szybko wygasa odrzut (m/s²).</param>
        public void Configure(float knockbackMultiplier, float decelerationPerSecond)
        {
            multiplier = Mathf.Max(0f, knockbackMultiplier);
            deceleration = Mathf.Max(0f, decelerationPerSecond);
        }

        public void Apply(Vector3 impulseVelocity)
        {
            impulseVelocity.y = 0f;
            velocity += impulseVelocity * multiplier;
        }

        public void Stop() => velocity = Vector3.zero;

        void Update()
        {
            if (!IsActive) return;

            Vector3 step = velocity * Time.deltaTime;
            if (controller != null && controller.enabled) controller.Move(step);
            else transform.position += step;

            velocity = Vector3.MoveTowards(velocity, Vector3.zero, deceleration * Time.deltaTime);
        }
    }
}
