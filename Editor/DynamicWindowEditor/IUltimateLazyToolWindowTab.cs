namespace UltimateLazy.Tools.Editor
{
    public interface IUltimateLazyToolWindowTab
    {
        string WindowName { get; } // Name of the window this tool belongs to
        string TabName { get; } // Name of the tab where this tool will be displayed
        void OnGUI(); // Method to render the UI for this tool
    }
}
