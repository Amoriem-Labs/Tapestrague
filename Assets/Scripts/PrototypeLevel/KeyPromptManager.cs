using UnityEngine;

public class KeyPromptManager : MonoBehaviour
{
    public Canvas canvas;

    private void Start()
    {
        if (canvas != null)
        {
            canvas.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canvas != null)
        {
            canvas.enabled = true; 
        }
    }
}
