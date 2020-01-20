using System;

namespace Ludiq.PeekCore
{
	internal abstract class LudiqCoreMigration : PluginMigration
	{
		protected LudiqCoreMigration(Plugin plugin) : base(plugin) { }

		protected void AddLegacyDefaultTypeOption(Type type)
		{
			if (!LudiqCore.Configuration.legacyTypeOptions.Contains(type))
			{
				LudiqCore.Configuration.legacyTypeOptions.Add(type);
				LudiqCore.Configuration.Save();
			}
		}

		protected void AddLegacyDefaultAssemblyOption(LooseAssemblyName assembly)
		{
			if (!LudiqCore.Configuration.legacyAssemblyOptions.Contains(assembly))
			{
				LudiqCore.Configuration.legacyAssemblyOptions.Add(assembly);
				LudiqCore.Configuration.Save();
			}
		}
	}
}
