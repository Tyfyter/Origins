using Microsoft.Xna.Framework;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
namespace Origins.Items.Weapons.Ammo {
	public class Explosive_Harpoon : ModItem {
		public static int ID { get; private set; }
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 99;
			ID = Type;
		}
		public override void SetDefaults() {
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.consumable = true;
			Item.maxStack = 99;
			Item.shoot = Explosive_Harpoon_P.ID;
			Item.ammo = Harpoon.ID;
			Item.value = Item.sellPrice(silver: 9);
			Item.rare = ItemRarityID.Green;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 4)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddRecipeGroup(RecipeGroupID.IronBar, 4)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(Type, 4)
			.AddIngredient(ItemID.ExplosivePowder)
			.AddIngredient(ModContent.ItemType<Harpoon>(), 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Explosive_Harpoon_P : Harpoon_P {
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
			if (Projectile.penetrate >= 0) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Explosive_Harpoon_Explosion>(),
					Projectile.damage * (int)2.7,
					Projectile.knockBack * 2,
					Projectile.owner
				);
			}
		}
	}
	public class Explosive_Harpoon_Explosion : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5;
			Projectile.hide = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item62);
				Projectile.ai[0] = 1;
			}
			if (Projectile.owner == Main.myPlayer && Projectile.ai[1] == 0) {
				Player player = Main.LocalPlayer;
				if (player.active && !player.dead && !player.immune) {
					Rectangle projHitbox = Projectile.Hitbox;
					ProjectileLoader.ModifyDamageHitbox(Projectile, ref projHitbox);
					Rectangle playerHitbox = new Rectangle((int)player.position.X, (int)player.position.Y, player.width, player.height);
					if (projHitbox.Intersects(playerHitbox)) {
						player.Hurt(
							PlayerDeathReason.ByProjectile(Main.myPlayer, Projectile.whoAmI),
							Main.DamageVar(Projectile.damage, -player.luck),
							Math.Sign(player.Center.X - Projectile.Center.X),
							true
						);
						Projectile.ai[1] = 1;
					}
				}
			}
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding => true;
	}
}
