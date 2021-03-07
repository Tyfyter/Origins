using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Brick_Breaker : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Brick Breaker");
			Tooltip.SetDefault("Very pointy");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.DeathbringerPickaxe);
			item.damage = 14;
			item.melee = true;
            item.pick = 0;
            item.hammer = 60;
			item.width = 34;
			item.height = 34;
			item.useTime = 17;
			item.useAnimation = 27;
			item.knockBack = 4f;
			item.value = 3600;
			item.rare = ItemRarityID.Blue;
			item.UseSound = SoundID.Item1;
		}
	}
}
