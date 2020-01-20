using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Ludiq.PeekCore;
using UnityEditor;
using UnityEngine;

[assembly: InitializeAfterPlugins(typeof(BackgroundWorker))]

namespace Ludiq.PeekCore
{
	public static class BackgroundWorker
	{
		static BackgroundWorker()
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

			queue = new Queue<Action>();

			ClearProgress();
			
			foreach (var registration in Codebase.GetTypeRegistrations<RegisterBackgroundWorkerAttribute>())
			{
				var backgroundWorkMethod = registration.type.GetMethod(registration.methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);

				if (backgroundWorkMethod != null)
				{
					tasks += () => backgroundWorkMethod.Invoke(null, new object[0]);
				}
				else
				{
					Debug.LogWarningFormat($"Missing '{registration.methodName}' method for '{registration.type}' background worker.");
				}
			}

			EditorApplication.update += DisplayProgress;

			EditorApplication.delayCall += delegate { new Thread(Work) { Name = "Background Worker" }.Start(); };
		}

		private static readonly object @lock = new object();
		private static bool clearProgress;

		private static readonly Type AsyncProgressBarType; // internal sealed class AsyncProgressBar
		private static readonly MethodInfo AsyncProgressBar_Display; // public static extern void AsyncStatusBar.Display(string progressInfo, float progress);
		private static readonly MethodInfo AsyncProgressBar_Clear; // public static extern void AsyncStatusBar.Clear();

		private static readonly Queue<Action> queue;

		public static event Action tasks
		{
			add
			{
				Schedule(value);
			}
			remove { }
		}

		public static string progressLabel { get; private set; }
		public static float progressProportion { get; private set; }
		public static bool hasProgress => progressLabel != null;

		public static void Schedule(Action action)
		{
			lock (queue)
			{
				queue.Enqueue(action);
			}
		}

		private static void Work()
		{
			while (true)
			{
				Action task = null;
				var remaining = 0;

				lock (queue)
				{
					if (queue.Count > 0)
					{
						remaining = queue.Count;
						task = queue.Dequeue();
					}
				}

				if (task != null)
				{
					ReportProgress($"{remaining} task{(queue.Count > 1 ? "s" : "")} remaining...", 0);

					try
					{
						task();
					}
					catch (Exception ex)
					{
						EditorApplication.delayCall += () => Debug.LogException(ex);
					}
					finally
					{
						ClearProgress();
					}
				}
				else
				{
					Thread.Sleep(100);
				}
			}
		}

		public static void ReportProgress(string title, float progress)
		{
			lock (@lock)
			{
				progressLabel = title;
				progressProportion = progress;
			}
		}

		public static void ClearProgress()
		{
			lock (@lock)
			{
				clearProgress = true;
				progressLabel = null;
				progressProportion = 0;
			}
		}

		private static void DisplayProgress()
		{
			lock (@lock)
			{
				if (clearProgress)
				{
					try
					{
						AsyncProgressBar_Clear.InvokeOptimized(null);
					}
					catch (Exception ex)
					{
						throw new UnityEditorInternalException(ex);
					}

					clearProgress = false;
				}

				if (progressLabel != null)
				{
					try
					{
						AsyncProgressBar_Display.InvokeOptimized(null, progressLabel, progressProportion);
					}
					catch (Exception ex)
					{
						throw new UnityEditorInternalException(ex);
					}
				}
			}
		}
	}
}