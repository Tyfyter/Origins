using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Stealth_Ravel : Ravel {
		public static new int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Stealth Ravel");
			Tooltip.SetDefault("Double tap down to transform into a small, rolling ball\nEnemies are less likely to target you when raveled");
			SacrificeTotal = 1;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory();
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 8);
			Item.shoot = ModContent.MountType<Stealth_Ravel_Mount>();
		}
		protected override void UpdateRaveled(Player player) {
			player.aggro -= 400;
			player.blackBelt = true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.PutridScent);
			recipe.AddIngredient(Ravel.ID);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
	public class Stealth_Ravel_Mount : Ravel_Mount {
		public override string Texture => "Origins/Items/Accessories/Stealth_Ravel";
		public static new int ID { get; private set; } = -1;
		protected override void SetID() {
			MountData.buff = ModContent.BuffType<Stealth_Ravel_Mount_Buff>();
			ID = Type;
		}
	}
	public class Stealth_Ravel_Mount_Buff : Ravel_Mount_Buff {
		public override string Texture => "Origins/Buffs/Ravel_Generic_Buff";
		protected override int MountID => ModContent.MountType<Stealth_Ravel_Mount>();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Stealth Ravel");
			Description.SetDefault("10% chance to dodge. Less likely to be targeted");
		}
	}
}
