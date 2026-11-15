using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An object that can initiate a DialogueSequence.
/// </summary>
public class DialogueInteractable : MonoBehaviour
{
    /// <summary>
    /// What dialogue sequence should play when this interactable is started.
    /// </summary>
    [SerializeField]
    private DialogueSequence dialogueSequence;

    /// <summary>
    /// The current line being displayed by this interactable.
    /// -1 if no line is being displayed.
    /// </summary>
    private int currentDisplayedLine = -1;

    void Start()
    {
        //TODO: Testing, remove.
        StartCoroutine(startTestDialogue());
    }

    //TODO: TESTING REMOVE:
    IEnumerator startTestDialogue()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("START DIALOGUE");
        StartDialogueSequence();
    }

    /// <summary>
    /// Starts the sequence for this interactable.
    /// If this interactable is already playing, just returns.
    /// </summary>
    public void StartDialogueSequence()
    {
        //Dialogue is not currently playing.
        if(currentDisplayedLine < 0)
        {
            displayNextLine();
        }
    }
    
    /// <summary>
    /// Displays the next dialogue line and queues the next line to be displayed. If at the end of the sequence, just returns.
    /// </summary>
    protected void displayNextLine()
    {
        //Dialogue over.
        if (currentDisplayedLine >= dialogueSequence.Lines.Count - 1)
        {
            currentDisplayedLine = -1;
            return;
        }

        DialogueManager.INSTANCE.DisplayLine(dialogueSequence.Lines[++currentDisplayedLine], displayNextLine);
    }
}
