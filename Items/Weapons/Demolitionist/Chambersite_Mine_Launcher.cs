using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Tiles.Other;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Chambersite_Mine_Launcher : ModItem {
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Chambersite_Mine>(38, 28, 7.5f, 48, 32);
			Item.DamageType = ModContent.GetInstance<Chambersite_Mine_Launcher_Damage>();
			Item.knockBack = 4f;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 2);
			Item.UseSound = SoundID.Item62.WithPitch(0.4f);
			Item.reuseDelay = 50;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofNight, 16)
			.AddIngredient<Carburite_Item>(22)
			.AddIngredient<Chambersite_Item>(10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-6f, 0);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			for (int i = 5; i-- > 0;) {
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
	public class Chambersite_Mine : ModProjectile, ICanisterProjectile {
		public override string Texture => typeof(Chambersite_Mine_Launcher).GetDefaultTMLName();
		public static AutoLoadingAsset<Texture2D> outerTexture = typeof(Chambersite_Mine).GetDefaultTMLName() + "_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = typeof(Chambersite_Mine).GetDefaultTMLName() + "_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
			Origins.MagicTripwireDetonationStyle[Type] = 2;
		}
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.aiStyle = 0;
			Projectile.DamageType = ModContent.GetInstance<Chambersite_Mine_Launcher_Damage>();
			Projectile.timeLeft = 240;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override void AI() {
			this.DoGravity(0.04f);
			Projectile.rotation += Projectile.velocity.Length() * Projectile.direction * 0.03f;
			if (Projectile.owner == Main.myPlayer && Projectile.timeLeft == 240) Projectile.timeLeft -= Main.rand.Next(30);
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			float gravMult = Projectile.GetGlobalProjectile<CanisterGlobalProjectile>().gravityMultiplier;
			float bounce = 1f - gravMult * 0.5f;
			Vector2 friction = Vector2.One;
			if (Projectile.velocity.X != oldVelocity.X) {
				Projectile.velocity.X = oldVelocity.X * -bounce;
				friction.Y *= 0.9f;
			}
			if (Projectile.velocity.Y != oldVelocity.Y) {
				Projectile.velocity.Y = oldVelocity.Y * -bounce;
				friction.X *= 0.9f;
			}
			Projectile.velocity *= friction;
			return false;
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			Vector2 origin = OuterTexture.Value.Size() * 0.5f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
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
