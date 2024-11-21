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
                UnityEngine.Debug.LogError("Could not determine the current branch name.");
                return;
            }

            // Find the merge base (common ancestor) with main, master or dev
            var mainBranch = "dev";
            var mergeBase = GitUtils.RunGitCommand($"merge-base {branchName} {mainBranch}").Trim();
            if (string.IsNullOrEmpty(mergeBase))
            {
                mainBranch = "main"; // Fall back to "master" if "main" doesn't exist
                mergeBase = GitUtils.RunGitCommand($"merge-base {branchName} {mainBranch}").Trim();
            }

            if (string.IsNullOrEmpty(mergeBase))
            {
                mainBranch = "master"; // Fall back to "master" if "main" doesn't exist
                mergeBase = GitUtils.RunGitCommand($"merge-base {branchName} {mainBranch}").Trim();
            }

            if (string.IsNullOrEmpty(mergeBase))
            {
                Logger.LogError("Could not determine the merge base with the main branch.");
                return;
            }

            // Log commits from the current branch since it diverged from main or master
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
