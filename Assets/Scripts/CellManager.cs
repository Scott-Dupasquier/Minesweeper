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
        // Cell revealed, nothing else to do
        if (revealed)
        {
            return;
        }

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
        if (revealed || cycleStatus != "default")
        {
            // Nothing to do, it's already revealed
            return;
        }

        if (value == -1)
        {
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
            GetComponentInChildren<TMP_Text>().text = value.ToString();
        }
        GetComponent<Image>().sprite = clickedSquare;
        // thisButton.interactable = false;
        
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
