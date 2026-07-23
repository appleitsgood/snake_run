using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(TrailRenderer))]
public class CursorMovement : MonoBehaviour
{
    public SpriteRenderer headRenderer;
    public TrailRenderer trailRenderer;
    public float speed = 2f;
    public float screenPadding = 0.75f;
    public float headSize = 0.3f;
    public float trailWidth = 0.08f;
    public float tailAlpha = 0.5f;
    public float tailFadeStart = 0.5f;
    public float trailLength = 3.5f;
    public bool useCustomBounds;
    public Vector2 customBoundsMin;
    public Vector2 customBoundsMax;

    private Color feedbackColor = Color.green;
    private Vector2 currentInput;

    void OnValidate() {
        ApplyCursorSettings();
    }

    void OnEnable() {
        ApplyCursorSettings();
        if (trailRenderer != null) {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }
    }

    void OnDisable() {
        if (trailRenderer != null) { trailRenderer.emitting = false; }
    }

    void Update() {
        currentInput = GetArrowInput();
        if (currentInput.sqrMagnitude > 1f) { currentInput.Normalize(); }
    }

    void FixedUpdate() {
        Vector3 nextPosition = transform.position + new Vector3(currentInput.x, currentInput.y, 0f) * speed * Time.fixedDeltaTime;
        transform.position = ClampToScreen(nextPosition);
    }

    Vector2 GetArrowInput() {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) { return Vector2.zero; }

        Vector2 input = Vector2.zero;
        if (keyboard.leftArrowKey.isPressed) { input += Vector2.left; }
        if (keyboard.rightArrowKey.isPressed) { input += Vector2.right; }
        if (keyboard.upArrowKey.isPressed) { input += Vector2.up; }
        if (keyboard.downArrowKey.isPressed) { input += Vector2.down; }

        return input;
    }

    public void RefreshSettings() {
        ApplyCursorSettings();
    }

    public void ResetForTrial() {
        transform.position = GetScreenCenterPosition();
        if (trailRenderer != null) { trailRenderer.Clear(); }
    }

    public void SetFeedbackColor(Color color) {
        feedbackColor = color;
        ApplyCursorSettings();
    }

    public void SetTrailVisible(bool visible) {
        if (trailRenderer == null) { trailRenderer = GetComponent<TrailRenderer>(); }
        if (trailRenderer == null) { return; }

        trailRenderer.emitting = visible;
        if (!visible) { trailRenderer.Clear(); }
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

    Vector3 ClampToScreen(Vector3 position) {
        if (useCustomBounds) {
            position.x = Mathf.Clamp(position.x, customBoundsMin.x + screenPadding, customBoundsMax.x - screenPadding);
            position.y = Mathf.Clamp(position.y, customBoundsMin.y + screenPadding, customBoundsMax.y - screenPadding);
            position.z = 0f;
            return position;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null) { return position; }

        float zDistance = -mainCamera.transform.position.z;
        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, zDistance));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, zDistance));

        position.x = Mathf.Clamp(position.x, min.x + screenPadding, max.x - screenPadding);
        position.y = Mathf.Clamp(position.y, min.y + screenPadding, max.y - screenPadding);
        position.z = 0f;
        return position;
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

    void ApplyCursorSettings() {
        if (trailRenderer == null) { trailRenderer = GetComponent<TrailRenderer>(); }

        if (headRenderer != null) {
            headRenderer.color = feedbackColor;
            headRenderer.transform.localScale = Vector3.one * headSize;
        }

        if (trailRenderer != null) {
            trailRenderer.time = Mathf.Max(0.01f, trailLength / Mathf.Max(speed, 0.01f));
            trailRenderer.startWidth = trailWidth;
            trailRenderer.endWidth = trailWidth;
            trailRenderer.numCornerVertices = 8;
            trailRenderer.numCapVertices = 8;
            trailRenderer.colorGradient = GetCursorGradient();
        }
    }

    Gradient GetCursorGradient() {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(feedbackColor, 0f), new GradientColorKey(feedbackColor, 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, Mathf.Clamp01(tailFadeStart)),
                new GradientAlphaKey(tailAlpha, 1f)
            }
        );
        return gradient;
    }
}
