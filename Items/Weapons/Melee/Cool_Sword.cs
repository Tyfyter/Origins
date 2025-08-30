using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class Cool_Sword : ModItem {
		public override void SetStaticDefaults() {
			OriginsSets.Items.SwungNoMeleeMelees[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 42;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = true;
			Item.crit = -4;
			Item.noMelee = true;
			Item.width = 180;
			Item.height = 188;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.shoot = ModContent.ProjectileType<Cool_Sword_Slash>();
			Item.shootSpeed = 1;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 4;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item71.WithPitch(-1.3f);
			Item.autoReuse = false;
		}
		public override bool MeleePrefix() => true;
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Aetherite_Bar>(15)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			const float sqrt_2 = 1.4142135623731f;
			velocity = new(sqrt_2 * player.direction, -sqrt_2);
		}
	}
	public class Cool_Sword_Slash : ModProjectile {
		public override string Texture => typeof(Cool_Sword_Slash).GetDefaultTMLName();
		public static int ExtraHitboxes => 5;
		public override void SetStaticDefaults() {
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

			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width * 0.4f;
			List<int> noSpawnStarIndices = new(ExtraHitboxes);
			for (int j = 0; j <= ExtraHitboxes; j++) noSpawnStarIndices.Add(j);
			float starThreshold = player.itemTimeMax / 10f;
			Projectile.ai[0]++;
			while (Projectile.rotation < 1 && Projectile.ai[0] >= starThreshold && noSpawnStarIndices.Count > 0) {
				noSpawnStarIndices.RemoveAt(Main.rand.Next(noSpawnStarIndices.Count));
				Projectile.ai[0] -= starThreshold;
			}
			for (int j = 0; j <= ExtraHitboxes; j++) {
				Projectile.EmitEnchantmentVisualsAt(Projectile.position + vel * j, Projectile.width, Projectile.height);
				if (!noSpawnStarIndices.Contains(j)) {
					Projectile.SpawnProjectile(null,
						Projectile.position + vel * j + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height)),
						(realRotation + 1 * player.direction).ToRotationVector2() * 4 + Projectile.velocity * Vector2.UnitX * 4,
						ModContent.ProjectileType<Cool_Sword_P>(),
						Projectile.damage / 3,
						Projectile.knockBack,
						Projectile.identity
					);
				}
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.ai[2] == 1) return false;
			Player player = Main.player[Projectile.owner];
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width * 0.4f;
			vel.Y *= player.gravDir;
			for (int j = 0; j <= ExtraHitboxes; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			//OriginGlobalNPC.InflictTorn(target, 20, targetSeverity: 0.4f, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
		public override void CutTiles() {
			Player player = Main.player[Projectile.owner];
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + Projectile.velocity.RotatedBy(Projectile.rotation).SafeNormalize(Vector2.UnitX) * new Vector2(1, player.gravDir) * 50f * Projectile.scale;
			Utils.PlotTileLine(Projectile.Center, end, 80f * Projectile.scale, DelegateMethods.CutTiles);
		}

		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			SpriteEffects effects = player.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			if (player.gravDir < 0) effects ^= SpriteEffects.FlipVertically;
			Rectangle slashFrame = TextureAssets.Projectile[Type].Value.Frame(verticalFrames: 11, frameY: (int)(8 * (1 - player.itemTime / (float)player.itemTimeMax)));
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				player.MountedCenter - Main.screenPosition,
				slashFrame,
				new Color(0.75f, 0.75f, 0.75f, 0.5f),
				0,
				new Vector2(217, 161).Apply(effects ^ SpriteEffects.FlipVertically, slashFrame.Size()),
				Projectile.scale,
				effects,
				0
			);
			effects = player.direction * player.gravDir > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;
			if (player.gravDir < 0) effects ^= SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;
			Texture2D texture = TextureAssets.Item[ModContent.ItemType<Cool_Sword>()].Value;
			Main.EntitySpriteDraw(
				texture,
				player.GetCompositeArmPosition(false) + GeometryUtils.Vec2FromPolar(8, (Projectile.rotation + Projectile.velocity.ToRotation()) * player.gravDir) - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation * player.gravDir + Projectile.velocity.ToRotation() + (MathHelper.PiOver4 * player.direction * player.gravDir) - (player.gravDir < 0).Mul(MathHelper.PiOver2 * player.direction),
				new Vector2(14, 8).Apply(effects ^ SpriteEffects.FlipVertically, texture.Size()),
				Projectile.scale,
				effects
			);
			return false;
		}
	}
	public class Cool_Sword_P : ModProjectile, ICanisterChildProjectile {
		public override string Texture => "Origins/Projectiles/Ammo/Aether_Star";
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Melee;
			Projectile.friendly = true;
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.aiStyle = -1;
			Projectile.penetrate = 25;
			Projectile.alpha = Main.rand.Next(180, 256);
			Projectile.timeLeft = Main.rand.Next(300, 451);
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			(Projectile.localAI[0], Projectile.localAI[1], Projectile.localAI[2]) = Aether_Canister.color.Value.ToVector3();
		}
		Projectile parent;
		int[] ParentCooldown {
			get {
				if (parent is null) {
					parent = Main.projectile.FirstOrDefault(proj => proj.owner == Projectile.owner && proj.identity == Projectile.ai[0]);
					if (parent is null) {
						Projectile.Kill();
						return Projectile.localNPCImmunity;
					}
				}
				return parent.localNPCImmunity;
			}
		}
		public override void AI() {
			float v = 0.75f + (float)(0.125f * (Math.Sin(Projectile.timeLeft / 5f) + 2 * Math.Sin(Projectile.timeLeft / 60f)));
			Lighting.AddLight(Projectile.Center, Projectile.localAI[0] * v, Projectile.localAI[1] * v, Projectile.localAI[2] * v);
			Projectile.velocity.Y -= 0.1f;
			Projectile.velocity *= 0.997f;
			Projectile.ai[1]++;
			if (parent is not null) parent.ai[1]++;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = height = 2;
			fallThrough = true;
			return true;
		}
		public override bool? CanHitNPC(NPC target) => ParentCooldown[target.whoAmI] == 0 ? null : false;
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = -Projectile.direction;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			ParentCooldown[target.whoAmI] = 10;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) => Projectile.ai[1] > 10;
		public override Color? GetAlpha(Color lightColor) {
			float v = 0.75f + (float)(0.125f * (Math.Sin(Projectile.timeLeft / 5f) + 2 * Math.Sin(Projectile.timeLeft / 60f)));
			return new(Projectile.localAI[0] * v, Projectile.localAI[1] * v, Projectile.localAI[2] * v, Projectile.alpha * v / 255f);
		}
	}
}
