using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.UI
{
    /// <summary>
    /// Displays a dynamic health bar above an enemy's head using a simple 2D sprite or transform scale.
    /// </summary>
    public class HealthBar2D : MonoBehaviour
    {
        [SerializeField] private Enemy enemy;
        [SerializeField] private Transform fillBar;
        [SerializeField] private Vector3 offset = new Vector3(0, 0.6f, 0);

        private Vector3 initialScale;

        private void Awake()
        {
            if (enemy == null)
            {
                enemy = GetComponentInParent<Enemy>();
            }

            if (fillBar != null)
            {
                initialScale = fillBar.localScale;
            }
        }

        private void OnEnable()
        {
            if (enemy != null)
            {
                enemy.OnHealthChanged += UpdateHealthBar;
            }
        }

        private void OnDisable()
        {
            if (enemy != null)
            {
                enemy.OnHealthChanged -= UpdateHealthBar;
            }
        }

        private void LateUpdate()
        {
            if (enemy != null)
            {
                transform.position = enemy.transform.position + offset;
            }
        }

        private void UpdateHealthBar(int current, int max)
        {
            if (fillBar == null || max <= 0) return;

            float ratio = Mathf.Clamp01((float)current / max);
            fillBar.localScale = new Vector3(initialScale.x * ratio, initialScale.y, initialScale.z);
        }
    }
}
