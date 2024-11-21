using System.Diagnostics;
using UnityEditor;
using Logger = UnityEngine.Debug;

namespace UltimateLazy.Tools.Editor
{
    [InitializeOnLoad]
    public static class GitInfoEditor
    {
        public static string GitBranch { get; private set; }
        public static string GitBaseBranch { get; private set; }

        static GitInfoEditor()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            EditorApplication.update -= Update;

            if (GitUtils.IsGitRepository())
            {
                GitBranch = GitUtils.GetCurrentGitBranch();
                GitBaseBranch = GitUtils.GetDefaultBaseBranch();
                Logger.Log($"Current branch: {GitBranch}\n" + $"Base branch: {GitBaseBranch}");
            }
            else
            {
                GitBranch = "No Git repository";
                Logger.LogWarning("The current directory is not a Git repository.");
            }
        }
    }
}
