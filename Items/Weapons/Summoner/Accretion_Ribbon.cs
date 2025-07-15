using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.NPCs;
using Origins.NPCs.MiscB.Shimmer_Construct;
using PegasusLib;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Summoner {
	public class Accretion_Ribbon : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToWhip(ModContent.ProjectileType<Accretion_Ribbon_P>(), 34, 5, 1, 28);
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.value = Item.sellPrice(gold: 4, silver: 60);
			Item.rare = ItemRarityID.LightRed;
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
	public class Accretion_Ribbon_P : ModProjectile {
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
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.ContinuouslyUpdateDamageStats = true;
			Projectile.friendly = true;
			Projectile.hide = true;
			Projectile.manualDirectionChange = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				Projectile.scale *= itemUse.Item.scale;
			}
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.HeldItem.ModItem is not Accretion_Ribbon) {
				Projectile.Kill();
				return;
			}
			Projectile.timeLeft = 2;
			if (player.altFunctionUse == 2) {
				float maxXVel = Math.Max(Math.Abs(player.velocity.X), Math.Max(player.maxRunSpeed * 2.5f, player.accRunSpeed * 2f));
				if (Projectile.direction == 0) {
					Projectile.direction = player.direction;
					if (player.velocity.Y == 0) {
						player.velocity += new Vector2(Projectile.direction * 2, -1) * (5.01f + player.jumpSpeedBoost);
					}
				}
				player.velocity.X = float.Clamp(player.velocity.X + Projectile.direction * 0.1f, -maxXVel, maxXVel);
				Projectile.velocity = Projectile.velocity.RotatedBy(0.35f * Projectile.direction);
			} else {
				PolarVec2 vel = (PolarVec2)Projectile.velocity;
				GeometryUtils.AngularSmoothing(ref vel.Theta, (Main.MouseWorld - player.MountedCenter).ToRotation(), 0.3f);
				Vector2 velocity = (Vector2)vel;
				if (Projectile.velocity != velocity) {
					Projectile.velocity = velocity;
					Projectile.netUpdate = true;
				}
				Projectile.direction = 0;
				if (player.ItemAnimationActive) {
					Projectile.velocity = Projectile.velocity.RotatedBy(0.4f * MathF.Sin((player.itemAnimation / (float)player.itemAnimationMax) * MathHelper.TwoPi + MathHelper.Pi));
				}
			}
			Projectile.position = player.MountedCenter + (Projectile.velocity * 162).Floor();
			Projectile.rotation = Projectile.velocity.ToRotation();
			player.heldProj = Projectile.whoAmI;
			for (int i = 0; i < Projectile.localNPCImmunity.Length; i++) {
				if (Projectile.localNPCImmunity[i] != 0) {
					NPC npc = Main.npc[i];
					if (!npc.active || !Projectile.Colliding(default, npc.Hitbox)) {
						Projectile.localNPCImmunity[i] = 0;
					}
				}
			}
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Main.player[Projectile.owner].Center.X);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Accretion_Ribbon_Buff.ID, 240);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float size = 4 * Projectile.scale;
			float _ = 0;
			for (int i = 1; i < Projectile.oldPos.Length; i++) {
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.oldPos[i - 1], Projectile.oldPos[i], size, ref _)) return true;
			}
			return false;
		}

		private static VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			if (renderTarget is null) {
				Main.QueueMainThreadAction(SetupRenderTargets);
				Main.OnResolutionChanged += Resize;
				return false;
			}
			Origins.shaderOroboros.Capture();

			MiscShaderData miscShaderData = GameShaders.Misc["Origins:Identity"];
			miscShaderData.UseImage0(TextureAssets.MagicPixel);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(0, 0, 0, 1));
			miscShaderData.Shader.Parameters["uSourceRect0"].SetValue(new Vector4(0, 0, 1, 1));
			float[] oldRot;
			oldRot = new float[Projectile.oldPos.Length];
			for (int i = 1; i < Projectile.oldPos.Length; i++) {
				oldRot[i] = (Projectile.oldPos[i] - Projectile.oldPos[i - 1]).ToRotation();
			}
			oldRot[0] = oldRot[1];
			miscShaderData.Apply();
			_vertexStrip.PrepareStrip(Projectile.oldPos, oldRot, (GetLightColor) => new(120, 80, 225), _ => 4, -Main.screenPosition, Projectile.oldPos.Length, includeBacksides: true);
			_vertexStrip.DrawTrail();

			Origins.shaderOroboros.DrawContents(renderTarget, Color.White, Main.GameViewMatrix.EffectMatrix);
			Origins.shaderOroboros.Reset(default);
			Vector2 center = renderTarget.Size() * 0.5f;
			SC_Phase_Three_Overlay.drawDatas.Add(new(
				renderTarget,
				center,
				null,
				Color.White,
				0,
				center,
				Vector2.One / Main.GameViewMatrix.Zoom,
				SpriteEffects.None
			));
			return false;
		}
		public override void OnKill(int timeLeft) {
			if (renderTarget is not null) {
				SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref renderTarget);
				Main.OnResolutionChanged -= Resize;
			}
		}
		internal RenderTarget2D renderTarget;
		public void Resize(Vector2 _) {
			if (Main.dedServ) return;
			SC_Phase_Three_Overlay.SendRenderTargetForDisposal(ref renderTarget);
			SetupRenderTargets();
		}
		void SetupRenderTargets() {
			if (renderTarget is not null && !renderTarget.IsDisposed) return;
			renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
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
			npc.GetGlobalNPC<OriginGlobalNPC>().acridSpoutDebuff = true;
		}
	}
}
