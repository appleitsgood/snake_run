using UnityEngine;
using UnityEngine.UI;

public class SnakeRunSettingInput : MonoBehaviour
{
    public string key;
    public InputField input;
    public Toggle toggle;

    public void SetValue(string value) {
        if (input == null) { input = GetComponentInChildren<InputField>(true); }
        if (toggle == null) { toggle = GetComponentInChildren<Toggle>(true); }

        if (input != null) { input.text = value; }
        if (toggle != null && bool.TryParse(value, out bool parsedValue)) { toggle.isOn = parsedValue; }
    }

    public string GetValue() {
        if (input == null) { input = GetComponentInChildren<InputField>(true); }
        if (toggle == null) { toggle = GetComponentInChildren<Toggle>(true); }

        if (input != null) { return input.text; }
        if (toggle != null) { return toggle.isOn.ToString(); }
        return "";
    }
}
