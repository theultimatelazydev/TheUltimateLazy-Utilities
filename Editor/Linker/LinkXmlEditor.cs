using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;
using Logger = UnityEngine.Debug;

namespace UltimateLazy.Tools.Editor
{
    public class LinkXmlEditor : IUltimateLazyToolWindowTab
    {
        private const string LinkXmlPath = "Assets/link.xml";
        private LinkXmlConfig config;

        public string WindowName => "The Ultimate Lazy Tools";
        public string TabName => "Link XML Editor";

        [MenuItem("Tools/The Ultimate Lazy Dev/Linker/Link XML Editor", priority = 2)]
        private static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<MainWindow>();
            window.ChangeTab("Link XML Editor");
        }

        public void OnGUI()
        {
            EditorGUILayout.LabelField("Add Assemblies to link.xml", EditorStyles.boldLabel);

            if (config == null)
            {
                LoadOrCreateConfig();
            }

            // Display and edit prefix list
            EditorGUILayout.LabelField("Prefixes:");
            for (int i = 0; i < config.prefixes.Count; i++)
            {
                config.prefixes[i] = EditorGUILayout.TextField(
                    $"Prefix {i + 1}",
                    config.prefixes[i]
                );
            }

            if (GUILayout.Button("Add Prefix"))
            {
                config.prefixes.Add(string.Empty);
            }

            // Display and edit ignored assemblies list
            EditorGUILayout.LabelField("Ignored Assemblies:");
            for (int i = 0; i < config.ignoredAssemblies.Count; i++)
            {
                config.ignoredAssemblies[i] = EditorGUILayout.TextField(
                    $"Ignored Assembly {i + 1}",
                    config.ignoredAssemblies[i]
                );
            }

            if (GUILayout.Button("Add Ignored Assembly"))
            {
                config.ignoredAssemblies.Add(string.Empty);
            }

            if (GUILayout.Button("Update link.xml"))
            {
                UpdateLinkXml();
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(config); // Save changes to the ScriptableObject
            }
        }

        private void LoadOrCreateConfig()
        {
            config = AssetDatabase.LoadAssetAtPath<LinkXmlConfig>("Assets/LinkXmlConfig.asset");

            if (config == null)
            {
                config = (LinkXmlConfig)ScriptableObject.CreateInstance(typeof(LinkXmlConfig));
                AssetDatabase.CreateAsset(config, "Assets/LinkXmlConfig.asset");
                AssetDatabase.SaveAssets();
                Logger.Log("Created new LinkXmlConfig.asset");
            }
        }

        private void UpdateLinkXml()
        {
            // Find assemblies matching any prefix and not in ignored list
            List<string> assembliesToAdd = AppDomain
                .CurrentDomain.GetAssemblies()
                .Select(a => a.GetName().Name)
                .Where(name =>
                    config.prefixes.Any(prefix =>
                        !string.IsNullOrEmpty(prefix) && name.StartsWith(prefix)
                    )
                    && !name.EndsWith(".Editor")
                    && (!config.ignoredAssemblies.Contains(name))
                )
                .ToList();

            // Create or load link.xml
            XDocument xmlDoc;
            if (File.Exists(LinkXmlPath))
            {
                xmlDoc = XDocument.Load(LinkXmlPath);
            }
            else
            {
                xmlDoc = new XDocument(new XElement("linker"));
            }

            XElement root = xmlDoc.Element("linker");

            // Remove ignored assemblies if they already exist in link.xml
            foreach (string ignoredAssembly in config.ignoredAssemblies)
            {
                if (string.IsNullOrEmpty(ignoredAssembly))
                    continue;

                var existingIgnoredAssembly = root.Elements("assembly")
                    .FirstOrDefault(a => (string)a.Attribute("fullname") == ignoredAssembly);
                if (existingIgnoredAssembly != null)
                {
                    existingIgnoredAssembly.Remove();
                    Logger.Log($"Removed ignored assembly {ignoredAssembly} from link.xml");
                }
            }

            // Add missing assemblies
            foreach (string assemblyName in assembliesToAdd)
            {
                bool assemblyExists = root.Elements("assembly")
                    .Any(a => (string)a.Attribute("fullname") == assemblyName);

                // Add new assembly element if it doesn't exist
                if (!assemblyExists)
                {
                    XElement newAssembly = new XElement(
                        "assembly",
                        new XAttribute("fullname", assemblyName)
                    );
                    newAssembly.Add(new XAttribute("preserve", "all"));
                    root.Add(newAssembly);
                    Logger.Log($"Added {assemblyName} to link.xml");
                }
            }

            // Save link.xml
            xmlDoc.Save(LinkXmlPath);
            AssetDatabase.Refresh();

            Logger.Log("link.xml updated successfully!");
        }
    }
}
