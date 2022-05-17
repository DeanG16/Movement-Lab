using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{
    public System.Action<float> mouseSensitivityChange;

    private InputManager inputManager;
    public Slider sensitivitySlider;
    public Button quitButton;

    public GameObject pauseMenu;
    public GameObject debugPanel;
    public GameObject crossHair;

    public Text sensitivityLabel;

    private void Start() {
        inputManager = GetComponentInParent<InputManager>();
        inputManager.pausedGame += HandlePause;
    }

    // Start is called before the first frame update
    void HandlePause(bool IsPaused) {
        if(IsPaused) {
            sensitivitySlider.value = inputManager.mouseXSensitivity;
            sensitivityLabel.text = inputManager.mouseXSensitivity.ToString();
            pauseMenu.SetActive(true);
            debugPanel.SetActive(false);
            crossHair.SetActive(false);
        } else {
            pauseMenu.SetActive(false);
            debugPanel.SetActive(true);
            crossHair.SetActive(true);
        }
    }

    public void UpdateSensitivity() {
        if(!pauseMenu.activeSelf) { return; }
        mouseSensitivityChange(sensitivitySlider.value);
        sensitivityLabel.text = inputManager.mouseXSensitivity.ToString();
    }

    public void QuitGame() {
        Application.Quit();
    }
}
