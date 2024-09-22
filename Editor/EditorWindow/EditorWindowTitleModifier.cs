/**
 * Thanks to https://github.com/mob-sakai/MainWindowTitleModifierForUnity
 */

using UnityEditor;

namespace UltimateLazy.Tools.Editor
{
    public class EditorWindowTitleModifier
    {
        private static string _defaultTitle = "{activeScene} - {applicationTitle} - {platform} - {unityVersion} - {gitBranch}";
        private const string kEditorPrefsKey = "tuld_WindowTitle";
        
        [MenuItem("Tools/The Ultimate Lazy Dev/Editor/Update Main Window Title", priority = 2)]
        public static void UpdateWindowTitle()
        {
            EditorApplication.updateMainWindowTitle += UpdateMainWindowTitle;
            EditorApplication.UpdateMainWindowTitle();
            EditorApplication.updateMainWindowTitle -= UpdateMainWindowTitle;
        }
        
        private static void UpdateMainWindowTitle(ApplicationTitleDescriptor applicationTitleDescriptor)
        {
            var title = EditorPrefs.GetString(kEditorPrefsKey,_defaultTitle);
            applicationTitleDescriptor.title = ParseTitle(applicationTitleDescriptor, title);
        }

        private static string ParseTitle(ApplicationTitleDescriptor titleDescriptor, string title)
        {
            return title.Replace("{activeScene}", titleDescriptor.activeSceneName)
                .Replace("{applicationTitle}", titleDescriptor.projectName)
                .Replace("{platform}", titleDescriptor.targetName)
                .Replace("{unityVersion}", titleDescriptor.unityVersion)
                .Replace("{gitBranch}", GitInfoEditor.GitBranch);
        }
    }
}
