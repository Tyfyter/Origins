using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Weapons.Ammo.Canisters;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Cold_Snap : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Launcher",
			"CanistahUser"
		];
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Cold_Snap_P>(15, 32, 12f, 50, 24);
			Item.knockBack = 3;
			Item.reuseDelay = 6;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.ExplosivePowder, 15)
			.AddIngredient(ItemID.IceBlock, 40)
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
	public class Cold_Snap_P : ModProjectile, ICanisterProjectile {
		public static AutoLoadingAsset<Texture2D> outerTexture = typeof(Cold_Snap_P).GetDefaultTMLName() + "_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = typeof(Cold_Snap_P).GetDefaultTMLName() + "_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 32;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Grenade);
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 180;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.velocity.X == 0f) {
				Projectile.velocity.X = -oldVelocity.X;
			}
			if (Projectile.velocity.Y == 0f) {
				Projectile.velocity.Y = -oldVelocity.Y;
			}
			Projectile.timeLeft = 1;
			return true;
		}
		public override void AI() {
			Dust.NewDust(Projectile.Center, 0, 0, DustID.IceTorch, 0, 0, 155, default, 0.75f);
			if (Projectile.ai[0] > 5f) {
				Projectile.velocity.Y -= 0.2f;
				this.DoGravity(0.2f);
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Frostburn, 300);
		}
	}
}
