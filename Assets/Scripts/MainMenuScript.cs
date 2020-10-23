using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void LoadGame()
    {
        SceneManager.LoadScene(1);
    }
}
