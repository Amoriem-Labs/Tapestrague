using TMPro;
using UnityEngine;

/// <summary>
/// A DialogueSpeaker is used to give certain voices specific text properties in DialogueSequences.
/// </summary>
public class DialogueSpeaker : MonoBehaviour
{
    /// <summary>
    /// The font that this speaker will use.
    /// </summary>
    [Header("Display Properties")]
    [SerializeField]
    private TMP_FontAsset dialogueFont;
    /// <summary>
    /// Used to specify where the dialogue text for this speaker will appear in relation to the speaker's center.
    /// </summary>
    [SerializeField]
    private Vector3 dialogueDisplayOffset = new Vector3(0, 1, 0);
    /// <summary>
    /// The size of font this speaker will use.
    /// </summary>
    [SerializeField]
    private float fontSize = 4;
    /// <summary>
    /// The color of font this speaker will use.
    /// </summary>
    [SerializeField]
    private Color dialogueColor = Color.white;

    /// <summary>
    /// Returns the font color this speaker uses.
    /// </summary>
    /// <returns></returns>
    public TMP_FontAsset GetDialogueFont() { return dialogueFont; }

    /// <summary>
    /// Returns the position that dialogue should appear at for this speaker.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetDialogueDisplayPosition() { return transform.position + dialogueDisplayOffset; }

    /// <summary>
    /// Returns the font size this speaker uses.
    /// </summary>
    /// <returns></returns>
    public float GetFontSize() { return fontSize; }

    /// <summary>
    /// Returns the font color this speaker uses.
    /// </summary>
    /// <returns></returns>
    public Color GetColor() { return dialogueColor; }
}
