using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A sequence of DialogueLines that represent a conversation at some point in the game.
/// </summary>
[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    /// <summary>
    /// Dialogue lines in the order they will be said.
    /// </summary>
    public List<DialogueLine> Lines = new List<DialogueLine>();
}
