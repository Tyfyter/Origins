using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Weapons.Ammo.Canisters;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Eruption : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Launcher",
			"CanistahUser"
		];
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Eruption_P>(27, 32, 16f, 50, 26, true);
			Item.knockBack = 6;
			Item.reuseDelay = 6;
			Item.value = Item.sellPrice(silver:50);
			Item.rare = ItemRarityID.Orange;
			Item.ArmorPenetration += 3;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.HellstoneBar, 18)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-2, 0);
		}
	}
	public class Eruption_P : ModProjectile, ICanisterProjectile {
		public static AutoLoadingAsset<Texture2D> outerTexture = typeof(Eruption_P).GetDefaultTMLName() + "_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = typeof(Eruption_P).GetDefaultTMLName() + "_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.hide = false;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.friendly = false;
		}
		public override void AI() {
			if (Projectile.owner == Main.myPlayer) {
				Rectangle hitbox = Projectile.Hitbox;
				if (!Projectile.TryGetGlobalProjectile(out CanisterGlobalProjectile global) || !global.CanisterData.HasSpecialEffect) {
					const int height = 16 * 15;
					hitbox.Y -= height;
					hitbox.Height += height;
				}
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy(Projectile) && npc.Hitbox.Intersects(hitbox) && Collision.CanHit(hitbox.Location.ToVector2(), hitbox.Width, hitbox.Height, npc.position, npc.width, npc.height)) {
						Projectile.Kill();
						return;
					}
				}
			}
			Projectile.velocity.Y += 0.3f;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (oldVelocity.X != Projectile.velocity.X) {
				Projectile.velocity.X *= -0.3f;
				Projectile.velocity.Y *= 0.9f;
			}
			if (oldVelocity.Y != Projectile.velocity.Y) {
				Projectile.velocity.X *= 0.9f;
				Projectile.velocity.Y *= -0.3f;
			}
			return false;
		}
		public override bool PreKill(int timeLeft) {
			Projectile.velocity.X = 0;
			Projectile.velocity.Y = -8;
			return true;
		}
		public void DefaultExplosion(Projectile projectile, int fireDustType = DustID.Torch, int size = 96) {
			Projectile.NewProjectile(
				Projectile.GetSource_FromAI(),
				Projectile.Center,
				-Vector2.UnitY,
				ModContent.ProjectileType<Eruption_Geyser>(),
				Projectile.damage,
				Projectile.knockBack,
				Main.myPlayer
			);
		}
	}
	public class Eruption_Geyser : ModProjectile, ICanisterChildProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GeyserTrap;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.GeyserTrap);
			Projectile.trap = false;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
		}
	}
}
