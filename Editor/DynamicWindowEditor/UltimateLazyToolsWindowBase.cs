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

        protected abstract string WindowName { get; } // Each window specifies its unique name
        protected virtual WindowLayout Layout => WindowLayout.Tabs; // Default to Tabs layout

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
        }

        private void OnEnable()
        {
            RefreshTools();
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

                    // Draw the content of the selected tab
                    GUILayout.Space(10);
                }

                if (toolsByTab.TryGetValue(tabNames[selectedTabIndex], out var toolsInTab))
                {
                    foreach (var tool in toolsInTab)
                    {
                        tool.OnGUI();
                    }
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

                // Sidebar
                EditorGUILayout.BeginVertical(GUILayout.Width(200)); // Adjust width to match Project Settings
                foreach (var (index, tabName) in tabNames.Select((name, idx) => (idx, name)))
                {
                    var style = new GUIStyle("PreferencesSection"); // Unity's built-in style
                    style.alignment = TextAnchor.MiddleLeft;
                    style.fixedHeight = 30; // Adjust the height for a compact view

                    if (index == selectedTabIndex)
                    {
                        style.normal.textColor = Color.white; // Highlight text
                        style.normal.background = MakeTexture(1, 1, new Color(0.22f, 0.36f, 0.53f)); // Custom highlight color
                    }

                    if (GUILayout.Button(tabName, style))
                    {
                        selectedTabIndex = index;
                    }
                }
                EditorGUILayout.EndVertical();

                // Content Area
                EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                if (toolsByTab.TryGetValue(tabNames[selectedTabIndex], out var toolsInTab))
                {
                    foreach (var tool in toolsInTab)
                    {
                        tool.OnGUI();
                    }
                }
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
