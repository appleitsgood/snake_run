using UnityEngine;

public class SettingsPreviewController : MonoBehaviour
{
    public Vector2 snakeBoundsMin = new Vector2(-8.2f, -4.4f);
    public Vector2 snakeBoundsMax = new Vector2(8.2f, 4.4f);
    public Vector2 cursorBoundsMin = new Vector2(-8.2f, -4.4f);
    public Vector2 cursorBoundsMax = new Vector2(8.2f, 4.4f);
    public Sprite cursorHeadSprite;

    SnakeMovement snakePreview;
    CursorMovement cursorPreview;
    GameObject snakeRoot;
    GameObject cursorRoot;

    void Awake() {
        EnsureCreated();
        ShowGeneral(null);
    }

    public void ApplySettings(SnakeRunSettingsData settings) {
        EnsureCreated();
        if (settings == null) { return; }

        settings.ApplyTo(snakePreview, cursorPreview);

        if (snakePreview != null) {
            snakePreview.SetCustomBounds(snakeBoundsMin, snakeBoundsMax);
        }

        if (cursorPreview != null) {
            cursorPreview.SetCustomBounds(cursorBoundsMin, cursorBoundsMax);
            cursorPreview.SetFeedbackColor(Color.green);
        }
    }

    public void ShowGeneral(SnakeRunSettingsData settings) {
        EnsureCreated();
        ApplySettings(settings);
        SetPreviewActive(false, false);
    }

    public void ShowSnake(SnakeRunSettingsData settings) {
        EnsureCreated();
        ApplySettings(settings);
        SetPreviewActive(true, false);

        if (snakePreview != null) {
            snakePreview.ResetForRun("random");
            snakePreview.enabled = true;
        }
    }

    public void ShowCursor(SnakeRunSettingsData settings) {
        EnsureCreated();
        ApplySettings(settings);
        SetPreviewActive(false, true);

        if (cursorPreview != null) {
            cursorPreview.ResetForRun();
            cursorPreview.SetFeedbackColor(Color.green);
            cursorPreview.SetTrailVisible(true);
            cursorPreview.enabled = true;
        }
    }

    void SetPreviewActive(bool showSnake, bool showCursor) {
        if (snakeRoot != null) { snakeRoot.SetActive(showSnake); }
        if (cursorRoot != null) { cursorRoot.SetActive(showCursor); }
    }

    void EnsureCreated() {
        if (snakeRoot == null) { CreateSnakePreview(); }
        if (cursorRoot == null) { CreateCursorPreview(); }
    }

    void CreateSnakePreview() {
        snakeRoot = new GameObject("SnakePreview");
        snakeRoot.transform.SetParent(transform);
        snakeRoot.transform.position = Vector3.zero;

        TrailRenderer trailRenderer = snakeRoot.AddComponent<TrailRenderer>();
        ApplyPreviewMaterial(trailRenderer);
        snakePreview = snakeRoot.AddComponent<SnakeMovement>();
        snakePreview.trailRenderer = trailRenderer;
        snakePreview.SetCustomBounds(snakeBoundsMin, snakeBoundsMax);
        snakePreview.SetMode("random");
        snakePreview.enabled = true;
    }

    void CreateCursorPreview() {
        cursorRoot = new GameObject("CursorPreview");
        cursorRoot.transform.SetParent(transform);
        cursorRoot.transform.position = Vector3.zero;

        TrailRenderer trailRenderer = cursorRoot.AddComponent<TrailRenderer>();
        ApplyPreviewMaterial(trailRenderer);
        cursorPreview = cursorRoot.AddComponent<CursorMovement>();
        cursorPreview.trailRenderer = trailRenderer;

        GameObject head = new GameObject("CursorPreviewHead");
        head.transform.SetParent(cursorRoot.transform);
        head.transform.localPosition = Vector3.zero;
        head.transform.localScale = Vector3.one * cursorPreview.headSize;

        SpriteRenderer spriteRenderer = head.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = cursorHeadSprite != null ? cursorHeadSprite : CreateCircleSprite();
        cursorPreview.headRenderer = spriteRenderer;
        cursorPreview.SetCustomBounds(cursorBoundsMin, cursorBoundsMax);
        cursorPreview.SetFeedbackColor(Color.green);
        cursorPreview.enabled = true;
    }

    Sprite CreateCircleSprite() {
        int size = 64;
        float radius = size * 0.5f - 1f;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++) {
            for (int x = 0; x < size; x++) {
                float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
    }

    void ApplyPreviewMaterial(TrailRenderer trailRenderer) {
        if (trailRenderer == null) { return; }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) { shader = Shader.Find("Unlit/Color"); }
        if (shader != null) { trailRenderer.material = new Material(shader); }
    }
}
