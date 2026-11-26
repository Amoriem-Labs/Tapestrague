using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InputSystem.InputContexts;
using PuzzleSystem;
using UnityEngine;
using UnityEngine.InputSystem.iOS;
using UnityEngine.UI;


// Author: Nathan Burns
// Controls the four-part clock puzzle in First_Puzzle.
// The parts are ordered from the outer ring to the inner disc.
// It also should be as intereactable as possible - ie it can work from Unity
//
// This component only owns the puzzle state: rotating the held part and detecting
// when every part is aligned. Deciding which part the player is pointing at lives in
// CompassPuzzleInput, which calls TryRotatePart / ReleasePart.
public sealed class CompassPuzzle : MonoBehaviour, IExplorationClickable
{
    [Header("Clock Parts (outer to inner)")]
    [SerializeField] private Transform[] clockParts = new Transform[4];
    [SerializeField] private float[] startingZAngles = { 90f, 180f, 270f, 90f };
    [SerializeField] private float[] solvedZAngles = { 0f, 0f, 0f, 0f };
    private List<float> _radii = new List<float>();

    [Header("Rotation")]
    [SerializeField, Min(1f)] private float rotationDegreesPerSecond = 180f;
    [SerializeField, Min(0f)] private float alignmentSnapDegrees = 3f;
    [SerializeField, Min(0f)] private float alignmentTolerance = 0.5f;

    [Header("Completion")]
    [SerializeField] private AudioSource alignmentAudioSource;
    [SerializeField] private RectTransform door;
    [SerializeField] private float doorRiseDistance = 250f;
    [SerializeField, Min(0.01f)] private float doorRiseDuration = 0.75f;

    private bool[] partLocked;
    private bool[] mustLeaveSolvedAngle;
    private Vector2 closedDoorPosition;
    private int heldPartIndex = -1;
    private bool unlocked;
    private IPuzzleInitializeHandler[] _puzzleInitializeHandlers;
    private IPuzzleStartHandler[] _startHandlers;
    private IPuzzleFinishHandler[] _finishHandlers;

    // private DoorInteraction doorInteraction;

    public bool IsUnlocked => unlocked;
    public int PartCount => clockParts == null ? 0 : clockParts.Length;
    public int HeldPartIndex => heldPartIndex;
    public int AlignmentSoundPlayCount { get; private set; }

    private void Awake()
    {
        InitializePuzzle();
        // // called at the start to set the clock to the designated degrees
        // // as specified in the unity editor. Made to be flexible and not hardcoded
        // ResetPuzzle();
        // // doorInteraction = door.GetComponent<DoorInteraction>();

        _radii = clockParts.Select(c =>
        {
            var sprite = c.GetComponent<SpriteRenderer>();
            Vector2 localSize = sprite.sprite.bounds.size;
            Vector3 scale = sprite.transform.lossyScale;
            // average radius
            return (localSize.x * scale.x + localSize.y * scale.y) / 4;
        }).ToList();
    }


    private void Update()
    {
        if (unlocked || heldPartIndex < 0)
            return;

        RotateHeldPart(Time.deltaTime);
    }

    #region Input Handling

    public int TryGetPartIndexAtWorldPosition(Vector3 worldPosition)
    {
        var offset = worldPosition - transform.position;
        var dist = offset.magnitude;
        for (int i = 0; i < _radii.Count; i++)
        {
            var radius = _radii[i];
            // click point outside radius, -1 if outside and disk
            if (radius < dist) return i - 1;
        }

        return _radii.Count - 1;
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

    // Stops rotating whichever part is currently held (e.g. the player let go)
    public void ReleasePart()
    {
        heldPartIndex = -1;
    }

    #endregion

    #region Generic Puzzle Logic

    private void InitializePuzzle()
    {
        _puzzleInitializeHandlers = GetComponentsInChildren<IPuzzleInitializeHandler>();
        _startHandlers = GetComponentsInChildren<IPuzzleStartHandler>();
        _finishHandlers = GetComponentsInChildren<IPuzzleFinishHandler>();
        
        // TODO: shouldn't reset everytime the puzzle is initialized
        //  load from player save in the future
        ResetPuzzle();
        
        foreach (var handler in _puzzleInitializeHandlers)
        {
            handler.OnPuzzleInitialize(IsUnlocked);
        }
    }
    
    // What is called at the beginning of awake()
    private void ResetPuzzle()
    {
        int partCount = PartCount;
        partLocked = new bool[partCount];
        mustLeaveSolvedAngle = new bool[partCount];
        heldPartIndex = -1;
        unlocked = false;
        AlignmentSoundPlayCount = 0;

        for (int index = 0; index < partCount; index++)
        {
            if (clockParts[index] == null)
                continue;

            GetPartTransform(index).localRotation = Quaternion.Euler(0f, 0f, GetValue(startingZAngles, index, 0f));
        }
    }

    public void StartPuzzle()
    {
        if (AreAllPartsAligned()) return;
        
        Debug.Log("Puzzle start");
        foreach (var handler in _startHandlers)
        {
            handler.OnPuzzleStart();
        }
    }

    public void FinishPuzzle()
    {
        Debug.Log("Puzzle finish");
        foreach (var handler in _finishHandlers)
        {
            handler.OnPuzzleFinish(IsUnlocked);
        }
    }
    
    private void PuzzleSolved()
    {
        unlocked = true;
        FinishPuzzle();
    }

    #endregion

    #region Compass Puzzle Logic

    private void RotateHeldPart(float deltaTime)
    {
        if (!HasValidPartIndex(heldPartIndex))
        {
            heldPartIndex = -1;
            return;
        }

        Transform partTransform = GetPartTransform(heldPartIndex);
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
            Debug.Log("Aligned");
            // PlayAlignmentSound();

            if (AreAllPartsAligned())
            {
                PuzzleSolved();
            }

            return;
        }
        partTransform.localRotation = Quaternion.Euler(0f, 0f, currentAngle + rotationThisFrame);
    }

    // May delete this one later -> checks individual parts instead of checking the whole
    // useful for debugging the raw pieces since I can't tell NESW
    public bool IsPartAligned(int partIndex)
    {
        if (!HasValidPartIndex(partIndex))
            return false;

        float currentAngle = GetPartTransform(partIndex).localEulerAngles.z;
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

    // The only place that cares what a clock part is made of. Rotation works on the
    // Transform, so swapping Image for SpriteRenderer only needs the array type changed.
    private Transform GetPartTransform(int partIndex)
    {
        return clockParts[partIndex].transform;
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

    #endregion

    public void Click()
    {
        Debug.Log("Clicked");
        StartPuzzle();
    }
}
