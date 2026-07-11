using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SnakeTestManager : MonoBehaviour
{
    public SnakeMovement snakeMovement;
    public CursorMovement cursorMovement;
    public string selectedMode;

    public GameObject modePanel;
    public Button fixedButton;
    public Button randomButton;
    public Text countdownText;

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

        modePanel.SetActive(true);
        countdownText.gameObject.SetActive(false);
        fixedButton.onClick.AddListener(() => StartTest("fixed"));
        randomButton.onClick.AddListener(() => StartTest("random"));
    }

    void StartTest(string mode)
    {
        selectedMode = mode;

        if (snakeMovement != null)
        {
            snakeMovement.SetMode(mode);
        }

        modePanel.SetActive(false);
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        countdownText.gameObject.SetActive(true);

        for (int value = 3; value > 0; value--)
        {
            countdownText.text = value.ToString();
            yield return new WaitForSeconds(1f);
        }

        countdownText.gameObject.SetActive(false);

        if (snakeMovement != null) { snakeMovement.enabled = true; }
        if (cursorMovement != null) { cursorMovement.gameObject.SetActive(true); }
    }
}
