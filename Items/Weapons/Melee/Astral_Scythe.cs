using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Items.Weapons.Summoner;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.Projectiles;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.Items.Weapons.Melee {
	[LegacyName("Splitting_Image")]
	public class Astral_Scythe : ModItem {
		public bool altMode { get; set; }
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			OriginsSets.Items.SwungNoMeleeMelees[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 42;
			Item.DamageType = DamageClass.Melee;
			Item.noUseGraphic = true;
			Item.crit = -4;
			Item.width = 100;
			Item.height = 98;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.shoot = ModContent.ProjectileType<Astral_Scythe_Slash>();
			Item.shootSpeed = 6;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 6;
			Item.useTurn = false;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item71.WithPitch(-1f);
			Item.autoReuse = false;
		}
		public override bool CanRightClick() => !ItemSlot.ShiftInUse;
		public override void RightClick(Player player) {
			altMode ^= true;
		}
		public override bool ConsumeItem(Player player) {
			return false;
		}
		public override bool MeleePrefix() => true;
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Aetherite_Bar>(15)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool AltFunctionUse(Player player) {
			return !altMode;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				if (altMode) {
					type = Astral_Scythe_Chunk_Spawn.ID;
				} else {
					type = Astral_Scythe_Chunk_Spawn.ID;
					position = Main.MouseWorld;
				}
				damage = (int)(damage / 1.3f);
			} else {
				const float sqrt_2 = 1.4142135623731f;
				velocity += new Vector2(sqrt_2 * player.direction, -sqrt_2);
			}
		}
	}
	public class Astral_Scythe_Slash : ModProjectile, IPreDrawSceneProjectile, ITriggerSCBackground {
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.TerraBlade2Shot}";
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.TerraBlade2Shot);
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.noEnchantmentVisuals = true;
		}
		public override void AI() {/*
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
			//player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2);
			Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, realRotation - MathHelper.PiOver2) + GeometryUtils.Vec2FromPolar(32, realRotation);
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
			}*/
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {/*
			if (Projectile.ai[2] == 1) return false;
			Vector2 vel = Projectile.velocity.RotatedBy(Projectile.rotation) * Projectile.width * 0.4f;
			for (int j = 0; j <= ExtraHitboxes; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					return true;
				}
			}*/
			return base.Colliding(projHitbox, targetHitbox);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			//OriginGlobalNPC.InflictTorn(target, 20, targetSeverity: 0.4f, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}
		public override bool PreDraw(ref Color lightColor) {
			if (middleRenderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return false;
			}

			Origins.shaderOroboros.Capture();

			Main.graphics.GraphicsDevice.Textures[1] = middleRenderTarget;
			Accretion_Ribbon.EraseShader.Shader.Parameters["uImageSize1"]?.SetValue(new Vector2(middleRenderTarget.Width, middleRenderTarget.Height));
			Origins.shaderOroboros.Stack(Accretion_Ribbon.EraseShader);
			Origins.shaderOroboros.DrawContents(edgeRenderTarget, Color.White, Main.GameViewMatrix.EffectMatrix);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = edgeRenderTarget.Size() * 0.5f;
			Main.EntitySpriteDraw(
				edgeRenderTarget,
				center,
				null,
				Color.White,
				0,
				center,
				Vector2.One / Main.GameViewMatrix.Zoom,
				SpriteEffects.None
			);

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				texture.Size() - new Vector2(3, 6),
				Projectile.scale,
				SpriteEffects.None
			);
			return false;
		}
		public void PreDrawScene() {
			if (SC_Phase_Three_Midlay.DrawnMaskSources.Add(Projectile)) {
				Texture2D circle = TextureAssets.Projectile[Type].Value;
				SC_Phase_Three_Midlay.DrawDatas.Add(new(
					circle,
					Projectile.Center - Main.screenPosition,
					null,
					Color.White
				) {
					origin = circle.Size() * 0.5f,
					scale = Vector2.One * Projectile.scale
				});
			}
		}
		public override void OnKill(int timeLeft) {
			if (middleRenderTarget is not null) {
				SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref middleRenderTarget);
				SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref edgeRenderTarget);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D middleRenderTarget;
		internal RenderTarget2D edgeRenderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref middleRenderTarget);
			SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref edgeRenderTarget);
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (middleRenderTarget is not null && !middleRenderTarget.IsDisposed) return;
			middleRenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			edgeRenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
	}
	public class Astral_Scythe_Chunk_Spawn : ModProjectile, IPreDrawSceneProjectile, ITriggerSCBackground {
		public override string Texture => "Terraria/Images/Misc/StarDustSky/Planet";
		public static int ID { get; private set; }
		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.tileCollide = false;
			ID = Type;
			Projectile.localAI = [-1, -1, 0];
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				if (MathUtils.LinearSmoothing(ref Projectile.ai[1], 0.6f, 1 / 60f)) Projectile.ai[0] = 1;
			} else if (Projectile.ai[0]++ > 3 * 60) {
				if (Projectile.localAI[0] == -1) {
					Projectile.localAI[0] = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center - new Vector2(10, 0), default, Astral_Scythe_Chunk.ID, Projectile.damage, Projectile.knockBack, Projectile.owner);
					Projectile.localAI[1] = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + new Vector2(10, 0), default, Astral_Scythe_Chunk.ID, Projectile.damage, Projectile.knockBack, Projectile.owner);
				}
				if (MathUtils.LinearSmoothing(ref Projectile.ai[1], 0, 1 / 60f)) {
					Projectile.ai[0] = 0;
					Projectile.Kill();
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			if (middleRenderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return false;
			}

			Origins.shaderOroboros.Capture();

			Main.graphics.GraphicsDevice.Textures[1] = middleRenderTarget;
			Accretion_Ribbon.EraseShader.Shader.Parameters["uImageSize1"]?.SetValue(new Vector2(middleRenderTarget.Width, middleRenderTarget.Height));
			Origins.shaderOroboros.Stack(Accretion_Ribbon.EraseShader);
			Origins.shaderOroboros.DrawContents(edgeRenderTarget, Color.White, Main.GameViewMatrix.EffectMatrix);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = edgeRenderTarget.Size() * 0.5f;
			Main.EntitySpriteDraw(
				edgeRenderTarget,
				center,
				null,
				Color.White,
				0,
				center,
				Vector2.One / Main.GameViewMatrix.Zoom,
				SpriteEffects.None
			);/*

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				texture.Size() - new Vector2(3, 6),
				Projectile.scale,
				SpriteEffects.None
			);*/
			return false;
		}
		public void PreDrawScene() {
			if (SC_Phase_Three_Midlay.DrawnMaskSources.Add(Projectile)) {
				Texture2D circle = TextureAssets.Projectile[Type].Value;
				SC_Phase_Three_Midlay.DrawDatas.Add(new(
					circle,
					Projectile.Center - Main.screenPosition,
					null,
					Color.White
				) {
					origin = circle.Size() * 0.5f,
					scale = Vector2.One * Projectile.scale * Projectile.ai[1]
				});
			}
		}
		public override void OnKill(int timeLeft) {
			if (middleRenderTarget is not null) {
				SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref middleRenderTarget);
				SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref edgeRenderTarget);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D middleRenderTarget;
		internal RenderTarget2D edgeRenderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref middleRenderTarget);
			SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref edgeRenderTarget);
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (middleRenderTarget is not null && !middleRenderTarget.IsDisposed) return;
			middleRenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			edgeRenderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[0]);
			writer.Write(Projectile.localAI[1]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[0] = reader.ReadSingle();
			Projectile.localAI[1] = reader.ReadSingle();
		}
	}
	public class Astral_Scythe_Chunk : ModProjectile {
		public override string Texture => typeof(Shimmer_Chunk1).GetDefaultTMLName();
		//private AutoLoadingAsset<Texture2D>[] textures = [];
		public static int ID { get; private set; }
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Projectile.friendly = true;
			Projectile.width = 56;
			Projectile.height = 56;
			Projectile.aiStyle = -1;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
			Projectile.tileCollide = false;
			Projectile.localAI[0] = Main.rand.Next(3);
			ID = Type;
		}
		private void DoExplode(Entity? tgt) {
			Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.position, new(), ProjectileID.TerraBlade2Shot, Projectile.damage, 0, Projectile.owner);
			Projectile.active = false;
			if (tgt is not null && tgt is Projectile) Projectile.active = false;
		}
		public override void AI() {
			for (int i = 0; i < Main.npc.Length; i++) {
				NPC tgt = Main.npc[i];
				if (tgt.active && !tgt.friendly && !tgt.immortal && !OriginsSets.NPCs.TargetDummies[tgt.type] && Projectile.Hitbox.Intersects(tgt.Hitbox)) {
					DoExplode(tgt);
					break;
				}
				if (i == Main.npc.Length - 1) {
					for (int j = 0; j < Main.projectile.Length; j++) {
						Projectile tgt1 = Main.projectile[j];
						if (tgt1.active && tgt1?.ModProjectile is Astral_Scythe_Chunk) {
							DoExplode(tgt1);
							break;

						}
					}
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {

			return true;
		}
	}
}