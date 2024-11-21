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
            var mainBranch = GitUtils.GetTrueBaseBranch(branchName);
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
    }
}
