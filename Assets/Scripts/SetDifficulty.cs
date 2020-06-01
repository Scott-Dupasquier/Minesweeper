using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetDifficulty : MonoBehaviour
{
    // Difficulty buttons
    public Button easyButton;
    public Button intermediateButton;
    public Button hardButton;

    // Possible images for the buttons
    public Sprite checkedIm;
    public Sprite notCheckedIm;

    private string difficulty;

    void Awake()
    {
        // Default difficulty is intermediate, if they've played before take previous game difficulty
        if (PlayerPrefs.HasKey("difficulty"))
        {
            difficulty = PlayerPrefs.GetString("difficulty");
        }
        else
        {
            difficulty = "intermediate";
        }

        UpdateButtonImages();
    }

    // Change the difficulty to easy, intermediate, or hard
    public void ChangeDifficulty(string newDifficulty)
    {
        difficulty = newDifficulty;
        PlayerPrefs.SetString("difficulty", difficulty);
        UpdateButtonImages();
    }

    // Can only ever have one checked at a time, call this to update them
    public void UpdateButtonImages()
    {
        // Set the correct button to have the check mark
        if (difficulty == "easy")
        {
            easyButton.GetComponent<Image>().sprite = checkedIm;
            intermediateButton.GetComponent<Image>().sprite = notCheckedIm;
            hardButton.GetComponent<Image>().sprite = notCheckedIm;
        }
        else if (difficulty == "intermediate")
        {
            easyButton.GetComponent<Image>().sprite = notCheckedIm;
            intermediateButton.GetComponent<Image>().sprite = checkedIm;
            hardButton.GetComponent<Image>().sprite = notCheckedIm;
        }
        else
        {
            easyButton.GetComponent<Image>().sprite = notCheckedIm;
            intermediateButton.GetComponent<Image>().sprite = notCheckedIm;
            hardButton.GetComponent<Image>().sprite = checkedIm;
        }
    }
}
