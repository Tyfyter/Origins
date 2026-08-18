using Terraria;
using Terraria.ModLoader;

namespace Origins.Core; 
public ref struct CurrentEntity {
	public static Entity Entity { get; private set; }
	readonly Entity prev;
	public CurrentEntity(Entity entity) {
		prev = Entity;
		Entity = entity;
	}
	public readonly void Dispose() => Entity = prev;
	class Loader : ILoadable {
		void ILoadable.Load(Mod mod) => IgnoreThisLineOfTheStackTraceThisCodeChangesNothingItJustReadsData.LoadCurrentEntity();
		void ILoadable.Unload() { }
	}
}
