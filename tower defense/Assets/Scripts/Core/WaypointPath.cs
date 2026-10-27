using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Holds a sequenced list of waypoints that define an enemy's route through the map.
    /// Draws visual gizmos in the Scene view to facilitate level design.
    /// </summary>
    public class WaypointPath : MonoBehaviour
    {
        [Tooltip("List of waypoint transforms in sequential order from start to base.")]
        [SerializeField] private List<Transform> waypoints = new List<Transform>();

        [Header("Gizmo Settings")]
        [SerializeField] private Color pathColor = Color.yellow;
        [SerializeField] private Color nodeColor = Color.cyan;
        [SerializeField] private float nodeRadius = 0.25f;

        public int WaypointCount => waypoints.Count;

        private void Reset()
        {
            // Auto-populate from children if empty
            PopulateFromChildren();
        }

        [ContextMenu("Populate From Children")]
        public void PopulateFromChildren()
        {
            waypoints.Clear();
            foreach (Transform child in transform)
            {
                waypoints.Add(child);
            }
        }

        /// <summary>
        /// Retrieves the world position of the waypoint at the given index.
        /// </summary>
        public Vector3 GetWaypointPosition(int index)
        {
            if (index < 0 || index >= waypoints.Count)
            {
                Debug.LogWarning($"[WaypointPath] Index {index} out of range.");
                return transform.position;
            }
            return waypoints[index].position;
        }

        /// <summary>
        /// Calculates remaining distance from a given waypoint index and current position to the final base waypoint.
        /// Useful for 'First' tower targeting.
        /// </summary>
        public float GetRemainingDistance(int currentWaypointIndex, Vector3 currentPosition)
        {
            if (waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count) return 0f;

            float distance = Vector3.Distance(currentPosition, waypoints[currentWaypointIndex].position);
            for (int i = currentWaypointIndex; i < waypoints.Count - 1; i++)
            {
                distance += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
            }
            return distance;
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count == 0) return;

            Gizmos.color = pathColor;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }

            Gizmos.color = nodeColor;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(waypoints[i].position, nodeRadius);
                }
            }
        }
    }
}
