using UnityEngine;
using UnityEngine.UI;

public class SnakeRunSettingInput : MonoBehaviour
{
    public string key;
    public InputField input;

    public void SetText(string value) {
        if (input == null) { input = GetComponentInChildren<InputField>(true); }
        if (input != null) { input.text = value; }
    }

    public string GetText() {
        if (input == null) { input = GetComponentInChildren<InputField>(true); }
        if (input == null) { return ""; }
        return input.text;
    }
}
