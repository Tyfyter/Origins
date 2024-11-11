using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Acrid_Drill : ModItem, ICustomWikiStat {
		static short glowmask;
		public string[] Categories => [
			"Tool"
		];
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TitaniumDrill);
			Item.damage = 28;
			Item.pick = 195;
			Item.width = 28;
			Item.height = 26;
			Item.knockBack = 1f;
			Item.shootSpeed = 56f;
			Item.shoot = ModContent.ProjectileType<Acrid_Drill_P>();
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
			Item.glowMask = glowmask;
		}
		public override float UseTimeMultiplier(Player player) {
			return player.wet ? 1.5f : 1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 15)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Acrid_Drill_P : ModProjectile {
		public override void SetStaticDefaults() {
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
