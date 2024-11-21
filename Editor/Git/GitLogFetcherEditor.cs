using UnityEditor;
using Logger = UnityEngine.Debug;

namespace UltimateLazy.Tools.Editor
{
    public class GitLogFetcherEditor
    {
        [MenuItem("Tools/The Ultimate Lazy Dev/Git/Log Commits", priority = 2)]
        private static void LogGitCommitsSinceBranch()
        {
            if (!GitUtils.IsGitRepository())
            {
                Logger.LogError("The current directory is not a Git repository.");
                return;
            }

            // Get the current branch name
            var branchName = GitUtils.GetCurrentGitBranch().Trim();
            if (string.IsNullOrEmpty(branchName))
            {
                Logger.LogError("Could not determine the current branch name.");
                return;
            }

            // Detect the default base branch dynamically
            var mainBranch = GetDefaultBaseBranch();
            if (string.IsNullOrEmpty(mainBranch))
            {
                Logger.LogError("Could not determine the default base branch.");
                return;
            }

            // Find the merge base (common ancestor) with the detected base branch
            var mergeBase = GitUtils.RunGitCommand($"merge-base {branchName} {mainBranch}").Trim();
            if (string.IsNullOrEmpty(mergeBase))
            {
                Logger.LogError(
                    $"Could not determine the merge base with the base branch {mainBranch}."
                );
                return;
            }

            // Log commits from the current branch since it diverged from the base branch
            var logOutput = GitUtils.RunGitCommand($"log {branchName} --oneline --not {mergeBase}");
            if (string.IsNullOrEmpty(logOutput))
            {
                Logger.Log($"No commits found in the branch {branchName} since its creation.");
            }
            else
            {
                Logger.Log($"Commits in branch {branchName} since its creation:\n{logOutput}");
            }
        }

        private static string GetDefaultBaseBranch()
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
