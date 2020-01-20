using System;
using System.Reflection;
using System.Threading;
using UnityEditor;

namespace Ludiq.PeekCore
{
	public sealed class BackgroundTaskRunner : ITaskRunner
	{
		public static BackgroundTaskRunner instance { get; } = new BackgroundTaskRunner();

		private static readonly TaskThreadTracker threadTracker = new TaskThreadTracker();

		public bool runsCurrentThread => threadTracker.runsCurrent;

		public void Report(Task task)
		{
			Report(task.currentStepLabel ?? task.title, task.ratio);
		}

		public void Run(Task task)
		{
			if (UnityThread.isRunningOnMainThread)
			{
				new Thread(() => RunSynchronous(task)).Start();
			}
			else
			{
				RunSynchronous(task);
			}
		}

		private void RunSynchronous(Task task)
		{
			threadTracker.Enter();

			Report(task.title, 0);

			task.Begin();

			try
			{
				task.Run();
			}
			catch (ThreadAbortException) { }
			catch (OperationCanceledException) { }
			finally
			{
				task.End();
				Clear();
				threadTracker.Exit();
			}
		}

		static BackgroundTaskRunner()
		{
			try
			{
				AsyncProgressBarType = typeof(EditorWindow).Assembly.GetType("UnityEditor.AsyncProgressBar", true);
				AsyncProgressBar_Display = AsyncProgressBarType.GetMethod("Display", BindingFlags.Static | BindingFlags.Public);
				AsyncProgressBar_Clear = AsyncProgressBarType.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);

				if (AsyncProgressBar_Display == null)
				{
					throw new MissingMemberException(AsyncProgressBarType.FullName, "Display");
				}

				if (AsyncProgressBar_Clear == null)
				{
					throw new MissingMemberException(AsyncProgressBarType.FullName, "Clear");
				}
			}
			catch (Exception ex)
			{
				throw new UnityEditorInternalException(ex);
			}
			
			Clear();

			EditorApplication.update += Display;
		}

		private static readonly Type AsyncProgressBarType; // internal sealed class AsyncProgressBar
		private static readonly MethodInfo AsyncProgressBar_Display; // public static extern void AsyncStatusBar.Display(string progressInfo, float progress);
		private static readonly MethodInfo AsyncProgressBar_Clear; // public static extern void AsyncStatusBar.Clear();
		
		private static readonly object @lock = new object();

		private static bool clear;
		private static string label;
		private static float ratio;

		public static void Report(string label, float ratio)
		{
			lock (@lock)
			{
				BackgroundTaskRunner.label = label;
				BackgroundTaskRunner.ratio = ratio;
			}
		}

		public static void Clear()
		{
			lock (@lock)
			{
				clear = true;
				label = null;
				ratio = 0;
			}
		}

		private static void Display()
		{
			lock (@lock)
			{
				if (clear)
				{
					AsyncProgressBar_Clear.InvokeOptimized(null);
					clear = false;
				}

				if (label != null)
				{
					AsyncProgressBar_Display.InvokeOptimized(null, label, ratio);
				}
			}
		}
	}
}