using UnityEngine;

public class FadeScene : MonoBehaviour
{
    [SerializeField] private string targetSceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Fade(targetSceneName);
        }
    }

    public void Fade(string sceneName)
    {
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToBlack(sceneName);
        }
        else
        {
            Debug.LogError("SceneFader instance not found in the scene!");
        }
    }
}
