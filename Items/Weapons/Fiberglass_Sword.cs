using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons {
	public class Fiberglass_Sword : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Sword");
			Tooltip.SetDefault("Be careful, it's sharp");
		}
		public override void SetDefaults() {
			item.damage = 18;
			item.melee = true;
			item.width = 42;
			item.height = 42;
			item.useTime = 16;
			item.useAnimation = 16;
			item.useStyle = 1;
			item.knockBack = 2;
			item.value = 5000;
			item.autoReuse = true;
            item.useTurn = true;
			item.rare = ItemRarityID.Green;
			item.UseSound = SoundID.Item1;
		}
	}
}
