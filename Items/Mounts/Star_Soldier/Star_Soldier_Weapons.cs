#define EditAnimation
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Shaders;
using Origins.Dusts;
using Origins.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.NPCs.Ashen;
using Origins.Projectiles;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Mounts.Star_Soldier;
public abstract class Star_Soldier_Weapon : ModItem, IExpectToBeUnobtainable {
	public Asset<Texture2D> Icon { get; private set; }
	public static IReadOnlyList<Star_Soldier_Weapon> Weapons => field ??= ModContent.GetContent<Star_Soldier_Weapon>().OrderBy(w => w.Item.DamageType.Type).ToList();
	public override ModItem NewInstance(Item entity) {
		Star_Soldier_Weapon item = (Star_Soldier_Weapon)base.NewInstance(entity);
		item.Icon = Icon;
		return item;
	}
	public override void AutoStaticDefaults() {
		base.AutoStaticDefaults();
		Item.ResearchUnlockCount = 0;
		Icon = ModContent.Request<Texture2D>(Texture + "_Icon");
	}
	public virtual void ModifyDrawData(Star_Soldier.MountHandler mountHandler, ref DrawData drawData) { }
	public virtual void PreItemCheck(Player player, Star_Soldier.MountHandler handler, ref Star_Soldier.MountHandler.Arm arm) { }
	public virtual void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) { }
	public virtual void UpdateRotations(Player player, ref Star_Soldier.MountHandler.Arm arm) {
		Vector2 relativeTarget = player.OriginPlayer().relativeTarget;
		arm.GetPositions(player.MountedCenter, player.fullRotation, player.Directions, out _, out _, out Vector2 gunPos);
		float targetRotation = (relativeTarget + player.Bottom - gunPos).ToRotation();

		GeometryUtils.AngularSmoothing(ref arm.gunRotation, targetRotation, 0.2f * arm.rotationSpeedMult);
		if (!float.IsFinite(arm.gunRotation)) arm.gunRotation = 0;

		GeometryUtils.AngularSmoothing(ref arm.forearmRotation, targetRotation, 0.2f * arm.rotationSpeedMult);
		if (!float.IsFinite(arm.forearmRotation)) arm.forearmRotation = 0;

		float baseRot = MathHelper.PiOver2 - player.direction * 2f;
		targetRotation = baseRot + Math.Clamp(GeometryUtils.AngleDif(baseRot, targetRotation, out int dir) * dir * player.direction, -0.9f, MathHelper.PiOver4) * player.direction;
		GeometryUtils.AngularSmoothing(ref arm.shoulderRotation, targetRotation, 0.1f * arm.rotationSpeedMult);
		if (!float.IsFinite(arm.shoulderRotation)) arm.shoulderRotation = 0;
	}
	public override bool NeedsAmmo(Player player) => false;
	public abstract void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale, in Star_Soldier.MountHandler.Arm arm);
	public virtual void PlaySound(Player player) { }
	#region methods which are never used
	public sealed override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) { }
	public sealed override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) { }
	public sealed override void ModifyHitPvp(Player player, Player target, ref Player.HurtModifiers modifiers) { }
	public sealed override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo) { }
	#endregion
}
public class Star_Soldier_Blade : Star_Soldier_Weapon {
	static int CooldownTime => 60;
	float cooldown;
	float cooldownAlpha;
	public override void SetStaticDefaults() {
		Origins.AddGlowMask(this);
	}
	public override void SetDefaults() {
		Item.damage = 500;
		Item.DamageType = DamageClass.Melee;
		Item.useAnimation = 35;
		Item.useTime = 35;
		Item.shootSpeed = 1;
		//Item.UseSound = Origins.Sounds.HeavyCannon.WithPitch(2f).WithVolume(0.6f);
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
		Item.width = 60;
		Item.height = 26;
		Item.shoot = ModContent.ProjectileType<Star_Soldier_Blade_P>();
		Item.noMelee = true;
		Item.knockBack = 2.5f;
		Item.UseSound = SoundID.Item105.WithPitch(-0.6f);
		Item.rare = ItemRarityID.Yellow;
	}
	public override bool CanUseItem(Player player) => cooldown == 0;
	public override void PreItemCheck(Player player, Star_Soldier.MountHandler handler, ref Star_Soldier.MountHandler.Arm arm) => player.statDefense.AdditiveBonus += 0.1f;
	public override void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) {
		cooldown.Cooldown(rate: player.GetWeaponAttackSpeed(Item));
		if (arm.itemAnimation != 0) cooldown = CooldownTime;
		if (cooldown == 0) cooldownAlpha.Cooldown(rate: 1f / 15);
		else cooldownAlpha = 1;
		Star_Soldier.MountHandler handler = Star_Soldier.GetHandler(player);
		handler.maxDashes++;
		handler.dashCooldownMax -= 15;
		handler.dashLength += 5;
		handler.dashSpeed += 4;
	}
	public override void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale, in Star_Soldier.MountHandler.Arm arm) {
		if (this.cooldownAlpha == 0 || cooldown >= CooldownTime) return;
		float cooldownAlpha = this.cooldownAlpha * this.cooldownAlpha;
		int width = 64;
		int halfWidth = width / 2;
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position,
			new Rectangle(0, 0, width, 4),
			Color.OrangeRed * cooldownAlpha,
			0,
			new Vector2(halfWidth, 2),
			1,
			SpriteEffects.None,
		0);
		int factor = (int)((cooldown * width) / (CooldownTime - 1));
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position + halfWidth * Vector2.UnitX,
			new Rectangle(0, 0, factor, 4),
			Color.Black * cooldownAlpha,
			0,
			new Vector2(factor, 2),
			1,
			SpriteEffects.None,
		0);
		position.Y += 8 * float.Pow(Utils.GetLerpValue(0, 0.4f, this.cooldownAlpha, true), 2);
	}
	public class Star_Soldier_Blade_P : ModProjectile, IElementalProjectile {
		static readonly AdvancedMiscShaderData bladeShader = new(ModContent.Request<Effect>("Origins/Effects/Strip"), "StarSoldierLaserBlade", [
			new("uColorMatrix0", Matrix.Identity with { M44 = 0 })
		]);
		public static Parameter uColorMatrix1;
		public static Parameter uFinalColorMatrix;
		public static Parameter uOverbrightMatrix;
		public static Parameter uOverbrightMax;
		public const int trail_length = 20;
		public ushort Element => Elements.Fire;
		public override string Texture => "Origins/Items/Weapons/Melee/Personal_Laser_Blade";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			ProjectileID.Sets.TrailingMode[Projectile.type] = -1;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = trail_length * 2;
			OriginsSets.Projectiles.FireProjectiles[Type] = true;
			ID = Type;
			bladeShader.LoadThen(() => {
				bladeShader.CreateParameter(ref uColorMatrix1, nameof(uColorMatrix1), Matrix.Identity with { M44 = 0 });
				bladeShader.CreateParameter(ref uFinalColorMatrix, nameof(uFinalColorMatrix), Matrix.Identity);
				bladeShader.CreateParameter(ref uOverbrightMatrix, nameof(uOverbrightMatrix), Matrix.Identity);
				bladeShader.CreateParameter(ref uOverbrightMax, nameof(uOverbrightMax), new Vector4(float.PositiveInfinity));
			});
		}
		protected static int HitboxSteps => 7;
		protected static float Startup => 0.25f;
		protected static float End => 0.25f;
		protected static float SwingStartVelocity => 1f;
		protected static float SwingEndVelocity => 1f;
		protected static float TimeoutVelocity => 1f;
		protected static float MinAngle => -2.5f;
		protected static float MaxAngle => 2.5f;
		protected Rectangle lastHitHitbox;
		Star_Soldier.WeaponColors? colors = default(Star_Soldier.WeaponColors);
		Star_Soldier.WeaponColors Colors => colors ?? (OriginsModIntegrations.CheckAprilFools() ? Star_Soldier.RGB : Star_Soldier.DefaultColors);
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.PiercingStarlight);
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 3;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 600;
			Projectile.noEnchantmentVisuals = true;
			DrawHeldProjInFrontOfHeldItemAndArms = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse itemUse) {
				Projectile.scale *= itemUse.Item.scale;
				itemUse.Player.ApplyMeleeScale(ref Projectile.scale);
				Projectile.ai[1] = itemUse.Player.direction;
			}
			string[] contextArgs = source?.Context?.Split(';') ?? [];
			for (int i = 0; i < contextArgs.Length; i++) {
				if (contextArgs[i].StartsWith(nameof(Star_Soldier))) {
					_ = float.TryParse(contextArgs[i][nameof(Star_Soldier).Length..], out Projectile.ai[0]);
					break;
				}
			}
		}
		protected float SwingFactor {
			get => Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (!Projectile.TryGetOwner(out Player player) || Star_Soldier.GetHandler(player) is not Star_Soldier.MountHandler handler) {
				Projectile.Kill();
				return;
			}
			ref Star_Soldier.MountHandler.Arm arm = ref (Projectile.ai[0] == 1 ? ref handler.altItem : ref handler.chosenItem);
			if (arm.itemAnimation <= 0) {
				Projectile.Kill();
				return;
			}
			Projectile.hide = (Projectile.ai[0] == 1) == (player.direction == 1);
			if (Projectile.hide) player.heldProj = Projectile.whoAmI;
			Star_Soldier.InitializeColor(player, ref colors);
			float updateOffset = (Projectile.MaxUpdates - (Projectile.numUpdates + 1)) / (float)(Projectile.MaxUpdates + 1);
			SwingFactor = ((arm.itemTime - updateOffset) / (float)arm.itemTimeMax) * (1 + Startup + End) - End;
			if (SwingFactor > 0) SwingFactor = MathHelper.Lerp(MathF.Pow(SwingFactor, 2f), MathF.Pow(SwingFactor, 0.5f), SwingFactor * SwingFactor);
			Projectile.rotation = MathHelper.Lerp(
				MaxAngle,
				MinAngle,
				MathHelper.Clamp(SwingFactor, 0, 1)
			) * Projectile.ai[1] * player.gravDir;

			float realRotation = Projectile.rotation * player.gravDir + Projectile.velocity.ToRotation() * player.gravDir;
			arm.shoulderRotation = realRotation + (SwingFactor - 1) * 1.65f * Projectile.ai[1];
			arm.forearmRotation = realRotation - (SwingFactor - 1) * 0.1f * Projectile.ai[1];
			arm.gunRotation = realRotation - (SwingFactor - 1) * 0.25f * Projectile.ai[1];
			arm.GetPositions(player.MountedCenter, player.fullRotation, player.Directions, out _, out _, out Projectile.position);
			Projectile.position -= Projectile.Size * 0.5f;
			Projectile.position += arm.gunRotation.ToRotationVector2() * 52;
			Projectile.localAI[2] = arm.gunRotation;

			player.direction = Math.Sign(Projectile.velocity.X);
			Projectile.localAI[1].Cooldown();
			EmitEnchantmentVisuals();
			for (int i = Projectile.oldPos.Length - 1; i > 0; i--) {
				Projectile.oldPos[i] = Projectile.oldPos[i - 1];
				Projectile.oldRot[i] = Projectile.oldRot[i - 1];
				Projectile.oldSpriteDirection[i] = Projectile.oldSpriteDirection[i - 1];
			}
			Projectile.oldPos[0] = Projectile.position;
			Projectile.oldRot[0] = Projectile.localAI[2];
			Projectile.oldSpriteDirection[0] = Projectile.spriteDirection;
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			if (Projectile.hide) overPlayers.Add(index);
		}
		public virtual void EmitEnchantmentVisuals() {
			Vector2 vel = Projectile.localAI[2].ToRotationVector2() * Projectile.velocity.Length() * Projectile.width * 0.95f;
			float velocityMult = 8;
			float rotMult = 0.05f;
			for (int j = 0; j <= HitboxSteps; j++) {
				Projectile.EmitEnchantmentVisualsAt(Projectile.position + vel * j, Projectile.width, Projectile.height);
				if (j > 1 && Main.rand.NextFloat(2 * Projectile.MaxUpdates) < 1) {
					Dust dust = Dust.NewDustDirect(
						Projectile.position + vel * j,
						Projectile.width, Projectile.height,
						DustID.PortalBoltTrail,
						newColor: new(Colors.GetDustColor())
					);
					dust.velocity = dust.velocity * 0.25f + Projectile.velocity.RotatedBy(Projectile.rotation * rotMult) * velocityMult;
					dust.position += dust.velocity * 2;
					dust.noGravity = true;
				}
			}
		}
		public override void CutTiles() {
			DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
			Vector2 end = Projectile.Center + (Projectile.localAI[2].ToRotationVector2() * Projectile.velocity.Length() * Projectile.width * HitboxSteps);
			Utils.PlotTileLine(Projectile.Center, end, Projectile.width, DelegateMethods.CutTiles);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 vel = Projectile.localAI[2].ToRotationVector2() * Projectile.velocity.Length() * Projectile.width;
			Vector2 additionalOffset = vel.SafeNormalize(default) * 12;
			for (int j = 0; j <= HitboxSteps; j++) {
				Rectangle hitbox = projHitbox;
				Vector2 offset = vel * j + additionalOffset;
				hitbox.Offset((int)offset.X, (int)offset.Y);
				if (hitbox.Intersects(targetHitbox)) {
					lastHitHitbox = hitbox;
					return true;
				}
			}
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			LaserBladeDrawer trailDrawer = default;
			trailDrawer.Length = Projectile.velocity.Length() * Projectile.width * 0.9f * HitboxSteps;
			trailDrawer.Draw(Projectile, Colors);
			return false;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.ScalingArmorPenetration += 0.5f;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.ScalingArmorPenetration += 0.5f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			SoundEngine.PlaySound(SoundID.Item72.WithPitch(1.5f), target.Center);
			SoundEngine.PlaySound(SoundID.Item89.WithPitch(1.3f), target.Center);
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			SoundEngine.PlaySound(SoundID.Item72.WithPitch(1.5f), target.Center);
			SoundEngine.PlaySound(SoundID.Item89.WithPitch(1.3f), target.Center);
		}
		public struct LaserBladeDrawer {

			private static readonly VertexStrip _vertexStrip = new();

			static Color TrailColor = new(35, 35, 35, 0);
			static Color BladeColor = new(255, 255, 255, 128);
			static Color BladeSecondaryColor = new(255, 255, 255, 64);

			public float Length;

			int[] spriteDirections;
			public void Draw(Projectile proj, in Star_Soldier.WeaponColors colors) {
				if (renderTarget is null) {
					Main.QueueMainThreadAction(SetupRenderTargets);
					Main.OnResolutionChanged += Resize;
					return;
				}
				Origins.shaderOroboros.Capture();
				Main.graphics.GraphicsDevice.Clear(Color.Black);
				{
					MiscShaderData miscShaderData = GameShaders.Misc["EmpressBlade"];
					int num = 1;//1
					int num2 = 0;//0
					int num3 = 0;//0
					float w = 0.6f;//0.6f
					miscShaderData.UseShaderSpecificData(new Vector4(num, num2, num3, w));
					miscShaderData.Apply();
					float[] oldRot = new float[proj.oldRot.Length];
					Vector2[] oldPos = new Vector2[proj.oldPos.Length];
					Vector2 move = new(Length - 30, 0);
					for (int i = 0; i < oldPos.Length; i++) {
						if (proj.oldPos[i] == default) {
							Array.Resize(ref oldRot, i);
							Array.Resize(ref oldPos, i);
							if (i == 0) return;
							break;
						}
						oldRot[i] = proj.oldRot[i] + MathHelper.PiOver2;
						oldPos[i] = proj.oldPos[i] + move.RotatedBy(oldRot[i] - MathHelper.PiOver2);
					}
					spriteDirections = proj.oldSpriteDirection;
					_vertexStrip.PrepareStrip(oldPos, oldRot, AfterimageColors, AfterimageWidth, -Main.screenPosition + proj.Size / 2f, oldPos.Length, includeBacksides: true);
					_vertexStrip.DrawTrail();
				}
				{
					MiscShaderData miscShaderData = GameShaders.Misc["Origins:LaserBlade"];
					Vector2 velocity = proj.localAI[2].ToRotationVector2() * Length * 1.333f;
					Vector2[] positions = new Vector2[15];
					for (int i = 0; i < positions.Length; i++) {
						positions[i] = proj.Center + velocity * ((i + 1) / (float)(positions.Length + 1));
					}
					float[] rotations = [.. Enumerable.Repeat(proj.velocity.ToRotation() + proj.rotation, positions.Length)];
					miscShaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
					miscShaderData.UseImage0(TextureAssets.Extra[ExtrasID.FlameLashTrailShape]);
					miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 1, 1, 0));
					miscShaderData.UseSaturation(-1);
					miscShaderData.UseOpacity(2);
					miscShaderData.Apply();
					_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, BladeSecondaryColors, BladeWidth, -Main.screenPosition, true);
					_vertexStrip.DrawTrail();

					miscShaderData.UseSaturation(0.5f);
					miscShaderData.Apply();
					_vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, BladeColors, BladeWidth, -Main.screenPosition, true);
					_vertexStrip.DrawTrail();
				}
				Origins.shaderOroboros.DrawContents(renderTarget, Color.White, Matrix.Invert(Main.GameViewMatrix.TransformationMatrix));
				Origins.shaderOroboros.Reset(default);
				using GraphicsExt.SpritebatchOverride _ = Main.spriteBatch.OverrideState(SpriteSortMode.Immediate);
				//Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height), Color.Black);
				bladeShader.Apply(null,
					uColorMatrix1 with { Value = colors.InitialMult },
					uFinalColorMatrix with { Value = colors.FinalColor },
					uOverbrightMatrix with { Value = colors.OverbrightColor },
					uOverbrightMax with { Value = colors.OverbrightMax }
				);
				Main.spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			}

			private readonly Color AfterimageColors(float progressOnStrip) {
				if (float.IsNaN(progressOnStrip)) return default;
				Color result = TrailColor * MathHelper.Lerp(1f, 0.5f, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, clamped: true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip, clamped: true));
				result.A /= 2;
				result *= spriteDirections[Math.Max((int)(progressOnStrip * spriteDirections.Length) - 1, 0)];
				return result;
			}
			private readonly float AfterimageWidth(float progressOnStrip) {
				return 60;
			}

			private readonly Color BladeColors(float progressOnStrip) {
				return BladeColor * (progressOnStrip == 0 ? 0 : 1);
			}
			private readonly Color BladeSecondaryColors(float progressOnStrip) {
				return BladeSecondaryColor * (progressOnStrip == 0 ? 0 : 1);
			}
			private readonly float BladeWidth(float progressOnStrip) {
				return 24 - 8 * progressOnStrip;
			}
			static RenderTarget2D renderTarget;
			static void Resize(Vector2 _) {
				if (Main.dedServ) return;
				renderTarget.Dispose();
				SetupRenderTargets();
			}
			static void SetupRenderTargets() {
				if (renderTarget is not null && !renderTarget.IsDisposed) return;
				renderTarget = new RenderTarget2D(Main.instance.GraphicsDevice, Main.screenWidth, Main.screenHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			}
		}
	}
}
public class Star_Soldier_Gun : Star_Soldier_Weapon {
	static int AmmoMax => 32;
	static int ReloadLength => 51;
	int ammo = AmmoMax;
	int reloadTime = 0;
	bool usedFakeAmmo;
	int recoilTime = 0;
	float recoilMult = 0;
	public override void SetDefaults() {
		Item.damage = 50;
		Item.DamageType = DamageClass.Ranged;
		Item.useAnimation = 3;
		Item.useTime = 3;
		Item.shootSpeed = 19;
		Item.UseSound = Origins.Sounds.HeavyCannon.WithPitch(1.7f).WithVolume(0.45f);
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
		Item.crit += 4;
		Item.width = 60;
		Item.height = 26;
		Item.shoot = ProjectileID.Bullet;
		Item.useAmmo = AmmoID.Bullet;
		Item.noMelee = true;
		Item.knockBack = 2.5f;
		Item.rare = ItemRarityID.Yellow;
	}
	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		Vector2 offset = Vector2.Normalize(velocity);
		offset = offset * 24 + offset.RotatedBy(-MathHelper.PiOver2 * player.direction) * 8;
		position += offset;
		velocity = velocity.RotatedByRandom(0.08f);
	}
	public override bool CanUseItem(Player player) => ammo > 0;
	public override void PreItemCheck(Player player, Star_Soldier.MountHandler handler, ref Star_Soldier.MountHandler.Arm arm) => player.OriginPlayer().ammoConsumptionChance *= 0.9f;
	public override void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) {
		if (arm.itemAnimation != 0) {
			//SoundEngine.PlaySound(SoundID.Item61.WithPitch(2f), player.Center);
			reloadTime = 0;
			if (arm.itemTime == arm.itemTimeMax) {
				recoilTime = 9;
				if (usedFakeAmmo) ammo--;
				MathUtils.LinearSmoothing(ref recoilMult, 1, 0.1f);
				arm.GetPositions(player.MountedCenter, player.fullRotation, player.Directions, out _, out _, out Vector2 gunPos);
				Vector2 dir = arm.gunRotation.ToRotationVector2();
				Vector2 vel = dir.Perpendicular((int)(player.direction * player.gravDir)) * 4 - dir * 3;
				Dust dust = EfficientDust.NewDustDirect(gunPos, 4, 4, TM_Bullet_Casing.ID, vel.X, vel.Y, Layer: arm.IsBack(player).Mul(EfficientDust.dust_layer_behind));
				dust.position = gunPos + dir * 4;
				dust.customData = Main.rand.NextFloat(0.2f, 1f) * player.direction * player.gravDir;
			}
		} else {
			recoilMult.Cooldown(0, recoilMult * 0.1f + 0.01f);
			if (ammo < AmmoMax && reloadTime.Warmup(ReloadLength)) {
				SoundEngine.PlaySound(SoundID.Item53.WithPitch(0.5f), player.Center);
				ammo = AmmoMax;
			}
		}
		recoilTime.Cooldown();
	}
	public override void UpdateRotations(Player player, ref Star_Soldier.MountHandler.Arm arm) {
		float factor = (recoilTime >= 6 ? (1 - (recoilTime - 6f) / 3f) : recoilTime / 6f) * player.direction * player.gravDir;
		float shoulderOffset = -0.1f * factor;
		float forearmOffset = -0.0225f * factor;
		float gunOffset = 0.07f * factor;
		arm.shoulderRotation += shoulderOffset;
		arm.forearmRotation += forearmOffset;
		arm.gunRotation += gunOffset;

		base.UpdateRotations(player, ref arm);

		arm.shoulderRotation -= shoulderOffset;
		arm.forearmRotation -= forearmOffset;
		arm.gunRotation -= gunOffset;
	}
	public override bool? CanChooseAmmo(Item ammo, Player player) {
		usedFakeAmmo = false;
		return base.CanChooseAmmo(ammo, player);
	}
	public override bool NeedsAmmo(Player player) {
		if (needsAmmoChecking) return true;
		using (needsAmmoChecking.ScopedOverride(true)) {
			usedFakeAmmo = CombinedHooks.CanConsumeAmmo(player, Item, new(ItemID.MusketBall, ammo))
				&& (!player.ammoPotion || Main.rand.NextBool(4, 5))
				&& (!player.huntressAmmoCost90 || Main.rand.NextBool(9, 10))
				&& (!player.chloroAmmoCost80 || Main.rand.NextBool(4, 5))
				&& (!player.ammoCost80 || Main.rand.NextBool(4, 5))
				&& (!player.ammoCost75 || Main.rand.NextBool(3, 4));
		}
		return false;
	}
	static bool needsAmmoChecking = false;
	public override void OnConsumeAmmo(Item ammo, Player player) {
		SoundEngine.PlaySound(SoundID.Item149.WithPitch(-1f), player.Center);
		this.ammo--;
	}
	public override void ModifyDrawData(Star_Soldier.MountHandler mountHandler, ref DrawData drawData) {
		drawData.sourceRect = drawData.texture.Frame(1, 4, 0, 0);
	}
	public override void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale, in Star_Soldier.MountHandler.Arm arm) {
		int width = 64;
		int halfWidth = width / 2;
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position,
			new Rectangle(0, 0, width, 4),
			Color.Black,
			0,
			new Vector2(halfWidth, 2),
			1,
			SpriteEffects.None,
		0);
		int ammoFactor = (ammo * width) / AmmoMax;
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position + halfWidth * Vector2.UnitX,
			new Rectangle(0, 0, ammoFactor, 4),
			Color.OrangeRed,
			0,
			new Vector2(ammoFactor, 2),
			1,
			SpriteEffects.None,
		0);
		if (reloadTime > 1 && reloadTime < ReloadLength) {
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				position,
				new Rectangle(0, 0, ((reloadTime - 1) * width) / (ReloadLength - 1), 4),
				Color.White,
				0,
				new Vector2(halfWidth, 2),
				1,
				SpriteEffects.None,
			0);
		}
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position + halfWidth * Vector2.UnitX,
			new Rectangle(0, 0, ammoFactor, 4),
			Color.OrangeRed,
			0,
			new Vector2(ammoFactor, 2),
			new Vector2(1, 0.5f),
			SpriteEffects.None,
		0);
		position.Y += 8;
	}
}
public class Star_Soldier_Laser : Star_Soldier_Weapon {
	static float AmmoMax => 100;
	static float ChargeSpeed => 0.75f;
	static float RechargeSpeed => 0.6f;
	float ammo = AmmoMax;
	bool recharging = false;
	public override void SetStaticDefaults() {
		Origins.AddGlowMask(this);
	}
	public override void SetDefaults() {
		Item.damage = 72;
		Item.DamageType = DamageClass.Magic;
		Item.useAnimation = 30;
		Item.useTime = 30;
		Item.shootSpeed = 19;
		//Item.UseSound = Origins.Sounds.HeavyCannon.WithPitch(2f).WithVolume(0.6f);
		Item.mana = 100;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
		Item.width = 60;
		Item.height = 26;
		Item.shoot = ModContent.ProjectileType<Star_Soldier_Laser_Beam>();
		Item.noMelee = true;
		Item.knockBack = 2.5f;
		Item.rare = ItemRarityID.Yellow;
	}
	public override bool CanUseItem(Player player) => !recharging;
	public override void PreItemCheck(Player player, Star_Soldier.MountHandler handler, ref Star_Soldier.MountHandler.Arm arm) => player.GetAttackSpeed(DamageClass.Magic) *= 1.1f;
	public override void ModifyManaCost(Player player, ref float reduce, ref float mult) => mult *= player.GetWeaponAttackSpeed(Item);
	public override void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) {
		if (arm.itemAnimation != 0 && control) {
			const float precision = 100;
			int manaRedirect = (int)(ammo * precision);
			using (new ScopedRedirect<int>(ref player.statMana, ref manaRedirect)) {
				using ScopedOverride<int> _ = player.statManaMax2.ScopedOverride((int)(AmmoMax * precision));
				if (!player.CheckMana(Item, pay: true, blockQuickMana: true)) recharging = true;
			}
			ammo = manaRedirect / precision;
			if (!recharging) {
				SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(2.7f).WithVolume(0.3f), player.Center);
				SoundEngine.SoundPlayer.Play(SoundID.Item72.WithVolume(0.3f), player.Center);
				arm.itemTime = arm.itemTimeMax;
				arm.itemAnimation = arm.itemAnimationMax;
			} else {
				SoundEngine.SoundPlayer.Play(SoundID.NPCHit43.WithPitch(0.6f).WithVolume(0.3f), player.Center);
				arm.itemTime = 0;
				arm.itemAnimation = 0;
			}
		} else {
			SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(0.1f + ammo / 20).WithVolume(0.2f - ammo / 400), player.Center);
			arm.itemTime = 0;
			arm.itemAnimation = 0;
			if (ammo.Warmup(AmmoMax, recharging ? RechargeSpeed : ChargeSpeed)) {
				recharging = false;
				SoundEngine.SoundPlayer.Play(SoundID.Item75.WithPitch(0.6f).WithVolume(0.3f), player.Center);
				SoundEngine.SoundPlayer.Play(SoundID.Item84.WithPitch(0.8f).WithVolume(0.9f), player.Center);
			}
		}
	}
	public override void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale, in Star_Soldier.MountHandler.Arm arm) {
		int width = 64;
		int halfWidth = width / 2;
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position,
			new Rectangle(0, 0, width, 4),
			Color.Black,
			0,
			new Vector2(halfWidth, 2),
			1,
			SpriteEffects.None,
		0);
		spriteBatch.Draw(
			TextureAssets.MagicPixel.Value,
			position,
			new Rectangle(0, 0, (int)((ammo * width) / AmmoMax), 4),
			recharging ? Color.Purple : Color.DodgerBlue,
			0,
			new Vector2(halfWidth, 2),
			1,
			SpriteEffects.None,
		0);
		position.Y += 8;
	}
	public class Star_Soldier_Laser_Beam : ModProjectile {
		static readonly AdvancedMiscShaderData beamShader = new(ModContent.Request<Effect>("Origins/Effects/Beam"), "StarSoldierLaser", [
			new("uColorMatrix0", Matrix.Identity with { M44 = 0 })
		]);
		static readonly AdvancedMiscShaderData hitAOEShader = new(ModContent.Request<Effect>("Origins/Effects/Radial"), "StarSoldierLaserHit", [
			new("uOffset", new Vector2(0.5f)),
			new("uScale", float.Sqrt(0.5f)),
			new("uColorMatrix0", Matrix.Identity with { M44 = 0 })
		]);
		static class BeamParams {
			public static Parameter uColorMatrix1;
			public static Parameter uFinalColorMatrix;
			public static Parameter uOverbrightMatrix;
			public static Parameter uOverbrightMax;
			public static Parameter uShaderSpecificData;
		}
		static class AOEParams {
			public static Parameter uImageOffset1;
			public static Parameter uColorMatrix1;
			public static Parameter uFinalColorMatrix;
			public static Parameter uOverbrightMatrix;
			public static Parameter uOverbrightMax;
		}
		Star_Soldier.WeaponColors? colors = default(Star_Soldier.WeaponColors);
		Star_Soldier.WeaponColors Colors => colors ?? (OriginsModIntegrations.CheckAprilFools() ? Star_Soldier.RGB : Star_Soldier.DefaultColors);
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 3200 + 64;
			hitAOEShader.UseSamplerState(SamplerState.PointWrap)
			.UseImage1(TextureAssets.MagicPixel);
			GameShaders.Misc["Origins:StarSoldierLaser"] = beamShader;
			GameShaders.Misc["Origins:StarSoldierLaserHit"] = hitAOEShader;
			beamShader.LoadThen(() => {
				beamShader.CreateParameter(ref BeamParams.uColorMatrix1, nameof(BeamParams.uColorMatrix1), Matrix.Identity with { M44 = 0 });
				beamShader.CreateParameter(ref BeamParams.uFinalColorMatrix, nameof(BeamParams.uFinalColorMatrix), Matrix.Identity);
				beamShader.CreateParameter(ref BeamParams.uOverbrightMatrix, nameof(BeamParams.uOverbrightMatrix), Matrix.Identity);
				beamShader.CreateParameter(ref BeamParams.uOverbrightMax, nameof(BeamParams.uOverbrightMax), new Vector4(float.PositiveInfinity));
				beamShader.CreateParameter(ref BeamParams.uShaderSpecificData, nameof(BeamParams.uShaderSpecificData), Vector4.Zero);
			});
			hitAOEShader.LoadThen(() => {
				hitAOEShader.CreateParameter(ref AOEParams.uImageOffset1, nameof(AOEParams.uImageOffset1), Vector2.Zero);
				hitAOEShader.CreateParameter(ref AOEParams.uColorMatrix1, nameof(AOEParams.uColorMatrix1), Matrix.Identity with { M44 = 0 });
				hitAOEShader.CreateParameter(ref AOEParams.uFinalColorMatrix, nameof(AOEParams.uFinalColorMatrix), Matrix.Identity);
				hitAOEShader.CreateParameter(ref AOEParams.uOverbrightMatrix, nameof(AOEParams.uOverbrightMatrix), Matrix.Identity);
				hitAOEShader.CreateParameter(ref AOEParams.uOverbrightMax, nameof(AOEParams.uOverbrightMax), new Vector4(float.PositiveInfinity));
			});
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = -1;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 5;
		}
		public override bool ShouldUpdatePosition() => false;
		public Vector2 TargetPos {
			get => new(Projectile.ai[0], Projectile.ai[1]);
			set => (Projectile.ai[0], Projectile.ai[1]) = value;
		}
		public override void OnSpawn(IEntitySource source) {
			string[] contextArgs = source?.Context?.Split(';') ?? [];
			for (int i = 0; i < contextArgs.Length; i++) {
				if (contextArgs[i].StartsWith(nameof(Star_Soldier))) {
					_ = float.TryParse(contextArgs[i][nameof(Star_Soldier).Length..], out Projectile.ai[2]);
					break;
				}
			}
		}
		public override void AI() {
			if (!Projectile.TryGetOwner(out Player owner) || Star_Soldier.GetHandler(owner) is not Star_Soldier.MountHandler handler) {
				Projectile.Kill();
				return;
			}
			Star_Soldier.MountHandler.Arm arm = Projectile.ai[2] == 1 ? handler.altItem : handler.chosenItem;
			if (arm.itemAnimation <= 0) {
				Projectile.Kill();
				return;
			}
			Projectile.hide = (Projectile.ai[2] == 1) == (owner.direction == 1);
			Star_Soldier.InitializeColor(owner, ref colors);
			Projectile.velocity = arm.gunRotation.ToRotationVector2();
			arm.GetPositions(owner.MountedCenter, owner.fullRotation, owner.Directions, out _, out _, out Projectile.position);
			Projectile.position += Projectile.velocity.Perpendicular(owner.direction) * 4;
			Vector2 targetPos = Projectile.position + Projectile.velocity * Math.Max(Raymarch(Projectile.position, Projectile.velocity, ProjectileID.Sets.DrawScreenCheckFluff[Type] - 64), 64);

			//SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(2.7f).WithVolume(0.5f), Projectile.Center);
			//SoundEngine.SoundPlayer.Play(SoundID.Item72.WithVolume(0.5f), Projectile.Center);
			Dust.NewDust(targetPos - Vector2.One * 2, 4, 4, DustID.PortalBoltTrail, newColor: new(Colors.GetDustColor()));
			Projectile.localAI[2] += 1f / 60;
			TargetPos = targetPos;
			//SoundEngine.SoundPlayer.Play(SoundID.Item158.WithPitch(++owner.ai[3] / 10).WithVolume(0.5f), Projectile.Center);
			//SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(owner.ai[3] / 20).WithVolume(0.5f), Projectile.Center);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (targetHitbox.IsWithin(TargetPos, 16 * 5)) return true;
			return targetHitbox.Contains(targetHitbox.Center().SnapToLine(Projectile.position, TargetPos, radius: 12));
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (!Projectile.TryGetOwner(out Player owner) || Star_Soldier.GetHandler(owner) is not Star_Soldier.MountHandler handler) return;
			Star_Soldier.MountHandler.Arm arm = Projectile.ai[2] == 1 ? handler.altItem : handler.chosenItem;
			modifiers.FinalDamage *= owner.GetWeaponAttackSpeed(arm.item);
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			if (!Projectile.TryGetOwner(out Player owner) || Star_Soldier.GetHandler(owner) is not Star_Soldier.MountHandler handler) return;
			Star_Soldier.MountHandler.Arm arm = Projectile.ai[2] == 1 ? handler.altItem : handler.chosenItem;
			modifiers.FinalDamage *= owner.GetWeaponAttackSpeed(arm.item);
			if (!target.immune) modifiers = modifiers with { CooldownCounter = -2 };
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (info.CooldownCounter == -2) {
				target.immune = true;
				target.immuneTime = target.longInvince ? 16 : 8;
			}
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			if (Projectile.hide) overPlayers.Add(index);
		}
		public override bool PreDraw(ref Color lightColor) {
			if (!TargetPos.IsWithin(TargetPos.Clamp(Main.screenPosition, Main.screenPosition + Main.ScreenSize.ToVector2()), 64) && !Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), Projectile.position, TargetPos)) return false;
			using GraphicsExt.SpritebatchOverride _ = Main.spriteBatch.OverrideState(SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap);
			hitAOEShader.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]).Apply(null,
				AOEParams.uImageOffset1 with { Value = new Vector2(Projectile.localAI[2], Projectile.localAI[2] * -0.5f) },
				AOEParams.uColorMatrix1 with { Value = Colors.InitialMult },
				AOEParams.uFinalColorMatrix with { Value = Colors.FinalColor },
				AOEParams.uOverbrightMatrix with { Value = Colors.OverbrightColor },
				AOEParams.uOverbrightMax with { Value = Colors.OverbrightMax }
			);
			Main.spriteBatch.Draw(
				TextureAssets.Projectile[Type].Value,
				TargetPos - Main.screenPosition,
				null,
				default,
				Projectile.localAI[2],
				Vector2.One * 128,
				Vector2.One * 5 * Colors.AOEScale,
				0,
			0);
			Vector2 position = Projectile.position + Projectile.velocity * 62;
			Vector2 diff = TargetPos - position;
			position -= Main.screenPosition;
			/*Main.spriteBatch.Draw(
				TextureAssets.Extra[ExtrasID.RainbowRodTrailShape].Value,
				position,
				default
			);*/
			float rotation = diff.ToRotation();
			float dist = diff.Length();
			const float scale = 1f / 256f;
			DrawData data = new(
				TextureAssets.Extra[ExtrasID.MagicMissileTrailShape].Value,//TextureAssets.MagicPixel.Value,
				position,
				new(256 - (int)((Projectile.localAI[2] * 600) % 256), 0, (int)dist, 256),
				default,
				rotation,
				Vector2.UnitY * 128,
				new Vector2(1, 48 * scale * Colors.BeamScale),
				0
			);
			beamShader.Apply(null,
				BeamParams.uColorMatrix1 with { Value = Colors.InitialMult },
				BeamParams.uFinalColorMatrix with { Value = Colors.FinalColor },
				BeamParams.uOverbrightMatrix with { Value = Colors.OverbrightColor },
				BeamParams.uOverbrightMax with { Value = Colors.OverbrightMax },
				BeamParams.uShaderSpecificData with { Value = new Vector4(Colors.ExtraBeamData, 0) }
			);
			data.Draw(Main.spriteBatch);
			beamShader.Apply(null,
				BeamParams.uColorMatrix1 with { Value = Colors.InitialMult },
				BeamParams.uFinalColorMatrix with { Value = Colors.FinalColor },
				BeamParams.uOverbrightMatrix with { Value = Colors.OverbrightColor },
				BeamParams.uOverbrightMax with { Value = Colors.OverbrightMax },
				BeamParams.uShaderSpecificData with { Value = Vector4.Zero }
			);
			/*Main.spriteBatch.Draw(
				TextureAssets.Extra[ExtrasID.RainbowRodTrailShape].Value,
				position,
				default
			);*/
			return false;
		}
		float Raymarch(Vector2 position, Vector2 direction, float maxLength = float.PositiveInfinity) {
			float dist = CollisionExt.Raymarch(position, direction, maxLength);
			foreach (NPC npc in Main.ActiveNPCs) {
				if (dist < 16) return dist;
				if (npc.friendly) continue;
				if (position.Clamp(npc.Hitbox).DistanceSQ(position) >= dist * dist) continue;
				float collisionPoint = 1;
				if (Collision.CheckAABBvLineCollision(npc.position, npc.Size, position, position + direction * dist, 1, ref collisionPoint)) {
					Min(ref dist, collisionPoint);
				}
			}
			if (Main.player[Projectile.owner] is Player { hostile: true } owner) {
				foreach (Player player in Main.ActivePlayers) {
					if (!player.InOpposingTeam(owner)) continue;
					if (dist < 16) return dist;
					if (position.Clamp(player.Hitbox).DistanceSQ(position) >= dist * dist) continue;
					float collisionPoint = 1;
					if (Collision.CheckAABBvLineCollision(player.position, player.Size, position, position + direction * dist, 1, ref collisionPoint)) {
						Min(ref dist, collisionPoint);
					}
				}
			}
			return dist;
		}
	}
}
public class Star_Soldier_Droner : Star_Soldier_Weapon {
	public static int Cooldown => 10 * 60;
	public override void SetStaticDefaults() {
		Origins.AddGlowMask(this);
	}
	public override void SetDefaults() {
		Item.damage = 47;
		Item.DamageType = DamageClass.Summon;
		Item.useAnimation = 12;
		Item.useTime = 12;
		Item.shootSpeed = 15;
		Item.knockBack = 4f;
		Item.useAmmo = AmmoID.Rocket;
		Item.shoot = ModContent.ProjectileType<Star_Soldier_Drone>();
		Item.UseSound = null;
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.autoReuse = true;
		Item.rare = ItemRarityID.Yellow;
	}
	public override void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) {
		Star_Soldier.MountHandler handler = Star_Soldier.GetHandler(player);
		handler.maxDrones += 1 + handler.maxDrones;
	}
	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		ref int availableDrones = ref Star_Soldier.GetHandler(player).availableDrones;
		if (availableDrones <= 0) {
			//set type to tag projectile
			type = ProjectileID.Bullet;
			new Star_Soldier.MountHandler.Star_Soldier_Weapon_Sound(player, Type).Perform();
		} else availableDrones--;
	}
	public override void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale, in Star_Soldier.MountHandler.Arm arm) {
		Star_Soldier.MountHandler handler = Star_Soldier.GetHandler(Main.LocalPlayer);
		if (arm.Index != 0 && handler.maxDrones != 1) return;
		int width = 64;
		int halfWidth = width / 2;
		int cooldownWidth = (int)(width * (handler.droneCooldown / (float)Cooldown));
		int availableDrones = handler.availableDrones;
		for (int i = Main.LocalPlayer.ownedProjectileCounts[Item.shoot]; i < handler.maxDrones; i++) {
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				position,
				new Rectangle(0, 0, width, 4),
				Color.OrangeRed,
				0,
				new Vector2(halfWidth, 2),
				1,
				SpriteEffects.None,
			0);
			if (--availableDrones < 0 && handler.droneCooldown > 0) {
				spriteBatch.Draw(
					TextureAssets.MagicPixel.Value,
					position,
					new Rectangle(0, 0, cooldownWidth, 4),
					Color.Black,
					0,
					new Vector2(cooldownWidth - halfWidth, 2),
					1,
					SpriteEffects.None,
				0);
				cooldownWidth = width;
			}
			position.Y += 8;
		}
	}
	public class Star_Soldier_Drone : MinionBase, IArtifactMinion {
		public int MaxLife { get; set; }
		public float Life { get; set; }
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Star_Soldier_Drone).GetDefaultTMLName("_Glow");
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Main.projFrames[Type] = 4;
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ProjectileID.Sets.MinionCannotBeFreed[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minion = true;
			Projectile.width = 36;
			Projectile.height = 36;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60 * 10;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.netImportant = true;
			MaxLife = 500;
		}
		bool buff;
		public override ref bool HasBuff(Player player) {
			buff = player.mount.IsMount<Star_Soldier>();
			return ref buff;
		}
		public override void MoveTowardsTarget() {
			if (Star_Soldier.GetHandler(Owner)?.LockOnTarget is NPC target) targetingData.TargetID = target.whoAmI;
			bool foundTarget = targetingData.TargetID != -1;
			Rectangle targetHitbox = foundTarget ? targetingData.targetHitbox : RestRegion;

			Vector2 targetPos = Projectile.Center.Clamp(targetHitbox);
			Vector2 direction = (targetPos - Projectile.Center).Normalized(out float distance);
			if (distance == 0) return;
			float speed = 0.3f * SpeedModifier;
			if (distance <= 64) direction *= -1;
			Projectile.velocity *= float.Lerp(0.9f, 0.98f, float.Abs(Vector2.Dot(direction, Projectile.velocity.Normalized(out _))));
			if (distance <= 64 && distance > 38) return;
			Projectile.velocity += direction * speed;
			Projectile.velocity = Projectile.velocity.Normalized(out speed) * Math.Min(speed, 16);
		}
		protected override void BasicAI() {
			if (Projectile.soundDelay.TrySet(-1)) SoundEngine.PlaySound(Origins.Sounds.ThrusterChargeUp.WithPitch(3f).WithVolume(0.6f), Projectile.Center);
			Projectile.frameCounter.Warmup(30);
			Projectile.frame = Math.Max((Projectile.frameCounter - 12) / 6, 0);
			if (Projectile.frameCounter < 12) return;
			base.BasicAI();
			bool foundTarget = targetingData.TargetID != -1;
			if (foundTarget) {
				Projectile.rotation = (targetingData.targetHitbox.Center() - Projectile.Center).ToRotation() + MathHelper.PiOver2;
			}

			if (foundTarget && Projectile.ai[0].CycleUp(8, SpeedModifier)) {
				if (Projectile.ai[1].CycleUp(5) || !CollisionExt.CanHitRay(Projectile.Center, targetingData.targetHitbox.Center())) {
					if (RandomChoice(4)) Projectile.velocity += (Projectile.rotation + MathHelper.Pi * Main.rand.NextBool().ToInt()).ToRotationVector2() * 8;
				} else {
					Projectile.SpawnProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						(Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 8,
						ModContent.ProjectileType<Robo_Tail_Probe_Laser>(),
						Projectile.damage,
						Projectile.knockBack
					);
				}
			}
			Lighting.AddLight(Projectile.Center, 0.5f, 0, 0);
			if (Projectile.localAI[2] <= 0) {
				if (this.GetHurtByHostiles()) {
					// add sound
					Projectile.localAI[2] = 20;
				}
			} else {
				Projectile.localAI[2]--;
			}
		}
		/// <inheritdoc cref="RandomChoice(int, int)"/>
		public bool RandomChoice(int consequent) => RandomChoice(1, consequent);
		/// <summary>
		/// If the projectile is not locally owned, do nothing
		/// Otherwise, Returns true and set Projectile.netUpdate to true X out of Y times.
		/// </summary>
		/// <param name="antecedent">X</param>
		/// <param name="consequent">Y</param>
		/// <returns></returns>
		public bool RandomChoice(int antecedent, int consequent) {
			if (!Projectile.IsLocallyOwned()) return false;
			if (!Main.rand.NextBool(antecedent, consequent)) return false;
			Projectile.netUpdate = true;
			return true;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.damage > 0) {
				hit.HitDirection *= -1;
				hit.Knockback = 6;
				hit.Crit = false;
				Projectile.velocity = OriginExtensions.GetKnockbackFromHit(hit);
				this.DamageArtifactMinion(target.damage, new NPCDamageSource(target));
				Projectile.ai[1] = 1;
				Projectile.netUpdate = true;
			}
		}
		public override void OnKill(int timeLeft) {
			base.OnKill(timeLeft);
		}
		public override bool PreDraw(ref Color lightColor) {
			SpriteEffects effect = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Rectangle frame = TextureAssets.Projectile[Type].Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			DrawData data = new(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation,
				effect.ApplyToOrigin(new(26, 18), frame),
				1,
				effect
			);
			Main.EntitySpriteDraw(data);
			data.texture = glowTexture;
			data.color = Color.White;
			Main.EntitySpriteDraw(data);
			return false;
		}
	}
}
public class Star_Soldier_Pod : Star_Soldier_Weapon {
	static int AmmoMax => 4;
	static int ReloadLength => 100 / AmmoMax;
	int ammo = AmmoMax;
	int reloadTime = 0;
	bool reloading = false;
	public override void SetStaticDefaults() {
		AmmoID.Sets.SpecificLauncherAmmoProjectileFallback[Type] = ItemID.RocketLauncher;
	}
	public override void SetDefaults() {
		Item.DefaultToCanisterLauncher<Star_Soldier_Pod_Missile>(184, 12, 6, 0, 0, true);
		Item.DamageType = DamageClasses.Explosive;
		Item.useAnimation *= AmmoMax;
		Item.knockBack = 4f;
		Item.UseSound = Origins.Sounds.ThrusterChargeUp.WithPitch(3f).WithVolume(0.6f);
		Item.useStyle = ItemUseStyleID.Shoot;
		Item.rare = ItemRarityID.Yellow;
	}
	public override bool CanUseItem(Player player) => !reloading && ammo > 0;
	public override void PreItemCheck(Player player, Star_Soldier.MountHandler handler, ref Star_Soldier.MountHandler.Arm arm) => player.OriginPlayer().projectileSpeedBoost *= 1.1f;
	public override void ModifyDrawData(Star_Soldier.MountHandler mountHandler, ref DrawData drawData) {
		drawData.sourceRect = drawData.texture.Frame(verticalFrames: 2, frameY: reloading.ToInt());
	}
	public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
		position += velocity
			.Normalized(out _)
			.Perpendicular(player.direction * (int)player.gravDir)
			* (player.itemAnimation / player.itemTimeMax) switch {
				3 or 1 => -10, //second & fourth come from the bottom
				_ => 6 //first & third come from the top
			};
	}
	public override void UpdateEquipped(Player player, ref Star_Soldier.MountHandler.Arm arm, bool control) {
		if (arm.itemAnimation != 0) {
			reloadTime = 0;
			if (arm.itemTime == arm.itemTimeMax) ammo--;
		} else if (reloading) {
			if (reloadTime.CycleUp(ReloadLength)) {
				SoundEngine.PlaySound(SoundID.Item53.WithPitch(0.5f).WithVolume(0.35f), player.MountedCenter);
				if (ammo.Warmup(AmmoMax)) {
					SoundEngine.PlaySound(SoundID.Item108.WithPitch(-0.5f).WithVolume(0.5f), player.MountedCenter);
					SoundEngine.PlaySound(SoundID.Item103.WithPitch(1.5f).WithVolume(0.5f), player.MountedCenter);
					reloading = false;
				}
			}
		} else if (ammo < AmmoMax) {
			reloading = true;
		}
	}
	public override void PlaySound(Player player) {
		SoundEngine.PlaySound(SoundID.Item108.WithPitch(-1f), player.MountedCenter);
		SoundEngine.PlaySound(SoundID.Item113.WithPitch(1.2f), player.MountedCenter);
	}
	public override void DrawHud(SpriteBatch spriteBatch, ref Vector2 position, Vector2 scale, in Star_Soldier.MountHandler.Arm arm) {
		int width = 64;
		int segment = (int)((width * 0.75f) / AmmoMax);
		for (int i = 0; i < AmmoMax; i++) {
			Color color = reloading ? Color.White : Color.OrangeRed;
			if (i >= ammo) color = Color.Black;
			spriteBatch.Draw(
				TextureAssets.MagicPixel.Value,
				position,
				new Rectangle(0, 0, segment, 4),
				color,
				0,
				new Vector2(segment * ((i - 0.5f * (AmmoMax - 1)) * 1.1f + i / (float)(AmmoMax - 1)), 2),
				1,
				SpriteEffects.None,
			0);
		}
		position.Y += 8;
	}
	public class Star_Soldier_Pod_Missile : ModProjectile, ICanisterProjectile {
		public static AutoLoadingAsset<Texture2D> outerTexture = typeof(Star_Soldier_Pod_Missile).GetDefaultTMLName("_Outer");
		public static AutoLoadingAsset<Texture2D> innerTexture = typeof(Star_Soldier_Pod_Missile).GetDefaultTMLName("Inner");
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 4;
			Origins.MagicTripwireRange[Type] = 40;
			Defensive_Turret.TargetProjectilesLow[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = 420;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
			Projectile.alpha = 255;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.velocity != default) Projectile.ai[1] = MathF.Round(60 / Projectile.velocity.Length());
		}
		public override void AI() {
			if (Projectile.ai[1] > 0) {
				Projectile.ai[1]--;
				Projectile.numUpdates++;
				return;
			}
			if (++Projectile.localAI[0] < 30) {
				Projectile.velocity *= 0.97f;
				if (Projectile.velocity != default) Projectile.rotation = Projectile.velocity.ToRotation();
				Projectile.frame = (int)((Projectile.localAI[0] / 30) * (Main.projFrames[Type] - 1));
				return;
			}
			Projectile.frame = Main.projFrames[Type] - 1;
			Color dustColor = default;
			int dustType = DustID.Torch;
			if (Projectile.TryGetGlobalProjectile(out CanisterGlobalProjectile global)) {
				if (global.CanisterData?.HasSpecialEffect ?? false) {
					dustType = Tintable_Torch_Dust.ID;
					dustColor = global.CanisterData.InnerColor with { A = 100 };
				}
			}
			Vector2 dustBasePos = Projectile.Center - Projectile.rotation.ToRotationVector2() * 35 - new Vector2(4);
			for (int i = 0; i < 2; i++) {
				Vector2 offset = Projectile.velocity * i * 0.5f;

				if (Main.rand.NextBool(2)) {
					Dust dust = Dust.NewDustDirect(
						dustBasePos + offset - Projectile.velocity * 0.5f,
						Projectile.width - 8,
						Projectile.height - 8,
						dustType,
						0f,
						0f,
						100,
						dustColor
					);
					dust.scale *= 1.4f + Main.rand.Next(10) * 0.1f;
					dust.velocity *= 0.2f;
					dust.noGravity = true;
				}

				if (Main.rand.NextBool(2)) {
					Dust dust = Dust.NewDustDirect(
						dustBasePos + offset - Projectile.velocity * 0.5f,
						Projectile.width - 8,
						Projectile.height - 8,
						DustID.Smoke,
						0f,
						0f,
						100,
						default,
						0.5f
					);
					dust.fadeIn = 0.5f + Main.rand.Next(5) * 0.1f;
					dust.velocity *= 0.05f;
				}
			}
			if (Projectile.velocity != default) Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;

			float targetWeight = 300;
			Vector2 targetPos = default;
			bool foundTarget;
			Player player = Main.player[Projectile.owner];
			if (Star_Soldier.GetHandler(player)?.LockOnTarget is Entity entity) {
				targetPos = entity.Center;
				foundTarget = true;
			} else {
				foundTarget = player.DoHoming((target) => {
					Vector2 currentPos = target.Center;
					float dist = Math.Abs(Projectile.Center.X - currentPos.X) + Math.Abs(Projectile.Center.Y - currentPos.Y);
					if (target is Player) dist *= 2.5f;
					if (dist < targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
						targetWeight = dist;
						targetPos = currentPos;
						return true;
					}
					return false;
				});
			}

			Vector2 targetVelocity = foundTarget ? (targetPos - Projectile.Center).Normalized(out _) : Projectile.rotation.ToRotationVector2();
			float scaleFactor = 16f;
			float lerpValue = 0.083333336f;
			targetVelocity *= scaleFactor;
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity, lerpValue);
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Rectangle frame = TextureAssets.Projectile[Type].Value.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			Vector2 origin = spriteEffects.ApplyToOrigin(new(35, 10), frame);
			if (!canisterData.HasSpecialEffect) {
				Main.EntitySpriteDraw(
					TextureAssets.Projectile[Type].Value,
					projectile.Center - Main.screenPosition,
					frame,
					lightColor,
					projectile.rotation,
					origin,
					projectile.scale,
					spriteEffects
				);
				return;
			}
			Main.EntitySpriteDraw(
				InnerTexture,
				projectile.Center - Main.screenPosition,
				frame,
				canisterData.InnerColor,
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				OuterTexture,
				projectile.Center - Main.screenPosition,
				frame,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
		}
	}
}
