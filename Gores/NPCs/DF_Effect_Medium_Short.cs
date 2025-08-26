using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Origins.Gores.NPCs {
	public class DF_Effect_Medium1_Short : ModGore {
		public override string Texture => base.Texture[..^6];
		public override void SetStaticDefaults() {
			ChildSafety.SafeGore[Type] = true;
		}
		public override void OnSpawn(Gore gore, IEntitySource source) {
			gore.timeLeft = 60;
		}
	}
	public class DF_Effect_Medium2_Short : DF_Effect_Medium1_Short { }
	public class DF_Effect_Medium3_Short : DF_Effect_Medium1_Short { }
}
