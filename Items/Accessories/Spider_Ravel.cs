using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Spider_Ravel : Ravel {
        public static new int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Spider Ravel");
            Tooltip.SetDefault("Double tap down to transform into a small, rolling ball\nYou may cling to walls and ceilings when raveled");
            SacrificeTotal = 1;
            ID = Type;
        }
        public override void SetDefaults() {
            Item.width = 24;
            Item.height = 24;
            Item.accessory = true;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 8);
            Item.shoot = ModContent.MountType<Spider_Ravel_Mount>();
        }
		protected override void UpdateRaveled(Player player) {
            player.GetModPlayer<OriginPlayer>().spiderRavel = true;
            player.blackBelt = true;
        }
		public override void AddRecipes() {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Cobweb, 270);
            recipe.AddIngredient(ItemID.SpiderFang, 50);
            recipe.AddIngredient(Ravel.ID);
            recipe.AddTile(TileID.TinkerersWorkbench);
            recipe.Register();
		}
	}
    public class Spider_Ravel_Mount : Ravel_Mount {
        public override string Texture => "Origins/Items/Accessories/Spider_Ravel";
        public static new int ID { get; private set; } = -1;
        protected override void SetID() {
            MountData.buff = ModContent.BuffType<Spider_Ravel_Mount_Buff>();
            ID = Type;
        }
        public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity) {
            OriginPlayer originPlayer = mountedPlayer.GetModPlayer<OriginPlayer>();
            const float factor = 10f / 12f;
            if (originPlayer.ceilingRavel) {
                mountedPlayer.mount._frameCounter -= velocity.X * factor;
            } else {
                mountedPlayer.mount._frameCounter += velocity.X * factor;
                if (originPlayer.collidingX) {
                    mountedPlayer.mount._frameCounter -= velocity.Y * originPlayer.oldXSign * factor;
                }
            }
            return false;
        }
    }
    public class Spider_Ravel_Mount_Buff : Ravel_Mount_Buff {
        public override string Texture => "Origins/Buffs/Ravel_Generic_Buff";
		protected override int MountID => ModContent.MountType<Spider_Ravel_Mount>();
        public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Spider Ravel");
            Description.SetDefault("10% chance to dodge. Able to climb different surfaces");
        }
    }
}
