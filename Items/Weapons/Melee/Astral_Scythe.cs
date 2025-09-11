using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Projectiles;
using PegasusLib;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	[LegacyName("Splitting_Image")]
	public class Astral_Scythe : ModItem {
		public override void SetStaticDefaults() {
			OriginsSets.Items.SwungNoMeleeMelees[Type] = true;
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
		}
		public override void SetDefaults() {
			Item.damage = 64;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = true;
			Item.crit = -2;
			Item.noMelee = true;
			Item.width = 100;
			Item.height = 98;
			Item.useTime = 42;
			Item.useAnimation = 42;
			Item.shoot = ModContent.ProjectileType<Astral_Scythe_Slash>();
			Item.shootSpeed = 6;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 8;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item71.WithPitch(-1f);
			Item.autoReuse = false;
		}
		public override bool CanUseItem(Player player) {
			if (OriginsModIntegrations.CheckAprilFools()) return true;
			return !player.HasBuff<Astral_Scythe_Wait_Debuff>();
		}
		public override bool MeleePrefix() => true;
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Aetherite_Bar>(20)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool AltFunctionUse(Player player) {
			return !player.HasBuff<Astral_Scythe_Wait_Debuff>();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				int frameReduction = player.itemAnimationMax / 2;
				player.itemTime -= frameReduction;
				player.itemTimeMax -= frameReduction;
				player.itemAnimation -= frameReduction;
				player.itemAnimationMax -= frameReduction;

				type = 0;
				damage = (int)(damage * 0.3f);
				player.OriginPlayer().scytheHitCombo = 0;
				player.AddBuff(Astral_Scythe_Wait_Debuff.ID, 3 * 60);
			} else {
				const float sqrt_2 = 1.4142135623731f;
				velocity += new Vector2(sqrt_2 * player.direction, -sqrt_2);
				if (OriginsModIntegrations.CheckAprilFools() && player.HasBuff<Astral_Scythe_Wait_Debuff>()) damage = (int)(damage * 0.5f);
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int ai0 = 0;
			if (OriginsModIntegrations.CheckAprilFools() && player.HasBuff<Astral_Scythe_Wait_Debuff>()) ai0 = 2;
			else if (player.OriginPlayer().scytheHitCombo >= OriginPlayer.maxScytheHitCombo) ai0 = 1;

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, ai0: ai0);
			return false;
		}
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Texture2D texture = TextureAssets.Item[Type].Value;
			Player player = Main.LocalPlayer;
			bool hasDebuff = player.HasBuff<Astral_Scythe_Wait_Debuff>();

			int variant = 0;
			if (player.OriginPlayer().scytheHitCombo >= OriginPlayer.maxScytheHitCombo && !hasDebuff) variant = 2;
			if (hasDebuff) variant = 1;

			frame = texture.Frame(verticalFrames: 3, frameY: variant);
			spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, drawColor, 0, origin, scale, SpriteEffects.None, 0);
			return false;
		}
	}
	public class Astral_Scythe_Slash : ModProjectile {
		public override string Texture => typeof(Astral_Scythe).GetDefaultTMLName();
		public static int ExtraHitboxes => 1;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = 60;
			Projectile.height = 60;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.noEnchantmentVisuals = true;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (Projectile.ai[0] == 2 && !OriginsModIntegrations.CheckAprilFools()) Projectile.Kill();
			if (Projectile.ai[2] == 2) Projectile.ai[2] = 1;
			if (Projectile.ai[2] == 1) {
				Projectile.hide = true;
				Projectile.timeLeft = 2;
				if (Projectile.ai[1] > 1) {
					Projectile.ai[1] = 1;
				} else if (Projectile.ai[1] > 0) {
					Projectile.ai[1] = 0;
				} else {
					Projectile.Kill();
				}
				return;
			}
			Player player = Main.player[Projectile.owner];
			if (player.dead || player.CCed) {
				Projectile.active = false;
				return;
			}

			if (Projectile.ai[0] >= 1) Projectile.Size = new(60 / 1.4f);
			float swingFactor = 1 - player.itemTime / (float)player.itemTimeMax;
			Projectile.rotation = MathHelper.Lerp(-2f, 1.3f, swingFactor) * player.direction;
			float realRotation = Projectile.rotation + Projectile.velocity.ToRotation();
			Projectile.timeLeft = player.itemTime * Projectile.MaxUpdates + 2;
			if (Projectile.timeLeft <= 3) {
				Projectile.ai[2] = 2;
			}
			player.heldProj = Projectile.whoAmI;
			player.SetCompositeArmFront(false, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			Projectile.Center = player.GetCompositeArmPosition(false) + GeometryUtils.Vec2FromPolar(32, realRotation * player.gravDir);
			if (swingFactor < 0.4f) {
				player.bodyFrame.Y = player.bodyFrame.Height * 1;
			} else if (swingFactor < 0.7f) {
				player.bodyFrame.Y = player.bodyFrame.Height * 2;
				Projectile.position.X += 6 * player.direction * (1 - (swingFactor - 0.4f) / 0.6f);
			} else {
				player.bodyFrame.Y = player.bodyFrame.Height * 3;
				Projectile.position.X += 3 * player.direction;
				Projectile.position.Y += 8;
			}

			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation * player.gravDir - MathHelper.PiOver4 * (player.gravDir - 1) * player.direction) * Projectile.width * 0.13f;
			for (int j = 0; j <= ExtraHitboxes; j++) {
				Projectile.EmitEnchantmentVisualsAt(Projectile.position + vel * j, Projectile.width, Projectile.height);
			}
		}
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.ai[0] == 1) Projectile.NewProjectile(source, Projectile.position, Projectile.velocity, ModContent.ProjectileType<Astral_Scythe_Blade>(), (int)(Projectile.damage * 0.2f), Projectile.knockBack, Projectile.owner);
		}
		List<Rectangle> tmp = [];
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.ai[2] == 1) return false;
			Player player = Main.player[Projectile.owner];
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width * 0.13f;
			vel.Y *= player.gravDir;
			tmp.Clear();
			int boxes = ExtraHitboxes;// - (Projectile.ai[0] >= 1 ? 1 : 0);
			for (int j = 0; j <= boxes; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				tmp.Add(hitbox);
				if (hitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Player player = Main.player[Projectile.owner];
			player.OriginPlayer().scytheHitCombo++;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Player player = Main.player[Projectile.owner];
			player.OriginPlayer().scytheHitCombo++;
		}
		public override void CutTiles() {
			Player player = Main.player[Projectile.owner];
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * new Vector2(1, player.gravDir) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			SpriteEffects effects = player.direction * player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			if (player.gravDir < 0) effects ^= SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(verticalFrames: 3, frameY: Projectile.ai[0] >= 1 ? 1 : 0);

			Main.EntitySpriteDraw(
				texture,
				player.GetCompositeArmPosition(false) + GeometryUtils.Vec2FromPolar(8, (Projectile.rotation + Projectile.velocity.ToRotation()) * player.gravDir) - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation * player.gravDir + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * player.direction * player.gravDir) - (player.gravDir < 0).Mul(MathHelper.PiOver2 * player.direction),
				new Vector2(14, 8).Apply(effects ^ SpriteEffects.FlipVertically, texture.Size() / 3),
				Projectile.scale,
				effects
			);
			foreach (Rectangle rct in tmp) rct.DrawDebugOutlineSprite(Color.Blue);
			return false;
		}
	}
	public class Astral_Scythe_Blade : ModProjectile {
		public override string Texture => "Origins/Gores/NPCs/Shimmer_Construct_Piece10";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = 34;
			Projectile.height = 54;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 0;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Entity owner = Projectile.GetRelatedProjectile_Depreciated(2);
			if (owner is null) {
				if (!Projectile.ai[2].TrySet(-1)) Projectile.Kill();
				return;
			}
			Vector2 toOwner = (owner.Center - Projectile.Center).Normalized(out float dist);

			Projectile.position = owner.position;
		}
		public override void OnSpawn(IEntitySource source) {
			int Proj = ModContent.ProjectileType<Astral_Scythe_Slash>();
			Projectile.ai[2] = -1;
			foreach (Projectile proj in Main.ActiveProjectiles) {
				if (proj.owner == Projectile.owner && proj.type == Proj) {
					Projectile.ai[2] = proj.identity;
					break;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			SpriteEffects effects = player.direction * player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			if (player.gravDir < 0) effects ^= SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Main.EntitySpriteDraw(
				texture,
				Projectile.position - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation * player.gravDir + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * player.direction * player.gravDir) - (player.gravDir < 0).Mul(MathHelper.PiOver2 * player.direction),
				new Vector2(14, 8).Apply(effects ^ SpriteEffects.FlipVertically, texture.Size()),
				Projectile.scale,
				effects
			);
			Projectile.Hitbox.DrawDebugOutlineSprite(Color.Red);
			return false;
		}
	}
}
namespace Origins.Buffs {
	public class Astral_Scythe_Wait_Debuff : ModBuff {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
			ID = Type;
		}
	}
}