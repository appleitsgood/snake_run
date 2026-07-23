using System;
using System.Globalization;
using System.IO;
using UnityEngine;

[Serializable]
public class SnakeRunSettingsData
{
    public int trialCount = 1;
    public float trialDuration = 10f;
    public float breakDuration = 4f;
    public float countdownDuration = 3f;

    public float snakeSpeed = 2f;
    public float snakeTurnSpeed = 2f;
    public float snakeRandomTurnStrength = 0.8f;
    public float snakeEdgePadding = 0.75f;
    public float snakeEdgeAvoidStrength = 6f;
    public float snakeLineWidth = 0.15f;
    public float snakeTailAlpha = -0.1f;
    public float snakeLength = 2.5f;
    public float snakeFixedCircleRadius = 3f;

    public float cursorSpeed = 2.1f;
    public float cursorScreenPadding = 0.75f;
    public float cursorHeadSize = 0.2f;
    public float cursorTrailWidth = 0.03f;
    public float cursorTailAlpha = 0.1f;
    public float cursorTailFadeStart = 0.5f;
    public float cursorTrailLength = 2f;

    const string FileName = "snakerun_settings.json";

    public static string SettingsPath {
        get { return Path.Combine(Application.persistentDataPath, FileName); }
    }

    public static SnakeRunSettingsData CreateDefault(SnakeMovement snakeMovement, CursorMovement cursorMovement) {
        SnakeRunSettingsData settings = new SnakeRunSettingsData();

        if (snakeMovement != null) {
            settings.snakeSpeed = snakeMovement.speed;
            settings.snakeTurnSpeed = snakeMovement.turnSpeed;
            settings.snakeRandomTurnStrength = snakeMovement.randomTurnStrength;
            settings.snakeEdgePadding = snakeMovement.randomEdgePadding;
            settings.snakeEdgeAvoidStrength = snakeMovement.edgeAvoidStrength;
            settings.snakeLineWidth = snakeMovement.lineWidth;
            settings.snakeTailAlpha = snakeMovement.tailAlpha;
            settings.snakeLength = snakeMovement.snakeLength;
            settings.snakeFixedCircleRadius = snakeMovement.fixedCircleRadius;
        }

        if (cursorMovement != null) {
            settings.cursorSpeed = cursorMovement.speed;
            settings.cursorScreenPadding = cursorMovement.screenPadding;
            settings.cursorHeadSize = cursorMovement.headSize;
            settings.cursorTrailWidth = cursorMovement.trailWidth;
            settings.cursorTailAlpha = cursorMovement.tailAlpha;
            settings.cursorTailFadeStart = cursorMovement.tailFadeStart;
            settings.cursorTrailLength = cursorMovement.trailLength;
        }

        return settings;
    }

    public static SnakeRunSettingsData LoadOrDefault(SnakeMovement snakeMovement, CursorMovement cursorMovement) {
        SnakeRunSettingsData defaults = CreateDefault(snakeMovement, cursorMovement);

        if (!File.Exists(SettingsPath)) { return defaults; }

        try {
            string json = File.ReadAllText(SettingsPath);
            SnakeRunSettingsData loaded = JsonUtility.FromJson<SnakeRunSettingsData>(json);
            if (loaded == null) { return defaults; }

            loaded.Sanitize();
            return loaded;
        }
        catch (Exception exception) {
            Debug.LogWarning($"Could not load SnakeRun settings: {exception.Message}");
            return defaults;
        }
    }

    public void Save() {
        Sanitize();

        try {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath));
            File.WriteAllText(SettingsPath, JsonUtility.ToJson(this, true));
        }
        catch (Exception exception) {
            Debug.LogWarning($"Could not save SnakeRun settings: {exception.Message}");
        }
    }

    public void ApplyTo(SnakeMovement snakeMovement, CursorMovement cursorMovement) {
        Sanitize();

        if (snakeMovement != null) {
            snakeMovement.speed = snakeSpeed;
            snakeMovement.turnSpeed = snakeTurnSpeed;
            snakeMovement.randomTurnStrength = snakeRandomTurnStrength;
            snakeMovement.randomEdgePadding = snakeEdgePadding;
            snakeMovement.edgeAvoidStrength = snakeEdgeAvoidStrength;
            snakeMovement.lineWidth = snakeLineWidth;
            snakeMovement.tailAlpha = snakeTailAlpha;
            snakeMovement.snakeLength = snakeLength;
            snakeMovement.fixedCircleRadius = snakeFixedCircleRadius;
            snakeMovement.RefreshSettings();
        }

        if (cursorMovement != null) {
            cursorMovement.speed = cursorSpeed;
            cursorMovement.screenPadding = cursorScreenPadding;
            cursorMovement.headSize = cursorHeadSize;
            cursorMovement.trailWidth = cursorTrailWidth;
            cursorMovement.tailAlpha = cursorTailAlpha;
            cursorMovement.tailFadeStart = cursorTailFadeStart;
            cursorMovement.trailLength = cursorTrailLength;
            cursorMovement.RefreshSettings();
        }
    }

    public string GetValue(string key) {
        switch (key) {
            case "trialCount": return trialCount.ToString(CultureInfo.InvariantCulture);
            case "trialDuration": return FormatFloat(trialDuration);
            case "breakDuration": return FormatFloat(breakDuration);
            case "countdownDuration": return FormatFloat(countdownDuration);
            case "snakeSpeed": return FormatFloat(snakeSpeed);
            case "snakeTurnSpeed": return FormatFloat(snakeTurnSpeed);
            case "snakeRandomTurnStrength": return FormatFloat(snakeRandomTurnStrength);
            case "snakeEdgePadding": return FormatFloat(snakeEdgePadding);
            case "snakeEdgeAvoidStrength": return FormatFloat(snakeEdgeAvoidStrength);
            case "snakeLineWidth": return FormatFloat(snakeLineWidth);
            case "snakeTailAlpha": return FormatFloat(snakeTailAlpha);
            case "snakeLength": return FormatFloat(snakeLength);
            case "snakeFixedCircleRadius": return FormatFloat(snakeFixedCircleRadius);
            case "cursorSpeed": return FormatFloat(cursorSpeed);
            case "cursorScreenPadding": return FormatFloat(cursorScreenPadding);
            case "cursorHeadSize": return FormatFloat(cursorHeadSize);
            case "cursorTrailWidth": return FormatFloat(cursorTrailWidth);
            case "cursorTailAlpha": return FormatFloat(cursorTailAlpha);
            case "cursorTailFadeStart": return FormatFloat(cursorTailFadeStart);
            case "cursorTrailLength": return FormatFloat(cursorTrailLength);
            default: return "";
        }
    }

    public void SetValue(string key, string value) {
        if (key == "trialCount") {
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt)) {
                trialCount = parsedInt;
            }
            return;
        }

        if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedFloat)) { return; }

        switch (key) {
            case "trialDuration": trialDuration = parsedFloat; break;
            case "breakDuration": breakDuration = parsedFloat; break;
            case "countdownDuration": countdownDuration = parsedFloat; break;
            case "snakeSpeed": snakeSpeed = parsedFloat; break;
            case "snakeTurnSpeed": snakeTurnSpeed = parsedFloat; break;
            case "snakeRandomTurnStrength": snakeRandomTurnStrength = parsedFloat; break;
            case "snakeEdgePadding": snakeEdgePadding = parsedFloat; break;
            case "snakeEdgeAvoidStrength": snakeEdgeAvoidStrength = parsedFloat; break;
            case "snakeLineWidth": snakeLineWidth = parsedFloat; break;
            case "snakeTailAlpha": snakeTailAlpha = parsedFloat; break;
            case "snakeLength": snakeLength = parsedFloat; break;
            case "snakeFixedCircleRadius": snakeFixedCircleRadius = parsedFloat; break;
            case "cursorSpeed": cursorSpeed = parsedFloat; break;
            case "cursorScreenPadding": cursorScreenPadding = parsedFloat; break;
            case "cursorHeadSize": cursorHeadSize = parsedFloat; break;
            case "cursorTrailWidth": cursorTrailWidth = parsedFloat; break;
            case "cursorTailAlpha": cursorTailAlpha = parsedFloat; break;
            case "cursorTailFadeStart": cursorTailFadeStart = parsedFloat; break;
            case "cursorTrailLength": cursorTrailLength = parsedFloat; break;
        }
    }

    public void Sanitize() {
        trialCount = Mathf.Max(1, trialCount);
        trialDuration = Mathf.Max(0.01f, trialDuration);
        breakDuration = Mathf.Max(0f, breakDuration);
        countdownDuration = Mathf.Max(0f, countdownDuration);

        snakeSpeed = Mathf.Max(0.01f, snakeSpeed);
        snakeTurnSpeed = Mathf.Max(0f, snakeTurnSpeed);
        snakeEdgePadding = Mathf.Max(0f, snakeEdgePadding);
        snakeEdgeAvoidStrength = Mathf.Max(0f, snakeEdgeAvoidStrength);
        snakeLineWidth = Mathf.Max(0.001f, snakeLineWidth);
        snakeLength = Mathf.Max(0.01f, snakeLength);
        snakeFixedCircleRadius = Mathf.Max(0.01f, snakeFixedCircleRadius);

        cursorSpeed = Mathf.Max(0.01f, cursorSpeed);
        cursorScreenPadding = Mathf.Max(0f, cursorScreenPadding);
        cursorHeadSize = Mathf.Max(0.01f, cursorHeadSize);
        cursorTrailWidth = Mathf.Max(0.001f, cursorTrailWidth);
        cursorTailFadeStart = Mathf.Clamp01(cursorTailFadeStart);
        cursorTrailLength = Mathf.Max(0.01f, cursorTrailLength);
    }

    static string FormatFloat(float value) {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
