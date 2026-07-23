using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public sealed class ExitConfirmPopup : MonoBehaviour
{
    public static ExitConfirmPopup Show(UnityAction onYes, UnityAction onNo) {
        GameObject popupObject = new GameObject(
            "Exit Confirm Popup",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(ExitConfirmPopup));

        ExitConfirmPopup popup = popupObject.GetComponent<ExitConfirmPopup>();
        popup.Build(onYes, onNo);
        return popup;
    }

    void Build(UnityAction onYes, UnityAction onNo) {
        Canvas canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject dim = CreateImage(transform, "Dim", Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.7f));
        RectTransform dimRect = dim.GetComponent<RectTransform>();
        dimRect.anchorMin = Vector2.zero;
        dimRect.anchorMax = Vector2.one;
        dimRect.sizeDelta = Vector2.zero;

        GameObject panel = CreateImage(transform, "Panel", Vector2.zero, new Vector2(520f, 240f), new Color(0.12f, 0.12f, 0.12f, 1f));
        CreateText(panel.transform, "\uC9C4\uC9DC \uC885\uB8CC\uD560\uAE4C\uC694?", new Vector2(0f, 50f), new Vector2(440f, 60f), 30);
        CreateButton(panel.transform, "Yes Button", "Yes", new Vector2(-110f, -55f), onYes);
        CreateButton(panel.transform, "No Button", "No", new Vector2(110f, -55f), onNo);
    }

    static GameObject CreateImage(Transform parent, string name, Vector2 position, Vector2 size, Color color) {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        imageObject.GetComponent<Image>().color = color;
        return imageObject;
    }

    static void CreateText(Transform parent, string value, Vector2 position, Vector2 size, int fontSize) {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    static void CreateButton(Transform parent, string name, string label, Vector2 position, UnityAction onClick) {
        GameObject buttonObject = CreateImage(parent, name, position, new Vector2(160f, 56f), new Color(0.24f, 0.24f, 0.24f, 1f));
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        if (onClick != null) { button.onClick.AddListener(onClick); }
        CreateText(buttonObject.transform, label, Vector2.zero, new Vector2(160f, 56f), 24);
    }
}
