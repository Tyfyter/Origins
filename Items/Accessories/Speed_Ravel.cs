using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Speed_Ravel : Ravel {
		public static new int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Speed Ravel");
			Tooltip.SetDefault("Double tap down to transform into a small, rolling ball that moves very fast");
			SacrificeTotal = 1;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 24;
			Item.accessory = true;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 8);
			Item.shoot = ModContent.MountType<Speed_Ravel_Mount>();
		}
		protected override void UpdateRaveled(Player player) {
			player.blackBelt = true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.HermesBoots);
			recipe.AddIngredient(Ravel.ID);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.SailfishBoots);
			recipe.AddIngredient(Ravel.ID);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.FlurryBoots);
			recipe.AddIngredient(Ravel.ID);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
	public class Speed_Ravel_Mount : Ravel_Mount {
		public override string Texture => "Origins/Items/Accessories/Speed_Ravel";
		public static new int ID { get; private set; } = -1;
		protected override void SetID() {
			MountData.buff = ModContent.BuffType<Speed_Ravel_Mount_Buff>();
			ID = Type;
		}
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			MountData.acceleration = 0.38f;
			MountData.runSpeed = 12f;
		}
	}
	public class Speed_Ravel_Mount_Buff : Ravel_Mount_Buff {
		public override string Texture => "Origins/Buffs/Ravel_Generic_Buff";
		protected override int MountID => ModContent.MountType<Speed_Ravel_Mount>();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Speed Ravel");
			Description.SetDefault("10% chance to dodge. Move very fast");
		}
	}
}
