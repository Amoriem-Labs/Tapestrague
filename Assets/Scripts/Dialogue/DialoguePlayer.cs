using System.Collections.Generic;

public class DialoguePlayer
{
    /// <summary>
    /// What dialogue sequence should play when this interactable is started.
    /// </summary>
    private readonly DialogueSequence _dialogueSequence;

    /// <summary>
    /// The current line being displayed by this interactable.
    /// -1 if no line is being displayed.
    /// </summary>
    private int _currentDisplayedLine = -1;
    
    private List<IDialogueLineDisplayHandler> _lineDisplayHandlers = new();
    private List<IDialogueFinishHandler> _finishHandlers = new();

    public DialoguePlayer(DialogueSequence dialogue)
    {
        _dialogueSequence = dialogue;
    }

    /// <summary>
    /// Displays the next dialogue line and queues the next line to be displayed. If at the end of the sequence, just returns.
    /// </summary>
    private void displayNextLine()
    {
        //Dialogue over.
        if (_currentDisplayedLine >= _dialogueSequence.Lines.Count - 1)
        {
            _currentDisplayedLine = -1;
            foreach (var handler in _finishHandlers)
            {
                handler.OnDialogueFinish();
            }
            return;
        }

        var idx = ++_currentDisplayedLine;
        var line = _dialogueSequence.Lines[idx];
        foreach (var handler in _lineDisplayHandlers)
        {
            handler.OnDialogueLineDisplay(idx, line);
        }
        DialogueManager.INSTANCE.DisplayLine(line, displayNextLine);
    }

    /// <summary>
    /// Starts the sequence for this interactable.
    /// If this interactable is already playing, just returns.
    /// </summary>
    public void StartDialogueSequence()
    {
        //Dialogue is not currently playing.
        if(_currentDisplayedLine < 0)
        {
            displayNextLine();
        }
    }

    public DialoguePlayer AddLineDisplayHandler(IDialogueLineDisplayHandler handler)
    {
        _lineDisplayHandlers.Add(handler);
        return this;
    }

    public DialoguePlayer AddFinishHandler(IDialogueFinishHandler handler)
    {
        _finishHandlers.Add(handler);
        return this;
    }
}