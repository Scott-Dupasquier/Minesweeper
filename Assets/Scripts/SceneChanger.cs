using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Transitions learnt from https://www.youtube.com/watch?v=CE9VOZivb3I
    public Animator fade;

    // Start coroutine to switch to specified scene
    public void ChangeScene(string sceneName)
    {
        StartCoroutine(Load(sceneName));
    }

    // Exit the application
    public void Quit()
    {
        Application.Quit();
    }

    private IEnumerator Load(string sceneName)
    {
        fade.SetTrigger("CloseScene");
        
        // Wait 1 second for animation
        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(sceneName);
    }
}
