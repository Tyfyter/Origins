using Origins.Buffs;
using Origins.Items.Materials;
using Origins.NPCs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
	public class Scrap_Dart : ModItem {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CursedDart);
			Item.damage = 11;
			Item.shoot = ModContent.ProjectileType<Scrap_Dart_P>();
			Item.shootSpeed = 3f;
			Item.knockBack = 2.2f;
			Item.value = Item.sellPrice(copper: 6);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 100)
			.AddIngredient(ModContent.ItemType<Phoenum>())
			.Register();
		}
	}
	public class Scrap_Dart_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Ammo/Scrap_Dart";
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.CursedDart);
		}
		public override void AI() {
			Projectile.localAI[0] += 1f;
			if (Projectile.localAI[0] > 3f)
				Projectile.alpha = 0;

			if (Projectile.ai[0] >= 20f) {
				Projectile.ai[0] = 20f;
				Projectile.velocity.Y += 0.075f;
			}
			if (Projectile.ai[1].CycleUp(10)) {
				Projectile.SpawnProjectile(null,
					Projectile.Center,
					Main.rand.NextVector2Unit() * Main.rand.NextFloat(2, 4),
					ModContent.ProjectileType<Scrap_Dart_Shrapnel>(),
					Projectile.damage,
					Projectile.knockBack
				);
			}
		}
		public override Color? GetAlpha(Color lightColor) {
			return Projectile.alpha == 0 ? new Color(255, 255, 255, 200) : Color.Transparent;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictImpedingShrapnel(target, 120);
		}
		public class Scrap_Dart_Shrapnel : Impeding_Shrapnel_Shard {
			public override DamageClass DamageType => DamageClass.Ranged;
		}
	}
}
