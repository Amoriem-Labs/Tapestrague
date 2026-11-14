/*
Tapestrague
Xinzi Huang Nov8 2026
open and close UI (blank placeholder right now) when press escape
*/

using Unity.VisualScripting;
using UnityEngine;

public class EscapeMenuToggle : MonoBehaviour
{
    [SerializeField] private GameObject _escapeMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        _escapeMenu.SetActive(!_escapeMenu.activeSelf);
    }
}
