using UnityEngine;
using UnityEngine.SceneManagement; 

public class SceneLoader : MonoBehaviour
{
   
    public void LoadByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    
    public void LoadByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}