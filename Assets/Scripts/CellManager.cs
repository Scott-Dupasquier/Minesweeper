using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CellManager : MonoBehaviour
{
    // Load images
    public Sprite clickedSquare;
    public Sprite defaultSquare;
    public Sprite bomb;
    public Sprite clickedBomb;
    public Sprite flag;
    public Sprite incorrectFlag;

    private int value;
    private int row;
    private int col;
    // Use locked to lock passing PlayerPrefs so GridManager doesn't re-try expansion
    private bool locked = false;
    // Avoid revealing the cell twice (just in case)
    private bool revealed = false;
    // Status of the "right click cycle" (default -> flag -> ? -> etc.)
    private string cycleStatus;
    // Start is called before the first frame update
    void Start()
    {
        cycleStatus = "default";
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeValues(float cellSize, int y, int x)
    {
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize);

        row = y;
        col = x;
    }

    public int GetValue()
    {
        return value;
    }

    public bool GetRevealed()
    {
        return revealed;
    }

    public int GetRow()
    {
        return row;
    }

    public int GetCol()
    {
        return col;
    }

    public void SetSprite(string spriteToUse)
    {
        if (spriteToUse.ToLower() == "revealed")
        {
            Reveal();
        }
        else if (spriteToUse.ToLower() == "bomb")
        {
            GetComponent<Image>().sprite = bomb;
        }
        else if (spriteToUse.ToLower() == "rightclick")
        {
            UpdateRightClick();
        }
        else if (spriteToUse.ToLower() == "clickedbomb")
        {
            GetComponent<Image>().sprite = clickedBomb;
        }
    }

    public void SetLock(bool status)
    {
        locked = status;
    }

    public void SetValue(int newValue)
    {
        value = newValue;
    }

    public void Reveal()
    {
        if (value == -1)
        {
            // Don't allow a reveal click on flag or ? (? can be revealed if game is over however)
            if (cycleStatus == "flag" || (cycleStatus == "?" && !PlayerPrefs.HasKey("finished")))
            {
                return;
            }

            if (PlayerPrefs.HasKey("finished"))
            {
                // We're revealing the board
                GetComponent<Image>().sprite = bomb;
            }
            else
            {
                // Hit a bomb, game finished
                GetComponent<Image>().sprite = clickedBomb;
                PlayerPrefs.SetString("finished", "clickedbomb");
            }

            revealed = true;
            return;
        }
        else if (value != 0)
        {
            if (cycleStatus == "flag")
            {
                // Incorrectly marked this cell as a bomb
                GetComponent<Image>().sprite = incorrectFlag;
                return;
            }

            var tmp = GetComponentInChildren<TMP_Text>();
            tmp.text = value.ToString();
            switch (value)
            {
                // 1 = red, 2 = green, 3 = red, 4 = purple
                // 5 = maroon, 6 = turquoise, 7 = black, 8 = Gray
                case 1:
                    tmp.color = new Color32(0, 0, 200, 255);
                    break;
                case 2:
                    tmp.color = new Color32(0, 128, 0, 255);
                    break;
                case 3:
                    tmp.color = new Color32(255, 0, 0, 255);
                    break;
                case 4:
                    tmp.color = new Color32(75, 0, 130, 255);
                    break;
                case 5:
                    tmp.color = new Color32(128, 0, 0, 255);
                    break;
                case 6:
                    tmp.color = new Color32(0, 139, 139, 255);
                    break;
                case 7:
                    tmp.color = new Color32(0, 0, 0, 255);
                    break;
                default:
                    tmp.color = new Color32(200, 200, 200, 255);
                    break;
            }
        }
        GetComponent<Image>().sprite = clickedSquare;
        
        if (value == 0 && !locked)
        {
            PlayerPrefs.SetInt("row", row);
            PlayerPrefs.SetInt("col", col);
        }
        revealed = true;
    }

    private void UpdateRightClick()
    {
        if (cycleStatus == "default")
        {
            GetComponent<Image>().sprite = flag;
            cycleStatus = "flag";
        }
        else if (cycleStatus == "flag")
        {
            GetComponent<Image>().sprite = defaultSquare;
            GetComponentInChildren<TMP_Text>().text = "?";
            cycleStatus = "?";
        }
        else
        {
            GetComponentInChildren<TMP_Text>().text = "";
            cycleStatus = "default";
        }
    }
}
