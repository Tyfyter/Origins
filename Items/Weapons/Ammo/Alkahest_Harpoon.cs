using Microsoft.Xna.Framework;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.NPCs;
namespace Origins.Items.Weapons.Ammo {
	public class Alkahest_Harpoon : ModItem, ICustomWikiStat {
		public static int ID { get; private set; }
        public string[] Categories => [
            "Harpoon"
        ];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			On_Player.IsAmmoFreeThisShot += (orig, self, weapon, ammo, projToShoot) => {
				if (projToShoot == Alkahest_Harpoon_P.ID) return false;
				return orig(self, weapon, ammo, projToShoot);
			};
			ID = Type;
		}
		public override void SetDefaults() {
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.shoot = Alkahest_Harpoon_P.ID;
			Item.ammo = Harpoon.ID;
			Item.value = Item.sellPrice(silver: 9);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient<Alkahest>()
			.AddRecipeGroup(RecipeGroupID.IronBar, 4)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(Type, 4)
			.AddIngredient<Alkahest>()
			.AddIngredient<Harpoon>(4)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Alkahest_Harpoon_P : Harpoon_P {
		public static new int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void AI() {
			if (Projectile.ai[0] == 1 && Projectile.penetrate >= 0) {
				Projectile.aiStyle = 1;
				Projectile.velocity = Projectile.oldVelocity;
				Projectile.tileCollide = true;
				Vector2 diff = Main.player[Projectile.owner].itemLocation - Projectile.Center;
				SoundEngine.PlaySound(SoundID.Item10, Projectile.Center + diff / 2);
				float len = diff.Length() * 0.25f;
				diff /= len;
				Vector2 pos = Projectile.Center;
				for (int i = 0; i < len; i++) {
					Dust.NewDust(pos - new Vector2(2), 4, 4, DustID.Stone, Scale: 0.75f);
					pos += diff;
				}
			}
			if (Projectile.penetrate == 1) {
				Projectile.penetrate--;
			}
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, SoundID.Shatter, smokeDustAmount: 10, smokeGoreAmount: 0, fireDustType: -1);
			for (int i = 0; i < 20; i++) {
				Dust dust = Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.GemTopaz,
					0f,
					0f,
					100,
					default,
					1.5f
				);
				dust.noGravity = true;
				dust.velocity *= 3f;
				Dust.NewDustDirect(
					Projectile.position,
					Projectile.width,
					Projectile.height,
					DustID.GemTopaz,
					0f,
					0f,
					100,
					default,
					0.4f
				);
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 60, 180, 0.5f, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
	}
}
