using Microsoft.Xna.Framework.Graphics;
using Origins.Layers;
using Origins.Projectiles;
using Origins.Reflection;
using PegasusLib.Graphics;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace Origins.Items.Accessories;
public abstract class Retool_Arm : ModItem {
	static readonly List<Retool_Arm> arms = [];
	protected KinematicUtils.Arm arm = new() {
		bone0 = new(18.9f, 0),
		bone1 = new(29, 0),
	};
	public float ArmBaseLength => 18.9f * Item.scale;
	public sealed override void AutoStaticDefaults() {
		base.AutoStaticDefaults();
		if (arms.Count == 0) {
			try {
				MonoModHooks.Add(((orig_AltFunctionUse)ItemLoader.AltFunctionUse).Method, ReplaceAltFunctionUse);
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(ReplaceAltFunctionUse), e)) throw;
			}
		}
		arms.Add(this);
	}
	static bool ReplaceAltFunctionUse(orig_AltFunctionUse orig, Item item, Player player) {
		OriginPlayer originPlayer = player.OriginPlayer();
		if (!originPlayer.retoolArmVanity && (originPlayer.retoolArm?.ReplacesAltFunctionUse(player) ?? false)) return false;
		return orig(item, player);
	}
	delegate bool orig_AltFunctionUse(Item item, Player player);
	public override void SetDefaults() {
		Item.DefaultToAccessory();
		Item.rare = ItemRarityID.Pink;
		Item.value = Item.sellPrice(gold: 2);
		Item.master = true;
		Item.maxStack = 1;
	}
	public sealed override void UpdateAccessory(Player player, bool hideVisual) {
		OriginPlayer originPlayer = player.OriginPlayer();
		originPlayer.retoolArm = this;
		if (player.whoAmI != Main.myPlayer) return;
		if (!Keybindings.RetoolArm.JustPressed) return;
		originPlayer.retoolArmTimer = 0;
		for (int i = 0; i < arms.Count; i++) {
			if (arms[i].Type == Type) {
				SoundEngine.PlaySound(Origins.Sounds.MetalBoxOpen.WithPitch(0.2f).WithVolume(0.2f));
				originPlayer.retoolArm.OnSwitchFrom(player);
				TagCompound tag = ItemIO.Save(Item);
				Retool_Arm nextArm = arms[(i + 1) % arms.Count];
				tag["mod"] = nextArm.Mod.Name;
				tag["name"] = nextArm.Name;
				ItemIO.Load(Item, tag);
				originPlayer.retoolArm = nextArm;
				originPlayer.retoolArm.OnSwitchTo(player);
				return;
			}
		}
	}
	public override void UpdateVanity(Player player) {
		OriginPlayer originPlayer = player.OriginPlayer();
		originPlayer.retoolArm = this;
		originPlayer.retoolArmVanity = true;
	}
	public sealed override void UpdateItemDye(Player player, int dye, bool hideVisual) {
		player.OriginPlayer().retoolArmDye = dye;
	}
	public override void ModifyTooltips(List<TooltipLine> tooltips) {
		tooltips.SubstituteKeybind(Keybindings.RetoolArm);
	}
	public override bool MagicPrefix() => true;
	public override bool MeleePrefix() => true;
	public override bool RangedPrefix() => true;
	public override bool WeaponPrefix() => true;
	public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player) => equippedItem.ModItem is not Retool_Arm || incomingItem.ModItem is not Retool_Arm;
	public virtual bool ReplacesAltFunctionUse(Player player) => false;
	public abstract void UpdateArm(Player player);
	public abstract void DrawArm(ref PlayerDrawSet drawInfo);
	public virtual void OnSwitchFrom(Player player) { }
	public virtual void OnSwitchTo(Player player) { }
}
public class Retool_Arm_Cannon : Retool_Arm {
	public override string Texture => typeof(Retool_Arm).GetDefaultTMLName();
	AutoLoadingAsset<Texture2D> armTexture = typeof(Retool_Arm_Cannon).GetDefaultTMLName("_Use");
	AutoLoadingAsset<Texture2D> armGlowTexture = typeof(Retool_Arm_Cannon).GetDefaultTMLName("_Use_Glow");
	public override void SetStaticDefaults() {
		Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 4));
	}
	public override void SetDefaults() {
		base.SetDefaults();
		Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
		Item.shootSpeed = 6;
		Item.useTime = 56;
		Item.useAnimation = 56;
		Item.damage = 60;
		Item.knockBack = 6f;
	}
	public override void UpdateArm(Player player) {
		OriginPlayer originPlayer = player.OriginPlayer();
		arm.start = Retool_Arm_Layer.GetShoulder(player, player.position);
		arm.bone0.R = ArmBaseLength;
		arm.bone1.R = 29 * Item.scale;
		Vector2 targetPos = arm.start + new Vector2(16f * player.direction, -20f);
		float[] rotations = arm.GetTargetAngles(targetPos, targetPos.X < arm.start.X);
		float maxDist = 16 * 50;
		maxDist *= maxDist;
		bool hasTarget = player.DoHoming((target) => {
			Vector2 currentPos = target.Center;
			float dist = currentPos.DistanceSQ(arm.start);
			if (dist < maxDist) {
				maxDist = dist;
				targetPos = currentPos;
				return true;
			}
			return false;
		});
		if (!float.IsNaN(rotations[0])) {
			GeometryUtils.AngularSmoothing(ref originPlayer.retoolArmBaseRotation, rotations[0], 0.2f);
		}
		Vector2 spinningPoint = Retool_Arm_Layer.GetShoulder(player, player.position) + (originPlayer.retoolArmBaseRotation.ToRotationVector2() * ArmBaseLength);
		if (GeometryUtils.AngleToTarget(targetPos - spinningPoint, Item.shootSpeed) is not float angle) {
			angle = -MathHelper.PiOver2 + MathHelper.PiOver4 * player.direction;
			hasTarget = false;
		}
		GeometryUtils.AngularSmoothing(ref originPlayer.retoolArmRotation, angle, 0.2f);
		if (hasTarget || originPlayer.retoolArmTimer > 0) {
			if (++originPlayer.retoolArmTimer == 4) {
				if (!originPlayer.retoolArmVanity && hasTarget) {
					player.SpawnProjectile(
						player.GetSource_Accessory(Item),
						spinningPoint,
						originPlayer.retoolArmRotation.ToRotationVector2() * Item.shootSpeed,
						ModContent.ProjectileType<Retool_Arm_Bomb>(),
						player.GetWeaponDamage(Item),
						player.GetWeaponKnockback(Item)
					);
				} else {
					originPlayer.retoolArmTimer = 0;
				}
			}
			if (originPlayer.retoolArmTimer >= 4 + Item.useTime) originPlayer.retoolArmTimer = 0;
		}
	}
	public override void DrawArm(ref PlayerDrawSet drawInfo) {
		Player player = drawInfo.drawPlayer;
		OriginPlayer originPlayer = player.OriginPlayer();
		int frameNum = int.Clamp((originPlayer.retoolArmTimer - 4) * 4 / Item.useTime + (originPlayer.retoolArmTimer >= 4).ToInt(), 0, 3);
		SpriteEffects effects = drawInfo.playerEffect.Transpose();
		Rectangle frame = armTexture.Value.Frame(verticalFrames: 4, frameY: frameNum);
		DrawData data = new(
			armTexture,
			Retool_Arm_Layer.GetShoulder(player, drawInfo.Position).Floor() + (originPlayer.retoolArmBaseRotation.ToRotationVector2() * ArmBaseLength).Floor() - Main.screenPosition,
			frame,
			drawInfo.colorArmorHead,
			originPlayer.retoolArmRotation,
			new Vector2(5, 13).Apply(effects, frame.Size()),
			Item.scale,
			effects
			) {
			shader = originPlayer.retoolArmDye
		};
		drawInfo.DrawDataCache.Add(data);
		data.texture = armGlowTexture;
		data.color = Color.White;
		drawInfo.DrawDataCache.Add(data);
	}
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Ranged, 1), (PrefixCategory.Accessory, 2));
	}
}
public class Retool_Arm_Bomb : ModProjectile {
	public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.DirtGolfBall}";
	public override void SetStaticDefaults() {
		ProjectileID.Sets.IsARocketThatDealsDoubleDamageToPrimaryEnemy[Type] = true;
	}
	public override void SetDefaults() {
		Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
		Projectile.friendly = true;
		Projectile.width = 12;
		Projectile.height = 12;
		Projectile.extraUpdates = 1;
		Projectile.appliesImmunityTimeOnSingleHits = true;
		Projectile.usesLocalNPCImmunity = true;
		Projectile.localNPCHitCooldown = -1;
	}
	public override void AI() {
		Projectile.ai[0] += 1f / 15;
		Projectile.velocity.Y += 0.04f;
		if (Projectile.ai[0] % 2 >= 1) {
			Lighting.AddLight(Projectile.Center, 0.5f, 0, 0);
		}
		Projectile.rotation += float.Sign(Projectile.velocity.X) * 0.2f;
	}
	public override void OnKill(int timeLeft) {
		ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, sound: SoundID.Item14, fireDustAmount: 10, smokeDustAmount: 15, smokeGoreAmount: 0);
	}
	public override Color? GetAlpha(Color lightColor) {
		if (Projectile.ai[0] % 2 >= 1) {
			lightColor.R = byte.Max(lightColor.R, 180);
			lightColor.G /= 8;
			lightColor.B /= 8;
		} else {
			lightColor.R /= 8;
			lightColor.G /= 8;
			lightColor.B /= 8;
		}
		return lightColor;
	}
}
public class Retool_Arm_Laser : Retool_Arm {
	public override string Texture => typeof(Retool_Arm).GetDefaultTMLName();
	AutoLoadingAsset<Texture2D> armTexture = typeof(Retool_Arm_Laser).GetDefaultTMLName("_Use");
	AutoLoadingAsset<Texture2D> armGlowTexture = typeof(Retool_Arm_Laser).GetDefaultTMLName("_Use_Glow");
	public static int ShaderID { get; private set; }
	public override void SetStaticDefaults() {
		Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 4) { Frame = 1 });

		GameShaders.Armor.BindShader(Type, new ArmorShaderData(
			Mod.Assets.Request<Effect>("Effects/Overbrighten"),
			"Overbrighten"
		)).UseOpacity(2);
		ShaderID = GameShaders.Armor.GetShaderIdFromItemId(Type);
	}
	public override void SetDefaults() {
		base.SetDefaults();
		Item.DamageType = DamageClass.Magic;
		Item.shootSpeed = 8;
		Item.useTime = 22;
		Item.useAnimation = 22;
		Item.damage = 60;
		Item.mana = 60;
		Item.knockBack = 6f;
		Item.dye = 0;
	}
	public override void UpdateArm(Player player) {
		OriginPlayer originPlayer = player.OriginPlayer();
		arm.start = Retool_Arm_Layer.GetShoulder(player, player.position);
		arm.bone0.R = ArmBaseLength;
		arm.bone1.R = 29 * Item.scale;
		Vector2 targetPos = arm.start + new Vector2(16f * player.direction, -20f);
		float[] rotations = arm.GetTargetAngles(targetPos, targetPos.X < arm.start.X);
		float maxDist = 16 * 50;
		maxDist *= maxDist;
		bool hasTarget = player.DoHoming((target) => {
			Vector2 currentPos = target.Center;
			float dist = currentPos.DistanceSQ(arm.start);
			if (dist < maxDist) {
				maxDist = dist;
				targetPos = currentPos;
				return true;
			}
			return false;
		});
		if (!float.IsNaN(rotations[0])) {
			GeometryUtils.AngularSmoothing(ref originPlayer.retoolArmBaseRotation, rotations[0], 0.2f);
		}
		Vector2 spinningPoint = Retool_Arm_Layer.GetShoulder(player, player.position) + (originPlayer.retoolArmBaseRotation.ToRotationVector2() * ArmBaseLength);
		int beamID = ModContent.ProjectileType<Retool_Arm_Laser_Beam>();
		if (player.ownedProjectileCounts[beamID] > 0) return;
		if (!originPlayer.retoolArmVanity && player.controlUseTile) {
			player.SpawnProjectile(
				player.GetSource_Accessory(Item),
				spinningPoint,
				originPlayer.retoolArmRotation.ToRotationVector2(),
				beamID,
				player.GetWeaponDamage(Item),
				player.GetWeaponKnockback(Item),
				Item.useTime
			);
			return;
		}
		GeometryUtils.AngularSmoothing(ref originPlayer.retoolArmRotation, (targetPos - spinningPoint).ToRotation(), 0.2f);
		if (hasTarget || originPlayer.retoolArmTimer > 0) {
			if (++originPlayer.retoolArmTimer == 4) {
				if (!originPlayer.retoolArmVanity && hasTarget) {
					player.SpawnProjectile(
						player.GetSource_Accessory(Item),
						spinningPoint,
						originPlayer.retoolArmRotation.ToRotationVector2() * Item.shootSpeed,
						ModContent.ProjectileType<Retool_Arm_Laser_P>(),
						player.GetWeaponDamage(Item),
						player.GetWeaponKnockback(Item)
					);
				} else {
					originPlayer.retoolArmTimer = 0;
				}
			}
			if (originPlayer.retoolArmTimer >= 4 + Item.useTime) originPlayer.retoolArmTimer = 0;
		}
	}
	public override void DrawArm(ref PlayerDrawSet drawInfo) {
		Player player = drawInfo.drawPlayer;
		OriginPlayer originPlayer = player.OriginPlayer();
		SpriteEffects effects = drawInfo.playerEffect.Transpose();
		Rectangle frame = armTexture.Value.Frame();
		DrawData data = new(
			armTexture,
			Retool_Arm_Layer.GetShoulder(player, drawInfo.Position).Floor() + (originPlayer.retoolArmBaseRotation.ToRotationVector2() * ArmBaseLength).Floor() - Main.screenPosition,
			frame,
			drawInfo.colorArmorHead,
			originPlayer.retoolArmRotation,
			new Vector2(5, 9).Apply(effects, frame.Size()),
			Item.scale,
			effects
			) {
			shader = originPlayer.retoolArmDye
		};
		drawInfo.DrawDataCache.Add(data);
		data.texture = armGlowTexture;
		data.color = Color.White;
		drawInfo.DrawDataCache.Add(data);
	}
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Magic, 1), (PrefixCategory.Accessory, 2));
	}
	public override bool ReplacesAltFunctionUse(Player player) => true;
}
public class Retool_Arm_Laser_P : ModProjectile {
	public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MiniRetinaLaser}";
	public override void SetDefaults() {
		Projectile.DamageType = DamageClass.Magic;
		Projectile.friendly = true;
		Projectile.alpha = 255;
		Projectile.width = 12;
		Projectile.height = 12;
		Projectile.extraUpdates = 1;
	}
	public override void AI() {
		if (Projectile.soundDelay.TrySet(-1)) SoundEngine.PlaySound(SoundID.Item12, Projectile.position);
		Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		if (Projectile.alpha > 0)
			Projectile.alpha -= 15;
		Max(ref Projectile.alpha, 0);
		Lighting.AddLight(Projectile.Center, 0.8f, 0, 0.5f);
	}
	public override void OnKill(int timeLeft) {
		Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
		SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
	}
	public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;
}
public class Retool_Arm_Laser_Beam : ModProjectile, IShadedProjectile {
	public override string Texture => $"Terraria/Images/NPC_0";
	public float ManaMultiplier => 1;
	public float ChargeTime => Projectile.ai[0] * 2;
	public bool Charged => Projectile.ai[2] >= ChargeTime;
	public int Shader => Retool_Arm_Laser.ShaderID;
	public override void SetStaticDefaults() {
		ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600 + 64;
		Origins.HomingEffectivenessMultiplier[Type] = 50;
	}
	public override void SetDefaults() {
		Projectile.DamageType = DamageClass.Magic;
		Projectile.width = 0;
		Projectile.height = 0;
		Projectile.penetrate = -1;
		Projectile.friendly = true;
		Projectile.tileCollide = false;
		Projectile.localNPCHitCooldown = 6;
		Projectile.usesLocalNPCImmunity = true;
	}
	public override bool ShouldUpdatePosition() => false;
	public Vector2 TargetPos {
		get => new(Projectile.localAI[0], Projectile.localAI[1]);
		set => (Projectile.localAI[0], Projectile.localAI[1]) = value;
	}
	public override void AI() {
		Player player = Main.player[Projectile.owner];
		OriginPlayer originPlayer = player.OriginPlayer();
		Retool_Arm arm = originPlayer.retoolArm;
		if (arm is null or not Retool_Arm_Laser || !Lunatics_Rune.CheckMana(player, arm.Item, ManaMultiplier / 60f, pay: true)) {
			Projectile.Kill();
			return;
		}
		if (Projectile.velocity.X != 0) player.ChangeDir(Math.Sign(Projectile.velocity.X));

		SoundEngine.SoundPlayer.Play(SoundID.Item158.WithPitch(Projectile.ai[2] / 10).WithVolume(0.24f), player.position);
		SoundEngine.SoundPlayer.Play(SoundID.Item132.WithPitch(Projectile.ai[2] / 10).WithVolume(0.24f), player.position);
		Projectile.position = Retool_Arm_Layer.GetShoulder(player, player.position) + (originPlayer.retoolArmBaseRotation.ToRotationVector2() * arm.ArmBaseLength);
		if (player.mount?.Active ?? false) Projectile.position.Y -= player.mount.PlayerOffset;
		Projectile.position = player.RotatedRelativePoint(Projectile.position);
		if (Projectile.IsLocallyOwned()) {
			if (!player.controlUseTile) {
				Projectile.Kill();
				return;
			}
			Vector2 position = Projectile.position;
			Vector2 direction = Main.MouseWorld - position;

			Vector2 velocity = Vector2.Normalize(direction);
			if (velocity.HasNaNs()) velocity = -Vector2.UnitY;
			if (Projectile.velocity != velocity) {
				float diff = GeometryUtils.AngleDif(Projectile.velocity.ToRotation(), velocity.ToRotation(), out int dir);
				if (diff > 0) {
					Min(ref diff, 0.04f);
					Projectile.velocity = Projectile.velocity.RotatedBy(diff * dir);
					Projectile.netUpdate = true;
				}
			}
		}
		Projectile.position += Projectile.velocity * 16;
		originPlayer.retoolArmRotation = Projectile.velocity.ToRotation() - player.fullRotation;
		Vector2 newTarget = Projectile.position + Projectile.velocity * CollisionExt.Raymarch(Projectile.position, Projectile.velocity, ProjectileID.Sets.DrawScreenCheckFluff[Type] - 64);
		TargetPos = newTarget;
		Projectile.ai[2]++;
	}
	public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
		overPlayers.Add(index);
	}
	public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
		if (!Charged) return false;
		float collisionPoint = 1;
		return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.position, TargetPos, 16, ref collisionPoint);
	}
	public override void OnKill(int timeLeft) { }
	public override bool PreDraw(ref Color lightColor) {
		if (!Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), Projectile.position, TargetPos)) return false;
		SpriteBatchState state = Main.spriteBatch.GetState();
		Main.spriteBatch.Restart(state, samplerState: SamplerState.LinearWrap);
		Vector2 diff = TargetPos - Projectile.position;
		Vector2 position = Projectile.position;
		position -= Main.screenPosition;
		float rotation = diff.ToRotation();
		float dist = diff.Length();
		const float scale = 1f / 256f;
		Color color = new(255, 0, 80, 0);
		float progress = Projectile.ai[2] / ChargeTime;
		Min(ref progress, 1);
		Rectangle frame = new(256 - (int)((Projectile.ai[2] * 16) % 256), 0, (int)(dist), 256);
		DrawData data = new(
			TextureAssets.Extra[ExtrasID.RainbowRodTrailErosion].Value,//TextureAssets.MagicPixel.Value,
			position,
			frame,
			color * progress * progress,
			rotation,
			Vector2.UnitY * 128,
			new Vector2(1, 32 * scale),
			0
		);
		Main.EntitySpriteDraw(data);
		data.color = color * progress;
		Vector2 offset = (rotation + MathHelper.PiOver2).ToRotationVector2() * (1 - progress) * 8;
		data.position = position + offset;
		frame.Width = (int)CollisionExt.Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist * 1.15f + 16);
		data.sourceRect = frame;
		Main.EntitySpriteDraw(data);
		data.position = position - offset;
		frame.Width = (int)CollisionExt.Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist * 1.15f + 16);
		data.sourceRect = frame;
		Main.EntitySpriteDraw(data);
		Main.spriteBatch.Restart(state);
		return false;
	}
}
public class Retool_Arm_Saw : Retool_Arm {
	public override string Texture => typeof(Retool_Arm).GetDefaultTMLName();
	AutoLoadingAsset<Texture2D> armTexture = typeof(Retool_Arm_Saw).GetDefaultTMLName("_Use");
	public override void SetStaticDefaults() {
		Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 4) { Frame = 2 });
	}
	public override void SetDefaults() {
		base.SetDefaults();
		Item.useTime = 5;
		Item.useAnimation = 5;
		Item.DamageType = DamageClass.Melee;
		Item.damage = 35;
		Item.knockBack = 0.5f;
		Item.axe = 10;
	}
	Rectangle hitbox = new(0, 0, 22, 22);
	bool spin;
	public override void UpdateArm(Player player) {
		OriginPlayer originPlayer = player.OriginPlayer();
		bool hit = originPlayer.retoolArmTimer.CycleUp(Item.useTime) && !originPlayer.retoolArmVanity;
		arm.start = Retool_Arm_Layer.GetShoulder(player, player.position);
		arm.bone0.R = ArmBaseLength;
		arm.bone1.R = 18f * Item.scale;
		Vector2 targetPos = arm.start + new Vector2(16f * player.direction, -20f);
		float maxDist = 16 * 16 * 4 * 4;
		bool hasTarget = !originPlayer.retoolArmVanity && player.DoHoming((target) => {
			Vector2 currentPos = arm.start.Clamp(target.Hitbox);
			float dist = currentPos.DistanceSQ(arm.start);
			if (dist < maxDist) {
				maxDist = dist;
				targetPos = currentPos;
				return true;
			}
			return false;
		});
		spin = hasTarget;
		if (player.whoAmI == Main.myPlayer && player.ItemAnimationActive && player.HeldItem.axe > 0) {
			Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
			bool canChop = !player.noBuilding && tile.HasTile && Main.tileAxe[tile.TileType] && player.IsTargetTileInItemRange(player.HeldItem);
			if (canChop) {
				if (hit) DoChop(player, Player.tileTargetX, Player.tileTargetY);
				spin = true;
				targetPos = new(Player.tileTargetX * 16, Player.tileTargetY * 16);
			}
		}
		float[] rotations = arm.GetTargetAngles(targetPos, targetPos.X < arm.start.X);
		if (!float.IsNaN(rotations[0]) && !float.IsNaN(rotations[1])) {
			GeometryUtils.AngularSmoothing(ref originPlayer.retoolArmBaseRotation, rotations[0], 0.2f);
			GeometryUtils.AngularSmoothing(ref originPlayer.retoolArmRotation, rotations[0] + rotations[1] + MathHelper.PiOver2 - MathHelper.PiOver2 * player.direction, 0.2f);
		}

		hitbox = hitbox.Recentered(arm.start + originPlayer.retoolArmBaseRotation.ToRotationVector2() * ArmBaseLength + originPlayer.retoolArmRotation.ToRotationVector2() * 18f * Item.scale * player.direction);
		//hitbox.DrawDebugOutline();

		Rectangle itemRectangle = PlayerMethods.ItemCheck_EmitUseVisuals(player, Item, hitbox);
		if (hit && hasTarget) {
			int weaponDamage = player.GetWeaponDamage(Item);
			float knockBack = player.GetWeaponKnockback(Item);
			PlayerMethods.ProcessHitAgainstAllNPCsNoCooldown(player, Item, itemRectangle, weaponDamage, knockBack);
			PlayerMethods.ItemCheck_MeleeHitPVP(player, Item, itemRectangle, weaponDamage, knockBack);
		}
		if (originPlayer.wasUsingRetoolArmSaw.TrySet(spin)) {
			if (spin) {
				SoundEngine.PlaySound(Origins.Sounds.SmallSawStart.WithVolume(0.2f), player.MountedCenter);
			} else {
				SoundEngine.PlaySound(Origins.Sounds.SmallSawEnd.WithVolume(0.2f), player.MountedCenter);
			}
		}
		if (spin) {
			loopedUseSound.PlaySoundIfInactive(Origins.Sounds.SmallSaw, player.MountedCenter, sound => {
				sound.Position = player.MountedCenter;
				return spin;
			});
		} else {
			loopedUseSound.TryStop();
		}
	}
	public override void OnSwitchFrom(Player player) {
		player.OriginPlayer().wasUsingRetoolArmSaw = false;
		loopedUseSound.TryStop();
		spin = false;
	}
	public override void OnSwitchTo(Player player) {
		player.OriginPlayer().wasUsingRetoolArmSaw = false;
		loopedUseSound.TryStop();
		spin = false;
	}
	SlotId loopedUseSound = SlotId.Invalid;
	private static void DoChop(Player player, int x, int y) {
		Tile tile = Main.tile[x, y];
		if (!Main.tileAxe[tile.TileType]) return;
		int axePower = player.OriginPlayer().retoolArm.Item.axe;

		ModTile modTile = TileLoader.GetTile(tile.TileType);
		int damage = 0;
		if (Main.tileNoFail[tile.TileType]) {
			damage = 100;
		}
		if (modTile != null) {
			damage += (int)(axePower / modTile.MineResist);
		} else {
			damage += (int)(axePower * 1.2f * (tile.TileType == TileID.Cactus ? 3 : 1));
			if (Main.getGoodWorld) damage = (int)(damage * 1.3f);
		}
		if (!WorldGen.CanKillTile(x, y)) return;
		AchievementsHelper.CurrentlyMining = true;
		int hitNum = player.hitTile.HitObject(x, y, 1);
		bool isBottom = false;
		bool fail = player.hitTile.AddDamage(hitNum, damage) < 100;
		if (!fail) {
			PlayerMethods.ClearMiningCacheAt(player, x, y, 1);
			isBottom = PlayerMethods.IsBottomOfTreeTrunkNoRoots(x, y);
		}
		WorldGen.KillTile(x, y, fail: fail);
		if (NetmodeActive.MultiplayerClient) NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, x, y, fail.ToInt());
		if (isBottom && !fail && player.HeldItem.type == ItemID.AcornAxe) {
			PlayerMethods.TryReplantingTree(player, x, y);
		}
		if (damage != 0) player.hitTile.Prune();
		AchievementsHelper.CurrentlyMining = false;
	}
	public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
		modifiers.HitDirectionOverride = Math.Sign(target.Center.X - player.Center.X);
	}
	public override void DrawArm(ref PlayerDrawSet drawInfo) {
		Player player = drawInfo.drawPlayer;
		OriginPlayer originPlayer = player.OriginPlayer();
		SpriteEffects effects = drawInfo.playerEffect;
		Rectangle frame = armTexture.Value.Frame(verticalFrames: 4);
		drawInfo.DrawDataCache.Add(new(
			armTexture,
			Retool_Arm_Layer.GetShoulder(player, drawInfo.Position).Floor() + (originPlayer.retoolArmBaseRotation.ToRotationVector2() * ArmBaseLength).Floor() - Main.screenPosition,
			armTexture.Value.Frame(verticalFrames: 6, frameY: spin ? (player.miscCounter % 4) + 1 : 0),
			drawInfo.colorArmorHead,
			originPlayer.retoolArmRotation,
			new Vector2(5, 13).Apply(effects, frame.Size()),
			Item.scale,
			effects
			) {
			shader = originPlayer.retoolArmDye
		});
	}
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Melee, 1), (PrefixCategory.Accessory, 2));
	}
}
public class Retool_Arm_Vice : Retool_Arm {
	public override string Texture => typeof(Retool_Arm).GetDefaultTMLName();
	AutoLoadingAsset<Texture2D> armTexture = typeof(Retool_Arm_Vice).GetDefaultTMLName("_Use");
	AutoLoadingAsset<Texture2D> armGlowTexture = typeof(Retool_Arm_Vice).GetDefaultTMLName("_Use_Glow");
	public override void SetStaticDefaults() {
		Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 4) { Frame = 3 });
	}
	public override void SetDefaults() {
		base.SetDefaults();
		Item.useTime = 15;
		Item.useAnimation = 15;
		Item.DamageType = DamageClass.Melee;
		Item.damage = 60;
		Item.knockBack = 12f;
	}
	Rectangle hitbox = new(0, 0, 16, 16);
	Entity targetEntity;
	public override void UpdateArm(Player player) {
		OriginPlayer originPlayer = player.OriginPlayer();
		arm.start = Retool_Arm_Layer.GetShoulder(player, player.position);
		arm.bone0.R = ArmBaseLength;
		arm.bone1.R = 31 * Item.scale;
		Vector2 targetPos = arm.start + new Vector2(16f * player.direction, -20f);
		float maxDist = ArmBaseLength + 31 * Item.scale;
		maxDist *= maxDist;
		if (originPlayer.retoolArmTimer < Item.useTime) {
			originPlayer.retoolArmTimer++;
		} else if (!originPlayer.retoolArmVanity && originPlayer.retoolArmTimer == Item.useTime) {
			if (player.DoHoming((target) => {
				if (target is not NPC npc || CombinedHooks.CanPlayerHitNPCWithItem(player, Item, npc) == false) return false;
				Vector2 currentPos = arm.start.Clamp(target.Hitbox);
				float dist = currentPos.DistanceSQ(arm.start);
				if (dist < maxDist) {
					maxDist = dist;
					targetEntity = target;
					targetPos = currentPos;
					return true;
				}
				return false;
			}, false)) originPlayer.retoolArmTimer++;
		} else if (targetEntity is not null) {
			targetPos = arm.start.Clamp(targetEntity.Hitbox);
		}
		float[] rotations = arm.GetTargetAngles(targetPos, targetPos.X < arm.start.X);
		if (!float.IsNaN(rotations[0]) && !float.IsNaN(rotations[1])) {
			GeometryUtils.AngularSmoothing(ref originPlayer.retoolArmBaseRotation, rotations[0], 0.2f);
			GeometryUtils.AngularSmoothing(ref originPlayer.retoolArmRotation, rotations[0] + rotations[1] + MathHelper.PiOver2 - MathHelper.PiOver2 * player.direction, 0.2f);
		}

		hitbox = hitbox.Recentered(arm.start + originPlayer.retoolArmBaseRotation.ToRotationVector2() * ArmBaseLength + originPlayer.retoolArmRotation.ToRotationVector2() * 31f * Item.scale * player.direction);
		//hitbox.DrawDebugOutline();

		Rectangle itemRectangle = PlayerMethods.ItemCheck_EmitUseVisuals(player, Item, hitbox);

		if (originPlayer.retoolArmTimer > Item.useTime) {
			if (originPlayer.retoolArmTimer < Item.useTime + 5) {
				originPlayer.retoolArmTimer++;
			} else if (++originPlayer.retoolArmTimer >= 60) {
				if (originPlayer.retoolArmTimer >= 64) originPlayer.retoolArmTimer = 0;
			} else if (itemRectangle.Intersects(targetEntity.Hitbox)) {
				originPlayer.retoolArmTimer = 60;
				PlayerMethods.ProcessHitAgainstAllNPCsNoCooldown(player, Item, itemRectangle, player.GetWeaponDamage(Item), player.GetWeaponKnockback(Item));
			}
		}
	}
	public override void ModifyHitNPC(Player player, NPC target, ref NPC.HitModifiers modifiers) {
		modifiers.HitDirectionOverride = Math.Sign(target.Center.X - player.Center.X);
	}
	public override void DrawArm(ref PlayerDrawSet drawInfo) {
		Player player = drawInfo.drawPlayer;
		OriginPlayer originPlayer = player.OriginPlayer();
		int frameNum = 0;
		if (originPlayer.retoolArmTimer >= 60) {
			frameNum = 2;
		} else if (originPlayer.retoolArmTimer >= Item.useTime + 4) {
			frameNum = 1;
		}
		SpriteEffects effects = drawInfo.playerEffect;
		Rectangle frame = armTexture.Value.Frame(verticalFrames: 3, frameY: frameNum);
		DrawData data = new(
			armTexture,
			Retool_Arm_Layer.GetShoulder(player, drawInfo.Position).Floor() + (originPlayer.retoolArmBaseRotation.ToRotationVector2() * ArmBaseLength).Floor() - Main.screenPosition,
			frame,
			drawInfo.colorArmorHead,
			originPlayer.retoolArmRotation,
			new Vector2(5, 13).Apply(effects, frame.Size()),
			Item.scale,
			effects
			) {
			shader = originPlayer.retoolArmDye
		};
		drawInfo.DrawDataCache.Add(data);
		data.texture = armGlowTexture;
		data.color = Color.White;
		drawInfo.DrawDataCache.Add(data);
	}
	public override int ChoosePrefix(UnifiedRandom rand) {
		return OriginExtensions.GetAllPrefixes(Item, rand, (PrefixCategory.AnyWeapon, 1), (PrefixCategory.Melee, 1), (PrefixCategory.Accessory, 2));
	}
	//public override bool ReplacesAltFunctionUse(Player player) => true;
}
