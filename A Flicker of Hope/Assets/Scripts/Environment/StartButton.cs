using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public string sceneToLoad = "Natural";

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
