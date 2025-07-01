using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ammo.Canisters;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Glass_Cannon : ModItem, IElementalItem {
		public ushort Element => Elements.Fiberglass;
		public override void SetStaticDefaults() {
			//Origins.FlatDamageMultiplier[Type] = 1.5f;
		}
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Glass_Cannon_P>(2, 28, 7.5f, 48, 32);
			Item.knockBack = 4f;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 1, silver: 20);
			Item.UseSound = SoundID.Item62.WithPitch(0.4f);
			Item.reuseDelay = 50;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-6f, 0);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = 4; i-- > 0;) {
				Projectile.NewProjectile(
					source,
					position,
					velocity.RotatedByRandom(0.25f) * Main.rand.NextFloat(0.9f, 1f),
					type,
					damage,
					knockback
				);
			}
			return false;
		}
	}
	public class Glass_Cannon_P : ModProjectile, ICanisterProjectile, IElementalProjectile {
		public ushort Element => Elements.Fiberglass;
		public static AutoLoadingAsset<Texture2D> outerTexture = typeof(Glass_Cannon_P).GetDefaultTMLName() + "_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = typeof(Glass_Cannon_P).GetDefaultTMLName() + "_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
			Origins.MagicTripwireDetonationStyle[Type] = 2;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.timeLeft = 120;
			Projectile.scale = 0.85f;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void AI() {
			this.DoGravity(0.08f);
			Projectile.rotation += Projectile.velocity.X * 0.05f;
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			Vector2 origin = OuterTexture.Value.Size() * 0.5f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				projectile.Center - Main.screenPosition,
				null,
				lightColor,
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				InnerTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.InnerColor,
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				OuterTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
		}
	}
}
