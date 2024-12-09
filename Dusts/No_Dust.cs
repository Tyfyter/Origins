using Terraria;
using Terraria.ModLoader;

namespace Origins.Dusts {
	public class No_Dust : ModDust {
		public override string Texture => "Origins/Items/Accessories/Ravel";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void OnSpawn(Dust dust) {
			dust.active = false;
		}
	}
}