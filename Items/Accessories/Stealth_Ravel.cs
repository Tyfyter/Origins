using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Stealth_Ravel : Ravel {
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			ID = Type;
            glowmask = Origins.AddGlowMask(this);
        }
        static short glowmask;
        public override void SetDefaults() {
			base.SetDefaults();
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 6);
			Item.mountType = ModContent.MountType<Stealth_Ravel_Mount>();
            Item.glowMask = glowmask;
        }
		protected override void UpdateRaveled(Player player) {
			player.aggro -= 9999;
			player.blackBelt = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.PutridScent)
			.AddIngredient(Ravel.ID)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Stealth_Ravel_Mount : Ravel_Mount {
		public override string Texture => "Origins/Items/Accessories/Stealth_Ravel";
		public static new int ID { get; private set; }
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
		}
	}
}
