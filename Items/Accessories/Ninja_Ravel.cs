using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Ninja_Ravel : Ravel {
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
			Item.value = Item.sellPrice(gold: 12);
			Item.shoot = ModContent.MountType<Ninja_Ravel_Mount>();
            Item.glowMask = glowmask;
        }
		protected override void UpdateRaveled(Player player) {
			player.GetModPlayer<OriginPlayer>().spiderRavel = true;
			player.aggro -= 400;
			player.blackBelt = true;
		}
		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(Spider_Ravel.ID);
			recipe.AddIngredient(Stealth_Ravel.ID);
			recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
		}
	}
	public class Ninja_Ravel_Mount : Ravel_Mount {
		public override string Texture => "Origins/Items/Accessories/Ninja_Ravel";
		public static new int ID { get; private set; } = -1;
		protected override void SetID() {
			MountData.buff = ModContent.BuffType<Ninja_Ravel_Mount_Buff>();
			ID = Type;
		}
	}
	public class Ninja_Ravel_Mount_Buff : Ravel_Mount_Buff {
		public override string Texture => "Origins/Buffs/Ravel_Generic_Buff";
		protected override int MountID => ModContent.MountType<Ninja_Ravel_Mount>();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
		}
	}
}
