using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Acrid_Drill : ModItem {
		static short glowmask;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Drill");
			Tooltip.SetDefault("Increased mining speed when submerged");
			glowmask = Origins.AddGlowMask(this);
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TitaniumDrill);
			Item.damage = 28;
			Item.pick = 195;
			Item.width = 28;
			Item.height = 26;
			Item.knockBack *= 2f;
			Item.shootSpeed = 56f;
			Item.shoot = ModContent.ProjectileType<Acrid_Drill_P>();
			Item.value = Item.buyPrice(gold: 4, silver: 4);
			Item.rare = ItemRarityID.LightRed;
			Item.glowMask = glowmask;
		}
		public override float UseTimeMultiplier(Player player) {
			return player.wet ? 1.5f : 1;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 15);
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
			Projectile.position -= Projectile.velocity.SafeNormalize(default) * 4;
			if (Main.player[Projectile.owner].wet) ++Projectile.frameCounter;
			if (++Projectile.frameCounter > 4) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame > 1) {
					Projectile.frame = 0;
				}
			}
		}
	}
}
