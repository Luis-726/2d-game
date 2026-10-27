using TowerDefense.Audio;
using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Represents projectiles fired by ranged towers (e.g. Arrows for Archers, Fireballs for Mages).
    /// Supports direct single-target impact or explosive area-of-effect (AoE) splash damage.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Flight Characteristics")]
        [SerializeField] private float speed = 10f;
        [SerializeField] private bool rotateTowardsTarget = true;

        [Header("Damage & Effects")]
        [SerializeField] private int damage = 2;
        [SerializeField] private bool isAreaOfEffect = false;
        [SerializeField] private float splashRadius = 1.5f;

        [Header("Visual & Audio Feedback")]
        [SerializeField] private GameObject impactEffectPrefab;

        private Transform targetTransform;
        private Vector3 targetLastPosition;
        private bool hasTarget = false;

        public void Seek(Transform target, int projectileDamage, bool aoe = false, float aoeRadius = 1.5f)
        {
            targetTransform = target;
            damage = projectileDamage;
            isAreaOfEffect = aoe;
            splashRadius = aoeRadius;

            if (targetTransform != null)
            {
                targetLastPosition = targetTransform.position;
                hasTarget = true;
            }
        }

        private void Update()
        {
            if (targetTransform != null)
            {
                targetLastPosition = targetTransform.position;
            }
            else if (!hasTarget)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 direction = (targetLastPosition - transform.position).normalized;
            float distanceThisFrame = speed * Time.deltaTime;

            if (rotateTowardsTarget && direction != Vector3.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            if (Vector3.Distance(transform.position, targetLastPosition) <= distanceThisFrame)
            {
                HitTarget();
                return;
            }

            transform.Translate(direction * distanceThisFrame, Space.World);
        }

        private void HitTarget()
        {
            if (impactEffectPrefab != null)
            {
                GameObject effect = Instantiate(impactEffectPrefab, targetLastPosition, Quaternion.identity);
                Destroy(effect, 0.5f);
            }

            if (isAreaOfEffect)
            {
                ExplodeSplash();
            }
            else
            {
                if (targetTransform != null)
                {
                    Enemy enemy = targetTransform.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(damage);
                    }
                }
            }

            Destroy(gameObject);
        }

        private void ExplodeSplash()
        {
            AudioManager.Instance?.PlayMagicExplosionSound();

            Collider2D[] colliders = Physics2D.OverlapCircleAll(targetLastPosition, splashRadius);
            foreach (Collider2D col in colliders)
            {
                Enemy enemy = col.GetComponentInParent<Enemy>();
                if (enemy != null && enemy.CurrentHealth > 0)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (isAreaOfEffect)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, splashRadius);
            }
        }
    }
}
