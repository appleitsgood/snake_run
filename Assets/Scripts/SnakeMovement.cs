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
    public float fixedCircleRadius = 2f;
    public bool useCustomBounds;
    public Vector2 customBoundsMin;
    public Vector2 customBoundsMax;

    private bool useRandomTrajectory;
    private bool useFixedTrajectory;
    private Vector2 direction;
    private float noiseSeedX;
    private float noiseSeedY;
    private float fixedAngle;
    private Vector3 fixedCenter;
    private float positionLogTimer;
    private const float PositionLogInterval = 1f;

    void Awake() {
        ResetRandomDirection();
        ApplyTrailSettings();
    }

    void OnValidate() {
        ApplyTrailSettings();
    }

    void OnEnable() {
        ApplyTrailSettings();
        if (useRandomTrajectory) { ResetRandomDirection(); }
        if (useFixedTrajectory) { ResetFixedCircle(); }

        if (trailRenderer != null) {
            trailRenderer.Clear();
            trailRenderer.emitting = useRandomTrajectory || useFixedTrajectory;
        }
    }

    void OnDisable() {
        if (trailRenderer != null) { trailRenderer.emitting = false; }
    }

    void Update() {
        if (useRandomTrajectory) { MoveHeadRandom(); }
        if (useFixedTrajectory) { MoveHeadFixed(); }
        LogPosition();
    }

    public void SetMode(string mode) {
        useFixedTrajectory = mode == "fixed";
        useRandomTrajectory = mode == "random";
        if (useFixedTrajectory) { ResetFixedCircle(); }
        if (useRandomTrajectory) { ResetRandomDirection(); }

        if (trailRenderer != null) {
            trailRenderer.Clear();
            trailRenderer.emitting = (useFixedTrajectory || useRandomTrajectory) && isActiveAndEnabled;
        }
    }

    public void SetRandomMode(bool enabled) {
        SetMode(enabled ? "random" : "fixed");
    }

    public void RefreshSettings() {
        ApplyTrailSettings();
    }

    public void ResetForRun(string mode) {
        SetMode(mode);
        if (useRandomTrajectory) { transform.position = GetScreenCenterPosition(); }
    }

    public void SetCustomBounds(Vector2 min, Vector2 max) {
        useCustomBounds = true;
        customBoundsMin = min;
        customBoundsMax = max;
        transform.position = GetScreenCenterPosition();
    }

    public void ClearCustomBounds() {
        useCustomBounds = false;
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

    void MoveHeadFixed() {
        fixedCenter = GetScreenCenterPosition();
        float radius = GetFixedCircleRadius();
        fixedAngle += speed / radius * Time.deltaTime;
        transform.position = fixedCenter + new Vector3(Mathf.Cos(fixedAngle), Mathf.Sin(fixedAngle), 0f) * radius;
    }

    Vector2 GetSmoothRandomDirection() {
        float t = Time.time * 0.5f;
        float x = Mathf.PerlinNoise(noiseSeedX, t) * 2f - 1f;
        float y = Mathf.PerlinNoise(noiseSeedY, t) * 2f - 1f;
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
        Vector2 centerDirection = GetScreenCenterPosition() - transform.position;

        if (centerDirection.sqrMagnitude < 0.01f) { return direction; }
        return centerDirection.normalized;
    }

    Vector3 GetScreenCenterPosition() {
        if (useCustomBounds) {
            Vector2 boundsCenter = (customBoundsMin + customBoundsMax) * 0.5f;
            return new Vector3(boundsCenter.x, boundsCenter.y, 0f);
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null) { return Vector3.zero; }

        Vector3 center = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, -mainCamera.transform.position.z));
        center.z = 0f;
        return center;
    }

    float GetFixedCircleRadius() {
        if (useCustomBounds) {
            float boundsMaxRadius = Mathf.Min(customBoundsMax.x - customBoundsMin.x, customBoundsMax.y - customBoundsMin.y) * 0.5f - randomEdgePadding;
            return Mathf.Max(0.01f, Mathf.Min(fixedCircleRadius, boundsMaxRadius));
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null) { return Mathf.Max(0.01f, fixedCircleRadius); }

        float zDistance = -mainCamera.transform.position.z;
        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));
        float maxRadius = Mathf.Min(max.x - min.x, max.y - min.y) * 0.5f - randomEdgePadding;

        return Mathf.Max(0.01f, Mathf.Min(fixedCircleRadius, maxRadius));
    }

    bool IsInsideScreen(Vector3 position) {
        if (useCustomBounds) {
            return position.x >= customBoundsMin.x + randomEdgePadding
                && position.x <= customBoundsMax.x - randomEdgePadding
                && position.y >= customBoundsMin.y + randomEdgePadding
                && position.y <= customBoundsMax.y - randomEdgePadding;
        }

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
        if (useCustomBounds) {
            position.x = Mathf.Clamp(position.x, customBoundsMin.x + randomEdgePadding, customBoundsMax.x - randomEdgePadding);
            position.y = Mathf.Clamp(position.y, customBoundsMin.y + randomEdgePadding, customBoundsMax.y - randomEdgePadding);
            position.z = 0f;
            return position;
        }

        Camera mainCamera = Camera.main;
        float zDistance = -mainCamera.transform.position.z;
        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));

        position.x = Mathf.Clamp(position.x, min.x + randomEdgePadding, max.x - randomEdgePadding);
        position.y = Mathf.Clamp(position.y, min.y + randomEdgePadding, max.y - randomEdgePadding);
        position.z = 0f;
        return position;
    }

    void LogPosition() {
        if (!useRandomTrajectory && !useFixedTrajectory) { return; }

        positionLogTimer -= Time.deltaTime;
        if (positionLogTimer > 0f) { return; }

        Debug.Log($"x: {transform.position.x:F3}, y: {transform.position.y:F3}");
        positionLogTimer = PositionLogInterval;
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
        noiseSeedX = Random.Range(0f, 100f);
        noiseSeedY = Random.Range(100f, 200f);
    }

    void ResetFixedCircle() {
        fixedCenter = GetScreenCenterPosition();
        fixedAngle = 0f;
        transform.position = fixedCenter + Vector3.right * GetFixedCircleRadius();
    }
}
