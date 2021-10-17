using BennyKok.RuntimeDebug.Systems;
using UnityEngine;

namespace BennyKok.RuntimeDebug.Components.UI
{
    public class AutoFacingCamera : MonoBehaviour
    {
        public float distanceAwayCamera = 1;
        public float recenterTime = 0.2f;
        public float distanceThreshold = 2.5f;

        [Range(0, 300)]
        public float angleThreshold = 50f;

        private bool isUpdating;

        private Camera m_Camera;
        private Camera Camera
        {
            get
            {
                if (m_Camera == null) m_Camera = Camera.main;
                return m_Camera;
            }
        }

        private void Start()
        {
            Recenter();
        }

        private void OnEnable()
        {
            RuntimeDebugSystem.Instance.OnDebugMenuToggleEvent += OnMenuToggle;
        }

        private void OnDisable()
        {
            RuntimeDebugSystem.Instance.OnDebugMenuToggleEvent -= OnMenuToggle;
        }

        private void OnMenuToggle(bool isShowing)
        {
            if (isShowing) Recenter();
        }

        private Vector3 newPosition;
        private Quaternion newRotation;
        private Vector3 velocity = Vector3.zero;

        private void Recenter()
        {
            isUpdating = true;

            var dir = Camera.transform.forward;
            dir.y = 0;

            newPosition = Camera.transform.position + dir * distanceAwayCamera;
        }

        private void Update()
        {
            var distanceDiff = Vector3.Distance(transform.position, Camera.transform.position);
            var diff = transform.position - Camera.transform.position;
            diff.y = 0;
            newRotation = Quaternion.LookRotation(diff.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.unscaledDeltaTime / recenterTime);

            Vector3 lookDir = Camera.transform.position - transform.position;
            lookDir.y = 0;
            Vector3 yourDir = Camera.transform.forward;
            yourDir.y = 0;

            float angleDiff = Vector3.Angle(yourDir, -lookDir);
            // Debug.Log(angleDiff);
            if (distanceDiff > distanceThreshold || angleDiff > angleThreshold)
            {
                Recenter();
            }

            if (isUpdating)
            {
                transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, recenterTime, 20, Time.unscaledDeltaTime);
                if (transform.position == newPosition)
                {
                    isUpdating = false;
                }
            }
        }
    }
}