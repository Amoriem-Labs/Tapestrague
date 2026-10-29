using System.Collections;
using UnityEngine;

public sealed class CompassPuzzle : MonoBehaviour
{
    public enum DoorOpeningMethod
    {
        Slide,
        Swing
    }

    [System.Serializable]
    private class CompassPart
    {
        public string label;

        [Tooltip("Drag this compass layer's RectTransform here.")]
        public RectTransform image;

        [Tooltip("Distance from the compass center where this layer starts. 0 = center, 1 = outer edge.")]
        [Range(0f, 1f)] public float innerClickRadius;

        [Tooltip("Distance from the compass center where this layer ends. 0 = center, 1 = outer edge.")]
        [Range(0f, 1f)] public float outerClickRadius = 1f;

        [Tooltip("How far this part turns for each right-click. Positive values turn counter-clockwise.")]
        public float degreesPerRightClick = 90f;

        [Tooltip("The Z rotation, in degrees, that represents this part being correctly aligned.")]
        public float solvedZAngle = 0f;
    }

    [Header("Compass UI")]
    [Tooltip("An empty RectTransform centered on the compass and sized to its outer edge.")]
    [SerializeField] private RectTransform compassArea;

    [Tooltip("Keep these ordered from outermost ring to innermost piece.")]
    [SerializeField] private CompassPart[] compassParts =
    {
        new CompassPart
        {
            label = "Outer Ring",
            innerClickRadius = 0.72f,
            outerClickRadius = 1f
        },
        new CompassPart
        {
            label = "Middle Ring 1",
            innerClickRadius = 0.48f,
            outerClickRadius = 0.72f
        },
        new CompassPart
        {
            label = "Middle Ring 2",
            innerClickRadius = 0.25f,
            outerClickRadius = 0.48f
        },
        new CompassPart
        {
            label = "Inner Piece",
            innerClickRadius = 0f,
            outerClickRadius = 0.25f
        }
    };

    [Header("Solved State")]
    [SerializeField, Min(0f)] private float alignmentTolerance = 0.5f;

    [Tooltip("Assign the object labelled Door here.")]
    [SerializeField] private Transform door;

    [SerializeField] private DoorOpeningMethod doorOpeningMethod = DoorOpeningMethod.Slide;

    [Tooltip("For Slide: movement relative to the door's parent. Example: (0, 3, 0).")]
    [SerializeField] private Vector3 doorLocalSlideOffset = new Vector3(0f, 3f, 0f);

    [Tooltip("For Swing: rotation relative to the door's starting rotation. Use Y for most 3D doors, Z for 2D doors.")]
    [SerializeField] private Vector3 doorLocalSwingEuler = new Vector3(0f, 90f, 0f);

    [SerializeField, Min(0f)] private float doorOpenDuration = 1f;

    [Header("Sound")]
    [Tooltip("Assign an AudioSource with your downloaded solved-sound clip.")]
    [SerializeField] private AudioSource solvedAudioSource;

    private bool solved;
    private Vector3 closedDoorLocalPosition;
    private Quaternion closedDoorLocalRotation;

    private void Awake()
    {
        if (door != null)
        {
            closedDoorLocalPosition = door.localPosition;
            closedDoorLocalRotation = door.localRotation;
        }
    }

    private void Update()
    {
        if (solved || !Input.GetMouseButtonDown(1))
            return;

        if (!TryGetClickedCompassPart(out CompassPart clickedPart))
            return;

        clickedPart.image.Rotate(
            Vector3.forward,
            clickedPart.degreesPerRightClick,
            Space.Self);

        if (AllCompassPartsAreAligned())
            SolvePuzzle();
    }

    private bool TryGetClickedCompassPart(out CompassPart clickedPart)
    {
        clickedPart = null;

        if (compassArea == null)
            return false;

        Canvas canvas = compassArea.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                compassArea,
                Input.mousePosition,
                uiCamera,
                out Vector2 localMousePosition))
        {
            return false;
        }

        Rect rect = compassArea.rect;

        if (rect.width <= 0f || rect.height <= 0f)
            return false;

        // Makes 1.0 equal the outer circular edge of the compass.
        Vector2 normalizedFromCenter = new Vector2(
            (localMousePosition.x - rect.center.x) / (rect.width * 0.5f),
            (localMousePosition.y - rect.center.y) / (rect.height * 0.5f));

        float clickRadius = normalizedFromCenter.magnitude;

        if (clickRadius > 1f)
            return false;

        foreach (CompassPart part in compassParts)
        {
            if (part == null || part.image == null)
                continue;

            if (clickRadius >= part.innerClickRadius &&
                clickRadius <= part.outerClickRadius)
            {
                clickedPart = part;
                return true;
            }
        }

        return false;
    }

    private bool AllCompassPartsAreAligned()
    {
        foreach (CompassPart part in compassParts)
        {
            if (part == null || part.image == null)
                return false;

            float difference = Mathf.Abs(
                Mathf.DeltaAngle(part.image.localEulerAngles.z, part.solvedZAngle));

            if (difference > alignmentTolerance)
                return false;
        }

        return true;
    }

    private void SolvePuzzle()
    {
        solved = true;

        if (solvedAudioSource != null && solvedAudioSource.clip != null)
            solvedAudioSource.Play();

        StartCoroutine(OpenDoor());
    }

    private IEnumerator OpenDoor()
    {
        if (door == null)
            yield break;

        Vector3 openPosition = closedDoorLocalPosition + doorLocalSlideOffset;
        Quaternion openRotation =
            closedDoorLocalRotation * Quaternion.Euler(doorLocalSwingEuler);

        if (doorOpenDuration <= 0f)
        {
            ApplyDoorState(openPosition, openRotation, 1f);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < doorOpenDuration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / doorOpenDuration);
            float easedProgress = progress * progress * (3f - 2f * progress);

            ApplyDoorState(openPosition, openRotation, easedProgress);

            yield return null;
        }

        ApplyDoorState(openPosition, openRotation, 1f);
    }

    private void ApplyDoorState(
        Vector3 openPosition,
        Quaternion openRotation,
        float progress)
    {
        if (doorOpeningMethod == DoorOpeningMethod.Slide)
        {
            door.localPosition = Vector3.Lerp(
                closedDoorLocalPosition,
                openPosition,
                progress);
        }
        else
        {
            door.localRotation = Quaternion.Slerp(
                closedDoorLocalRotation,
                openRotation,
                progress);
        }
    }
}