using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using VHierarchy.Libs;


/// <summary>
/// Controls the execution of dialogue sequences.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    //Singleton.
    public static DialogueManager INSTANCE = null;

    /// <summary>
    /// Refers a speaker to a given ID.
    /// </summary>
    [Serializable]
    class ID_DialogueSpeaker
    {
        [SerializeField]
        public string ID;
        [SerializeField]
        public DialogueSpeaker Speaker;
    }

    /// <summary>
    /// A list of DialogueSpeakers and their corresponding IDs.
    /// </summary>
    [SerializeField]
    private List<ID_DialogueSpeaker> speakers = new List<ID_DialogueSpeaker>();
    //(apparently you can't serialize Dictionary in Unity.)

    /// <summary>
    /// Returns the speaker with the specified ID.
    /// </summary>
    /// <param name="speakerID">The ID of the speaker.</param>
    /// <returns></returns>
    public DialogueSpeaker GetSpeaker(string speakerID)
    {
        foreach(ID_DialogueSpeaker speaker in speakers)
        {
            if(speaker.ID == speakerID) { return speaker.Speaker; }
        }

        Debug.LogWarning($"Attempted to retrieve a non-existant DialogueSpeaker. (id: {speakerID})");
        return null;
        
    }

    /// <summary>
    /// A currently displayed line of dialogue in the scene.
    /// </summary>
    class DisplayedDialogue
    {
        /// <summary>
        /// The object holding the text that is shown.
        /// </summary>
        public readonly GameObject Object;
        /// <summary>
        /// How long the line has been displayed for.
        /// </summary>
        public float Lifetime { get; private set; }
        /// <summary>
        /// The line that is being displayed.
        /// </summary>
        public readonly DialogueLine Line;
        /// <summary>
        /// A function that should be called after the dialogue has finished displaying.
        /// </summary>
        public readonly Action FinishDisplayCallback;

        /// <summary>
        /// Increases the lifetime of the dialogue by the given amount.
        /// Also updates the display of the text to match the new lifetime.
        /// </summary>
        /// <param name="lifetime">How much to add to the lifetime.</param>
        public void AddLifetime(float lifetime)
        {
            Lifetime += lifetime;

            if (Lifetime <= Line.FadeDuration)
            {
                TextMeshPro text = Object.GetComponent<TextMeshPro>();
                text.alpha = Lifetime / Line.FadeDuration;

                int displayedLength = Mathf.CeilToInt((Lifetime / Line.FadeDuration) * (float)Line.Line.Length);
                text.text = Line.Line.Substring(0, displayedLength);
            }

            float fadeOutTime = (Line.DisplayDuration - Line.FadeDuration);
            if (Lifetime >= fadeOutTime)
            {
                TextMeshPro text = Object.GetComponent<TextMeshPro>();
                text.alpha = 1.0f - ((Lifetime - fadeOutTime) / Line.FadeDuration);
            }
        }

        public DisplayedDialogue(GameObject obj /*Should have a TextMeshPro component*/, DialogueLine line, Action finishDisplayCallback)
        {
            Object = obj;
            Lifetime = 0.0f;
            Line = line;
            FinishDisplayCallback = finishDisplayCallback;
            obj.GetComponent<TextMeshPro>().text = ""; //So that the text can be typed out over time.
        }

        /// <summary>
        /// Destroys the object being displayed and calls the FinishDisplayCallback.
        /// </summary>
        internal void Remove()
        {
            FinishDisplayCallback?.Invoke();
            this.Object.Destroy();
        }
    }

    /// <summary>
    /// Any dialogue that is currently being shown.
    /// </summary>
    private List<DisplayedDialogue> displayedDialogue = new List<DisplayedDialogue>();

    void Start()
    {
        //Singleton.
        if (INSTANCE) { Destroy(this); }

        INSTANCE = this;
    }

    void Update()
    {
        //Update the lifetime for all displayed dialogue.
        for(int i = 0; i < displayedDialogue.Count; i++)
        {
            displayedDialogue[i].AddLifetime(Time.deltaTime);

            if (displayedDialogue[i].Lifetime >= displayedDialogue[i].Line.DisplayDuration)
            {
                displayedDialogue[i].Remove();
                displayedDialogue.RemoveAt(i--);
            }
        }
    }

    /// <summary>
    /// Display a line of dialogue with the given callback (or null) when finished.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="finishDisplayCallback"></param>
    public void DisplayLine(DialogueLine line, Action finishDisplayCallback = null)
    {
        DisplayLine(line, GetSpeaker(line.SpeakingCharacter), finishDisplayCallback);
    }

    public void DisplayLine(DialogueLine line, DialogueSpeaker speaker, Action finishDisplayCallback = null) { DisplayLine(line, speaker, speaker.GetDialogueDisplayPosition(), speaker.transform, finishDisplayCallback); }
    public void DisplayLine(DialogueLine line, DialogueSpeaker speaker, Vector3 position, Transform parent = null, Action finishDisplayCallback = null)
    {
        GameObject dialogueText = new GameObject(parent ? "Dialogue_" + parent.gameObject.name : "Dialogue");
        TextMeshPro text = dialogueText.AddComponent<TextMeshPro>();
        text.text = "";
        text.font = speaker.GetDialogueFont();
        text.fontSize = speaker.GetFontSize();
        text.color = speaker.GetColor();
        text.alignment = TextAlignmentOptions.Center;

        dialogueText.transform.SetParent(parent ? parent : transform);
        dialogueText.transform.position = position;

        displayedDialogue.Add(new DisplayedDialogue(dialogueText, line, finishDisplayCallback));
    }

}
