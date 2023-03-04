using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
    public class Missing_File : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Missing File");
			Tooltip.SetDefault("Click on the portrait of the targetted enemy for a damage bonus\n'Let's demake some code...'");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CrystalVileShard);
			Item.shoot = ModContent.ProjectileType<Potato_P>();
			Item.damage = 64;
			Item.useAnimation = 30;
			Item.useTime = 30;
			Item.shootSpeed = 12;
			Item.useStyle = ItemHoldStyleID.HoldUp;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Yellow;
			Item.mana = 18;
		}
	}
}
