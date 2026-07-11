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
        Vector2 input = GetArrowInput();
        if (input.sqrMagnitude > 1f) { input.Normalize(); }

        Vector3 nextPosition = transform.position + new Vector3(input.x, input.y, 0f) * speed * Time.deltaTime;
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

    Vector3 ClampToScreen(Vector3 position) {
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

    void ApplyCursorSettings() {
        if (trailRenderer == null) { trailRenderer = GetComponent<TrailRenderer>(); }

        if (headRenderer != null) {
            headRenderer.color = Color.green;
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
            new GradientColorKey[] { new GradientColorKey(Color.green, 0f), new GradientColorKey(Color.green, 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, Mathf.Clamp01(tailFadeStart)),
                new GradientAlphaKey(tailAlpha, 1f)
            }
        );
        return gradient;
    }
}
