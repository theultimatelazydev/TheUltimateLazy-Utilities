using System.Collections.Generic;
using UnityEngine;

namespace UltimateLazy.Tools.Editor
{
    [CreateAssetMenu(fileName = "LinkXmlConfig", menuName = "ScriptableObjects/LinkXmlConfig")]
    public class LinkXmlConfig : ScriptableObject
    {
        public List<string> prefixes = new List<string> { "MyName" }; // Default prefixes
        public List<string> ignoredAssemblies = new List<string> { "IgnoredAssembly" }; // Default ignored assemblies
    }
}
