using InputSystem;
using InputSystem.InputContexts;

namespace Dialogue.DialogueHandlers
{
    public class DialogueBlockInputHandler : BlockAllInputContext, IDialogueLineDisplayHandler, IDialogueFinishHandler
    {
        public void OnDialogueLineDisplay(int idx, DialogueLine line)
        {
            if (idx != 0) return;
            InputRouter.Push(this);
        }

        public void OnDialogueFinish()
        {
            InputRouter.Remove(this);
        }
    }
}