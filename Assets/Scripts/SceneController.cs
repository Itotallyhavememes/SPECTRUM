using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [SerializeField] string loadingSceneName;

    public void SetSceneByName(string name)
    {
        if (SceneManager.GetSceneByName(name) != null)
        {
            if (SceneManager.GetSceneByName(loadingSceneName) != null)
                SceneManager.LoadScene(loadingSceneName);

            SceneManager.LoadSceneAsync(name);
        }
    }
}
