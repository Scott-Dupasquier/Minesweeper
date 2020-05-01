using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CellManager : MonoBehaviour
{
    private int value;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSize(float cellSize)
    {
        var rectTransform = GetComponent<RectTransform>();
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize);
    }

    public int GetValue()
    {
        return value;
    }

    public void SetValue(int newValue)
    {
        value = newValue;
    }

    public void Reveal()
    {
        GetComponentInChildren<Text>().text = value.ToString();
        GetComponent<Button>().interactable = false;
    }
}
