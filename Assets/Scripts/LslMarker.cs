using LSL;
using UnityEngine;

public class LslMarker : MonoBehaviour
{
    public const int TrialStart = 11;
    public const int TrialEnd = 12;
    public const int TrackingStart = 21;
    public const int TrackingEnd = 22;

    public string streamName = "SnakeRunMarkers";
    public string streamType = "Markers";
    public string sourceId = "snakerun_markers";
    public bool logConsumerStatus = true;
    public float consumerLogInterval = 2f;

    StreamOutlet outlet;
    int[] sample = new int[1];
    float nextConsumerLogTime;
    bool lastConsumerState;
    bool hasLoggedConsumerState;

    public bool IsReady {
        get { return outlet != null; }
    }

    public bool HasConsumer {
        get { return outlet != null && outlet.have_consumers(); }
    }

    void Start() {
        CreateOutlet();
    }

    void Update() {
        if (!logConsumerStatus) { return; }
        if (outlet == null) { return; }
        if (Time.unscaledTime < nextConsumerLogTime) { return; }

        LogConsumerStatus();
        nextConsumerLogTime = Time.unscaledTime + Mathf.Max(0.1f, consumerLogInterval);
    }

    void OnDestroy() {
        if (outlet != null) {
            outlet.Dispose();
            outlet = null;
        }
    }

    public void PushTrialStart() {
        PushMarker(TrialStart, "trial_start");
    }

    public void PushTrialEnd() {
        PushMarker(TrialEnd, "trial_end");
    }

    public void PushTrackingStart() {
        PushMarker(TrackingStart, "tracking_start");
    }

    public void PushTrackingEnd() {
        PushMarker(TrackingEnd, "tracking_end");
    }

    public void PushMarker(int markerCode, string markerName = "") {
        if (outlet == null) {
            Debug.LogWarning($"LSL marker outlet is not ready. Marker was not sent: {markerCode} {markerName}");
            return;
        }

        sample[0] = markerCode;
        outlet.push_sample(sample, LSL.LSL.local_clock());

        if (!HasConsumer) {
            Debug.LogWarning($"LSL marker sent without a consumer: {markerCode} {markerName}");
            return;
        }

        Debug.Log($"LSL marker sent: {markerCode} {markerName}");
    }

    void CreateOutlet() {
        if (outlet != null) { return; }

        StreamInfo streamInfo = new StreamInfo(
            streamName,
            streamType,
            1,
            LSL.LSL.IRREGULAR_RATE,
            channel_format_t.cf_int32,
            sourceId
        );

        outlet = new StreamOutlet(streamInfo);
        Debug.Log($"LSL marker outlet ready: {streamName}");
    }

    void LogConsumerStatus() {
        bool hasConsumer = HasConsumer;
        if (hasLoggedConsumerState && hasConsumer == lastConsumerState) { return; }

        if (hasConsumer) {
            Debug.Log($"LSL marker outlet has consumer: {streamName}");
        }
        else {
            Debug.LogWarning($"LSL marker outlet has no consumer yet: {streamName}");
        }

        lastConsumerState = hasConsumer;
        hasLoggedConsumerState = true;
    }
}
