using UnityEngine;
using System.Collections;

[System.Serializable]
public class ParallaxLayer
{
    public Transform layerTransform;
    public Vector2 parallaxFactor = new Vector2(0.5f, 0.5f);
}

public class CineMotion2D : MonoBehaviour
{
    [SerializeField] private Transform targetPlayer;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] public Vector2 offset = new Vector2(0f, 1f);
    [SerializeField] public float deadzoneRadius = 0.5f;
    [SerializeField] private Vector3 velocity = Vector3.zero;

    [SerializeField] private bool useSmoothFollow = true;
    [SerializeField] private bool useGridSnap = false;
    [SerializeField] private float gridSize = 1f;

    [SerializeField] public bool useBoundaries = false;
    [SerializeField] public Vector2 minBoundary;
    [SerializeField] public Vector2 maxBoundary;

    [SerializeField] private float shakeDuration = 0f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    [SerializeField] private Camera cam;
    [SerializeField] private float defaultZoom = 5f;
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float targetZoom;

    [SerializeField] private bool isCinematicActive = false;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private int currentWaypointIndex = 0;
    [SerializeField] private float cinematicSpeed = 3f;

    [SerializeField] private ParallaxLayer[] parallaxLayers;
    private Vector3 lastCameraPosition;

    private Transform originalTarget; // for focus feature

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        targetZoom = defaultZoom;
        lastCameraPosition = transform.position;
        originalTarget = targetPlayer;

        isCinematicActive = false;
    }

    void LateUpdate()
    {
        if (isCinematicActive)
        {
            HandleCinematicPanning();
            UpdateParallax();
            lastCameraPosition = transform.position;
            return;
        }

        if (targetPlayer == null)
        {
            Debug.LogError("Target Player is not assigned");
            return;
        }

        Vector3 targetPosition = targetPlayer.position + (Vector3)offset;
        targetPosition.z = transform.position.z;

        if (useSmoothFollow)
        {
            if (Vector2.Distance(transform.position, targetPlayer.position) > deadzoneRadius)
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / followSpeed);
            }
        }
        else if (useGridSnap)
        {
            targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
            targetPosition.y = Mathf.Round(targetPosition.y / gridSize) * gridSize;
            transform.position = targetPosition;
        }

        if (useBoundaries)
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minBoundary.x, maxBoundary.x),
                Mathf.Clamp(transform.position.y, minBoundary.y, maxBoundary.y),
                transform.position.z
            );
        }

        if (shakeDuration > 0)
        {
            Vector2 shakeOffset = new Vector2(
                Mathf.PerlinNoise(Time.time * 25f, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, Time.time * 25f) - 0.5f
            ) * shakeMagnitude;

            transform.position += (Vector3)shakeOffset;
            shakeDuration -= Time.deltaTime;
        }

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);

        UpdateParallax();
        lastCameraPosition = transform.position;
    }

    void HandleCinematicPanning()
    {
        if (waypoints.Length == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        transform.position = Vector3.Lerp(transform.position, targetWaypoint.position, Time.deltaTime * cinematicSpeed);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                isCinematicActive = false;
                currentWaypointIndex = 0;

                if (targetPlayer != null)
                {
                    Vector3 targetPosition = targetPlayer.position + (Vector3)offset;
                    targetPosition.z = transform.position.z;
                    transform.position = targetPosition;
                    velocity = Vector3.zero;
                    lastCameraPosition = transform.position;
                }
            }
        }
    }

    void UpdateParallax()
    {
        Vector3 deltaMovement = transform.position - lastCameraPosition;

        foreach (var layer in parallaxLayers)
        {
            if (layer.layerTransform == null) continue;

            float parallaxX = deltaMovement.x * layer.parallaxFactor.x;
            float parallaxY = deltaMovement.y * layer.parallaxFactor.y;

            Vector3 layerPos = layer.layerTransform.position;
            layer.layerTransform.position = new Vector3(
                layerPos.x + parallaxX,
                layerPos.y + parallaxY,
                layerPos.z
            );
        }
    }

    public void ShakeCamera(float duration, float magnitude, Vector2 impactDirection)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        transform.position += (Vector3)(impactDirection.normalized * magnitude);
    }

    public void SetZoom(float newZoom)
    {
        targetZoom = Mathf.Clamp(newZoom, 2f, 10f);
    }

    public void SetTarget(Transform newTarget)
    {
        targetPlayer = newTarget;
    }

    public void StartCinematic()
    {
        isCinematicActive = true;
        currentWaypointIndex = 0;
    }

    public void FocusOn(Transform newFocus, float duration)
    {
        StartCoroutine(FocusRoutine(newFocus, duration));
    }

    private IEnumerator FocusRoutine(Transform newFocus, float duration)
    {
        SetTarget(newFocus);
        yield return new WaitForSeconds(duration);
        SetTarget(originalTarget);
    }

    void OnValidate()
    {
        if (useSmoothFollow && useGridSnap)
        {
            useGridSnap = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, deadzoneRadius);

        if (useBoundaries)
        {
            Gizmos.color = Color.green;
            Vector3 bottomLeft = new Vector3(minBoundary.x, minBoundary.y, transform.position.z);
            Vector3 topRight = new Vector3(maxBoundary.x, maxBoundary.y, transform.position.z);
            Gizmos.DrawLine(bottomLeft, new Vector3(topRight.x, bottomLeft.y, bottomLeft.z));
            Gizmos.DrawLine(bottomLeft, new Vector3(bottomLeft.x, topRight.y, bottomLeft.z));
            Gizmos.DrawLine(topRight, new Vector3(topRight.x, bottomLeft.y, bottomLeft.z));
            Gizmos.DrawLine(topRight, new Vector3(bottomLeft.x, topRight.y, bottomLeft.z));
        }
    }

    public void CenterOnPlayer()
    {
        if (targetPlayer != null)
        {
            Vector3 targetPosition = targetPlayer.position + (Vector3)offset;
            targetPosition.z = transform.position.z;
            transform.position = targetPosition;
        }
    }

    public void SnapToGrid()
    {
        if (targetPlayer != null)
        {
            Vector3 targetPosition = targetPlayer.position + (Vector3)offset;
            targetPosition.z = transform.position.z;

            targetPosition.x = Mathf.Round(targetPosition.x / gridSize) * gridSize;
            targetPosition.y = Mathf.Round(targetPosition.y / gridSize) * gridSize;
            transform.position = targetPosition;
        }
    }

    public void ResetZoom()
    {
        targetZoom = defaultZoom;
    }
}
