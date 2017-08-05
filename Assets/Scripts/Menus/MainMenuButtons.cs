using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AndroidTest2D
{
    public class MainMenuButtons : MonoBehaviour
    {
        public void LoadTestScene()
        {
            SceneManager.LoadScene("Test");
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