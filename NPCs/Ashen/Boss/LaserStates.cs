using CalamityMod.NPCs.TownNPCs;
using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Shaders;
using Origins.Graphics;
using Origins.Items.Weapons.Magic;
using System;
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
	public static float ShotRate => 20 - DifficultyMult * 2;
	public static int ShotDamage => (int)(25 * DifficultyMult);
	public static int ChargeTime => 60;
	public static int ActiveTime => 30;
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
				GeometryUtils.AngularSmoothing(ref npc.rotation, diff.ToRotation(), 0.04f * npc.ai[1] / ChargeTime + 2 / diff.Length());
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
	public override double GetWeight(Trenchmaker boss, int[] previousStates) {
		if (boss.NPC.targetRect.Center().WithinRange(boss.NPC.Bottom + Vector2.UnitY * 16 * 4, 16 * 10)) return 0;
		return base.GetWeight(boss, previousStates);
	}
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
				Dust.NewDust(targetPos - Vector2.One * 2, 4, 4, DustID.AmberBolt);
			}
			Projectile.localAI[2] += 1f / 60;
			TargetPos = targetPos;
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
			frame.Width = (int)Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist + 16);
			data.sourceRect = frame;
			data.Draw(Main.spriteBatch);
			data.position = position - offset;
			frame.Width = (int)Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist + 16);
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
