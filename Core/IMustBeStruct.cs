using System;
using Terraria.ModLoader;

namespace Origins.Core; 
public interface IMustBeStruct : IAutoload<IMustBeStruct.Checker> {
	class Checker : IAutoloader {
		static void IAutoloader.Autoload(Mod mod, Type type) {
			if (!type.IsValueType) throw new UsageException($"{type} is not a struct");
		}
	}
}
public interface IMustBeClass : IAutoload<IMustBeClass.Checker> {
	class Checker : IAutoloader {
		static void IAutoloader.Autoload(Mod mod, Type type) {
			if (type.IsValueType) throw new UsageException($"{type} is not a class");
		}
	}
}
