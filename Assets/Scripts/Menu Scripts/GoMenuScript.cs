using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoMenuScript : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyDown("backspace"))
        {
            GoMenuScene();

        }

    }
    public void GoMenuScene()
    {
        SceneManager.LoadScene("1MenuScene");

    }
}
