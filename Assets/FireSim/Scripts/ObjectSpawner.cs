using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    public class ObjectSpawner : MonoBehaviour
    {
        Camera m_CameraToFace;

        [SerializeField]
        List<GameObject> m_ObjectPrefabs = new List<GameObject>();

        public List<GameObject> objectPrefabs
        {
            get => m_ObjectPrefabs;
            set => m_ObjectPrefabs = value;
        }

        [SerializeField]
        int m_SpawnOptionIndex = -1;

        public int spawnOptionIndex
        {
            get => m_SpawnOptionIndex;
            set => m_SpawnOptionIndex = value;
        }

        [SerializeField]
        bool m_OnlySpawnInView = true;
        public bool onlySpawnInView
        {
            get => m_OnlySpawnInView;
            set => m_OnlySpawnInView = value;
        }

        public event Action<GameObject> objectSpawned;

        void Awake()
        {
            if (m_CameraToFace == null)
                m_CameraToFace = Camera.main;
        }


        public bool TrySpawnObject(Vector3 spawnPoint, Vector3 spawnNormal)
        {
            if (m_OnlySpawnInView)
            {
                float inViewMin = 0.15f;
                float inViewMax = 1f - inViewMin;

                var pointInViewportSpace = m_CameraToFace.WorldToViewportPoint(spawnPoint);

                if (pointInViewportSpace.z < 0f ||
                    pointInViewportSpace.x > inViewMax ||
                    pointInViewportSpace.x < inViewMin ||
                    pointInViewportSpace.y > inViewMax ||
                    pointInViewportSpace.y < inViewMin)
                {
                    return false;
                }
            }

            int objectIndex = (m_SpawnOptionIndex < 0 || m_SpawnOptionIndex >= m_ObjectPrefabs.Count)
                              ? Random.Range(0, m_ObjectPrefabs.Count)
                              : m_SpawnOptionIndex;

            if (objectIndex < 0 || objectIndex >= m_ObjectPrefabs.Count)
            {
                return false;
            }

            var newObject = Instantiate(m_ObjectPrefabs[objectIndex]);

            var trackable = newObject.GetComponent<TrackableFire>();
            if (trackable != null && TrainingStatsManager.Instance != null)
            {
                TrainingStatsManager.Instance.RegisterFire(trackable);
                var menu = FindAnyObjectByType<ARTemplateMenuManager>();
                menu?.UpdateStatsText();
            }

            newObject.transform.parent = transform;

            newObject.transform.position = spawnPoint;

            var facePosition = m_CameraToFace.transform.position;
            var forward = facePosition - spawnPoint;
            BurstMathUtility.ProjectOnPlane(forward, spawnNormal, out var projectedForward);
            newObject.transform.rotation = Quaternion.LookRotation(projectedForward, spawnNormal);

            bool applyRandomAngle = true;
            float spawnAngleRange = 45f;

            if (applyRandomAngle)
            {
                var randomRotation = Random.Range(-spawnAngleRange, spawnAngleRange);
                newObject.transform.Rotate(Vector3.up, randomRotation);
            }
            objectSpawned?.Invoke(newObject);

            return true;
        }

    }
}