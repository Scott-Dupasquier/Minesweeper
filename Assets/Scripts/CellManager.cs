using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CellManager : MonoBehaviour, IPointerClickHandler
{
    // Main grid
    public GameObject grid;

    public Animator animator;

    // Load images
    public Sprite clickedSquare;
    public Sprite defaultSquare;
    public Sprite bomb;
    public Sprite clickedBomb;
    public Sprite flag;
    public Sprite incorrectFlag;

    public Button restartButton;

    private int value;
    private int row;
    private int col;
    // Use locked to lock passing PlayerPrefs so GridManager doesn't re-try expansion
    private bool locked = false;
    // Avoid revealing the cell twice (just in case)
    private bool revealed = false;
    // Status of the "right click cycle" (default -> flag -> ? -> etc.)
    private string cycleStatus;
    private AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        cycleStatus = "default";
        audioManager = FindObjectOfType<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void OnPointerClick (PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Reveal();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            UpdateRightClick();
        }
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

    public string GetCycleStatus()
    {
        return cycleStatus;
    }

    public int GetRow()
    {
        return row;
    }

    public int GetCol()
    {
        return col;
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
        if (revealed)
        {
            return;
        }

        var finished = grid.GetComponent<GridManager>().GetFinished();
        if (value == -1)
        {
            // Don't allow a reveal click on flag
            if (cycleStatus == "flag")
            {
                return;
            }

            revealed = true;
            if (finished)
            {
                // We're revealing the board
                animator.SetTrigger("Explode");
                GetComponent<Image>().sprite = bomb;
            }
            else
            {
                // Hit a bomb, game finished
                GetComponent<Image>().sprite = clickedBomb;
                animator.SetTrigger("Explode");
                audioManager.PlaySound("explosion");
                grid.GetComponent<GridManager>().GameOver("clickedbomb");
                grid.GetComponent<GridManager>().RevealCells(col, row);
            }

            return;
        }
        else if (value != 0)
        {
            if (cycleStatus == "flag" && finished)
            {
                // Incorrectly marked this cell as a bomb
                GetComponent<Image>().sprite = incorrectFlag;
                return;
            }
            else if (cycleStatus == "?" && !finished)
            {
                // Don't reveal a cell if marked ?, only reveal if game over
                return;
            }

            if (!finished)
            {
                audioManager.PlaySound("dig");
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
        
        if (value == 0 && !locked && !finished)
        {
            grid.GetComponent<GridManager>().RevealZeros(col, row);
        }
        revealed = true;
    }

    private void UpdateRightClick()
    {
        if (cycleStatus == "default")
        {
            GetComponent<Image>().sprite = flag;
            cycleStatus = "flag";
            grid.GetComponent<GridManager>().AdjustBombAmt(1);
        }
        else if (cycleStatus == "flag")
        {
            GetComponent<Image>().sprite = defaultSquare;
            GetComponentInChildren<TMP_Text>().text = "?";
            cycleStatus = "?";
            grid.GetComponent<GridManager>().AdjustBombAmt(-1);
        }
        else
        {
            GetComponentInChildren<TMP_Text>().text = "";
            cycleStatus = "default";
        }
    }
}
