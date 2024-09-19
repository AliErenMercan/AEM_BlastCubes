using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScene : MonoBehaviour
{

    public GameObject buttonText;
    public int levelCount;

    void Start()
    {
        //PlayerPrefs.DeleteAll();
        if (!PlayerPrefs.HasKey("level"))
        {
            PlayerPrefs.SetInt("level", 0);
            PlayerPrefs.Save();
        }

        int currentLevel = PlayerPrefs.GetInt("level");
        if (currentLevel < levelCount)
        {
            buttonText.GetComponent<TMP_Text>().text = "Level " + (currentLevel + 1);
        }
        else
        {
            buttonText.GetComponent<TMP_Text>().text = "Finished";
        }
    }

    public void loadLevel()
    {
        if(!buttonText.GetComponent<TMP_Text>().text.Equals("Finished"))
        {
            SceneManager.LoadScene("LevelScene", LoadSceneMode.Single);
        }
    }
}
