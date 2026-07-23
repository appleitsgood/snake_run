using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public GameObject panel;
    public GameObject generalContent;
    public GameObject snakeContent;
    public GameObject cursorContent;
    public GameObject controlsRoot;

    public Button openButton;
    public Button generalTabButton;
    public Button snakeTabButton;
    public Button cursorTabButton;
    public Button saveButton;
    public Button closeButton;
    public Button resetDefaultsButton;
    public Button backButton;
    public Button hidePanelButton;
    public Button showPanelButton;
    public SettingsPreviewController previewController;
    public string backSceneName = "TestScene";
    public bool initializeOnAwake;
    public bool showOnAwake;
    public bool hideAfterSave = true;
    public Color selectedTabColor = new Color(0.12f, 0.35f, 0.58f, 1f);
    public Color normalTabColor = new Color(0.12f, 0.16f, 0.2f, 1f);

    public SnakeRunSettingsData CurrentSettings { get; private set; }

    RectTransform panelRect;
    Image panelImage;
    SnakeMovement snakeMovement;
    CursorMovement cursorMovement;
    SnakeRunSettingsData defaultSettings;
    SnakeRunSettingInput[] settingInputs;
    GameObject selectedTab;
    bool fillingInputs;

    void Awake() {
        panelRect = panel != null ? panel.GetComponent<RectTransform>() : null;
        panelImage = panel != null ? panel.GetComponent<Image>() : null;
        CacheInputs();
        RegisterButton(openButton, Show);
        RegisterButton(generalTabButton, () => ShowTab(generalContent));
        RegisterButton(snakeTabButton, () => ShowTab(snakeContent));
        RegisterButton(cursorTabButton, () => ShowTab(cursorContent));
        RegisterButton(saveButton, SaveAndHide);
        RegisterButton(closeButton, Hide);
        RegisterButton(resetDefaultsButton, ResetDefaults);
        RegisterButton(backButton, Back);
        RegisterButton(hidePanelButton, HideControls);
        RegisterButton(showPanelButton, ShowControls);
        Hide();

        if (initializeOnAwake) { Initialize(null, null); }
        if (showOnAwake) { Show(); }
    }

    public void Initialize(SnakeMovement snakeMovement, CursorMovement cursorMovement) {
        this.snakeMovement = snakeMovement;
        this.cursorMovement = cursorMovement;
        defaultSettings = SnakeRunSettingsData.CreateDefault(snakeMovement, cursorMovement);
        CurrentSettings = SnakeRunSettingsData.LoadOrDefault(snakeMovement, cursorMovement);
        CurrentSettings.ApplyTo(snakeMovement, cursorMovement);
        ApplyPreview();
        FillInputs();
        ShowTab(generalContent);
    }

    public void Show() {
        if (panel != null) { panel.SetActive(true); }
        ShowControls();
        FillInputs();
        ShowTab(generalContent);
    }

    public void Hide() {
        if (panel != null) { panel.SetActive(false); }
    }

    public void SaveSettings() {
        if (CurrentSettings == null) {
            CurrentSettings = SnakeRunSettingsData.LoadOrDefault(snakeMovement, cursorMovement);
        }

        ReadInputs();
        CurrentSettings.ApplyTo(snakeMovement, cursorMovement);
        ApplyPreview();
        CurrentSettings.Save();
        FillInputs();
    }

    void SaveAndHide() {
        SaveSettings();
        if (hideAfterSave) { Hide(); }
    }

    void Back() {
        SceneManager.LoadScene(backSceneName);
    }

    void ResetDefaults() {
        CurrentSettings = Clone(defaultSettings);
        CurrentSettings.ApplyTo(snakeMovement, cursorMovement);
        ApplyPreview();
        CurrentSettings.Save();
        FillInputs();
    }

    void ShowTab(GameObject selectedContent) {
        selectedTab = selectedContent;
        ApplyLayout(selectedContent);

        if (generalContent != null) { generalContent.SetActive(selectedContent == generalContent); }
        if (snakeContent != null) { snakeContent.SetActive(selectedContent == snakeContent); }
        if (cursorContent != null) { cursorContent.SetActive(selectedContent == cursorContent); }

        SetTabSelected(generalTabButton, selectedContent == generalContent);
        SetTabSelected(snakeTabButton, selectedContent == snakeContent);
        SetTabSelected(cursorTabButton, selectedContent == cursorContent);

        if (previewController != null) {
            if (selectedContent == snakeContent) { previewController.ShowSnake(CurrentSettings); }
            else if (selectedContent == cursorContent) { previewController.ShowCursor(CurrentSettings); }
            else { previewController.ShowGeneral(CurrentSettings); }
        }
    }

    void ApplyLayout(GameObject selectedContent) {
        bool previewMode = selectedContent == snakeContent || selectedContent == cursorContent;
        if (hidePanelButton != null) { hidePanelButton.gameObject.SetActive(previewMode); }

        if (panelRect == null) { return; }

        panelRect.anchorMin = new Vector2(0f, 0f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(640f, 0f);
        if (panelImage != null) { panelImage.color = new Color(0.055f, 0.07f, 0.085f, 0.94f); }
        ApplySidebarLayout(previewMode);
    }

    void ApplySidebarLayout(bool previewMode) {
        SetRect(generalTabButton, new Vector2(-205f, 425f), new Vector2(170f, 56f));
        SetRect(snakeTabButton, new Vector2(0f, 425f), new Vector2(170f, 56f));
        SetRect(cursorTabButton, new Vector2(205f, 425f), new Vector2(170f, 56f));
        SetRect(generalContent, new Vector2(0f, 40f), new Vector2(590f, 680f));
        SetRect(snakeContent, new Vector2(0f, 40f), new Vector2(590f, 680f));
        SetRect(cursorContent, new Vector2(0f, 40f), new Vector2(590f, 680f));
        SetRect(hidePanelButton, new Vector2(-245f, -445f), new Vector2(110f, 56f));
        SetRect(resetDefaultsButton, new Vector2(-80f, -445f), new Vector2(190f, 56f));
        SetRect(saveButton, new Vector2(100f, -445f), new Vector2(110f, 56f));
        SetRect(backButton, new Vector2(230f, -445f), new Vector2(110f, 56f));
        ApplySettingRowLayout();
    }

    void ApplySettingRowLayout() {
        if (settingInputs == null) { return; }

        foreach (SnakeRunSettingInput settingInput in settingInputs) {
            if (settingInput == null) { continue; }

            RectTransform rowRect = settingInput.GetComponent<RectTransform>();
            SetRect(rowRect, rowRect != null ? rowRect.anchoredPosition : Vector2.zero, new Vector2(540f, 46f));

            if (settingInput.input != null) {
                SetRect(settingInput.input.GetComponent<RectTransform>(), new Vector2(165f, 0f), new Vector2(160f, 42f));
            }

            if (settingInput.toggle != null) {
                SetRect(settingInput.toggle.GetComponent<RectTransform>(), new Vector2(165f, 0f), new Vector2(42f, 42f));
            }

            Text[] texts = settingInput.GetComponentsInChildren<Text>(true);
            foreach (Text text in texts) {
                if (text == null) { continue; }
                if (settingInput.input != null && text.transform.IsChildOf(settingInput.input.transform)) { continue; }
                if (settingInput.toggle != null && text.transform.IsChildOf(settingInput.toggle.transform)) { continue; }
                SetRect(text.GetComponent<RectTransform>(), new Vector2(-115f, 0f), new Vector2(290f, 38f));
            }
        }
    }

    void SetRect(Button button, Vector2 anchoredPosition, Vector2 sizeDelta) {
        if (button == null) { return; }
        SetRect(button.GetComponent<RectTransform>(), anchoredPosition, sizeDelta);
    }

    void SetRect(GameObject target, Vector2 anchoredPosition, Vector2 sizeDelta) {
        if (target == null) { return; }
        SetRect(target.GetComponent<RectTransform>(), anchoredPosition, sizeDelta);
    }

    void SetRect(RectTransform rectTransform, Vector2 anchoredPosition, Vector2 sizeDelta) {
        if (rectTransform == null) { return; }
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
    }

    void SetTabSelected(Button button, bool selected) {
        if (button == null || button.targetGraphic == null) { return; }
        button.targetGraphic.color = selected ? selectedTabColor : normalTabColor;
    }

    void CacheInputs() {
        GameObject targetPanel = panel != null ? panel : gameObject;
        settingInputs = targetPanel.GetComponentsInChildren<SnakeRunSettingInput>(true);

        foreach (SnakeRunSettingInput settingInput in settingInputs) {
            if (settingInput == null) { continue; }
            if (settingInput.input == null) { settingInput.input = settingInput.GetComponentInChildren<InputField>(true); }
            if (settingInput.toggle == null) { settingInput.toggle = settingInput.GetComponentInChildren<Toggle>(true); }

            if (settingInput.input != null) {
                settingInput.input.onValueChanged.RemoveListener(OnInputChanged);
                settingInput.input.onValueChanged.AddListener(OnInputChanged);
            }

            if (settingInput.toggle != null) {
                settingInput.toggle.onValueChanged.RemoveListener(OnToggleChanged);
                settingInput.toggle.onValueChanged.AddListener(OnToggleChanged);
            }
        }
    }

    void FillInputs() {
        if (CurrentSettings == null) { return; }
        if (settingInputs == null || settingInputs.Length == 0) { CacheInputs(); }

        fillingInputs = true;
        foreach (SnakeRunSettingInput settingInput in settingInputs) {
            if (settingInput == null) { continue; }
            settingInput.SetValue(CurrentSettings.GetValue(settingInput.key));
        }
        fillingInputs = false;
    }

    void ReadInputs() {
        if (settingInputs == null || settingInputs.Length == 0) { CacheInputs(); }

        foreach (SnakeRunSettingInput settingInput in settingInputs) {
            if (settingInput == null) { continue; }
            CurrentSettings.SetValue(settingInput.key, settingInput.GetValue());
        }

        CurrentSettings.Sanitize();
    }

    void OnInputChanged(string value) {
        if (fillingInputs) { return; }
        if (CurrentSettings == null) { return; }

        ReadInputs();
        ApplyPreview();
    }

    void OnToggleChanged(bool value) {
        OnInputChanged(value.ToString());
    }

    void ApplyPreview() {
        if (previewController != null) { previewController.ApplySettings(CurrentSettings); }
    }

    void HideControls() {
        if (selectedTab == generalContent) { return; }
        GameObject target = controlsRoot != null ? controlsRoot : panel;
        if (target != null) { target.SetActive(false); }
        if (showPanelButton != null) { showPanelButton.gameObject.SetActive(true); }
    }

    void ShowControls() {
        GameObject target = controlsRoot != null ? controlsRoot : panel;
        if (target != null) { target.SetActive(true); }
        if (showPanelButton != null) { showPanelButton.gameObject.SetActive(false); }
        ApplyLayout(selectedTab);
    }

    void RegisterButton(Button button, UnityEngine.Events.UnityAction action) {
        if (button == null) { return; }
        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    SnakeRunSettingsData Clone(SnakeRunSettingsData source) {
        return JsonUtility.FromJson<SnakeRunSettingsData>(JsonUtility.ToJson(source));
    }
}
