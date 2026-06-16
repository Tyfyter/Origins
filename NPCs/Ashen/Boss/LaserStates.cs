using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Shaders;
using Origins.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Magic;
using Origins.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.Boss.Trenchmaker;

namespace Origins.NPCs.Ashen.Boss; 
public class Fire_Lasers_State : AIState {
	#region stats
	public static int ShotDamage => (int)(25 * DifficultyMult);
	public static int ChargeTime => 120;
	public static int ActiveTime => 70;
	#endregion stats
	public override bool Ranged => true;
	public override GunKind? ForGunType => GunKind.Laser;
	public override void Load() {
		PhaseOneIdleState.aiStates.Add(this);
	}
	public override void DoAIState(Trenchmaker boss) {
		NPC npc = boss.NPC;
		if (npc.HasValidTarget) npc.targetRect = npc.GetTargetData().Hitbox;
		Vector2 diff = npc.targetRect.Center() - boss.GunPos;
		npc.ai[1].Cooldown();
		switch (npc.ai[0]) {
			case 0: {
				Vector2 dir = npc.rotation.ToRotationVector2();
				npc.SpawnProjectile(null,
					boss.GunPos + dir * 16,
					dir,
					ModContent.ProjectileType<Trenchmaker_Laser_P>(),
					ShotDamage,
					1
				);
				npc.ai[0] = 1;
				npc.ai[1] = ChargeTime;
				break;
			}
			case 1: {
				GeometryUtils.AngularSmoothing(ref npc.rotation, diff.ToRotation(), 0.04f * float.Pow(npc.ai[1] / ChargeTime, 2) + 2 / diff.Length());
				if (npc.ai[1] == 0) {
					npc.ai[0] = 2;
					npc.ai[1] = ActiveTime;
				}
				break;
			}
			case 2: {
				GeometryUtils.AngularSmoothing(ref npc.rotation, diff.ToRotation(), 2 / diff.Length());
				if (npc.ai[1] == 0) boss.StartIdle();
				break;
			}
		}
	}
	public override bool ShouldRotateGuns(Trenchmaker boss) => false;
	public class Trenchmaker_Laser_P : ModProjectile {
		static readonly AdvancedMiscShaderData hitAOEShader = new(ModContent.Request<Effect>("Origins/Effects/Radial"), "TrenchmakerLaserHit", [
			new("uOffset", new Vector2(0.5f)),
			new("uScale", float.Sqrt(0.5f))
		]);
		static Parameter uImageOffset1;
		static Parameter uColorMatrix0;
		static Parameter uColorMatrix1;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 3200 + 64;
			hitAOEShader.UseSamplerState(SamplerState.PointWrap)
			.UseImage1(TextureAssets.MagicPixel);
			GameShaders.Misc["Origins:TrenchmakerLaserHit"] = hitAOEShader;
			hitAOEShader.LoadThen(() => {
				hitAOEShader.CreateParameter(ref uImageOffset1, nameof(uImageOffset1), Vector2.Zero);
				hitAOEShader.CreateParameter(ref uColorMatrix0, nameof(uColorMatrix0), Matrix.Identity);
				hitAOEShader.CreateParameter(ref uColorMatrix1, nameof(uColorMatrix1), Matrix.Identity);
			});
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 5;
		}
		public override bool ShouldUpdatePosition() => false;
		public Vector2 TargetPos {
			get => new(Projectile.ai[0], Projectile.ai[1]);
			set => (Projectile.ai[0], Projectile.ai[1]) = value;
		}
		bool IsActive {
			get => Projectile.localAI[0] != 0;
			set => Projectile.localAI[0] = value.ToInt();
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[2] = -1;
			if (source is EntitySource_Parent { Entity: NPC owner }) Projectile.ai[2] = owner.whoAmI;
		}
		public override void AI() {
			if (Main.npc.GetIfInRange((int)Projectile.ai[2]) is not NPC { active: true } owner || owner.ModNPC is not Trenchmaker trenchmaker || trenchmaker.GetState() is not Fire_Lasers_State) {
				Projectile.Kill();
				return;
			}
			IsActive = owner.ai[0] == 2;
			Vector2 gunPos = trenchmaker.GunPos;
			Projectile.localAI[1] = owner.ai[1];
			Projectile.velocity = owner.rotation.ToRotationVector2();
			Projectile.position = gunPos + Projectile.velocity * 24;
			Vector2 targetPos = Projectile.position + Projectile.velocity * Raymarch(Projectile.position, Projectile.velocity, ProjectileID.Sets.DrawScreenCheckFluff[Type] - 64);
			if (IsActive) {
				SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(2.7f).WithVolume(0.5f), Projectile.Center);
				SoundEngine.SoundPlayer.Play(SoundID.Item72.WithVolume(0.8f), Projectile.Center);
				Dust.NewDust(targetPos - Vector2.One * 2, 4, 4, DustID.AmberBolt);
			}
			Projectile.localAI[2] += 1f / 60;
			TargetPos = targetPos;
			SoundEngine.SoundPlayer.Play(SoundID.Item158.WithPitch(++owner.ai[3] / 10).WithVolume(0.8f), Projectile.Center);
			SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(owner.ai[3] / 20).WithVolume(0.8f), Projectile.Center);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (!IsActive) return false;
			if (targetHitbox.IsWithin(TargetPos, 16 * 5)) return true;
			return targetHitbox.Contains(targetHitbox.Center().SnapToLine(Projectile.position, TargetPos, radius: 12));
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			if (!target.immune) modifiers = modifiers with { CooldownCounter = -2 };
			modifiers.Knockback *= 1.5f;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (info.CooldownCounter == -2) {
				target.immune = true;
				target.immuneTime = target.longInvince ? 16 : 8;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			if (!TargetPos.IsWithin(TargetPos.Clamp(Main.screenPosition, Main.screenPosition + Main.ScreenSize.ToVector2()), 64) && !Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), Projectile.position, TargetPos)) return false;
			using GraphicsExt.SpritebatchOverride _ = Main.spriteBatch.OverrideState(SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap);
			float reduce = IsActive ? 0 : -(Projectile.localAI[1] / ChargeTime);
			hitAOEShader.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]).Apply(null,
				uImageOffset1 with { Value = new Vector2(Projectile.localAI[2], Projectile.localAI[2] * -0.5f) },
				uColorMatrix0 with { Value = Matrix.Identity with {
					M14 = reduce,
					M24 = reduce,
					M34 = reduce
				} },
				uColorMatrix1
			);
			Main.spriteBatch.Draw(
				TextureAssets.Projectile[Type].Value,
				TargetPos - Main.screenPosition,
				null,
				new Color(255, IsActive ? 40 : 100, 0, 0),
				Projectile.localAI[2],
				Vector2.One * 128,
				Vector2.One * 5,
				0,
			0);
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			Vector2 diff = TargetPos - Projectile.position;
			Vector2 position = Projectile.position;
			position -= Main.screenPosition;
			float rotation = diff.ToRotation();
			float dist = diff.Length();
			const float scale = 1f / 256f;
			DrawData data = new(
				TextureAssets.Extra[ExtrasID.RainbowRodTrailShape].Value,//TextureAssets.MagicPixel.Value,
				position,
				null,
				new Color(255, IsActive ? 40 : 100, 0, 0),
				rotation,
				Vector2.UnitY * 128,
				new Vector2(dist * scale, 24 * scale),
				0
			);
			data.Draw(Main.spriteBatch);
			Rectangle frame = new(256 - (int)((Projectile.localAI[2] * 600) % 256), 0, (int)dist, 256);
			data.scale.X = 1;
			data.scale.Y *= 2;
			data.texture = TextureAssets.Extra[ExtrasID.MagicMissileTrailShape].Value;
			float progress = 1 - Projectile.localAI[1] / ChargeTime;
			progress *= progress;
			if (IsActive) progress = 1;
			Min(ref progress, 1);
			data.color *= progress;
			Vector2 offset = (rotation + MathHelper.PiOver2).ToRotationVector2() * (1 - progress) * 24;
			data.position = position + offset;
			frame.Width = (int)Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist + 16).OrXIf(dist + 16, dist);
			data.sourceRect = frame;
			data.Draw(Main.spriteBatch);
			data.position = position - offset;
			frame.Width = (int)Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist + 16).OrXIf(dist + 16, dist);
			data.sourceRect = frame;
			data.Draw(Main.spriteBatch);
			return false;
		}
		public static float Raymarch(Vector2 position, Vector2 direction, float maxLength = float.PositiveInfinity) {
			float dist = CollisionExt.Raymarch(position, direction, maxLength);
			foreach (NPC npc in Main.ActiveNPCs) {
				if (dist < 16) return dist;
				if (!npc.friendly) continue;
				if (position.Clamp(npc.Hitbox).DistanceSQ(position) >= dist * dist) continue;
				float collisionPoint = 1;
				if (Collision.CheckAABBvLineCollision(npc.position, npc.Size, position, position + direction * dist, 1, ref collisionPoint)) {
					Min(ref dist, collisionPoint);
				}
			}
			foreach (Player player in Main.ActivePlayers) {
				if (dist < 16) return dist;
				if (position.Clamp(player.Hitbox).DistanceSQ(position) >= dist * dist) continue;
				float collisionPoint = 1;
				if (Collision.CheckAABBvLineCollision(player.position, player.Size, position, position + direction * dist, 1, ref collisionPoint)) {
					Min(ref dist, collisionPoint);
				}
			}
			return dist;
		}
	}
}
public class Laser_Target_Locator_State : AIState {
	#region stats
	public static int MissileDamage => (int)(15 + 35 * DifficultyMult);
	public static int MissileRate => (int)(40 - 5 * DifficultyMult);
	public static int MissileSpeed => 24;
	public static int MissileCount => 16;
	public static int ChargeTime => 60;
	#endregion stats
	public override bool Ranged => true;
	public override GunKind? ForGunType => GunKind.Laser;
	public override void Load() {
		PhaseOneIdleState.aiStates.Add(this);
	}
	public override void DoAIState(Trenchmaker boss) {
		NPC npc = boss.NPC;
		if (npc.HasValidTarget) npc.targetRect = npc.GetTargetData().Hitbox;
		Vector2 diff = npc.targetRect.Center() - boss.GunPos;
		switch (npc.ai[0]) {
			case 0: {
				Vector2 dir = npc.rotation.ToRotationVector2();
				npc.SpawnProjectile(null,
					boss.GunPos + dir * 16,
					dir,
					ModContent.ProjectileType<Trenchmaker_Laser_Target_Locator>(),
					MissileDamage,
					1
				);
				npc.ai[0] = 1;
				npc.ai[1] = 0;
				break;
			}
			case 1: {
				GeometryUtils.AngularSmoothing(ref npc.rotation, diff.ToRotation(), 0.05f);
				break;
			}
			case 2: {
				boss.StartIdle();
				break;
			}
		}
	}
	public override bool ShouldRotateGuns(Trenchmaker boss) => false;
	public override double GetWeight(Trenchmaker boss, int[] previousStates) {
		int marker = ModContent.ProjectileType<Trenchmaker_Laser_Target_Locator_Marker>();
		int missile = ModContent.ProjectileType<Trenchmaker_Laser_Target_Locator_Missile>();
		foreach (Projectile proj in Main.ActiveProjectiles) {
			if (proj.type == marker || proj.type == missile) return 0;
		}
		return base.GetWeight(boss, previousStates);
	}
	public class Trenchmaker_Laser_Target_Locator : ModProjectile {
		public override string Texture => typeof(Laser_Target_Locator).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600 + 64;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.hostile = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 5;
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool? CanDamage() => false;
		public Vector2 TargetPos {
			get => new(Projectile.ai[0], Projectile.ai[1]);
			set => (Projectile.ai[0], Projectile.ai[1]) = value;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[2] = -1;
			if (source is EntitySource_Parent { Entity: NPC owner }) Projectile.ai[2] = owner.whoAmI;
		}
		public override void AI() {
			if (Main.npc.GetIfInRange((int)Projectile.ai[2]) is not NPC { active: true } owner || owner.ModNPC is not Trenchmaker trenchmaker || trenchmaker.GetState() is not Laser_Target_Locator_State) {
				Projectile.Kill();
				return;
			}
			Vector2 gunPos = trenchmaker.GunPos;
			Projectile.localAI[1] = owner.ai[1];
			Projectile.velocity = owner.rotation.ToRotationVector2();
			Projectile.position = gunPos + Projectile.velocity * 24;
			Vector2 newTarget = Projectile.position + Projectile.velocity * Raymarch(Projectile.position, Projectile.velocity, ProjectileID.Sets.DrawScreenCheckFluff[Type] - 64);
			owner.ai[1] -= float.Pow(Projectile.ai[0] - newTarget.X, 2) * 0.01f;
			Max(ref owner.ai[1], 0);
			TargetPos = newTarget;
			SoundEngine.SoundPlayer.Play(SoundID.Item158.WithPitch(++owner.ai[3] / 10).WithVolume(0.8f), Projectile.Center);
			SoundEngine.SoundPlayer.Play(SoundID.Item67.WithPitch(owner.ai[3] / 20).WithVolume(0.8f), Projectile.Center);
			if (++owner.ai[1] >= ChargeTime) {
				owner.ai[0] = 2;
				owner.ai[1] = 0;
				SoundEngine.SoundPlayer.Play(SoundID.ResearchComplete.WithPitch(-2f).WithVolume(0.5f), TargetPos);
				Projectile.SpawnProjectile(
					Projectile.GetSource_FromAI(),
					TargetPos,
					default,
					ModContent.ProjectileType<Trenchmaker_Laser_Target_Locator_Marker>(),
					Projectile.damage,
					Projectile.knockBack
				);
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			if (!Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), Projectile.position, TargetPos)) return false;

			Vector2 diff = TargetPos - Projectile.position;
			Vector2 position = Projectile.position;
			position -= Main.screenPosition;
			float rotation = diff.ToRotation();
			float dist = diff.Length();
			const float scale = 1f / 256f;
			Rectangle frame = new(0, 0, (int)dist, 256);
			DrawData data = new(
				TextureAssets.Extra[ExtrasID.RainbowRodTrailShape].Value,//TextureAssets.MagicPixel.Value,
				position,
				frame,
				new Color(0, 255, 0, 0),
				rotation,
				Vector2.UnitY * 128,
				new Vector2(1, 8 * scale),
				0
			);
			data.Draw(Main.spriteBatch);
			float progress = Projectile.localAI[1] / ChargeTime;
			progress *= progress;
			Min(ref progress, 1);
			data.color *= progress;
			Vector2 offset = (rotation + MathHelper.PiOver2).ToRotationVector2() * (1 - progress) * 16;
			data.position = position + offset;
			frame.Width = (int)Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist + 16).OrXIf(dist + 16, dist);
			data.sourceRect = frame;
			data.Draw(Main.spriteBatch);
			data.position = position - offset;
			frame.Width = (int)Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist + 16).OrXIf(dist + 16, dist);
			data.sourceRect = frame;
			data.Draw(Main.spriteBatch);
			return false;
		}
		public static float Raymarch(Vector2 position, Vector2 direction, float maxLength = float.PositiveInfinity) {
			float dist = CollisionExt.Raymarch(position, direction, maxLength);
			foreach (NPC npc in Main.ActiveNPCs) {
				if (dist < 16) return dist;
				if (!npc.friendly) continue;
				if (position.Clamp(npc.Hitbox).DistanceSQ(position) >= dist * dist) continue;
				float collisionPoint = 1;
				if (Collision.CheckAABBvLineCollision(npc.position, npc.Size, position, position + direction * dist, 1, ref collisionPoint)) {
					Min(ref dist, collisionPoint);
				}
			}
			foreach (Player player in Main.ActivePlayers) {
				if (dist < 16) return dist;
				if (position.Clamp(player.Hitbox).DistanceSQ(position) >= dist * dist) continue;
				float collisionPoint = 1;
				if (Collision.CheckAABBvLineCollision(player.position, player.Size, position, position + direction * dist, 1, ref collisionPoint)) {
					Min(ref dist, collisionPoint);
				}
			}
			return dist;
		}
	}
	public class Trenchmaker_Laser_Target_Locator_Marker : ModProjectile {
		public override string Texture => typeof(Laser_Target_Locator_Marker).GetDefaultTMLName();
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.width = 36;
			Projectile.height = 32;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if ((int)(++Projectile.ai[0] / MissileRate) > Projectile.localAI[1] && Projectile.IsLocallyOwned()) {
				const float spawn_dist = 2400;
				Vector2 position = Projectile.Center;
				position.X += Main.rand.NextFloat(16 * 5) * Main.rand.NextBool().ToDirectionInt();
				Vector2 direction = Vector2.UnitY.RotatedByRandom(0.2f);
				float speed = MissileSpeed;
				SoundEngine.SoundPlayer.Play(SoundID.Item92.WithPitch(-1.2f).WithPitchVarience(1f).WithVolume(0.6f), position);
				SoundEngine.SoundPlayer.Play(SoundID.Item103.WithPitch(-1.2f).WithPitchVarience(1f), position);
				Projectile.NewProjectile(
					Projectile.GetSource_FromAI(),
					position - direction * spawn_dist,
					direction * speed,
					ModContent.ProjectileType<Trenchmaker_Laser_Target_Locator_Missile>(),
					Projectile.damage,
					Projectile.knockBack,
					ai0: ((spawn_dist - CollisionExt.Raymarch(position, -direction, spawn_dist)) + 40) / speed
				);
				if (++Projectile.localAI[1] >= MissileCount) Projectile.Kill();
			}
		}
		public override Color? GetAlpha(Color lightColor) => Color.White * Utils.Remap(float.Sin(Projectile.ai[0] * (MathHelper.Pi / MissileRate)), -1, 1, 0.25f, 1f);
	}
	public class Trenchmaker_Laser_Target_Locator_Missile : ModProjectile {
		public override string Texture => typeof(Laser_Target_Locator_Missile).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.projFrames[Type] = 3;
			OriginsSets.Projectiles.AllowAboveWorld[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
			Projectile.width = Projectile.height = 34;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.hostile = true;
			Projectile.penetrate = 1;
			Projectile.extraUpdates = 1;
			Projectile.tileCollide = false;
		}
		public override void AI() {
			Dust.NewDustPerfect(Projectile.Top, DustID.Torch, Projectile.velocity * 0.85f).noGravity = true;
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (++Projectile.frameCounter >= 12) {
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Type];
			}
			Projectile.tileCollide = --Projectile.ai[0] <= 0;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => null;
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (target.statLife > 0) Projectile.penetrate--;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.life <= 0) Projectile.penetrate++;
		}
		public override void OnKill(int timeLeft) {
			Main.instance.CameraModifiers.Add(new CameraShakeModifier(
				Projectile.Center, 10f, 3f, 23, 500f, -1f, nameof(Laser_Target_Locator)
			));
			ExplosiveGlobalProjectile.DoExplosion(
				Projectile,
				128,
				sound: SoundID.Item62.WithPitch(-0.5f),
				hostile: true
			);
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				TextureAssets.Projectile[Type].Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame),
				lightColor,
				Projectile.rotation,
				new(112 - 17, 17),
				1,
				SpriteEffects.None
			);
			return false;
		}
	}
}
