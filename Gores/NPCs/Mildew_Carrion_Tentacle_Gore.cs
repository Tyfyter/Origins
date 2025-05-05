using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Origins.NPCs.Brine.Boss;
using PegasusLib;

namespace Origins.Gores.NPCs {
	public class Mildew_Carrion_Tentacle_Gore : ModGore {
		public override string Texture => typeof(Mildew_Carrion_Tentacle).GetDefaultTMLName();
		public override void OnSpawn(Gore gore, IEntitySource source) {
			gore.numFrames = 5;
		}
	}
}
