using UnityEngine;
//Author: Danielle Campisi

public class UIBounce : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float bounceHeight = 15f;
    [SerializeField] private float bounceSpeed = 5f;

    private RectTransform rectTransform;
    private Vector2 startPos;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        // Use a sine wave to create a smooth up-and-down floating motion
        float newY = startPos.y + (Mathf.Sin(Time.time * bounceSpeed) * bounceHeight);
        rectTransform.anchoredPosition = new Vector2(startPos.x, newY);
    }
}
