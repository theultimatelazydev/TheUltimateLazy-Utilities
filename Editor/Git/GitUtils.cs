using System.Diagnostics;
using Logger = UnityEngine.Debug;

namespace UltimateLazy.Tools.Editor
{
    public static class GitUtils
    {
        public static bool IsGitRepository()
        {
            return RunGitCommand("rev-parse --is-inside-work-tree") == "true";
        }

        public static string GetCurrentGitBranch()
        {
            return RunGitCommand("rev-parse --abbrev-ref HEAD");
        }
        
        public static string RunGitCommand(string arguments)
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
