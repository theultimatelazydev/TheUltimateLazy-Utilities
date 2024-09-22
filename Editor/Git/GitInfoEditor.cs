using System.Diagnostics;
using UnityEditor;
using Logger = UnityEngine.Debug;

namespace UltimateLazy.Tools.Editor
{
    [InitializeOnLoad]
    public static class GitInfoEditor
    {
        public static string GitBranch { get; private set; }
        
        static GitInfoEditor()
        {
            EditorApplication.update += Update;
        }

        private static void Update()
        {
            EditorApplication.update -= Update;
            
            if (IsGitRepository())
            {
                GitBranch = GetCurrentGitBranch();
                Logger.Log($"Current branch: {GitBranch}");
            }
            else
            {
                GitBranch = "No Git repository";
                Logger.LogWarning("The current directory is not a Git repository.");
            }
        }

        private static bool IsGitRepository()
        {
            return RunGitCommand("rev-parse --is-inside-work-tree") == "true";
        }

        private static string GetCurrentGitBranch()
        {
            return RunGitCommand("rev-parse --abbrev-ref HEAD");
        }

        private static string RunGitCommand(string arguments)
        {
            // Creates a new process to execute the git command
            Process process = new Process();
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Remove extra whitespace
                return output.Trim();
            }
            catch (System.Exception ex)
            {
                Logger.LogError($"Error executing Git command: {ex.Message}");
                return null;
            }
        }
    }
}
