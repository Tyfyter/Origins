using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo.Canisters;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Glass_Cannon : ModItem {
		public override void SetStaticDefaults() {
			Origins.FlatDamageMultiplier[Type] = 1.5f;
		}
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Glass_Cannon_P>(2, 28, 11f, 48, 32);
			Item.knockBack = 4f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 20);
			Item.UseSound = null;
			Item.reuseDelay = 50;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-6f, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = 0; i < 4; i++) {
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(0.9f, 1f),
					type,
					damage,
					knockback
				);
			}
			SoundEngine.PlaySound(SoundID.Item62.WithPitch(0.4f), position);
			return false;
		}
	}
	public class Glass_Cannon_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.timeLeft = 120;
			Projectile.scale = 0.85f;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			Projectile.velocity.Y += 0.08f;
			Projectile.rotation += Projectile.velocity.X * 0.05f;
		}
	}
}
