using UnityEditor;

namespace UltimateLazy.Tools.Editor
{
    public class MainWindow : UltimateLazyToolsWindowBase
    {
        protected override string WindowName => "The Ultimate Lazy Tools";
        protected override WindowLayout Layout => WindowLayout.Sidebar;

        [MenuItem("Tools/The Ultimate Lazy Dev/Ultimate Lazy Tools", priority = 2)]
        public static void ShowWindow()
        {
            GetWindow<MainWindow>("The Ultimate Lazy Tools");
        }
    }
}
