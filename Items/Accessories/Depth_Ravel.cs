using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Depth_Ravel : Ravel {
		public static new int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
            Item.ResearchUnlockCount = 1;
			ID = Type;
		}
        static short glowmask;
        public override void SetDefaults() {
			Item.DefaultToAccessory();
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 6);
			Item.shoot = ModContent.MountType<Depth_Ravel_Mount>();
            Item.glowMask = glowmask;
        }
		protected override void UpdateRaveled(Player player) {
			player.accFlipper = true;
			player.breathMax += 126;
			player.ignoreWater = true;
			player.blackBelt = true;
			Lighting.AddLight(player.Center, new Vector3(1.5f, 0.85f, 0));
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.ArcticDivingGear);
			recipe.AddIngredient(ItemID.Flipper);
			recipe.AddIngredient(Ravel.ID);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
	public class Depth_Ravel_Mount : Ravel_Mount {
		public override string Texture => "Origins/Items/Accessories/Depth_Ravel";
		public static new int ID { get; private set; } = -1;
		protected override void SetID() {
			MountData.buff = ModContent.BuffType<Depth_Ravel_Mount_Buff>();
			ID = Type;
		}
	}
	public class Depth_Ravel_Mount_Buff : Ravel_Mount_Buff {
		public override string Texture => "Origins/Buffs/Ravel_Generic_Buff";
		protected override int MountID => ModContent.MountType<Depth_Ravel_Mount>();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
		}
	}
}
