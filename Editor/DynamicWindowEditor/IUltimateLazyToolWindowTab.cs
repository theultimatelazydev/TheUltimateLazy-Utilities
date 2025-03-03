namespace UltimateLazy.Tools.Editor
{
    public interface IUltimateLazyToolWindowTab
    {
        /// <summary>
        /// Name of the window this tool belongs to
        /// </summary>
        string WindowName { get; }

        /// <summary>
        /// Name of the tab where this tool will be displayed
        /// </summary>
        string TabName { get; }

        /// <summary>
        /// Specify the parent tab (empty or null for top-level tabs)
        /// </summary>
        string ParentTabName => "";

        /// <summary>
        /// Method to render the UI for this tool
        /// </summary>
        void OnGUI();
    }
}
