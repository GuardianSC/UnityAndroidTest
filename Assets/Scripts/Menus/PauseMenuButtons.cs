using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AndroidTest2D
{
    public class PauseMenuButtons : MonoBehaviour
    {
        public GameObject pausePanel; // In game pause panel
        public GameObject pauseButton;
        
        // Bring up pause panel, deactivate the pause button, and pause the game (timeScale = 0)
        public void Pause()
        {
            pausePanel.SetActive(true);
            pauseButton.SetActive(false);
            Time.timeScale = 0;
        }

        // Deactivate the pause panel, activate the pause button, and resume the game (timeScale = 1)
        public void Resume()
        {
            pausePanel.SetActive(false);
            pauseButton.SetActive(true);
            Time.timeScale = 1;
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

    }
}