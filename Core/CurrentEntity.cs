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
		void ILoadable.Load(Mod mod) {
			On_Projectile.Update += (orig, self, i) => {
				using CurrentEntity cur = new(self);
				orig(self, i);
			};
			On_Player.Update += (orig, self, i) => {
				using CurrentEntity cur = new(self);
				orig(self, i);
			};
			On_NPC.UpdateNPC += (orig, self, i) => {
				using CurrentEntity cur = new(self);
				orig(self, i);
			};
		}

		void ILoadable.Unload() { }
	}
}
