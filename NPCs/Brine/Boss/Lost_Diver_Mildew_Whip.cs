using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Summoner;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Brine.Boss {
	public class Lost_Diver_Mildew_Whip : ModProjectile {
		public static int UseTime => 45;
		public override string Texture => typeof(Mildew_Whip_P).GetDefaultTMLName();
		public NPC Owner => Main.npc[(int)Projectile.ai[2]];
		public override void SetStaticDefaults() {
			Amebic_Vial.CanBeDeflected[Type] = false;
		}
		public override void SetDefaults() {
			Projectile.DefaultToWhip();
			Projectile.aiStyle = -1;
			Projectile.ownerHitCheck = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.extraUpdates = 1;
			Projectile.friendly = false;
			Projectile.friendly = true;
			Projectile.hostile = true;
			Projectile.WhipSettings.Segments = 20;
			Projectile.WhipSettings.RangeMultiplier = 0.8f * Projectile.scale;
		}
		public override void AI() {
			NPC owner = Owner;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi / 2f;
			Projectile.ai[0] += 1f;
			float timeToFlyOut = UseTime * Projectile.MaxUpdates;
			Vector2 handPosition = owner.Center;
			if (owner.ModNPC is Lost_Diver lostDiver) {
				handPosition = lostDiver.GetHandPosition();
			}
			Projectile.Center = handPosition + Projectile.velocity * (Projectile.ai[0] - 1f);
			Projectile.spriteDirection = Vector2.Dot(Projectile.velocity, Vector2.UnitX) >= 0f ? 1 : -1;
			if (Projectile.ai[0] >= timeToFlyOut) {
				Projectile.Kill();
				return;
			}
			if (Projectile.ai[0] >= (int)(timeToFlyOut / 2f) && Projectile.soundDelay >= 0) {
				Projectile.soundDelay = -1;
				Projectile.WhipPointsForCollision.Clear();
				FillWhipControlPoints(Projectile, handPosition, Projectile.WhipPointsForCollision);
				Vector2 vector = Projectile.WhipPointsForCollision[^1];
				SoundEngine.PlaySound(SoundID.Item153, vector);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Projectile.WhipPointsForCollision.Clear();
			NPC owner = Owner;
			Vector2 handPosition = owner.Center;
			if (owner.ModNPC is Lost_Diver lostDiver) {
				handPosition = lostDiver.GetHandPosition();
			}
			FillWhipControlPoints(Projectile, handPosition, Projectile.WhipPointsForCollision);
			for (int m = 0; m < Projectile.WhipPointsForCollision.Count; m++) {
				Point point = Projectile.WhipPointsForCollision[m].ToPoint();
				projHitbox.Location = new Point(point.X - projHitbox.Width / 2, point.Y - projHitbox.Height / 2);
				if (projHitbox.Intersects(targetHitbox)) {
					return true;
				}
			}
			return false;
		}
		public override bool? CanHitNPC(NPC target) {
			if (Mildew_Creeper.FriendlyNPCTypes.Contains(target.type)) return false;
			return null;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.ScalingArmorPenetration += Brine_Pool_NPC.ScalingArmorPenetrationToCompensateForTSNerf;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			if (Main.rand.NextBool(2, 3)) target.AddBuff(Toxic_Shock_Debuff.ID, 120);
			Projectile.damage = (int)(Projectile.damage * 0.8);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (Main.rand.NextBool(2, 3)) target.AddBuff(Toxic_Shock_Debuff.ID, 120);
			Projectile.damage = (int)(Projectile.damage * 0.8);
		}
		public override bool PreDraw(ref Color lightColor) {
			List<Vector2> list = new();
			NPC owner = Owner;
			Vector2 handPosition = owner.Center;
			if (owner.ModNPC is Lost_Diver lostDiver) {
				handPosition = lostDiver.GetHandPosition();
			}
			FillWhipControlPoints(Projectile, handPosition, list);

			SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

			Main.instance.LoadProjectile(Type);
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			Vector2 pos = list[0];

			for (int i = 0; i < list.Count; i++) {
				// These two values are set to suit this projectile's sprite, but won't necessarily work for your own.
				// You can change them if they don't!
				Rectangle frame = new(0, 0, 48, 28);
				Vector2 origin = new(24, 14);
				Vector2 scale = new Vector2(0.85f) * Projectile.scale;

				if (i == list.Count - 1) {
					frame.Y = 112;
				} else if (i > 10) {
					frame.Y = 84;
				} else if (i > 5) {
					frame.Y = 56;
				} else if (i > 0) {
					frame.Y = 28;
				}

				Vector2 element = list[i];
				Vector2 diff;
				if (i == list.Count - 1) {
					diff = element - list[i - 1];
				} else {
					diff = list[i + 1] - element;
				}

				float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
				Color color = Lighting.GetColor(element.ToTileCoordinates());

				Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

				pos += diff;
			}
			return false;
		}
		public static void FillWhipControlPoints(Projectile proj, Vector2 playerArmPosition, List<Vector2> controlPoints) {
			int timeToFlyOut = UseTime * proj.MaxUpdates;
			int segments = proj.WhipSettings.Segments;
			float rangeMultiplier = proj.WhipSettings.RangeMultiplier;
			float num = proj.ai[0] / timeToFlyOut;
			float num2 = 0.5f;
			float num3 = 1f + num2;
			float num4 = MathHelper.Pi * 10f * (1f - num * num3) * (-proj.spriteDirection) / segments;
			float num5 = num * num3;
			float num6 = 0f;
			if (num5 > 1f) {
				num6 = (num5 - 1f) / num2;
				num5 = MathHelper.Lerp(1f, 0f, num6);
			}
			float num7 = (UseTime * 2) * num;
			float num8 = proj.velocity.Length() * num7 * num5 * rangeMultiplier / segments;
			float num9 = 1f;
			Vector2 vector = playerArmPosition;
			float num10 = -MathHelper.PiOver2;
			Vector2 vector2 = vector;
			float num11 = MathHelper.PiOver2 + MathHelper.PiOver2 * proj.spriteDirection;
			Vector2 vector3 = vector;
			float num12 = MathHelper.PiOver2;
			controlPoints.Add(playerArmPosition);
			for (int i = 0; i < segments; i++) {
				float num13 = i / (float)segments;
				float num14 = num4 * num13 * num9;
				Vector2 vector4 = vector + num10.ToRotationVector2() * num8;
				Vector2 vector5 = vector3 + num12.ToRotationVector2() * (num8 * 2f);
				Vector2 vector6 = vector2 + num11.ToRotationVector2() * (num8 * 2f);
				float num15 = 1f - num5;
				float num16 = 1f - num15 * num15;
				Vector2 value = Vector2.Lerp(vector5, vector4, num16 * 0.9f + 0.1f);
				Vector2 vector7 = Vector2.Lerp(vector6, value, num16 * 0.7f + 0.3f);
				Vector2 spinningpoint = playerArmPosition + (vector7 - playerArmPosition) * new Vector2(1f, num3);
				float num17 = num6;
				num17 *= num17;
				Vector2 item = spinningpoint.RotatedBy(proj.rotation + 4.712389f * num17 * proj.spriteDirection, playerArmPosition);
				controlPoints.Add(item);
				num10 += num14;
				num12 += num14;
				num11 += num14;
				vector = vector4;
				vector3 = vector5;
				vector2 = vector6;
			}
		}
	}
}
