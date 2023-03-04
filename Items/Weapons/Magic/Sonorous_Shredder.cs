using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
    public class Sonorous_Shredder : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Sonorous Shredder");
			Tooltip.SetDefault("'Split apart, lost one'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.MagicalHarp);
			Item.damage = 54;
			Item.useAnimation = 16;
			Item.useTime = 16;
			Item.shootSpeed = 18;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.LightRed;
			Item.mana = 9;
			//Item.UseSound = Origins.Sounds.RivenBass.WithPitch(-0.05f);
		}
	}
}
