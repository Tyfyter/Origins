using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Mounts {
    public class Spider_Ravel : Ravel {
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
			Item.mountType = ModContent.MountType<Spider_Ravel_Mount>();
            Item.glowMask = glowmask;
        }
		protected override void UpdateRaveled(Player player) {
			player.GetModPlayer<OriginPlayer>().spiderRavel = true;
			player.blackBelt = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient(ItemID.Cobweb, 270)
			.AddIngredient(ItemID.SpiderFang, 10)
			.AddIngredient(Ravel.ID)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Spider_Ravel_Mount : Ravel_Mount {
		public override string Texture => "Origins/Items/Mounts/Spider_Ravel";
		public static new int ID { get; private set; }
		protected override void SetID() {
			MountData.buff = ModContent.BuffType<Spider_Ravel_Mount_Buff>();
			ID = Type;
		}
	}
	public class Spider_Ravel_Mount_Buff : Ravel_Mount_Buff {
		public override string Texture => "Origins/Buffs/Ravel_Generic_Buff";
		protected override int MountID => ModContent.MountType<Spider_Ravel_Mount>();
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
		}
	}
}
