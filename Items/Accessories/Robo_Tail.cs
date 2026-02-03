using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Projectiles;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Robo_Tail : ModItem {
		public static int TailSegmentCount(Player player) => player.maxMinions * 2;
		public static DyedLight dyeableGlow;
		public override void Load() {
			dyeableGlow = new(new(0.5f, 0, 0));
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(24, 22);
			Item.damage = 35;
			Item.DamageType = DamageClass.Summon;
			Item.knockBack = 3;
			Item.rare = ItemRarityID.LightRed;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().roboTail = Item;
		}
		public override void UpdateVanity(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.roboTail = Item;
			originPlayer.roboTailVanity = true;
		}
		public override void UpdateItemDye(Player player, int dye, bool hideVisual) {
			if (!hideVisual) player.OriginPlayer().roboTailDye = dye;
		}
		internal Vector3 glowColor;
		internal ulong rateLimitTimer;
	}
	public class Robo_Tail_Glow_Dye_Slot : ExtraDyeSlot {
		public override bool UseForSlot(Item equipped, Item vanity, bool equipHidden) => equipped?.ModItem is Robo_Tail || vanity?.ModItem is Robo_Tail;
		public override void ApplyDye(Player player, [NotNull] Item dye) {
			player.GetModPlayer<OriginsDyeSlots>().cRoboTailGlow = dye.dye;
		}
	}
	[ReinitializeDuringResizeArrays]
	public abstract class Robo_Tail_Tail : WormMinion, IShadedProjectile {
		static readonly bool[] SegmentTypes = ProjectileID.Sets.Factory.CreateBoolSet();
		public override string Texture => typeof(Robo_Tail_Tail).GetDefaultTMLName();
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Robo_Tail_Tail).GetDefaultTMLName("_Glow");
		public int Shader => Main.player[Projectile.owner].OriginPlayer().roboTailDye;
		protected abstract Rectangle Frame { get; }
		public override float ChildDistance => 9;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			SegmentTypes[Type] = true;
			Main.projFrames[Type] = 2;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minion = true;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.penetrate = -1;
			Projectile.timeLeft *= 5;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.netImportant = true;
			Projectile.ContinuouslyUpdateDamageStats = true;
			//Projectile.extraUpdates = 2;
		}
		static bool hasBuff;
		public override ref bool HasBuff(Player player) {
			hasBuff = player.OriginPlayer().roboTail is not null;
			return ref hasBuff;
		}
		public override bool CanInsert(Projectile parent, Projectile child) => child is null;
		public override bool IsValidParent(Projectile segment) => SegmentTypes[segment.type];
		public override bool PreDraw(ref Color lightColor) {
			Rectangle frame = Frame;
			DrawData data = new(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				frame,
				lightColor,
				Projectile.rotation + MathHelper.PiOver2,
				frame.Size() * 0.5f,
				1,
				Projectile.spriteDirection == 1 ? SpriteEffects.FlipVertically : SpriteEffects.None
			);
			Main.EntitySpriteDraw(data);
			if (Main.player[Projectile.owner].GetModPlayer<OriginsDyeSlots>().cRoboTailGlow is int cRoboTailGlow) {
				Main.instance.PrepareDrawnEntityDrawing(Projectile, cRoboTailGlow, null);
			}
			data.texture = glowTexture;
			data.color = Color.White;
			Main.EntitySpriteDraw(data);
			return false;
		}
		public void DoUpdate() {
			Vector2 parentCenter;
			float parentRotation;
			float parentDistance;

			if (OriginExtensions.GetProjectile(Projectile.owner, wormData.Parent) is Projectile parent) {
				if (parent.active && parent.ModProjectile is WormMinion parentSegment && parentSegment.Part.CanHaveChild() && IsValidParent(parent)) {
					parentCenter = parent.Center;
					parentRotation = parent.rotation;
					parentDistance = parentSegment.ChildDistance;
				} else return;
			} else return;
			Projectile.velocity = Vector2.Zero;
			Vector2 offset = parentCenter - Projectile.Center;
			if (parentRotation != Projectile.rotation) {
				float angleDiff = MathHelper.WrapAngle(parentRotation - Projectile.rotation);
				offset = offset.RotatedBy(angleDiff * 0.1f);
			}

			Projectile.rotation = offset.ToRotation() + MathHelper.PiOver2;

			if (offset != Vector2.Zero)
				Projectile.Center = parentCenter - Vector2.Normalize(offset) * parentDistance;

			Projectile.spriteDirection = (offset.X > 0f).ToDirectionInt();
		}
	}
	public class Robo_Tail_Tail_Head : Robo_Tail_Tail {
		public override BodyPart Part => BodyPart.Head;
		protected override Rectangle Frame => new(0, 0, 4, 14);
		public override bool CanInsert(Projectile parent, Projectile child) => false;
		public override void AI() {
			DoActiveCheck();
			Player player = Main.player[Projectile.owner];
			Projectile.direction = player.direction;
			Projectile.Center = player.MountedCenter + new Vector2(player.direction * -10, player.gravDir * 10 + player.gfxOffY);
			Projectile.rotation = MathHelper.PiOver2 * Projectile.direction;
			if (wormData.Child == -1) return;
			int child = wormData.Child;
			HashSet<int> walked = [];
			bool anyMissing = false;
			while (OriginExtensions.GetProjectile(Projectile.owner, child)?.ModProjectile is Robo_Tail_Tail segment) {
				segment.DoUpdate();
				anyMissing |= segment.Projectile.ai[0] != 0;
				child = segment.wormData.Child;
				if (!walked.Add(child)) break;
			}
			OriginPlayer originPlayer = player.OriginPlayer();
			if (!anyMissing) {
				if (originPlayer.roboTailHealCount > 0) originPlayer.roboTailHealCount--;
			} else if (player.statLife >= player.statLifeMax2) {
				originPlayer.roboTailHealCount++;
			}
			if (OriginsModIntegrations.CheckAprilFools()) {
				if (player.statLife >= player.statLifeMax2 * 0.95f) {
					Projectile.rotation += (Utils.PingPongFrom01To010(player.miscCounter / 15f % 1) * 2 - 1) * 2f;
				} else {
					Projectile.rotation -= Projectile.direction * (1 - player.statLife / (float)player.statLifeMax2) * 2;
				}
			} else {
				Projectile.rotation += Projectile.direction * (player.controlUp.ToInt() - player.controlDown.ToInt());
			}
		}
	}
	public class Robo_Tail_Tail_Body : Robo_Tail_Tail {
		public override BodyPart Part => BodyPart.Body;
		int FrameY => (int)Projectile.ai[0] * 14;
		protected override Rectangle Frame => (int)((Projectile.localAI[1] - 1) * 3f / Robo_Tail.TailSegmentCount(Main.player[Projectile.owner])) switch {
			0 => new(6, FrameY, 10, 14),
			1 => new(18, FrameY, 8, 14),
			2 => new(28, FrameY, 10, 14),
			_ => new(0, FrameY, 4, 14)
		};
		public override bool CanInsert(Projectile parent, Projectile child) => parent.type == ModContent.ProjectileType<Robo_Tail_Tail_Head>();
		public override void AI() {
			base.AI();
			if (Projectile.localAI[0] != 0 && Projectile.ai[2] == 0) {
				const int delay = 12;
				Projectile.ai[1] = (GetChild()?.ai[1] ?? (Projectile.localAI[0] * delay - delay)) + delay;
				Projectile.ai[2] = 1;
			}
			Player player = Main.player[Projectile.owner];
			if (Projectile.localAI[1] > Robo_Tail.TailSegmentCount(player)) Projectile.Kill();
			OriginPlayer originPlayer = player.OriginPlayer();
			if (originPlayer.roboTailVanity || originPlayer.roboTail is null) return;
			int threshold = (player.statLifeMax2 / 2) / Robo_Tail.TailSegmentCount(player);
			switch (Projectile.ai[0]) {
				case 0:
				Projectile parent = GetParent();
				if (originPlayer.roboTailHurtCount > threshold && (parent.type != Type || parent?.ai[0] != 0)) {
					originPlayer.roboTailHurtCount -= threshold;
					Projectile.ai[0] = 1;
					Projectile.SpawnProjectile(
						player.GetSource_Accessory(originPlayer.roboTail),
						Projectile.Center,
						default,
						ModContent.ProjectileType<Robo_Tail_Probe>(),
						Projectile.damage,
						Projectile.knockBack
					);
				}

				ResetTargetingData();
				player.OriginPlayer().GetMinionTarget(TargetingAlgorithm);
				if (Projectile.ai[1].CycleUp(120, SpeedModifier) && targetingData.TargetID != -1 && CollisionExt.CanHitRay(Projectile.Center, targetingData.targetHitbox.Center())) {
					Projectile.SpawnProjectile(
						Projectile.GetSource_FromAI(),
						Projectile.Center,
						Projectile.Center.DirectionTo(targetingData.targetHitbox.Center()) * 8,
						ModContent.ProjectileType<Robo_Tail_Probe_Laser>(),
						Projectile.damage,
						Projectile.knockBack
					);
				}
				if (originPlayer.roboTail.ModItem is Robo_Tail roboTail) {
					Robo_Tail.dyeableGlow.GetColorWithRateLimit(ref roboTail.glowColor, ref roboTail.rateLimitTimer,
						() => player.GetModPlayer<OriginsDyeSlots>().cRoboTailGlow ?? originPlayer.roboTailDye, player, Projectile
					);
					Lighting.AddLight(Projectile.Center, roboTail.glowColor);
				}
				break;

				default:
				Projectile.ai[0] = 1;
				if (originPlayer.roboTailHealCount > threshold && (GetChild()?.ai[0] ?? 0) == 0) {
					originPlayer.roboTailHealCount -= threshold;
					Projectile.ai[0] = 0;
				}
				break;
			}
		}
	}
	public class Robo_Tail_Probe : MinionBase, IArtifactMinion, IShadedProjectile {
		public int MaxLife { get; set; }
		public float Life { get; set; }
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Robo_Tail_Probe).GetDefaultTMLName("_Glow");
		public int Shader => Main.player[Projectile.owner].OriginPlayer().roboTailDye;
		public override void SetStaticDefaults() {
			// These below are needed for a minion
			// Denotes that this projectile is a pet or minion
			Main.projPet[Projectile.type] = true;

			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			OriginsSets.Projectiles.SupportsRealSpeedBuffs[Type] = RealSpeedBuffAction;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minion = true;
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60 * 10;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.netImportant = true;
			MaxLife = 100;
		}
		public override bool MinionContactDamage() => true;
		static bool hasBuff;
		public override ref bool HasBuff(Player player) {
			hasBuff = player.OriginPlayer().roboTail is not null;
			return ref hasBuff;
		}
		public override void MoveTowardsTarget() {
			bool foundTarget = targetingData.TargetID != -1;
			Rectangle targetHitbox = foundTarget ? targetingData.targetHitbox : RestRegion;

			Vector2 targetPos = Projectile.Center.Clamp(targetHitbox);
			Vector2 direction = (targetPos - Projectile.Center).Normalized(out float distance);
			if (distance == 0) return;
			float speed = 0.3f * SpeedModifier;
			Projectile.velocity *= float.Lerp(0.8f, 0.98f, float.Abs(Vector2.Dot(direction, Projectile.velocity.Normalized(out _))));
			Projectile.velocity += direction * speed;
			Projectile.velocity = Projectile.velocity.Normalized(out speed) * Math.Min(speed, 16);
		}
		protected override void BasicAI() {
			base.BasicAI();
			bool foundTarget = targetingData.TargetID != -1;
			if (foundTarget) {
				Projectile.rotation = (targetingData.targetHitbox.Center() - Projectile.Center).ToRotation() + MathHelper.PiOver2;
			}
			if (Projectile.ai[1].TrySet(0)) SoundEngine.PlaySound(SoundID.NPCHit4, Projectile.Center);

			if (foundTarget && Projectile.ai[0].CycleDown(30, SpeedModifier) && CollisionExt.CanHitRay(Projectile.Center, targetingData.targetHitbox.Center())) {
				Projectile.SpawnProjectile(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					(Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 8,
					ModContent.ProjectileType<Robo_Tail_Probe_Laser>(),
					Projectile.damage,
					Projectile.knockBack
				);
			}
			Lighting.AddLight(Projectile.Center, 0.5f, 0, 0);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.damage > 0) {
				hit.HitDirection *= -1;
				hit.Knockback = 6;
				hit.Crit = false;
				Projectile.velocity = OriginExtensions.GetKnockbackFromHit(hit);
				this.DamageArtifactMinion(target.damage);
				Projectile.ai[1] = 1;
				Projectile.netUpdate = true;
			}
		}
		public override void OnKill(int timeLeft) {
			base.OnKill(timeLeft);
		}
		public override bool PreDraw(ref Color lightColor) {
			DrawData data = new(
				TextureAssets.Projectile[Type].Value,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				TextureAssets.Projectile[Type].Size() * 0.5f,
				1,
				Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			Main.EntitySpriteDraw(data);
			if (Main.player[Projectile.owner].GetModPlayer<OriginsDyeSlots>().cRoboTailGlow is int cRoboTailGlow) {
				Main.instance.PrepareDrawnEntityDrawing(Projectile, cRoboTailGlow, null);
			}
			data.texture = glowTexture;
			data.color = Color.White;
			Main.EntitySpriteDraw(data);
			return false;
		}
	}
	public class Robo_Tail_Probe_Laser : ModProjectile, IShadedProjectile {
		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MiniRetinaLaser}";
		public int Shader {
			get {
				Player player = Main.player[Projectile.owner];
				return player.GetModPlayer<OriginsDyeSlots>().cRoboTailGlow ?? player.OriginPlayer().roboTailDye;
			}
		}

		public override void SetStaticDefaults() {
			ProjectileID.Sets.MinionShot[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.alpha = 255;
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.extraUpdates = 1;
		}
		public override void AI() {
			if (Projectile.soundDelay.TrySet(-1)) SoundEngine.PlaySound(SoundID.Item12.WithVolumeScale(0.85f), Projectile.position);
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
}
