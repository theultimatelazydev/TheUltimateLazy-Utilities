using UnityEditor;

namespace UltimateLazy.Tools.Editor
{
    public class EditorWindowTitleModifierWindow : UltimateLazyToolsWindowBase
    {
        protected override string WindowName => "Editor Window Title Modifier";

        [MenuItem("Tools/The Ultimate Lazy Dev/Editor/Editor Window Title Modifier", priority = 2)]
        public static void ShowWindow()
        {
            GetWindow<EditorWindowTitleModifierWindow>("Editor Window Title Modifier");
        }
    }
}
