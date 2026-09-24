using UnityEngine;

namespace Osiedle.Combat
{
    /// <summary>
    /// Jedna śrubka Złomu. Wylatuje z trafionego celu, spada na ziemię, a gdy gracz jest blisko
    /// (i ma miejsce na Złom) — leci do niego. Żyje w puli <see cref="ScrapSpawner"/>, nie jest niszczona.
    /// </summary>
    public class ScrapPickup : MonoBehaviour
    {
        const float SpinDegreesPerSecond = 360f;

        enum State { Flying, Resting, Magnet }

        ScrapSpawner owner;
        State state;
        Vector3 velocity;
        float groundY;
        float spawnTime;

        public void Launch(ScrapSpawner spawner, Vector3 position, Vector3 initialVelocity, float groundHeight)
        {
            owner = spawner;
            transform.position = position;
            velocity = initialVelocity;
            groundY = groundHeight;
            spawnTime = Time.time;
            state = State.Flying;
        }

        void Update()
        {
            if (owner == null) return;

            var data = owner.Data;
            float dt = Time.deltaTime;
            Vector3 position = transform.position;

            if (state == State.Flying)
            {
                velocity.y -= data.scrapGravity * dt;
                position += velocity * dt;
                if (position.y <= groundY && velocity.y < 0f)
                {
                    position.y = groundY;
                    velocity = Vector3.zero;
                    state = State.Resting;
                }
            }

            Vector3 target = owner.CollectPoint;
            bool canMagnet = owner.CanCollect && Time.time - spawnTime >= data.scrapMagnetDelay;

            if (state != State.Magnet && canMagnet && FlatDistance(position, target) <= data.scrapMagnetRadius)
                state = State.Magnet;

            if (state == State.Magnet)
            {
                if (!owner.CanCollect)
                {
                    // Złom pełny: śrubka spada i czeka na ziemi.
                    velocity = Vector3.zero;
                    state = State.Flying;
                }
                else
                {
                    position = Vector3.MoveTowards(position, target, data.scrapMagnetSpeed * dt);
                    if (Vector3.Distance(position, target) <= data.scrapCollectDistance)
                    {
                        owner.Collect(this);
                        return;
                    }
                }
            }

            transform.position = position;
            transform.Rotate(0f, SpinDegreesPerSecond * dt, 0f, Space.World);
        }

        static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}
