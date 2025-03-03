using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
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
        private DynamicTabDictionary _tabs;
        private List<string> _parentTabs;
        private Dictionary<string, DynamicTabDictionary> _childTabsByParent;
        private Dictionary<string, bool> _foldoutStates = new Dictionary<string, bool>();

        private TreeViewState _treeViewState;
        private UltimateLazyDynamicTreeView _treeView;

        private IUltimateLazyToolWindowTab _selectedTab;
        private string _selectedToolName;

        private List<string> _tabNames;
        private int _selectedTabIndex;
        private string _initialTabName;

        private Vector2 _sidebarScrollPosition;
        private Vector2 _contentScrollPosition;

        protected abstract string WindowName { get; }
        protected virtual WindowLayout Layout => WindowLayout.Tabs;
        protected virtual float MinWidth => 400f;
        protected virtual float MinHeight => 300f;

        private float _sidebarWidth = 200f;
        private bool _isResizing;
        private float _resizeHandleWidth = 5f;

        public void ChangeTab(string tabName)
        {
            _initialTabName = tabName;

            if (_tabs != null && _tabNames != null)
            {
                _selectedTabIndex = _tabNames.IndexOf(_initialTabName);
                if (_selectedTabIndex == -1)
                {
                    Debug.LogWarning(
                        $"Tab '{_initialTabName}' not found. Defaulting to the first tab."
                    );
                    _selectedTabIndex = 0;
                }

                Repaint();
            }
        }

        public void RefreshTabs()
        {
            var tabsTypes = AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(IUltimateLazyToolWindowTab).IsAssignableFrom(type)
                    && !type.IsInterface
                    && !type.IsAbstract
                );

            _tabNames = new List<string>();
            _childTabsByParent = new Dictionary<string, DynamicTabDictionary>();
            _tabs = new DynamicTabDictionary();
            _parentTabs = new List<string>();

            foreach (var tabType in tabsTypes)
            {
                var tab = (IUltimateLazyToolWindowTab)Activator.CreateInstance(tabType);

                if (tab.WindowName != WindowName)
                    continue;

                var tabElement = new DynamicWindowTabElement(tab);

                if (tabElement.HasParent)
                {
                    if (!_childTabsByParent.ContainsKey(tabElement.ParentTabName))
                        _childTabsByParent[tabElement.ParentTabName] = new DynamicTabDictionary();

                    _childTabsByParent[tabElement.ParentTabName]
                        .AddTab(tabElement.DisplayName, tabElement);

                    if (!_parentTabs.Contains(tabElement.ParentTabName))
                    {
                        _parentTabs.Add(tabElement.ParentTabName);
                    }
                }
                else
                {
                    if (!_parentTabs.Contains(tabElement.TabName))
                    {
                        _parentTabs.Add(tabElement.TabName);
                    }
                }

                _tabNames.Add(tabElement.TabName);
                _tabs.AddTab(tabElement.TabName, tabElement);
            }

            _parentTabs.Sort();
            foreach (var key in _childTabsByParent.Keys.ToList())
            {
                _childTabsByParent[key].Sort();
            }
        }

        private void OnEnable()
        {
            RefreshTabs();
            minSize = new Vector2(MinWidth, MinHeight);
        }

        private void OnGUI()
        {
            if (_tabs == null || _tabNames == null)
            {
                RefreshTabs();
            }

            switch (Layout)
            {
                case WindowLayout.Tabs:
                    DrawTabsLayout();
                    break;
                case WindowLayout.Sidebar:
                    DrawSidebarLayout();
                    break;
            }
        }

        private void DrawTabsLayout()
        {
            if (_tabNames.Count > 0)
            {
                if (_tabNames.Count > 1)
                {
                    _selectedTabIndex = GUILayout.Toolbar(
                        _selectedTabIndex,
                        _tabNames.ToArray(),
                        GUILayout.Height(25)
                    );
                    GUILayout.Space(10);
                }

                if (_tabs.TryGetValue(_tabNames[_selectedTabIndex], out var toolsInTab))
                {
                    DrawContent(toolsInTab);
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
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.Width(_sidebarWidth));
            DrawSidebarTreeView();
            EditorGUILayout.EndVertical();

            DrawDivider();
            DrawContentArea();

            EditorGUILayout.EndHorizontal();
        }

        private void InitializeTreeView()
        {
            if (_treeViewState == null)
                _treeViewState = new TreeViewState();

            _treeView = new UltimateLazyDynamicTreeView(
                _treeViewState,
                _parentTabs,
                _childTabsByParent
            );
            _treeView.OnTabSelected += OnTabSelected;
        }

        private void DrawSidebarTreeView()
        {
            if (_treeView == null)
                InitializeTreeView();

            Rect treeViewRect = GUILayoutUtility.GetRect(_sidebarWidth, position.height);
            _treeView.OnGUI(treeViewRect);
        }

        private void OnTabSelected(string selectedElement)
        {
            _selectedToolName = selectedElement;
            Repaint();
        }

        private void DrawContentArea()
        {
            EditorGUILayout.BeginVertical();
            _contentScrollPosition = EditorGUILayout.BeginScrollView(_contentScrollPosition);

            if (_selectedTab != null)
            {
                _selectedTab.OnGUI();
            }
            else if (!string.IsNullOrEmpty(_selectedToolName))
            {
                if (_tabs.TryGetValue(_selectedToolName, out var toolsInTab))
                {
                    foreach (var tool in toolsInTab)
                    {
                        tool.Tab.OnGUI();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField(
                        $"Selected: {_selectedToolName}",
                        EditorStyles.boldLabel
                    );
                }
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Select a tab to view its content.",
                    EditorStyles.centeredGreyMiniLabel
                );
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawDivider()
        {
            var dividerRect = new Rect(_sidebarWidth, 0, 1, position.height);
            EditorGUI.DrawRect(dividerRect, Color.black);

            var resizeHandleRect = new Rect(_sidebarWidth - 2, 0, 5, position.height);
            EditorGUIUtility.AddCursorRect(resizeHandleRect, MouseCursor.ResizeHorizontal);

            if (
                Event.current.type == EventType.MouseDown
                && resizeHandleRect.Contains(Event.current.mousePosition)
            )
            {
                _isResizing = true;
            }

            if (_isResizing)
            {
                _sidebarWidth = Mathf.Clamp(
                    Event.current.mousePosition.x,
                    100,
                    position.width - 200
                );
                Repaint();
            }

            if (Event.current.type == EventType.MouseUp)
            {
                _isResizing = false;
            }
        }

        private void DrawContent(List<DynamicWindowTabElement> toolsInTab)
        {
            _contentScrollPosition = EditorGUILayout.BeginScrollView(
                _contentScrollPosition,
                GUILayout.ExpandHeight(true)
            );

            for (int i = 0; i < toolsInTab.Count; i++)
            {
                toolsInTab[i].Tab.OnGUI();

                if (i < toolsInTab.Count - 1)
                {
                    DrawDivider();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height);
            var pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }

    public struct DynamicWindowTabElement
    {
        public IUltimateLazyToolWindowTab Tab { get; }
        public string DisplayName => Tab.TabName;
        public string ParentTabName => Tab.ParentTabName;

        public string TabName =>
            string.IsNullOrEmpty(Tab.ParentTabName)
                ? Tab.TabName
                : $"{Tab.ParentTabName}/{Tab.TabName}";

        public bool HasParent => !string.IsNullOrEmpty(ParentTabName);

        public DynamicWindowTabElement(IUltimateLazyToolWindowTab tab)
        {
            Tab = tab;
        }
    }

    public class DynamicTabDictionary : Dictionary<string, List<DynamicWindowTabElement>>
    {
        public void AddTab(string parent, DynamicWindowTabElement tab)
        {
            if (!ContainsKey(parent))
            {
                Add(parent, new List<DynamicWindowTabElement>());
            }

            this[parent].Add(tab);
        }

        public void Sort()
        {
            foreach (var key in Keys)
            {
                this[key].Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));
            }
        }
    }
}
