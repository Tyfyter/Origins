using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Items.Materials;

namespace Origins.Items.Tools {
	public class Acrid_Drill : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Drill");
			Tooltip.SetDefault("Non-corrosive");
			glowmask = Origins.AddGlowMask(this);
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.TitaniumDrill);
			Item.damage = 28;
            Item.pick = 195;
			Item.width = 34;
			Item.height = 32;
			Item.knockBack*=2f;
            Item.shootSpeed = 56f;
            Item.shoot = ModContent.ProjectileType<Acrid_Drill_P>();
			Item.value = 3600;
			Item.rare = ItemRarityID.LightRed;
			Item.glowMask = glowmask;
		}
        public override float UseTimeMultiplier(Player player) {
            return player.wet?1.5f:1;
        }
		public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Acrid_Bar>(), 15);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
    public class Acrid_Drill_P : ModProjectile {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Drill");
			Main.projFrames[Projectile.type] = 2;
		}
		public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.TitaniumDrill);
		}
        public override void AI() {
            if(Main.player[Projectile.owner].wet)++Projectile.frameCounter;
			if (++Projectile.frameCounter > 4) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame > 1) {
					Projectile.frame = 0;
				}
			}
        }
    }
}
