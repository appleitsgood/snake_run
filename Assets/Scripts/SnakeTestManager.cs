using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SnakeTestManager : MonoBehaviour
{
    public SnakeMovement snakeMovement;
    public CursorMovement cursorMovement;
    public string selectedMode;
    public string runId = "sub01";
    public LslMarker lslMarker;
    public CSVLogger csvLogger;

    public GameObject modePanel;
    public Button fixedButton;
    public Button randomButton;
    public Button settingsButton;
    public string settingsSceneName = "SettingsScene";
    public Text countdownText;

    Coroutine testRoutine;
    SnakeRunSettingsData settings;
    ExitConfirmPopup exitConfirmPopup;
    float timeScaleBeforeExitDialog = 1f;
    bool exitDialogOpen;
    bool loggedLslMarkersDisabled;
    bool loggedCsvLoggingDisabled;

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

        SetupLoggers();
        ApplyLogSettings();

        modePanel.SetActive(true);
        if (countdownText != null) { countdownText.gameObject.SetActive(false); }
        fixedButton.onClick.AddListener(() => StartTest("fixed"));
        randomButton.onClick.AddListener(() => StartTest("random"));
        if (settingsButton != null) { settingsButton.onClick.AddListener(OpenSettings); }
    }

    void Update() {
        if (testRoutine == null) { return; }
        if (exitDialogOpen) { return; }
        if (!WasEscapePressed()) { return; }

        ShowExitConfirmPopup();
    }

    void OnDisable() {
        HideExitConfirmPopup(true);
    }

    void OpenSettings() {
        SceneManager.LoadScene(settingsSceneName);
    }

    void StartTest(string mode)
    {
        if (testRoutine != null) { return; }

        selectedMode = mode;

        modePanel.SetActive(false);
        if (IsCsvLoggingEnabled()) { csvLogger.CreateLog(runId, snakeMovement, cursorMovement); }
        testRoutine = StartCoroutine(RunTrials());
    }

    IEnumerator RunTrials()
    {
        SnakeRunSettingsData activeSettings = settings != null
            ? settings
            : SnakeRunSettingsData.CreateDefault(snakeMovement, cursorMovement);

        for (int trialIndex = 0; trialIndex < activeSettings.trialCount; trialIndex++) {
            PushTrialStartMarker();
            ShowCountdownCursor(activeSettings.countdownDuration);
            yield return new WaitForSeconds(activeSettings.countdownDuration);

            PushTrackingStartMarker();
            StartTrial();
            if (IsCsvLoggingEnabled()) { csvLogger.StartTracking(); }
            yield return new WaitForSeconds(activeSettings.trialDuration);

            if (IsCsvLoggingEnabled()) { csvLogger.StopTracking(); }
            PushTrackingEndMarker();
            HideTrialObjects();
            PushTrialEndMarker();
            if (trialIndex < activeSettings.trialCount - 1) { yield return new WaitForSeconds(activeSettings.breakDuration); }
        }

        testRoutine = null;
        if (IsCsvLoggingEnabled()) { csvLogger.SaveLog(); }
        modePanel.SetActive(true);
    }

    void ShowCountdownCursor(float countdownDuration) {
        HideTrialObjects();
        if (countdownDuration <= 0f) { return; }
        if (cursorMovement == null) { return; }

        cursorMovement.enabled = false;
        cursorMovement.gameObject.SetActive(true);
        cursorMovement.ResetForTrial();
        cursorMovement.SetFeedbackColor(Color.yellow);
        cursorMovement.SetTrailVisible(false);
    }

    void StartTrial() {
        if (snakeMovement != null) {
            snakeMovement.gameObject.SetActive(true);
            snakeMovement.ResetForTrial(selectedMode);
            snakeMovement.enabled = true;
        }

        if (cursorMovement != null) {
            cursorMovement.gameObject.SetActive(true);
            cursorMovement.ResetForTrial();
            cursorMovement.SetFeedbackColor(Color.green);
            cursorMovement.enabled = true;
            cursorMovement.SetTrailVisible(true);
        }
    }

    void HideTrialObjects() {
        if (snakeMovement != null) {
            snakeMovement.enabled = false;
            snakeMovement.gameObject.SetActive(false);
        }

        if (cursorMovement != null) {
            cursorMovement.enabled = false;
            cursorMovement.gameObject.SetActive(false);
        }
    }

    void PushTrialStartMarker() {
        if (!AreLslMarkersEnabled()) { return; }
        if (lslMarker == null) { return; }
        lslMarker.PushTrialStart();
    }

    void PushTrialEndMarker() {
        if (!AreLslMarkersEnabled()) { return; }
        if (lslMarker == null) { return; }
        lslMarker.PushTrialEnd();
    }

    void PushTrackingStartMarker() {
        if (!AreLslMarkersEnabled()) { return; }
        if (lslMarker == null) { return; }
        lslMarker.PushTrackingStart();
    }

    void PushTrackingEndMarker() {
        if (!AreLslMarkersEnabled()) { return; }
        if (lslMarker == null) { return; }
        lslMarker.PushTrackingEnd();
    }

    void SetupLoggers() {
        if (lslMarker == null) { lslMarker = GetComponent<LslMarker>(); }
        if (lslMarker == null) { lslMarker = gameObject.AddComponent<LslMarker>(); }

        if (csvLogger == null) { csvLogger = GetComponent<CSVLogger>(); }
        if (csvLogger == null) { csvLogger = gameObject.AddComponent<CSVLogger>(); }
    }

    void ApplyLogSettings() {
        if (settings == null) { return; }

        if (lslMarker != null) { lslMarker.enabled = settings.enableLslMarkers; }
        if (csvLogger != null) { csvLogger.enabled = settings.enableCsvLogging; }

        if (settings.enableLslMarkers) { loggedLslMarkersDisabled = false; }
        else if (!loggedLslMarkersDisabled) {
            Debug.Log("LSL markers are disabled by settings.");
            loggedLslMarkersDisabled = true;
        }

        if (settings.enableCsvLogging) { loggedCsvLoggingDisabled = false; }
        else if (!loggedCsvLoggingDisabled) {
            Debug.Log("CSV logging is disabled by settings.");
            loggedCsvLoggingDisabled = true;
        }
    }

    bool AreLslMarkersEnabled() {
        return settings != null && settings.enableLslMarkers && lslMarker != null;
    }

    bool IsCsvLoggingEnabled() {
        return settings != null && settings.enableCsvLogging && csvLogger != null;
    }

    bool WasEscapePressed() {
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
    }

    void ShowExitConfirmPopup() {
        exitConfirmPopup = ExitConfirmPopup.Show(ConfirmExitExperiment, CancelExitExperiment);

        timeScaleBeforeExitDialog = Time.timeScale;
        Time.timeScale = 0f;
        exitDialogOpen = true;
    }

    void CancelExitExperiment() {
        HideExitConfirmPopup(true);
    }

    void ConfirmExitExperiment() {
        HideExitConfirmPopup(true);
        EndCurrentExperiment();
    }

    void HideExitConfirmPopup(bool restoreTimeScale) {
        if (exitConfirmPopup != null) {
            Destroy(exitConfirmPopup.gameObject);
            exitConfirmPopup = null;
        }

        if (restoreTimeScale) { RestoreTimeScale(); }
        exitDialogOpen = false;
    }

    void RestoreTimeScale() {
        if (!exitDialogOpen) { return; }
        Time.timeScale = timeScaleBeforeExitDialog;
    }

    public void EndCurrentExperiment() {
        if (testRoutine != null) {
            StopCoroutine(testRoutine);
            testRoutine = null;
        }

        selectedMode = "";
        if (IsCsvLoggingEnabled()) { csvLogger.SaveLog(); }
        HideTrialObjects();
        if (countdownText != null) { countdownText.gameObject.SetActive(false); }
        if (modePanel != null) { modePanel.SetActive(true); }
    }
}
