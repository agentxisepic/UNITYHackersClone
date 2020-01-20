using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Ludiq.PeekCore
{
	[InitializeOnLoad]
	public static class PluginContainer
	{
		static PluginContainer()
		{
			if (EditorApplication.isUpdating)
			{
				EditorApplication.delayCall += () =>
				{
					AssetDatabase.Refresh();
					Initialize();
				};
			}
			else
			{
				Initialize();
			}
		}

		private static bool initializing;

		private static Dictionary<string, Plugin> pluginsById;

		private static Dictionary<string, Type> pluginTypesById;

		internal static Dictionary<string, HashSet<string>> pluginDependencies;
		
		private static readonly Queue<Action> delayQueue = new Queue<Action>();

		public static event Action delayCall
		{
			add
			{
				Ensure.That(nameof(value)).IsNotNull(value);

				if (initialized)
				{
					value.Invoke();
				}
				else
				{
					lock (delayQueue)
					{
						delayQueue.Enqueue(value);
					}
				}
			}
			remove { }
		}

		public static bool initialized { get; private set; }

		public static IEnumerable<Plugin> plugins
		{
			get
			{
				EnsureInitialized();

				return pluginsById.Values;
			}
		}

		public static void UpdateVersionMismatch()
		{
			anyVersionMismatch = plugins.Any(p => p.manifest.versionMismatch);
		}

		private static void Initialize()
		{
			using (ProfilingUtility.SampleBlock("Plugin Container Initialization"))
			{
				if (EditorApplication.isUpdating)
				{
					Debug.LogError("Initializing plugin container while asset database is updating.\nLoading assets might fail!");
				}

				// Seems to cause issues with Peek, not sure why exactly though:
				// https://support.ludiq.io/communities/41/topics/4173-peek-linux-nullreferenceexception-plugincontainerinitialize
				// Also, not clear why this line was needed in the first place.
				// Maybe it isn't any more now that we delay initialization during updating.
				// AssetDatabase.Refresh();

				initializing = true;

				pluginTypesById = Codebase.GetTypeRegistrations<RegisterPluginAttribute>()
										  .ToDictionary(r => r.id, r => r.type);
				
				pluginDependencies = new Dictionary<string, HashSet<string>>();

				foreach (var pluginId in pluginTypesById.Keys)
				{
					pluginDependencies.Add(pluginId, new HashSet<string>());
				}

				foreach (var pluginDependencyRegistration in Codebase.GetAssemblyAttributes<RegisterPluginDependencyAttribute>())
				{
					if (!pluginDependencies.TryGetValue(pluginDependencyRegistration.dependerId, out var dependencies))
					{
						dependencies = new HashSet<string>();
						pluginDependencies.Add(pluginDependencyRegistration.dependerId, dependencies);
					}

					dependencies.Add(pluginDependencyRegistration.dependencyId);
				}
				 
				var moduleTypeRegistrations = Codebase.GetTypeRegistrations<RegisterPluginModuleTypeAttribute>();

				pluginsById = new Dictionary<string, Plugin>();

				var allModules = new List<IPluginModule>();

				foreach (var pluginId in pluginTypesById.Keys.OrderByDependencies(pluginId => pluginDependencies[pluginId]))
				{
					var pluginType = pluginTypesById[pluginId];

					Plugin plugin;

					try
					{
						using (ProfilingUtility.SampleBlock($"{pluginType.Name} (Instantiation)"))
						{
							plugin = (Plugin)pluginType.Instantiate();
						}
					}
					catch (Exception ex)
					{
						throw new TargetInvocationException($"Could not instantiate plugin '{pluginId}' ('{pluginType}').", ex);
					}

					var modules = new List<IPluginModule>();

					foreach (var moduleTypeRegistration in moduleTypeRegistrations)
					{
						var moduleType = moduleTypeRegistration.type;
						var required = moduleTypeRegistration.required;
						
						try
						{
							var moduleProperty = pluginType.GetProperties().FirstOrDefault(p => p.PropertyType.IsAssignableFrom(moduleType));

							if (moduleProperty == null)
							{
								continue;
							}

							IPluginModule module = null;
							
							var moduleOverrideType = Codebase.GetTypeRegistrations<MapToPluginAttribute>()
								.FirstOrDefault(r => r.pluginId == pluginId &&
								                     moduleType.IsAssignableFrom(r.type) &&
								                     r.type.IsConcrete())?.type;

							if (moduleOverrideType != null)
							{
								try
								{
									using (ProfilingUtility.SampleBlock($"{moduleOverrideType.Name} (Instantiation)"))
									{
										module = (IPluginModule)InstantiateMappedType(moduleOverrideType, plugin);
									}
								}
								catch (Exception ex)
								{
									throw new TargetInvocationException($"Failed to instantiate user-defined plugin module '{moduleOverrideType}' for '{pluginId}'.", ex);
								}
							}
							else if (moduleType.IsConcrete())
							{
								try
								{
									using (ProfilingUtility.SampleBlock($"{moduleType.Name} (Instantiation)"))
									{
										module = (IPluginModule)InstantiateMappedType(moduleType, plugin);
									}
								}
								catch (Exception ex)
								{
									throw new TargetInvocationException($"Failed to instantiate built-in plugin module '{moduleType}' for '{pluginId}'.", ex);
								}
							}
							else if (required)
							{
								throw new InvalidImplementationException($"Missing implementation of plugin module '{moduleType}' for '{pluginId}'.");
							}

							if (module != null)
							{
								moduleProperty.SetValue(plugin, module, null);

								modules.Add(module);
								allModules.Add(module);
							}
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
						}
					}

					pluginsById.Add(plugin.id, plugin);

					foreach (var module in modules)
					{
						try
						{
							using (ProfilingUtility.SampleBlock($"{module.GetType().Name} (Initialization)"))
							{
								module.Initialize();
							}
						}
						catch (Exception ex)
						{
							Debug.LogException(new Exception($"Failed to initialize plugin module '{plugin.id}.{module.GetType()}'.", ex));
						}
					}

					if (plugin.manifest.versionMismatch)
					{
						anyVersionMismatch = true;
					}
				}

				initializing = false;

				initialized = true;

				using (ProfilingUtility.SampleBlock($"Product Container Initialization"))
				{
					ProductContainer.Initialize();
				}

				foreach (var module in allModules)
				{
					try
					{
						using (ProfilingUtility.SampleBlock($"{module.GetType().Name} (Late Initialization)"))
						{
							module.LateInitialize();
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(new Exception($"Failed to late initialize plugin module '{module.plugin.id}.{module.GetType()}'.", ex));
					}
				}
				 
				var afterPluginTypes = Codebase.GetRegisteredTypes<InitializeAfterPluginsAttribute>();

				using (ProfilingUtility.SampleBlock($"BeforeInitializeAfterPlugins"))
				{
					EditorApplicationUtility.BeforeInitializeAfterPlugins();
				}

				foreach (var afterPluginType in afterPluginTypes)
				{
					using (ProfilingUtility.SampleBlock($"{afterPluginType.Name} (Static Initializer)"))
					{
						RuntimeHelpers.RunClassConstructor(afterPluginType.TypeHandle);
					}
				}

				using (ProfilingUtility.SampleBlock($"AfterInitializeAfterPlugins"))
				{
					EditorApplicationUtility.AfterInitializeAfterPlugins();
				}

				using (ProfilingUtility.SampleBlock($"Launch Setup Wizards"))
				{
					// Automatically show setup wizards

					if (!EditorApplication.isPlayingOrWillChangePlaymode)
					{
						var productsRequiringSetup = ProductContainer.products.Where(product => product.requiresSetup).ToHashSet();
						var productsHandlingAllSetups = productsRequiringSetup.ToHashSet();

						// Do not show product setups if another product already
						// includes all the same plugins or more. For example,
						// if both Bolt and Ludiq require setup, but Bolt requires
						// all of the Ludiq plugins, then only the Bolt setup wizard
						// should be shown.
						foreach (var product in productsRequiringSetup)
						{
							foreach (var otherProduct in productsRequiringSetup)
							{
								if (product == otherProduct)
								{
									continue;
								}

								var productPlugins = product.plugins.ResolveDependencies().ToHashSet();
								var otherProductPlugins = otherProduct.plugins.ResolveDependencies().ToHashSet();

								if (productPlugins.IsSubsetOf(otherProductPlugins))
								{
									productsHandlingAllSetups.Remove(product);
								}
							}
						}

						foreach (var product in productsHandlingAllSetups)
						{
							// Delay call is used here to avoid showing multiple wizards during an import
							EditorApplication.delayCall += () => SetupWizard.Show(product);
						}
					}
				}

				using (ProfilingUtility.SampleBlock($"Launch Update Wizard"))
				{
					// Automatically show update wizard

					if (!EditorApplication.isPlayingOrWillChangePlaymode && plugins.Any(plugin => plugin.manifest.versionMismatch))
					{
						// Delay call seems to be needed here to avoid arcane exceptions...
						// Too lazy to debug why, it works that way.
						EditorApplication.delayCall += UpdateWizard.Show;
					}
				}

				using (ProfilingUtility.SampleBlock($"Delayed Calls"))
				{
					lock (delayQueue)
					{
						while (delayQueue.Count > 0)
						{
							delayQueue.Dequeue().Invoke();
						}
					}
				}

				InternalEditorUtility.RepaintAllViews();

				ProfilingUtility.Clear();
				//ConsoleProfiler.Dump();
			}
		}

		private static void EnsureInitialized()
		{
			if (initializing)
			{
				return;
			}

			if (!initialized)
			{
				throw new InvalidOperationException("Trying to access Ludiq plugin container before it is initialized.");
			}
		}
		
		public static Plugin GetPlugin(string pluginId)
		{
			EnsureInitialized();

			Ensure.That(nameof(pluginId)).IsNotNull(pluginId);

			return pluginsById[pluginId];
		}

		public static bool HasPlugin(string pluginId)
		{
			EnsureInitialized();

			Ensure.That(nameof(pluginId)).IsNotNull(pluginId);

			return pluginsById.ContainsKey(pluginId);
		}

		private static IPluginAddon InstantiateMappedType(Type type, Plugin plugin)
		{
			Ensure.That(nameof(type)).IsNotNull(type);
			Ensure.That(nameof(plugin)).IsNotNull(plugin);

			// Too performance intensive, slowing down startup
			//Ensure.That(nameof(linkedType)).IsOfType<IPluginLinked>(linkedType);
			//Ensure.That(nameof(linkedType)).HasConstructorAccepting(linkedType, plugin.GetType());

			return (IPluginAddon)type.GetConstructorAccepting(plugin.GetType()).Invoke(new object[] { plugin });
		}

		internal static IEnumerable<Type> GetMappedTypes(Type type, string pluginId)
		{
			Ensure.That(nameof(type)).IsNotNull(type);

			// Too performance intensive, slowing down startup
			// Ensure.That(nameof(linkedType)).IsOfType<IPluginLinked>(linkedType);

			return Codebase.GetTypeRegistrations<MapToPluginAttribute>().Where(r => r.pluginId == pluginId && type.IsAssignableFrom(r.type) && r.type.IsConcrete()).Select(r => r.type);
		}

		internal static IPluginAddon[] InstantiateMappedTypes(Type type, Plugin plugin)
		{
			Ensure.That(nameof(type)).IsNotNull(type);
			Ensure.That(nameof(plugin)).IsNotNull(plugin);

			return GetMappedTypes(type, plugin.id).Select(t => InstantiateMappedType(t, plugin)).ToArray();
		}
		
		public static bool anyVersionMismatch { get; private set; }
	}
}