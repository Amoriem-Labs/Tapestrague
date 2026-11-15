using System.Collections;
using System.Collections.Generic;
using Dialogue.DialogueHandlers;
using UnityEngine;

/// <summary>
/// An object that can initiate a DialogueSequence.
/// </summary>
public class TestDialogueInteractable : MonoBehaviour
{
    [SerializeField]
    private DialogueSequence dialogueSequence;
    
    private DialoguePlayer _dialoguePlayer;

    void Start()
    {
        var blockInputHandler = new DialogueBlockInputHandler();

        _dialoguePlayer =
            new DialoguePlayer(dialogueSequence)
                .AddLineDisplayHandler(blockInputHandler)
                .AddFinishHandler(blockInputHandler);
        StartCoroutine(startTestDialogue());
    }

    //TODO: TESTING REMOVE:
    IEnumerator startTestDialogue()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("START DIALOGUE");
        _dialoguePlayer.StartDialogueSequence();
    }
}
