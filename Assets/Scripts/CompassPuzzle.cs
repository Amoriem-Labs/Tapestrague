using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


// Author: Nathan Burns
// Controls the four-part clock puzzle in First_Puzzle.
// The parts are ordered from the outer ring to the inner disc.
// It also should be as intereactable as possible - ie it can work from Unity 
public sealed class CompassPuzzle : MonoBehaviour
{
    [Header("Clock Parts (outer to inner)")]
    [SerializeField] private RectTransform compassArea;
    [SerializeField] private Image[] clockParts = new Image[4];
    [SerializeField] private float[] startingZAngles = { 90f, 180f, 270f, 90f };
    [SerializeField] private float[] solvedZAngles = { 0f, 0f, 0f, 0f };

    [Header("Click Zones (outer to inner)")]
    [SerializeField] private float[] innerClickRadii = { 0.60f, 0.42f, 0.24f, 0f };
    [SerializeField] private float[] outerClickRadii = { 1f, 0.60f, 0.42f, 0.24f };

    [Header("Rotation")]
    [SerializeField, Min(1f)] private float rotationDegreesPerSecond = 180f;
    [SerializeField, Min(0f)] private float alignmentSnapDegrees = 3f;
    [SerializeField, Min(0f)] private float alignmentTolerance = 0.5f;

    [Header("Completion")]
    [SerializeField] private AudioSource alignmentAudioSource;
    [SerializeField] private RectTransform door;
    [SerializeField] private float doorRiseDistance = 250f;
    [SerializeField, Min(0.01f)] private float doorRiseDuration = 0.75f;

    private Canvas parentCanvas;
    private bool[] partLocked;
    private bool[] mustLeaveSolvedAngle;
    private Vector2 closedDoorPosition;
    private int heldPartIndex = -1;
    private bool unlocked;

    private DoorInteraction doorInteraction;

    public bool IsUnlocked => unlocked;
    public int AlignmentSoundPlayCount { get; private set; }

    private void Awake()
    {
        // called at the start to set the clock to the designated degrees 
        // as specified in the unity editor. Made to be flexible and not hardcoded 
        ResetPuzzle();
        doorInteraction = door.GetComponent<DoorInteraction>();
    }

    private void Update()
    {
        if (unlocked || Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            heldPartIndex = -1;
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryRotatePartAtScreenPosition(Mouse.current.position.ReadValue());

        if (Mouse.current.leftButton.isPressed && heldPartIndex >= 0)
            RotateHeldPart(Time.deltaTime);
    }

    // if the player presses on the clock
    // A previously solved part can also be unlcoked 
    // so the player can turn it again before the whole puzzle is done
    // I included the ability to keep turning in case we want to remove the sound 
    // until only after it all was done
    public bool TryRotatePart(int partIndex)
    {
        if (unlocked || !HasValidPartIndex(partIndex))
            return false;

        mustLeaveSolvedAngle[partIndex] = partLocked[partIndex];
        partLocked[partIndex] = false;
        heldPartIndex = partIndex;
        return true;
    }

    
    // Applies the same hit testing used by a real mouse click, then starts
    // holding the selected part without overlapping UI images competing for pointer events
    public bool TryRotatePartAtScreenPosition(Vector2 screenPosition)
    {
        return TryGetClockPartAt(screenPosition, out int partIndex) && TryRotatePart(partIndex);
    }

    // What is called at the beginning of awake()
    public void ResetPuzzle()
    {
        parentCanvas = compassArea != null ? compassArea.GetComponentInParent<Canvas>() : null;

        int partCount = clockParts == null ? 0 : clockParts.Length;
        partLocked = new bool[partCount];
        mustLeaveSolvedAngle = new bool[partCount];
        heldPartIndex = -1;
        unlocked = false;
        AlignmentSoundPlayCount = 0;

        for (int index = 0; index < partCount; index++)
        {
            if (clockParts[index] == null)
                continue;

            clockParts[index].rectTransform.localRotation = Quaternion.Euler(0f, 0f, GetValue(startingZAngles, index, 0f));
        }

        if (door != null)
            closedDoorPosition = door.anchoredPosition;
    }

    // just checks to make sure the clock parts are working as intended
    private bool TryGetClockPartAt(Vector2 screenPosition, out int partIndex)
    {
        partIndex = -1;

        if (compassArea == null || clockParts == null || clockParts.Length == 0)
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

        for (int index = 0; index < clockParts.Length; index++)
        {
            if (clockParts[index] == null)
                continue;

            float innerRadius = GetValue(innerClickRadii, index, 0f);
            float outerRadius = GetValue(outerClickRadii, index, 0f);

            if (normalizedRadius >= innerRadius && normalizedRadius <= outerRadius)
            {
                partIndex = index;
                return true;
            }
        }

        return false;
    }


    private void RotateHeldPart(float deltaTime)
    {
        if (!HasValidPartIndex(heldPartIndex))
        {
            heldPartIndex = -1;
            return;
        }

        RectTransform partTransform = clockParts[heldPartIndex].rectTransform;
        float currentAngle = partTransform.localEulerAngles.z;
        float solvedAngle = GetValue(solvedZAngles, heldPartIndex, 0f);
        float degreesUntilSolved = Mathf.Repeat(solvedAngle - currentAngle, 360f);
        float rotationThisFrame = rotationDegreesPerSecond * Mathf.Max(0f, deltaTime);

        // A part being picked up from its exact solved angle must first leave that angle
        //  otherwise its zero-degree distance would snap it straight
        // back into place before the player sees it move.
        if (mustLeaveSolvedAngle[heldPartIndex])
        {
            mustLeaveSolvedAngle[heldPartIndex] = false;
            partTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle + rotationThisFrame);
            return;
        }

        // Snap precisely to the answer instead of requiring frame-perfect input. 
        // this was the solution I found to stop the angles from being too precise 
        // otherwise I would like "finish" the puzzle but not open the door
        if (degreesUntilSolved <= rotationThisFrame + alignmentSnapDegrees)
        {
            partTransform.localRotation = Quaternion.Euler(0f, 0f, solvedAngle);
            partLocked[heldPartIndex] = true;
            heldPartIndex = -1;
            PlayAlignmentSound();

            if (AreAllPartsAligned())
            {
                unlocked = true;
                doorInteraction.OpenDoor();
            }

            return;
        }
        partTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle + rotationThisFrame);
    }

    // May delete this one later -> checks individual parts instead of checking the whole
    // useful for debugging the raw pieces since I can't tell NESW
    private bool IsPartAligned(int partIndex)
    {
        if (!HasValidPartIndex(partIndex))
            return false;

        float currentAngle = clockParts[partIndex].rectTransform.localEulerAngles.z;
        float solvedAngle = GetValue(solvedZAngles, partIndex, 0f);
        return Mathf.Abs(Mathf.DeltaAngle(currentAngle, solvedAngle)) <= alignmentTolerance;
    }

    // boolean that checks if the clock parts are aligned. Should be called after every movement of the  clock
    private bool AreAllPartsAligned()
    {
        if (clockParts == null || clockParts.Length != 4)
            return false;

        for (int index = 0; index < clockParts.Length; index++)
        {
            if (partLocked == null || !partLocked[index] || !IsPartAligned(index))
                return false;
        }

        return true;
    }

    // function to move the door up so the player can move. May delete later if other person 
    // already built function for door
    /*
    private IEnumerator RaiseDoor()
    {
        if (door == null)
            yield break;

        Vector2 openDoorPosition = closedDoorPosition + Vector2.up * doorRiseDistance;
        float elapsed = 0f;

        while (elapsed < doorRiseDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / doorRiseDuration);
            float easedProgress = progress * progress * (3f - 2f * progress);
            door.anchoredPosition = Vector2.Lerp(closedDoorPosition, openDoorPosition, easedProgress);
            yield return null;
        }

        door.anchoredPosition = openDoorPosition;
    }
    */


    // funcion that plays the sound
    private void PlayAlignmentSound()
    {
        if (alignmentAudioSource == null || alignmentAudioSource.clip == null)
            return;

        alignmentAudioSource.PlayOneShot(alignmentAudioSource.clip);
        AlignmentSoundPlayCount++;
    }

    private bool HasValidPartIndex(int partIndex)
    {
        return clockParts != null &&
               partIndex >= 0 &&
               partIndex < clockParts.Length &&
               clockParts[partIndex] != null &&
               partLocked != null &&
               mustLeaveSolvedAngle != null;
    }

    private static float GetValue(float[] values, int index, float fallback)
    {
        return values != null && index >= 0 && index < values.Length ? values[index] : fallback;
    }
}
