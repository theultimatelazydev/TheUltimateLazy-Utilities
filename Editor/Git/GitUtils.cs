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

        public static string GetClosestBaseBranch(string currentBranch)
        {
            // Get all local branches
            var branchesOutput = RunGitCommand("branch --list").Trim();
            if (string.IsNullOrEmpty(branchesOutput))
            {
                return null;
            }

            var branches = branchesOutput.Split('\n');
            string closestBranch = null;
            string closestMergeBase = null;

            foreach (var branch in branches)
            {
                var branchName = branch.Replace("*", "").Trim(); // Remove the active branch indicator (*)
                if (branchName == currentBranch)
                    continue; // Skip the current branch

                // Get the merge base with this branch
                var mergeBase = RunGitCommand($"merge-base {currentBranch} {branchName}").Trim();
                if (string.IsNullOrEmpty(mergeBase))
                    continue;

                // Check if this is the closest branch (most recent common ancestor)
                if (
                    string.IsNullOrEmpty(closestMergeBase)
                    || IsMergeBaseCloser(mergeBase, closestMergeBase)
                )
                {
                    closestBranch = branchName;
                    closestMergeBase = mergeBase;
                }
            }

            return closestBranch;
        }

        private static bool IsMergeBaseCloser(string mergeBaseA, string mergeBaseB)
        {
            // Compare two merge bases: the closer one will have a more recent commit date
            var commitDateA = RunGitCommand($"show -s --format=%ct {mergeBaseA}").Trim();
            var commitDateB = RunGitCommand($"show -s --format=%ct {mergeBaseB}").Trim();

            if (
                long.TryParse(commitDateA, out var dateA)
                && long.TryParse(commitDateB, out var dateB)
            )
            {
                return dateA > dateB; // Return true if mergeBaseA is more recent
            }

            return false;
        }
    }
}
