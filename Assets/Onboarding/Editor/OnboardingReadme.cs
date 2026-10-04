using UnityEditor;
using UnityEngine;

public sealed class OnboardingReadme : ScriptableObject
{
    public string title;
}

[InitializeOnLoad]
internal sealed class OnboardingReadmeWindow : EditorWindow
{
    private const string ReadmePath = "Assets/Onboarding/Readme.asset";
    private const string SessionKey = "Tapestrague.Onboarding.WindowStartupHandled";
    private const string CodingGuidelinesUrl = "https://docs.google.com/document/d/1C47Qn0jPrjnpoEh3vSjEQ4_ohW9fnNKQT6m74o58FQY/edit?usp=sharing";
    private static string PreferenceKey => "Tapestrague.Onboarding.Hide:" + Application.dataPath;
    private string currentPage;
    private Vector2 scrollPosition;
    [SerializeField] private OnboardingReadme readme;

    static OnboardingReadmeWindow()
    {
        if (!Application.isBatchMode && !SessionState.GetBool(SessionKey, false))
            EditorApplication.update += ShowOnStartup;
    }

    private static void ShowOnStartup()
    {
        // Asset loading must wait until startup compilation and import finish.
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        EditorApplication.update -= ShowOnStartup;
        SessionState.SetBool(SessionKey, true);

        if (EditorPrefs.GetBool(PreferenceKey, false) || EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        EditorApplication.delayCall += ShowReadme;
    }

    [MenuItem("Window/Onboarding README")]
    private static void ShowReadme()
    {
        var asset = AssetDatabase.LoadAssetAtPath<OnboardingReadme>(ReadmePath);
        if (asset == null)
            return;

        var window = GetWindow<OnboardingReadmeWindow>(false, "Onboarding README", true);
        window.minSize = new Vector2(440, 320);
        window.readme = asset;
        window.currentPage = null;
        window.scrollPosition = Vector2.zero;
        window.Repaint();
    }

    private void OnGUI()
    {
        using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
        {
            scrollPosition = scrollView.scrollPosition;
            DrawReadme();
        }
    }

    private void DrawReadme()
    {
        if (readme == null)
            return;

        var titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 18, wordWrap = true };
        var buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14, fixedHeight = 40 };
        if (currentPage != null)
        {
            EditorGUILayout.LabelField(currentPage == "Onboarding" ? "Add Your Name Sprite" : currentPage, titleStyle);
            EditorGUILayout.Space();
            if (currentPage == "Onboarding")
            {
                EditorGUILayout.LabelField(
                    "1. Open the Collaborators folder through Assets/Art/Sprites/Collaborators or this link:",
                    EditorStyles.wordWrappedLabel);
                if (GUILayout.Button("Open and select Collaborators in Unity", buttonStyle))
                {
                    EditorApplication.delayCall += () =>
                    {
                        var folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/Art/Sprites/Collaborators");
                        if (folder == null)
                            return;

                        Selection.activeObject = folder;
                    };
                }
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "2. Find the name sprite you want to add in your OS File Explorer.\n\n" +
                    "3. Drag the name sprite from the File Explorer into the Unity editor and drop it onto/into the Collaborators folder.\n\n" +
                    "4. Add the new file (GitHub Desktop automatically adds it) and commit the changes to your branch.\n" +
                    "    • The commit message should be “upload name sprite ” + your name, e.g., \"upload name sprite Zhongye\".\n" +
                    "    • Please confirm that the sprite file and its .meta file are the only modifications you have made.\n\n" +
                    "5. Push the branch to GitHub.\n\n" +
                    "6. Open a pull request into main. There should be a button \"make pull request\" in your GitHub Desktop.\n\n" +
                    "7. You are all set.",
                    EditorStyles.wordWrappedLabel);
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Please follow the following structure unless otherwise specified.\n\n" +
                    "• Art (All art assets go here)\n" +
                    "    • Sprites (All 2D sprite assets go here)\n\n" +
                    "• Audio (All sound assets go here)\n\n" +
                    "• Resources\n" +
                    "    • Prefabs (All prefabs go here)\n" +
                    "    • Data (Almost all scriptable objects go here)\n\n" +
                    "• Scenes (All Unity scenes go here)\n\n" +
                    "• Scripts (All C# scripts go here)",
                    EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("Return", buttonStyle))
                Navigate(null);
            return;
        }

        EditorGUILayout.LabelField(readme.title, titleStyle);
        EditorGUILayout.Space();
        if (GUILayout.Button("Onboarding", buttonStyle))
            Navigate("Onboarding");
        if (GUILayout.Button("Coding Guidelines", buttonStyle))
            Application.OpenURL(CodingGuidelinesUrl);
        if (GUILayout.Button("Folder Structure", buttonStyle))
            Navigate("Folder Structure");

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        bool close = GUILayout.Button("Close", buttonStyle);
        bool hide = GUILayout.Button("Don't Show Again", buttonStyle);
        EditorGUILayout.EndHorizontal();

        if (hide)
            EditorPrefs.SetBool(PreferenceKey, true);
        if (close || hide)
            EditorApplication.delayCall += Close;
    }

    private void Navigate(string page)
    {
        // Keep the layout unchanged until the current GUI event has finished.
        EditorApplication.delayCall += () =>
        {
            if (this == null)
                return;

            currentPage = page;
            scrollPosition = Vector2.zero;
            Repaint();
        };
    }
}
