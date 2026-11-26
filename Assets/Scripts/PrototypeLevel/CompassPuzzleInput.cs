using UnityEngine;
using UnityEngine.InputSystem;


// Reads the mouse and turns clicks on the compass into CompassPuzzle calls.
// Hit testing is done by distance from the compass centre (ring click zones),
// so overlapping images never compete for pointer events.
// Runs before CompassPuzzle so a part grabbed this frame also rotates this frame.
[DefaultExecutionOrder(-10)]
public sealed class CompassPuzzleInput : MonoBehaviour
{
    [SerializeField] private CompassPuzzle puzzle;

    [Header("Click Area")]
    [SerializeField] private RectTransform compassArea;

    [Header("Click Zones (outer to inner)")]
    [SerializeField] private float[] innerClickRadii = { 0.60f, 0.42f, 0.24f, 0f };
    [SerializeField] private float[] outerClickRadii = { 1f, 0.60f, 0.42f, 0.24f };

    private Canvas parentCanvas;

    private void Awake()
    {
        if (puzzle == null)
            puzzle = GetComponent<CompassPuzzle>();

        parentCanvas = compassArea != null ? compassArea.GetComponentInParent<Canvas>() : null;
    }

    private void Update()
    {
        if (puzzle == null || puzzle.IsUnlocked || Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            puzzle.ReleasePart();
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryRotatePartAtScreenPosition(Mouse.current.position.ReadValue());
    }

    // Applies the same hit testing used by a real mouse click, then starts
    // holding the selected part without overlapping UI images competing for pointer events
    public bool TryRotatePartAtScreenPosition(Vector2 screenPosition)
    {
        return TryGetClockPartAt(screenPosition, out int partIndex) && puzzle.TryRotatePart(partIndex);
    }

    // just checks to make sure the clock parts are working as intended
    private bool TryGetClockPartAt(Vector2 screenPosition, out int partIndex)
    {
        partIndex = -1;

        if (compassArea == null || puzzle == null || puzzle.PartCount == 0)
            return false;

        Camera eventCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? parentCanvas.worldCamera : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(compassArea, screenPosition, eventCamera, out Vector2 localPoint))
        {
            return false;
        }

        Rect rect = compassArea.rect;
        float halfSmallestDimension = Mathf.Min(rect.width, rect.height) * 0.5f;

        if (halfSmallestDimension <= 0f)
            return false;

        float normalizedRadius = (localPoint - rect.center).magnitude / halfSmallestDimension;

        if (normalizedRadius > 1f)
            return false;

        for (int index = 0; index < puzzle.PartCount; index++)
        {
            float innerRadius = GetValue(innerClickRadii, index, 0f);
            float outerRadius = GetValue(outerClickRadii, index, 0f);

            if (normalizedRadius >= innerRadius && normalizedRadius <= outerRadius)
            {
                partIndex = index;
                // // fail if the part is already aligned
                // return !puzzle.IsPartAligned(partIndex);
                return true;
            }
        }

        return false;
    }

    private static float GetValue(float[] values, int index, float fallback)
    {
        return values != null && index >= 0 && index < values.Length ? values[index] : fallback;
    }
}
