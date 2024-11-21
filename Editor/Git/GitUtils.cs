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

        public static string GetDefaultBaseBranch()
        {
            // Check the remote HEAD reference to find the default branch
            var defaultBranchOutput = GitUtils
                .RunGitCommand("symbolic-ref refs/remotes/origin/HEAD")
                .Trim();
            if (!string.IsNullOrEmpty(defaultBranchOutput))
            {
                // Extract the branch name from the output
                var defaultBranch = defaultBranchOutput.Replace("refs/remotes/origin/", "").Trim();
                return defaultBranch;
            }

            // Fallback: Look for common default branches if HEAD reference is not found
            var commonBranches = new[] { "main", "master", "dev" };
            foreach (var branch in commonBranches)
            {
                var branchExists = !string.IsNullOrEmpty(
                    GitUtils.RunGitCommand($"rev-parse --verify origin/{branch}").Trim()
                );
                if (branchExists)
                {
                    return branch;
                }
            }

            return null; // No base branch found
        }
    }
}
