using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace UltimateLazy.Tools.Editor
{
    public class UltimateLazyDynamicTreeView : TreeView
    {
        private readonly List<string> _parents;
        private readonly Dictionary<string, DynamicTabDictionary> _children;
        private readonly Dictionary<int, string> _elementsById;
        private int _currentId = 0;

        public event System.Action<string> OnTabSelected;

        public UltimateLazyDynamicTreeView(
            TreeViewState state,
            List<string> parents,
            Dictionary<string, DynamicTabDictionary> children
        )
            : base(state)
        {
            _parents = parents;
            _children = children;
            _elementsById = new Dictionary<int, string>();
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            _elementsById.Clear();
            var root = new TreeViewItem
            {
                id = -1,
                depth = -1,
                displayName = "Root",
            };

            foreach (var parent in _parents)
            {
                var parentId = GetNextId();
                var parentItem = new TreeViewItem
                {
                    id = parentId,
                    displayName = parent,
                    depth = 0,
                };

                // Add parent-only entry
                _elementsById[parentId] = parent;
                root.AddChild(parentItem);

                if (!_children.TryGetValue(parent, out var tab))
                {
                    continue;
                }

                // Add child tabs under the parent
                foreach (var child in tab.Values)
                {
                    var childId = GetNextId();
                    var childItem = new TreeViewItem
                    {
                        id = childId,
                        displayName = child[0].DisplayName,
                        depth = 1,
                    };

                    _elementsById[childId] = child[0].TabName;
                    parentItem.AddChild(childItem);
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count > 0 && _elementsById.TryGetValue(selectedIds[0], out var element))
            {
                OnTabSelected?.Invoke(element);
            }
        }

        private int GetNextId()
        {
            return ++_currentId;
        }
    }
}
