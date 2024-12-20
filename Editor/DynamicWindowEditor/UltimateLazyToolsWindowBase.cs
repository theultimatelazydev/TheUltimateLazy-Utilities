using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UltimateLazy.Tools.Editor
{
    public enum WindowLayout
    {
        Tabs,
        Sidebar,
    }

    public abstract class UltimateLazyToolsWindowBase : EditorWindow
    {
        private Dictionary<string, List<IUltimateLazyToolWindowTab>> toolsByTab;
        private string[] tabNames;
        private int selectedTabIndex = 0;
        private string initialTabName;

        private Vector2 sidebarScrollPosition = Vector2.zero;
        private Vector2 contentScrollPosition = Vector2.zero;

        protected abstract string WindowName { get; } // Each window specifies its unique name
        protected virtual WindowLayout Layout => WindowLayout.Tabs; // Default to Tabs layout
        protected virtual float MinWidth => 400f; // Default minimum width
        protected virtual float MinHeight => 300f; // Default minimum height

        private float sidebarWidth = 200f; // Initial width of the sidebar
        private bool isResizing = false; // Tracks if the user is resizing
        private float resizeHandleWidth = 5f; // Width of the draggable area

        public void ChangeTab(string tabName)
        {
            initialTabName = tabName;

            // Refresh the tools if already initialized
            if (toolsByTab != null && tabNames != null)
            {
                selectedTabIndex = Array.IndexOf(tabNames, initialTabName);
                if (selectedTabIndex == -1)
                {
                    Debug.LogWarning(
                        $"Tab '{initialTabName}' not found. Defaulting to the first tab."
                    );
                    selectedTabIndex = 0;
                }

                // Force a repaint to update the UI
                Repaint();
            }
        }

        public void RefreshTools()
        {
            // Find all tools implementing IUltimateLazyToolWindowTab
            var toolTypes = AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(IUltimateLazyToolWindowTab).IsAssignableFrom(type)
                    && !type.IsInterface
                    && !type.IsAbstract
                );

            // Filter tools for the specific window
            toolsByTab = new Dictionary<string, List<IUltimateLazyToolWindowTab>>();
            foreach (var type in toolTypes)
            {
                var toolInstance = (IUltimateLazyToolWindowTab)Activator.CreateInstance(type);
                if (toolInstance.WindowName != WindowName)
                    continue; // Skip tools not belonging to this window

                if (string.IsNullOrEmpty(toolInstance.TabName))
                    continue; // Skip tools that don't have a tab

                if (!toolsByTab.ContainsKey(toolInstance.TabName))
                {
                    toolsByTab[toolInstance.TabName] = new List<IUltimateLazyToolWindowTab>();
                }
                toolsByTab[toolInstance.TabName].Add(toolInstance);
            }

            // Cache tab names for use in GUI
            tabNames = toolsByTab.Keys.ToArray();

            // Set the initial tab if specified
            if (!string.IsNullOrEmpty(initialTabName))
            {
                selectedTabIndex = Array.IndexOf(tabNames, initialTabName);
                if (selectedTabIndex == -1)
                {
                    Debug.LogWarning(
                        $"Tab '{initialTabName}' not found. Defaulting to the first tab."
                    );
                    selectedTabIndex = 0;
                }
            }
        }

        private void OnEnable()
        {
            RefreshTools();

            // Apply minimum size constraints
            minSize = new Vector2(MinWidth, MinHeight);
        }

        private void OnGUI()
        {
            if (toolsByTab == null || tabNames == null)
            {
                RefreshTools(); // Refresh if not initialized
            }

            if (Layout == WindowLayout.Tabs)
            {
                DrawTabsLayout();
            }
            else if (Layout == WindowLayout.Sidebar)
            {
                DrawSidebarLayout();
            }
        }

        private void DrawTabsLayout()
        {
            if (tabNames.Length > 0)
            {
                if (tabNames.Length > 1)
                {
                    selectedTabIndex = GUILayout.Toolbar(
                        selectedTabIndex,
                        tabNames,
                        GUILayout.Height(25)
                    );

                    // Add space between toolbar and content
                    GUILayout.Space(10);
                }

                if (toolsByTab.TryGetValue(tabNames[selectedTabIndex], out var toolsInTab))
                {
                    DrawContent(toolsInTab); // Use the reusable method
                }
            }
            else
            {
                EditorGUILayout.LabelField(
                    $"No tools found for {WindowName}!",
                    EditorStyles.boldLabel
                );
            }
        }

        private void DrawSidebarLayout()
        {
            if (tabNames.Length > 0)
            {
                EditorGUILayout.BeginHorizontal();

                // Sidebar with Scroll
                EditorGUILayout.BeginVertical(GUILayout.Width(sidebarWidth));
                sidebarScrollPosition = EditorGUILayout.BeginScrollView(
                    sidebarScrollPosition,
                    GUILayout.ExpandHeight(true)
                );

                foreach (var (index, tabName) in tabNames.Select((name, idx) => (idx, name)))
                {
                    var style = new GUIStyle("PreferencesSection");
                    style.alignment = TextAnchor.MiddleLeft;
                    style.fixedHeight = 30;

                    if (index == selectedTabIndex)
                    {
                        style.normal.textColor = Color.white;
                        style.normal.background = MakeTexture(1, 1, new Color(0.22f, 0.36f, 0.53f));
                    }

                    if (GUILayout.Button(tabName, style))
                    {
                        selectedTabIndex = index;
                    }
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                // Draw a thin black line as the divider
                var dividerRect = new Rect(sidebarWidth, 0, 1, position.height);
                EditorGUI.DrawRect(dividerRect, Color.black); // Thin black line for the divider

                // Invisible resizing handle
                var resizeHandleRect = new Rect(sidebarWidth - 2, 0, 5, position.height);
                EditorGUIUtility.AddCursorRect(resizeHandleRect, MouseCursor.ResizeHorizontal);

                HandleSidebarResizing(resizeHandleRect);

                // Content Area with Scroll
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                contentScrollPosition = EditorGUILayout.BeginScrollView(
                    contentScrollPosition,
                    GUILayout.ExpandHeight(true)
                );

                if (toolsByTab.TryGetValue(tabNames[selectedTabIndex], out var toolsInTab))
                {
                    DrawContent(toolsInTab);
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField(
                    $"No tools found for {WindowName}!",
                    EditorStyles.boldLabel
                );
            }
        }

        private void HandleSidebarResizing(Rect resizeHandleRect)
        {
            Event e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (resizeHandleRect.Contains(e.mousePosition))
                    {
                        isResizing = true;
                        e.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (isResizing)
                    {
                        sidebarWidth += e.delta.x;
                        sidebarWidth = Mathf.Clamp(sidebarWidth, 100f, position.width - 100f); // Prevent extreme sizes
                        e.Use();
                        Repaint();
                    }
                    break;

                case EventType.MouseUp:
                    if (isResizing)
                    {
                        isResizing = false;
                        e.Use();
                    }
                    break;
            }
        }

        private void DrawContent(List<IUltimateLazyToolWindowTab> toolsInTab)
        {
            contentScrollPosition = EditorGUILayout.BeginScrollView(
                contentScrollPosition,
                GUILayout.ExpandHeight(true)
            );

            for (int i = 0; i < toolsInTab.Count; i++)
            {
                toolsInTab[i].OnGUI();

                // Draw divider only if it's not the last tool
                if (i < toolsInTab.Count - 1)
                {
                    DrawDivider();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawDivider()
        {
            GUILayout.Space(5);
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f, 1f)); // Dark gray divider line
            GUILayout.Space(5);
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height);
            var pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}
