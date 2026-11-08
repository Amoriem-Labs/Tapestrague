PAUSE MENU

Drag UI/Prefabs/PauseMenu.prefab into each gameplay scene, then save the scene.
Keep its GameObject active: the UI starts hidden and Esc opens/closes it.
Resume also closes it. Quit exits a built game or stops Play mode in the Editor.

Set Main Menu Scene on the prefab instance to your main-menu scene name/path,
and include that scene in File > Build Profiles > Scene List. This repository
does not yet contain a main-menu scene. Until configured, the button displays
"Main menu is not available yet" instead of interrupting gameplay.

Volume changes AudioListener.volume (all listener audio) and persists locally
on close/disable/quit. This does not change the project's audio assets.

Pause Gameplay freezes scaled time and restores its previous value on close.
Custom gameplay scripts that use unscaled time/input should check IsOpen.
The cursor is released while open and its prior state is restored on close.
Use one menu per scene. No extra EventSystem is needed for UI Toolkit alone;
mixed uGUI scenes should use their existing InputSystemUIInputModule.

Edit appearance in Resources/PauseMenuLayout.uxml and PauseMenuStyle.uss.
The prefab builds its UI document at runtime, so preview it in Play mode.
W/S navigates controls; A/D lowers/raises a focused volume slider in 5% steps. Space activates the selected button (select Resume to close). Mouse clicks and dragging still work. Esc toggles the menu. Tab, Enter and arrow-key menu controls are disabled. These bindings apply to this menu; gameplay input is configured separately.
