using Microsoft.Xna.Framework;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Magic {
    public class Area_Denial : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.OtherMagic
        ];
        public override void SetStaticDefaults() {
            Item.staff[Item.type] = true;
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Item.damage = 20;
            Item.noMelee = true;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 37;
			Item.useAnimation = 37;
			Item.shoot = ModContent.ProjectileType<Area_Denial_P>();
			Item.shootSpeed = 12f;
			Item.mana = 13;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item67;
			Item.autoReuse = false;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2) {
				foreach (Projectile projectile in Main.ActiveProjectiles) {
					if (projectile.owner == player.whoAmI && projectile.type == Item.shoot) {
						projectile.Kill();
					}
				}
				return false;
			}
			return true;
		}
	}
	public class Area_Denial_P : ModProjectile {
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.timeLeft = 5 * 60 * 60;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = 96;
			Projectile.ai[1] = Projectile.Distance(Main.MouseWorld) / Projectile.velocity.Length();
			if (source is EntitySource_Parent ps && ps.Entity is Player owner) Projectile.ai[0] = owner.GetModPlayer<OriginPlayer>().explosiveBlastRadius.ApplyTo(Projectile.ai[0]);
		}
		public override void AI() {
			if (Projectile.ai[1] > 0) {
				if (--Projectile.ai[1] <= 0) {
					Projectile.timeLeft = 5 * 60 * 60;
					Projectile.velocity = Vector2.Zero;
				}
			} else {
				if (Main.LocalPlayer.ownedProjectileCounts[Type] > 1) Projectile.Kill();
				if (Projectile.timeLeft % 60 == 0) {
					Projectile.velocity = Vector2.Zero;
					Vector2 pos = Projectile.Center; // + new Vector2(Main.rand.NextFloat(-Projectile.ai[0], Projectile.ai[0]), Main.rand.NextFloat(-Projectile.ai[0], Projectile.ai[0]));
					Projectile.NewProjectile(
						Projectile.GetSource_FromAI(),
						pos,
						default,
						ModContent.ProjectileType<Area_Denial_Explosion>(),
						Projectile.damage,
						Projectile.knockBack,
						Projectile.owner
					);
				}
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Projectile.ai[1] = 0;
            float speed = oldVelocity.Length();
            Projectile.velocity = Projectile.velocity + Projectile.velocity - oldVelocity;
            Projectile.velocity *= speed / Projectile.velocity.Length() * 0.35f;
			Projectile.timeLeft = 5 * 60 * 60;
			Projectile.tileCollide = true;
			return false;
		}
	}
	public class Area_Denial_Explosion : ModProjectile {
		public override string Texture => "Origins/CrossMod/Thorium/Items/Weapons/Bard/Sonorous_Shredder_P";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.friendly = true;
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
		public bool IsExploding() => true;
	}
}
