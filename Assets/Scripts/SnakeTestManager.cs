using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SnakeTestManager : MonoBehaviour
{
    public SnakeMovement snakeMovement;
    public CursorMovement cursorMovement;
    public string selectedMode;

    public GameObject modePanel;
    public Button fixedButton;
    public Button randomButton;
    public Button settingsButton;
    public SettingsPanel settingsPanel;
    public string settingsSceneName = "SettingsScene";
    public Text countdownText;

    Coroutine testRoutine;
    SnakeRunSettingsData settings;

    void Awake()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
        }

        if (snakeMovement != null)
        {
            snakeMovement.enabled = false;
        }

        if (cursorMovement != null) { cursorMovement.gameObject.SetActive(false); }

        settings = SnakeRunSettingsData.LoadOrDefault(snakeMovement, cursorMovement);
        settings.ApplyTo(snakeMovement, cursorMovement);

        if (settingsPanel != null) {
            settingsPanel.Initialize(snakeMovement, cursorMovement);
            settings = settingsPanel.CurrentSettings;
        }

        modePanel.SetActive(true);
        if (countdownText != null) { countdownText.gameObject.SetActive(false); }
        fixedButton.onClick.AddListener(() => StartTest("fixed"));
        randomButton.onClick.AddListener(() => StartTest("random"));
        if (settingsButton != null) { settingsButton.onClick.AddListener(OpenSettings); }
    }

    void OpenSettings() {
        SceneManager.LoadScene(settingsSceneName);
    }

    void StartTest(string mode)
    {
        if (testRoutine != null) { return; }

        selectedMode = mode;

        if (settingsPanel != null) {
            settingsPanel.SaveSettings();
            settings = settingsPanel.CurrentSettings;
        }

        modePanel.SetActive(false);
        testRoutine = StartCoroutine(RunTests());
    }

    IEnumerator RunTests()
    {
        SnakeRunSettingsData activeSettings = settingsPanel != null && settingsPanel.CurrentSettings != null
            ? settingsPanel.CurrentSettings
            : settings != null
                ? settings
            : SnakeRunSettingsData.CreateDefault(snakeMovement, cursorMovement);

        for (int runIndex = 0; runIndex < activeSettings.runCount; runIndex++) {
            ShowCountdownCursor(activeSettings.countdownDuration);
            yield return new WaitForSeconds(activeSettings.countdownDuration);

            StartRun();
            yield return new WaitForSeconds(activeSettings.runDuration);

            HideRunObjects();
            if (runIndex < activeSettings.runCount - 1) { yield return new WaitForSeconds(activeSettings.breakDuration); }
        }

        testRoutine = null;
        modePanel.SetActive(true);
    }

    void ShowCountdownCursor(float countdownDuration) {
        HideRunObjects();
        if (countdownDuration <= 0f) { return; }
        if (cursorMovement == null) { return; }

        cursorMovement.enabled = false;
        cursorMovement.gameObject.SetActive(true);
        cursorMovement.ResetForRun();
        cursorMovement.SetFeedbackColor(Color.yellow);
        cursorMovement.SetTrailVisible(false);
    }

    void StartRun() {
        if (snakeMovement != null) {
            snakeMovement.gameObject.SetActive(true);
            snakeMovement.ResetForRun(selectedMode);
            snakeMovement.enabled = true;
        }

        if (cursorMovement != null) {
            cursorMovement.gameObject.SetActive(true);
            cursorMovement.ResetForRun();
            cursorMovement.SetFeedbackColor(Color.green);
            cursorMovement.enabled = true;
            cursorMovement.SetTrailVisible(true);
        }
    }

    void HideRunObjects() {
        if (snakeMovement != null) {
            snakeMovement.enabled = false;
            snakeMovement.gameObject.SetActive(false);
        }

        if (cursorMovement != null) {
            cursorMovement.enabled = false;
            cursorMovement.gameObject.SetActive(false);
        }
    }
}
