using System;
using UnityEngine;

/// <summary>
/// A line of dialogue in a conversation.
/// </summary>
[Serializable]
public class DialogueLine
{
    /// <summary>
    /// What is being said.
    /// </summary>
    [SerializeField]
    public string Line = "";
    /// <summary>
    /// How long the text should remain (including fade in and fade out times).
    /// </summary>
    [SerializeField]
    public float DisplayDuration = 3.0f;
    /// <summary>
    /// How long the text takes to type and fade in / fade out.
    /// </summary>
    [SerializeField]
    public float FadeDuration = 0.5f;
    /// <summary>
    /// The speaker ID for who will be saying this line.
    /// Speaker IDs are assigned in the DialogueManager for a scene.
    /// </summary>
    [SerializeField]
    public string SpeakingCharacter = "DefaultSpeaker";
}