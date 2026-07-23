using System;
using System.Globalization;
using System.IO;
using LSL;
using UnityEngine;

public class CSVLogger : MonoBehaviour
{
    public string outputDirectory = "D:/Capstone1/snake_run_data";

    StreamWriter writer;
    SnakeMovement snakeMovement;
    CursorMovement cursorMovement;
    string runId = "sub01";
    bool isTracking;

    public bool IsLogOpen {
        get { return writer != null; }
    }

    public void CreateLog(string runId, SnakeMovement snakeMovement, CursorMovement cursorMovement) {
        SaveLog();

        this.runId = string.IsNullOrWhiteSpace(runId) ? "sub01" : runId;
        this.snakeMovement = snakeMovement;
        this.cursorMovement = cursorMovement;

        Directory.CreateDirectory(outputDirectory);
        string fileName = $"snakerun-{this.runId}-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
        string filePath = Path.Combine(outputDirectory, fileName);

        writer = new StreamWriter(filePath, false);
        writer.WriteLine("timestamp,run_id,phase,snake_x,snake_y,cursor_x,cursor_y");
        writer.Flush();

        Debug.Log($"CSV log created: {filePath}");
    }

    public void SaveLog() {
        StopTracking();

        if (writer == null) { return; }

        writer.Flush();
        writer.Close();
        writer = null;
        Debug.Log("CSV log saved.");
    }

    public void StartTracking() {
        if (writer == null) {
            Debug.LogWarning("CSV log is not open. Tracking samples will not be written.");
            return;
        }

        isTracking = true;
    }

    public void StopTracking() {
        isTracking = false;
    }

    void FixedUpdate() {
        if (!isTracking) { return; }
        if (writer == null) { return; }

        WriteTrackingSample();
    }

    void OnDestroy() {
        SaveLog();
    }

    void WriteTrackingSample() {
        Vector3 snakePosition = snakeMovement != null ? snakeMovement.transform.position : Vector3.zero;
        Vector3 cursorPosition = cursorMovement != null ? cursorMovement.transform.position : Vector3.zero;
        double timestamp = LSL.LSL.local_clock();

        writer.WriteLine(string.Join(",",
            FormatDouble(timestamp),
            Escape(runId),
            "tracking",
            FormatFloat(snakePosition.x),
            FormatFloat(snakePosition.y),
            FormatFloat(cursorPosition.x),
            FormatFloat(cursorPosition.y)
        ));
    }

    static string FormatFloat(float value) {
        return value.ToString("0.######", CultureInfo.InvariantCulture);
    }

    static string FormatDouble(double value) {
        return value.ToString("0.######", CultureInfo.InvariantCulture);
    }

    static string Escape(string value) {
        if (value == null) { return ""; }
        if (!value.Contains(",") && !value.Contains("\"") && !value.Contains("\n") && !value.Contains("\r")) { return value; }
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
