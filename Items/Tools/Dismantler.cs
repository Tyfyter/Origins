using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Dismantler : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Dismantler");
			Tooltip.SetDefault("Very pointy\nAble to mine Hellstone");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.DeathbringerPickaxe);
			item.damage = 14;
			item.melee = true;
            item.pick = 75;
			item.width = 34;
			item.height = 32;
			item.useTime = 22;
			item.useAnimation = 22;
			item.knockBack = 4f;
			item.value = 3600;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
		}
	}
}
