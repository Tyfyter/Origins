using Microsoft.Xna.Framework.Graphics;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Projectiles.Weapons;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Soulslasher : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Sword
		];
		public override void SetStaticDefaults() {
			OriginsSets.Items.SwungNoMeleeMelees[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 27;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.width = 48;
			Item.height = 48;
			Item.useTime = 27;
			Item.useAnimation = 27;
			Item.shoot = ModContent.ProjectileType<Soulslasher_Swing>();
			Item.shootSpeed = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6f;
			Item.useTurn = false;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item60;
			Item.ArmorPenetration = 0;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.DemoniteBar, 10)
			.AddIngredient(ItemID.ShadowScale, 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool MeleePrefix() => true;
	}
	public class Soulslasher_Swing : MeleeSlamProjectile {
		public override bool CanHitTiles() => false;
		public override float GetSwingFactor() {
			Player player = Main.player[Projectile.owner];
			float swingFactor = 1 - player.itemTime / (float)player.itemTimeMax;
			return MathHelper.Lerp(MathF.Pow(swingFactor, 2f), MathF.Pow(swingFactor, 0.5f), swingFactor * swingFactor);
		}
		public override float GetRotation(float swingFactor) {
			return base.GetRotation(MathF.Pow(swingFactor, 1.2f));
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.usesLocalNPCImmunity = false;
		}
		public override void AI() {
			base.AI();
			if (Projectile.rotation * Projectile.ai[1] > -0.5f && Projectile.localAI[0] == 0) {
				Projectile.localAI[0] = 1;
				Player player = Main.player[Projectile.owner];
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					player.MountedCenter + Projectile.velocity.SafeNormalize(Vector2.Zero) * 48 * Projectile.scale,
					Projectile.velocity,
					ModContent.ProjectileType<Soulslasher_P>(),
					Projectile.damage,
					Projectile.knockBack
				);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			DrawData data = GetDrawData(lightColor, new Vector2(8, 50));
			Main.EntitySpriteDraw(data);
			return false;
		}
	}
	public class Soulslasher_P : ModProjectile {
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 19;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.DD2SquireSonicBoom);
			Projectile.DamageType = DamageClass.Melee;
			Projectile.extraUpdates = 0;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 80;
			Projectile.usesLocalNPCImmunity = false;
		}
		public override void AI() {
			if (Projectile.timeLeft >= 40) {
				Projectile.rotation = Projectile.velocity.ToRotation();
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= Projectile.timeLeft / 80f; // falls off to 50% damage at max range
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Projectile.penetrate == 1) {
				Projectile.penetrate = 2;
				Projectile.timeLeft = 40;
				Projectile.velocity = Vector2.Zero;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.timeLeft < 40) {
				return false;
			}
			Rectangle hitbox;
			Vector2 offset = Vector2.Zero;
			Vector2 velocity = Vector2.UnitY * 8;
			for (int n = 0; n < 4; n++) {
				offset += velocity;
				hitbox = projHitbox;
				hitbox.Offset(offset.ToPoint());
				if (hitbox.Intersects(targetHitbox)) return true;
				hitbox = projHitbox;
				hitbox.Offset((-offset).ToPoint());
				if (hitbox.Intersects(targetHitbox)) return true;
			}
			return null;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2[] positions = new Vector2[Projectile.oldPos.Length + 1];
			Projectile.oldPos.CopyTo(positions, 1);
			positions[0] = Projectile.position;
			Vector2 halfSize = new Vector2(Projectile.width, Projectile.height) * 0.5f;
			int alpha = 0;
			int min = Math.Max(40 - Projectile.timeLeft, 0);
			for (int i = positions.Length - 1; i >= min; i--) {
				if ((i & 3) != 0) {
					continue;
				}
				alpha = (20 - i) * 10;
				Main.EntitySpriteDraw(
					texture,
					(positions[i] + halfSize) - Main.screenPosition,
					null,
					new Color(alpha, alpha, alpha, alpha),
					Projectile.rotation,
					texture.Size() * 0.5f,
					Projectile.scale,
					Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
					0);
			}
			return false;
		}
	}
	public class Soulslasher_Crit_Type : CritType<Soulslasher> {
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => projectile?.ModProjectile is Soulslasher_Swing;
		public override float CritMultiplier(Player player, Item item) => 1.4f;
	}
}
