using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using UnityEngine.InputSystem;

namespace Tapestrague.UI
{
    /// <summary>Place one PauseMenu prefab in each gameplay scene.</summary>
    [DisallowMultipleComponent]
    public sealed class PauseMenu : MonoBehaviour
    {
        [Tooltip("Name or full path of the main-menu scene included in the build's Scene List.")]
        [SerializeField] private string mainMenuScene = "MainMenu";
        [SerializeField] private bool pauseGameplay = true;
        private const string VolumeKey = "Tapestrague.MasterVolume";
        private static PauseMenu activeMenu;
        private UIDocument document;
        private PanelSettings panelSettings;
        private VisualElement overlay;
        private Button resumeButton;
        private Slider volumeSlider;
        private VisualElement[] controls;
        private Label volumeValue;
        private Label status;
        private float previousTimeScale;
        private bool previousCursorVisible;
        private CursorLockMode previousCursorLock;
        private bool volumeChanged;
        public bool IsOpen { get; private set; }

        private void OnEnable()
        {
            if (activeMenu != null && activeMenu != this)
            {
                Debug.LogWarning("Only one active PauseMenu is needed per gameplay scene.", this);
                enabled = false;
                return;
            }
            activeMenu = this;
            var layout = Resources.Load<VisualTreeAsset>("PauseMenuLayout");
            var theme = Resources.Load<ThemeStyleSheet>("PauseMenuTheme");
            if (layout == null || theme == null)
            {
                Debug.LogError("PauseMenu UI resources are missing.", this);
                enabled = false;
                return;
            }
            if (document == null)
            {
                panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                panelSettings.name = "Pause Menu Runtime Panel";
                panelSettings.themeStyleSheet = theme;
                panelSettings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
                panelSettings.referenceResolution = new Vector2Int(1280, 720);
                panelSettings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
                panelSettings.match = 0.5f;
                panelSettings.sortingOrder = 1000;
                document = gameObject.AddComponent<UIDocument>();
                document.panelSettings = panelSettings;
            }
            document.enabled = true;
            document.visualTreeAsset = layout;
            var root = document.rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            overlay = root.Q("pause-overlay");
            resumeButton = root.Q<Button>("resume");
            status = root.Q<Label>("status");
            volumeValue = root.Q<Label>("volume-value");
            var slider = root.Q<Slider>("volume");
            volumeSlider = slider;
            controls = new VisualElement[] { resumeButton, root.Q<Button>("main-menu"), slider, root.Q<Button>("quit") };
            // Keyboard controls are handled explicitly below. Suppress default Tab,
            // arrow, Enter and navigation bindings without affecting pointer events.
            root.RegisterCallback<KeyDownEvent>(evt => evt.StopImmediatePropagation(), TrickleDown.TrickleDown);
            root.RegisterCallback<KeyUpEvent>(evt => evt.StopImmediatePropagation(), TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationMoveEvent>(evt => evt.StopImmediatePropagation(), TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationSubmitEvent>(evt => evt.StopImmediatePropagation(), TrickleDown.TrickleDown);
            root.RegisterCallback<NavigationCancelEvent>(evt => evt.StopImmediatePropagation(), TrickleDown.TrickleDown);
            float volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, AudioListener.volume));
            AudioListener.volume = volume;
            slider.SetValueWithoutNotify(volume);
            UpdateVolumeLabel(volume);
            slider.RegisterValueChangedCallback(evt =>
            {
                AudioListener.volume = evt.newValue;
                UpdateVolumeLabel(evt.newValue);
                volumeChanged = true;
            });
            resumeButton.clicked += () => SetOpen(false);
            root.Q<Button>("main-menu").clicked += GoToMainMenu;
            root.Q<Button>("quit").clicked += Quit;
            overlay.style.display = DisplayStyle.None;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || overlay == null) return;
            if (!IsOpen)
            {
                if (keyboard.spaceKey.wasPressedThisFrame) SetOpen(true);
                return;
            }

            int selected = FocusedControlIndex();
            if (keyboard.wKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
            {
                int direction = keyboard.sKey.wasPressedThisFrame ? 1 : -1;
                selected = selected < 0 ? 0 : (selected + direction + controls.Length) % controls.Length;
                controls[selected].Focus();
            }
            if (selected == 2)
            {
                float step = (keyboard.dKey.wasPressedThisFrame ? 0.05f : 0f)
                    - (keyboard.aKey.wasPressedThisFrame ? 0.05f : 0f);
                if (step != 0f) volumeSlider.value = Mathf.Clamp01(volumeSlider.value + step);
            }
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                if (selected == 0) SetOpen(false);
                else if (selected == 1) GoToMainMenu();
                else if (selected == 3) Quit();
            }
        }

        private int FocusedControlIndex()
        {
            var focused = document.rootVisualElement.focusController.focusedElement as VisualElement;
            if (focused == null) return -1;
            for (int i = 0; i < controls.Length; i++)
                if (focused == controls[i] || controls[i].Contains(focused)) return i;
            return -1;
        }
        public void SetOpen(bool open)
        {
            if (overlay == null || IsOpen == open) return;
            IsOpen = open;
            if (open)
            {
                previousTimeScale = Time.timeScale;
                previousCursorVisible = Cursor.visible;
                previousCursorLock = Cursor.lockState;
                if (pauseGameplay) Time.timeScale = 0;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                status.text = "";
                overlay.style.display = DisplayStyle.Flex;
                resumeButton.Focus();
            }
            else
            {
                if (pauseGameplay) Time.timeScale = previousTimeScale;
                Cursor.lockState = previousCursorLock;
                Cursor.visible = previousCursorVisible;
                overlay.style.display = DisplayStyle.None;
                SaveVolume();
            }
        }

        public void GoToMainMenu()
        {
            if (string.IsNullOrWhiteSpace(mainMenuScene) ||
                !Application.CanStreamedLevelBeLoaded(mainMenuScene))
            {
                status.text = "Main menu is not available yet.";
                Debug.LogWarning("Set PauseMenu's Main Menu Scene to a scene included in the build's Scene List.", this);
                return;
            }
            SetOpen(false);
            SaveVolume();
            SceneManager.LoadScene(mainMenuScene);
        }

        public void Quit()
        {
            SetOpen(false);
            SaveVolume();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void UpdateVolumeLabel(float volume) => volumeValue.text = $"{Mathf.RoundToInt(volume * 100)}%";
        private void SaveVolume()
        {
            if (!volumeChanged) return;
            PlayerPrefs.SetFloat(VolumeKey, AudioListener.volume);
            PlayerPrefs.Save();
            volumeChanged = false;
        }
        private void OnApplicationPause(bool paused) { if (paused) SaveVolume(); }
        private void OnApplicationQuit() => SaveVolume();
        private void OnDisable()
        {
            SetOpen(false);
            SaveVolume();
            if (document != null) document.enabled = false;
            if (activeMenu == this) activeMenu = null;
        }
        private void OnDestroy()
        {
            if (panelSettings != null) Destroy(panelSettings);
        }
    }
}
