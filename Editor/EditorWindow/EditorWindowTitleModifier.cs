/**
 * Thanks to https://github.com/mob-sakai/MainWindowTitleModifierForUnity
 */

using UnityEditor;
using UnityEngine;

namespace UltimateLazy.Tools.Editor
{
    [InitializeOnLoad]
    public class EditorWindowTitleModifier : IUltimateLazyToolWindowTab
    {
        private static bool _autoUpdate = true;

        private const string _defaultTitle =
            "{activeScene} - {applicationTitle} - {platform} - {unityVersion} - {gitBranch}";
        private static string _title = string.Empty;
        private const string kEditorPrefsKey = "tuld_WindowTitle";

        public string WindowName => "The Ultimate Lazy Tools";
        public string TabName => "Editor Window Title Modifier";

        [MenuItem(
            "Tools/The Ultimate Lazy Dev/Unity Editor/Editor Window Title Modifier",
            priority = 2
        )]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<MainWindow>();
            window.ChangeTab("Editor Window Title Modifier");
        }

        static EditorWindowTitleModifier()
        {
            _title = EditorPrefs.GetString(kEditorPrefsKey, _defaultTitle);

            if (_autoUpdate)
            {
                EditorApplication.update += UpdateWindowTitle;
            }
        }

        public void OnGUI()
        {
            _title = EditorGUILayout.TextField($"Window Title", _title);

            if (GUILayout.Button("Reset to Default"))
            {
                _title = _defaultTitle;
                GUI.FocusControl(null);
                //Repaint();
                SaveTitle();
                UpdateWindowTitle();
            }

            if (GUILayout.Button("Update Window Title"))
            {
                GUI.FocusControl(null);
                //Repaint();
                SaveTitle();
                UpdateWindowTitle();
            }
        }

        private static void SaveTitle()
        {
            EditorPrefs.SetString(kEditorPrefsKey, _title);
        }

        //[MenuItem("Tools/The Ultimate Lazy Dev/Editor/Update Main Window Title", priority = 2)]
        public static void UpdateWindowTitle()
        {
            EditorApplication.updateMainWindowTitle += UpdateMainWindowTitle;
            EditorApplication.UpdateMainWindowTitle();
            EditorApplication.updateMainWindowTitle -= UpdateMainWindowTitle;
        }

        private static void UpdateMainWindowTitle(
            ApplicationTitleDescriptor applicationTitleDescriptor
        )
        {
            applicationTitleDescriptor.title = ParseTitle(applicationTitleDescriptor, _title);
        }

        private static string ParseTitle(ApplicationTitleDescriptor titleDescriptor, string title)
        {
            return title
                .Replace("{activeScene}", titleDescriptor.activeSceneName)
                .Replace("{applicationTitle}", titleDescriptor.projectName)
                .Replace("{platform}", titleDescriptor.targetName)
                .Replace("{unityVersion}", titleDescriptor.unityVersion)
                .Replace("{gitBranch}", GitInfoEditor.GitBranch);
        }
    }
}
