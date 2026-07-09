using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class SnakeMovement : MonoBehaviour
{
    public TrailRenderer trailRenderer;
    public float speed = 2f;
    public float turnSpeed = 2f;
    public float randomTurnStrength = 0.8f;
    public float randomEdgePadding = 0.75f;
    public float edgeAvoidStrength = 3f;
    public float lineWidth = 0.25f;
    public float tailAlpha = 0.6f;
    public float snakeLength = 3.5f;

    private bool useRandomTrajectory;
    private Vector2 direction;
    private float noiseSeed;

    void Awake() {
        ResetRandomDirection();
        ApplyTrailSettings();
    }

    void OnValidate() {
        ApplyTrailSettings();
    }

    void OnEnable() {
        ApplyTrailSettings();
        if (trailRenderer != null) {
            trailRenderer.Clear();
            trailRenderer.emitting = useRandomTrajectory;
        }
    }

    void OnDisable() {
        if (trailRenderer != null) { trailRenderer.emitting = false; }
    }

    void Update() {
        if (useRandomTrajectory) { MoveHeadRandom(); }
    }

    public void SetRandomMode(bool enabled) {
        useRandomTrajectory = enabled;
        if (trailRenderer != null) {
            trailRenderer.Clear();
            trailRenderer.emitting = enabled && isActiveAndEnabled;
        }
    }

    void MoveHeadRandom() {
        if (direction == Vector2.zero) { ResetRandomDirection(); }

        Vector2 randomSteering = GetSmoothRandomDirection();
        Vector2 edgeSteering = GetEdgeAvoidanceDirection();
        Vector2 desiredDirection = direction
            + randomSteering * randomTurnStrength
            + edgeSteering * edgeAvoidStrength;

        if (desiredDirection.sqrMagnitude < 0.01f) { desiredDirection = direction; }
        desiredDirection.Normalize();

        direction = Vector2.Lerp(direction, desiredDirection, turnSpeed * Time.deltaTime);
        direction.Normalize();

        Vector3 nextPosition = transform.position + new Vector3(direction.x, direction.y, 0f) * speed * Time.deltaTime;
        if (!IsInsideScreen(nextPosition)) {
            direction = Vector2.Lerp(direction, GetScreenCenterDirection(), edgeAvoidStrength * Time.deltaTime);
            direction.Normalize();
            nextPosition = transform.position + new Vector3(direction.x, direction.y, 0f) * speed * Time.deltaTime;
        }

        transform.position = ClampToScreen(nextPosition);
    }

    Vector2 GetSmoothRandomDirection() {
        float t = Time.time * 0.5f;
        float x = Mathf.PerlinNoise(noiseSeed, t) * 2f - 1f;
        float y = Mathf.PerlinNoise(noiseSeed + 50f, t) * 2f - 1f;
        Vector2 noiseDirection = new Vector2(x, y);

        if (noiseDirection.sqrMagnitude < 0.01f) { return Vector2.zero; }
        return noiseDirection.normalized;
    }

    Vector2 GetEdgeAvoidanceDirection() {
        Camera mainCamera = Camera.main;
        float zDistance = -mainCamera.transform.position.z;
        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));
        Vector3 position = transform.position;
        float avoidDistance = Mathf.Max(randomEdgePadding * 4f, speed);
        float left = min.x + randomEdgePadding;
        float right = max.x - randomEdgePadding;
        float bottom = min.y + randomEdgePadding;
        float top = max.y - randomEdgePadding;
        Vector2 steering = Vector2.zero;

        steering += Vector2.right * Mathf.Clamp01((left + avoidDistance - position.x) / avoidDistance);
        steering += Vector2.left * Mathf.Clamp01((position.x - (right - avoidDistance)) / avoidDistance);
        steering += Vector2.up * Mathf.Clamp01((bottom + avoidDistance - position.y) / avoidDistance);
        steering += Vector2.down * Mathf.Clamp01((position.y - (top - avoidDistance)) / avoidDistance);

        if (steering.sqrMagnitude > 1f) { steering.Normalize(); }
        return steering;
    }

    Vector2 GetScreenCenterDirection() {
        Camera mainCamera = Camera.main;
        Vector3 center = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -mainCamera.transform.position.z));
        Vector2 centerDirection = center - transform.position;

        if (centerDirection.sqrMagnitude < 0.01f) { return direction; }
        return centerDirection.normalized;
    }

    bool IsInsideScreen(Vector3 position) {
        Camera mainCamera = Camera.main;
        float zDistance = -mainCamera.transform.position.z;
        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));

        return position.x >= min.x + randomEdgePadding
            && position.x <= max.x - randomEdgePadding
            && position.y >= min.y + randomEdgePadding
            && position.y <= max.y - randomEdgePadding;
    }

    Vector3 ClampToScreen(Vector3 position) {
        Camera mainCamera = Camera.main;
        float zDistance = -mainCamera.transform.position.z;
        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));

        position.x = Mathf.Clamp(position.x, min.x + randomEdgePadding, max.x - randomEdgePadding);
        position.y = Mathf.Clamp(position.y, min.y + randomEdgePadding, max.y - randomEdgePadding);
        position.z = 0f;
        return position;
    }

    void ApplyTrailSettings() {
        if (trailRenderer == null) { trailRenderer = GetComponent<TrailRenderer>(); }
        if (trailRenderer == null && Application.isPlaying) { trailRenderer = gameObject.AddComponent<TrailRenderer>(); }
        if (trailRenderer == null) { return; }

        trailRenderer.time = Mathf.Max(0.01f, snakeLength / Mathf.Max(speed, 0.01f));
        trailRenderer.startWidth = lineWidth;
        trailRenderer.endWidth = lineWidth;
        trailRenderer.numCornerVertices = 8;
        trailRenderer.numCapVertices = 8;
        trailRenderer.colorGradient = GetSnakeGradient();
    }

    Gradient GetSnakeGradient() {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.7f),
                new GradientAlphaKey(tailAlpha, 1f)
            }
        );
        return gradient;
    }

    void ResetRandomDirection() {
        direction = Random.insideUnitCircle.normalized;
        if (direction == Vector2.zero) { direction = Vector2.right; }
        noiseSeed = Random.Range(0f, 100f);
    }
}
