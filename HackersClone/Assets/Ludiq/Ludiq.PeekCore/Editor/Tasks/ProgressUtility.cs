using UnityEditor;

namespace Ludiq.PeekCore
{
	public static class ProgressUtility
	{
		public static void DisplayProgressBar(string title, string info, float progress)
		{
			if (UnityThread.allowsAPI)
			{
				EditorUtility.DisplayProgressBar(title, info, progress);
			}
			else
			{
				BackgroundTaskRunner.Report(title, progress);
			}
		}

		[MenuItem("Tools/Peek/Ludiq/Developer/Force Clear Progress Bar", priority = LudiqProduct.DeveloperToolsMenuPriority + 601)]
		public static void ClearProgressBar()
		{
			if (UnityThread.allowsAPI)
			{
				EditorUtility.ClearProgressBar();
			}

			BackgroundTaskRunner.Clear();
		}
	}
}