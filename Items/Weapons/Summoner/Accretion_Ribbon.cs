using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.NPCs;
using Origins.NPCs.MiscB.Shimmer_Construct;
using PegasusLib;
using PegasusLib.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Accretion_Ribbon : ModItem {
		internal static ArmorShaderData EraseShader { get; set; }
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			EraseShader = new(Mod.Assets.Request<Effect>("Effects/Misc"), "Erase");
		}
		public override void SetDefaults() {
			Item.dye = 0;
			Item.DefaultToWhip(ModContent.ProjectileType<Accretion_Ribbon_P>(), 40, 5, 1, 28);
			Item.value = Item.sellPrice(gold: 2, silver: 60);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 1;
			//Item.UseSound = SoundID.Item46.WithPitch(-1f);
		}
		public override bool MeleePrefix() => true;
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 10)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override bool CanShoot(Player player) => false;
		public override bool AltFunctionUse(Player player) => true;
		public override void HoldItem(Player player) {
			if (player.ownedProjectileCounts[Item.shoot] <= 0) {
				Projectile proj = Projectile.NewProjectileDirect(player.GetSource_ItemUse(Item), player.Center, Vector2.UnitX, Item.shoot, Item.damage, player.GetWeaponKnockback(Item), player.whoAmI);
				proj.originalDamage = Item.damage;
			}
		}
	}
	public class Accretion_Ribbon_P : ModProjectile, IPreDrawSceneProjectile, ITriggerSCBackground {
		public override string Texture => base.Texture + "ole";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.IsAWhip[Type] = false; // can't be a whip rn because that overrides custom collision
			ProjectileID.Sets.TrailingMode[Type] = 0;
			ProjectileID.Sets.TrailCacheLength[Type] = 50;
		}
		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.extraUpdates = 0;
			Projectile.aiStyle = -1;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.ContinuouslyUpdateDamageStats = true;
			Projectile.friendly = true;
			Projectile.hide = true;
			Projectile.manualDirectionChange = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.ArmorPenetration += 9;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				Projectile.scale *= itemUse.Item.scale;
				Projectile.localAI[0] = itemUse.Item.prefix;
			}
		}
		public override void AI() {
			float max_dist = 162 * Projectile.scale;
			Player player = Main.player[Projectile.owner];
			if (Projectile.owner == Main.myPlayer && (!player.active || player.dead || player.HeldItem.ModItem is not Accretion_Ribbon || Projectile.localAI[0] != player.HeldItem.prefix)) {
				Projectile.Kill();
				return;
			}
			Projectile.timeLeft = 2;
			Projectile.extraUpdates = 0;
			if (player.altFunctionUse == 2 || player.controlUseTile) {
				Projectile.extraUpdates = 2;
				float maxXVel = Math.Max(Math.Abs(player.velocity.X), Math.Max(player.maxRunSpeed * 2.5f, player.accRunSpeed * 2f));
				bool first = false;
				if (Projectile.direction == 0) {
					first = true;
					Projectile.direction = player.direction;
					if (player.velocity.Y == 0) {
						player.velocity += new Vector2(Projectile.direction * 2, -player.gravDir) * (5.01f + player.jumpSpeedBoost);
					}
				}
				player.velocity.X = float.Clamp(player.velocity.X + Projectile.direction * 0.07f, -maxXVel, maxXVel);
				float compensation = first ? 3 : 1;
				if (first) Projectile.soundDelay -= 2;
				if (Projectile.soundDelay <= 0) {
					SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, player.MountedCenter);
					Projectile.soundDelay = player.itemAnimationMax * Projectile.MaxUpdates;
				}
				compensation *= player.gravDir;
				Projectile.velocity = Projectile.velocity.RotatedBy((MathHelper.PiOver2 * Projectile.direction * compensation / 0.75f) / player.itemAnimationMax);
				if (player.altFunctionUse != 2) Projectile.direction = 0;
			} else if (Projectile.owner == Main.myPlayer) {
				Vector2 diff = Main.MouseWorld - player.MountedCenter;
				Projectile.ai[0] = Math.Min(diff.Length(), max_dist);
				Vector2 velocity = Projectile.velocity;
				MathUtils.LinearSmoothing(ref velocity, diff.SafeNormalize(default), 1);
				if (Projectile.velocity != velocity) {
					Projectile.velocity = velocity;
					Projectile.netUpdate = true;
				}
				Projectile.direction = 0;
				if (player.ItemAnimationActive) {
					Projectile.velocity = Projectile.velocity.RotatedBy(0.4f * MathF.Sin((player.itemAnimation / (float)player.itemAnimationMax) * MathHelper.TwoPi + MathHelper.Pi));
				}
				player.ChangeDir(Math.Sign(velocity.X));
			}
			Vector2 centerDiff = player.MountedCenter - player.position;
			Projectile.position = Vector2.Lerp(player.oldPosition, player.position, 1 - (Projectile.numUpdates + 1) / (float)Projectile.MaxUpdates) + centerDiff + (Projectile.velocity * Projectile.ai[0]).Floor();
			Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
			player.SetCompositeArmFront(true, (Projectile.ai[0] * 4 / max_dist) switch {
				<= 1 => Player.CompositeArmStretchAmount.None,
				<= 2 => Player.CompositeArmStretchAmount.Quarter,
				<= 3 => Player.CompositeArmStretchAmount.ThreeQuarters,
				_ => Player.CompositeArmStretchAmount.Full
			}, Projectile.rotation * Main.LocalPlayer.gravDir + MathHelper.PiOver2 * (Main.LocalPlayer.gravDir - 1));
			player.heldProj = Projectile.whoAmI;
			for (int i = 0; i < partialImmunity.Length; i++) {
				if (partialImmunity[i] > 0) {
					NPC npc = Main.npc[i];
					if (!npc.active || !Projectile.Colliding(default, npc.Hitbox)) {
						partialImmunity[i] = 0;
					} else {
						partialImmunity[i]--;
					}
				}
			}
			if (player.teleportTime >= 0.9f) {
				Array.Clear(Projectile.oldPos);
			}
		}
		readonly int[] partialImmunity = new int[Main.maxNPCs];
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Main.player[Projectile.owner].Center.X);
			if (partialImmunity[target.whoAmI] > 0) {
				modifiers.SourceDamage *= 0.1f;
				modifiers.Knockback *= 0.5f;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (partialImmunity[target.whoAmI] <= 0) partialImmunity[target.whoAmI] = ProjectileID.Sets.TrailCacheLength[Type];
			if (target.life > 0 && target.CanBeChasedBy()) Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;
			target.AddBuff(Accretion_Ribbon_Buff.ID, 240);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float size = 4 * Projectile.scale;
			float _ = 0;
			for (int i = 1; i < Projectile.oldPos.Length; i++) {
				if (Projectile.oldPos[i] == default) break;
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.oldPos[i - 1], Projectile.oldPos[i], size, ref _)) return true;
			}
			return false;
		}

		private static VertexStrip _vertexStrip = new();
		static void DrawTrail(Projectile projectile, float size) {

			MiscShaderData miscShaderData = GameShaders.Misc["Origins:Identity"];
			miscShaderData.UseImage0(TextureAssets.MagicPixel);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(0, 0, 0, 1));
			miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(0, 0, 1, 1));
			float[] oldRot;
			oldRot = new float[projectile.oldPos.Length];
			for (int i = 1; i < projectile.oldPos.Length; i++) {
				oldRot[i] = (projectile.oldPos[i] - projectile.oldPos[i - 1]).ToRotation();
			}
			oldRot[0] = projectile.rotation;
			if (GeometryUtils.AngleDif(oldRot[1], projectile.rotation, out _) > GeometryUtils.AngleDif(oldRot[1], projectile.rotation  + MathHelper.Pi, out _)) {
				oldRot[0] = projectile.rotation + MathHelper.Pi;
			}
			miscShaderData.Apply();
			_vertexStrip.PrepareStrip(projectile.oldPos, oldRot, _ => Color.White, _ => size, -Main.screenPosition, projectile.oldPos.Length, includeBacksides: true);
			_vertexStrip.DrawTrail();
		}
		public void PreDrawScene() {
			if (middleRenderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return;
			}
			Origins.shaderOroboros.Capture();

			DrawTrail(Projectile, 4 * Projectile.scale);

			Origins.shaderOroboros.DrawContents(middleRenderTarget, Color.White, Main.GameViewMatrix.EffectMatrix);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = middleRenderTarget.Size() * 0.5f;
			SC_Phase_Three_Midlay.DrawDatas.Add(new(
				middleRenderTarget,
				center,
				null,
				Color.White,
				0,
				center,
				Vector2.One / Main.GameViewMatrix.Zoom,
				SpriteEffects.None
			));
		}
		public override bool PreDraw(ref Color lightColor) {
			if (middleRenderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return false;
			}

			Main.spriteBatch.Restart(Main.spriteBatch.GetState(), rasterizerState: RasterizerState.CullNone);
			Origins.shaderOroboros.Capture();
			Main.spriteBatch.Restart(Main.spriteBatch.GetState(), transformMatrix: Main.GameViewMatrix.ZoomMatrix);

			DrawTrail(Projectile, 4 * Projectile.scale + 2);

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
				Main.GameViewMatrix.Effects
			);

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Main.EntitySpriteDraw(
				texture,
				Projectile.position - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				texture.Size() - new Vector2(3, 6),
				Projectile.scale,
				SpriteEffects.None
			);
			return false;
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
	public class Accretion_Ribbon_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().accretionRibbonDebuff = true;
		}
	}
}
