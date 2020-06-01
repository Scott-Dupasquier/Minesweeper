using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
    // Load possible sprites for the face
    public Sprite happyFace;
    public Sprite openMouth;
    public Sprite deadFace;
    public Sprite sunglasses;

    // Call when a bomb has been hit
    public void Death()
    {
        GetComponent<Image>().sprite = deadFace;
    }

    // Call on a win to update face to the sunglasses face
    public void Win()
    {
        GetComponent<Image>().sprite = sunglasses;
    }

    // Set the face to have an open mouth while waiting to reveal a tiles
    public void Hold()
    {
        GetComponent<Image>().sprite = openMouth;
    }

    // Call when mouse button has been released to reset the face
    public void Release()
    {
        GetComponent<Image>().sprite = happyFace;
    }
}
