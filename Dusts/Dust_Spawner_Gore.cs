using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class Dust_Spawner_Gore : ModGore {
		public override string Texture => "Terraria/Images/NPC_0";
		public override void OnSpawn(Gore gore, IEntitySource source) {
			SpawnDust(gore.position, (int)gore.scale, gore.velocity);
			gore.active = false;
		}
		public virtual void SpawnDust(Vector2 Position, int Type, Vector2 Velocity) {
			Dust.NewDustPerfect(Position, Type, Velocity);
		}
	}
}
