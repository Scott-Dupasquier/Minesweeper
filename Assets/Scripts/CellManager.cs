using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellManager : MonoBehaviour
{
    private int value;
    private int row;
    private int col;
    // Use locked to lock passing PlayerPrefs so GridManager doesn't re-try expansion
    private bool locked = false;
    // Avoid revealing the cell twice (just in case)
    private bool revealed = false;
    public Sprite clickedSquare;
    // Start is called before the first frame update
    void Start()
    {

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
            // Nothing to do, it's already revealed
            return;
        }
        
        GetComponentInChildren<Text>().text = value.ToString();
        GetComponent<Image>().sprite = clickedSquare;
        // thisButton.interactable = false;
        
        if (value == 0 && !locked)
        {
            PlayerPrefs.SetInt("row", row);
            PlayerPrefs.SetInt("col", col);
        }
    }
}
