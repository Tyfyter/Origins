using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Misc;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Melee {
	public class The_Claw : ModItem, ICustomWikiStat {
		public static int HookCount => 3;
		public string[] Categories => [
			WikiCategories.Flail
		];
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			OriginsSets.Items.ItemsThatCanChannelWithRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 50;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 66;
			Item.height = 68;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4;
			Item.shoot = ModContent.ProjectileType<The_Claw_Hook>();
			Item.shootSpeed = 12f;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item1;
		}
		public override bool AltFunctionUse(Player player) => true;
		public override bool CanUseItem(Player player) => player.altFunctionUse != 2 || player.OriginPlayer().hookCooldown <= 0;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (player.altFunctionUse == 2) {
				type = ModContent.ProjectileType<The_Claw_Flail_P>();
				player.StartChanneling(type);
			}
		}
		public static void LimitClaws() => OriginExtensions.FadeOutOldProjectilesAtLimit([ModContent.ProjectileType<The_Claw_Hook>()], HookCount - Main.LocalPlayer.ownedProjectileCounts[ModContent.ProjectileType<The_Claw_Flail_P>()], 0);
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			LimitClaws();
			return true;
		}
	}
	public class The_Claw_Hook : ModProjectile {
		protected static AutoLoadingTexture chainTexture = typeof(The_Claw).GetDefaultTMLName("_Cable");
		protected static AutoGlowingTexture mandibleTextures = typeof(The_Claw).GetDefaultTMLName("_Mandible");
		public override string Texture => typeof(The_Claw_Hook).GetDefaultTMLName();
		protected virtual float ForwardOffset => -Math.Min(22, Projectile.Center.Distance(Main.player[Projectile.owner].MountedCenter) - 16);
		protected Vector2 CenterOffset => (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * ForwardOffset;
		public override void SetDefaults() {
			Projectile.netImportant = true;
			Projectile.width = Projectile.height = 18;
			Projectile.aiStyle = ProjAIStyleID.Hook;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.extraUpdates = 1;
			Projectile.tileCollide = false;
			Projectile.timeLeft *= 10;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool? CanUseGrapple(Player player) {
			if (player.OriginPlayer().hookCooldown > 0) return false;
			The_Claw.LimitClaws();
			return null;
		}
		public override void OnSpawn(IEntitySource source) {
			if (Main.projHook[Type] && source is EntitySource_ItemUse { Player: Player player, Item: Item item}) {
				player.OriginPlayer().hookCooldown = CombinedHooks.TotalUseTime(item.useTime, player, item);
			}
		}
		public override void NumGrappleHooks(Player player, ref int numHooks) => numHooks = The_Claw.HookCount;
		public override void GrappleRetreatSpeed(Player player, ref float speed) => speed = 8f;
		public override void GrapplePullSpeed(Player player, ref float speed) {
			if (hookTarget != -1) {
				speed = 8f;
				NPC hookTarget = Main.npc.GetIfInRange(this.hookTarget);
				if (hookTarget?.active != true) return;
				speed *= Utils.Remap(
					Vector2.Dot(hookTarget.velocity.Normalized(out float npcSpeed), Main.player[Projectile.owner].MountedCenter.DirectionTo(Projectile.Center)),
					-1,
					1,
					1,
					1 + Math.Min(npcSpeed / 8, 4)
				);
				float preventContactFactor = ((hookTarget.Center.Clamp(player.Hitbox) - player.MountedCenter.Clamp(hookTarget.Hitbox)) / 32).LengthSquared();
				Min(ref preventContactFactor, (((hookTarget.Center + hookTarget.velocity).Clamp(player.Hitbox) - player.MountedCenter.Clamp(hookTarget.Hitbox.Add(hookTarget.velocity))) / 32).LengthSquared());
				if (preventContactFactor < 1) {
					preventContactFactor = float.Sqrt(preventContactFactor) * 2 - 1;
					if (preventContactFactor > 0 && preventContactFactor < 0.1f) preventContactFactor = 0;
					speed *= preventContactFactor;
				}
			} else {
				speed = 12f;
			}
		}
		public override float GrappleRange() => 440;
		public override bool? GrappleCanLatchOnTo(Player player, int x, int y) {
			if (hookTarget != -1 && Projectile.ai[0] == 2) return true;
			return base.GrappleCanLatchOnTo(player, x, y);
		}
		public override void GrappleTargetPoint(Player player, ref float grappleX, ref float grappleY) {
			base.GrappleTargetPoint(player, ref grappleX, ref grappleY);
		}
		int hookTarget = -1;
		public override bool? CanDamage() => hookTarget == -1;
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Projectile.aiStyle = -1;
			Projectile.ai[0] = 2;
			hookTarget = target.whoAmI;
			(Projectile.ai[1], Projectile.ai[2]) = (Projectile.Center.Clamp(target.Hitbox) - target.Center).RotatedBy(-target.rotation);
			Projectile.velocity = default;
			Projectile.netUpdate = true;
		}
		public override void AI() {
			if (hookTarget != -1) {
				Projectile.aiStyle = ProjAIStyleID.Hook;
				switch (Projectile.ai[0]) {
					case 2: {
						NPC hookTarget = Main.npc.GetIfInRange(this.hookTarget);
						if (hookTarget?.active != true) {
							Projectile.ai[0] = 1f;
							return;
						}
						Vector2 newCenter = hookTarget.Center + new Vector2(Projectile.ai[1], Projectile.ai[2]).RotatedBy(hookTarget.rotation);
						switch (hookTarget.type) {
							case NPCID.WallofFlesh:
							case NPCID.WallofFleshEye:
							if (hookTarget.direction >= 0) {
								Max(ref newCenter.X, hookTarget.Right.X);
							} else {
								Min(ref newCenter.X, hookTarget.Left.X);
							}
							break;
						}
						Projectile.Center = newCenter;
						Vector2 ownerDiff = Projectile.Center - Main.player[Projectile.owner].MountedCenter;
						break;
					}

					default:
					hookTarget = -1;
					break;
				}
			}
		}
		public override bool PreDrawExtras() {
			Rectangle frame = chainTexture.Value.Bounds;
			chainTexture.Value.DrawChain(
				Projectile.Center + CenterOffset, Main.player[Projectile.owner].MountedCenter,
				i => frame,
				10,
				verticalChainTexture: true
			);
			return false;
		}
		public override bool PreDraw(ref Color lightColor) {
			const float close_rot = 0.4f;
			Vector2 position = Projectile.Center + CenterOffset - Main.screenPosition;
			float rotation = Projectile.rotation + MathHelper.Pi;
			DrawData data = new(
				TextureAssets.Projectile[Type].Value,
				position,
				null,
				lightColor,
				rotation,
				new(30, 34),
				Projectile.scale,
				SpriteEffects.None
			);
			Main.EntitySpriteDraw(data);
			if (Projectile.ai[0] >= 2 || hookTarget != -1) data.rotation = rotation - close_rot;
			Vector2 xFactor = rotation.ToRotationVector2();
			Vector2 yFactor = xFactor.YX();
			xFactor.Y *= -1;
			data.texture = mandibleTextures.Texture;
			data.position = position + new Vector2(-23, 13).MatrixMult(xFactor, yFactor);
			data.origin = new(7, 7);
			Main.EntitySpriteDraw(data);
			data.texture = mandibleTextures.GlowTexture;
			data.color = Color.White;
			Main.EntitySpriteDraw(data);

			if (Projectile.ai[0] >= 2 || hookTarget != -1) data.rotation = rotation + close_rot;
			data.texture = mandibleTextures.Texture;
			data.color = lightColor;
			data.position = position + new Vector2(23, 13).MatrixMult(xFactor, yFactor);
			data.origin = new(15, 7);
			data.effect = SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(data);
			data.texture = mandibleTextures.GlowTexture;
			data.color = Color.White;
			Main.EntitySpriteDraw(data);
			return false;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			projHitbox.Inflate(12, 12);
			return projHitbox.Add((Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * (16 + ForwardOffset)).Intersects(targetHitbox);
		}
	}
	public class The_Claw_Flail_P : The_Claw_Hook, IShadedProjectile {
		const int ai_state_spinning = 0;
		const int ai_state_launching_forward = 1;
		const int ai_state_retracting = 2;
		const int ai_state_unused_state = 3;
		const int ai_state_forced_retracting = 4;
		const int ai_state_ricochet = 5;
		const int ai_state_dropping = 6;
		public int Shader => Main.player[Projectile.owner].cGrapple;
		protected override float ForwardOffset => 0;
		public override string Texture => typeof(The_Claw_Hook).GetDefaultTMLName();
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.aiStyle = ProjAIStyleID.Flail;
			Projectile.tileCollide = true;
			Projectile.localNPCHitCooldown = 10;
			Projectile.extraUpdates = 0;
		}
		static bool controlUseItem;
		float preAIRot;
		public override bool PreAI() {
			// this won't change anything outside of this projectile unless an exception is thrown, because this runs after the global version
			Player player = Main.player[Projectile.owner];
			controlUseItem = player.controlUseItem;
			player.controlUseItem = player.controlUseTile;
			preAIRot = Projectile.localAI[1];
			return base.PreAI();
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			player.controlUseItem = controlUseItem;
			Projectile.rotation = (Projectile.Center - player.MountedCenter).ToRotation() + MathHelper.PiOver2;
			if (Projectile.ai[0] is ai_state_retracting or ai_state_forced_retracting) {
				Rectangle hitbox = Projectile.Hitbox.Add((Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * 16);
				float maxSpeed = 0.2f;
				if (Projectile.ai[1] == 0) {
					SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundMiss, Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item147, Projectile.Center);
					Array.Clear(Projectile.localNPCImmunity);
					hitbox.Inflate(4, 4);
					maxSpeed = 4;
				}
				hitbox.Inflate(4, 4);
				foreach (Item item in Main.ActiveItems) {
					if (hitbox.Intersects(item.Hitbox)) {
						item.position += Projectile.velocity;
						item.velocity += (hitbox.Center() - item.Center).WithMaxLength(maxSpeed);
					}
				}
				Projectile.ai[1]++;
			}
			if (Projectile.ai[0] == 0f && Projectile.localAI[2].CycleDown(MathHelper.TwoPi * 2, Projectile.localAI[1] - preAIRot)) {
				SoundEngine.PlaySound(SoundID.DD2_MonkStaffSwing, Projectile.Center);
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (Projectile.ai[0] == 0f) {
				Vector2 mountedCenter = Main.player[Projectile.owner].MountedCenter;
				Vector2 diff = targetHitbox.ClosestPointInRect(mountedCenter) - mountedCenter;
				diff.Y /= 0.8f;
				float num = 77f;
				return diff.LengthSquared() <= num * num;
			}
			return base.Colliding(projHitbox, targetHitbox);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) { }
	}
	public struct AutoGlowingTexture(string asset) : IUnloadable, IBatchLoadable {
		AutoLoadingTexture texture = asset;
		AutoLoadingTexture glowTexture = asset + "_Glow";
		public Texture2D Texture => texture.Value;
		public Texture2D GlowTexture => glowTexture.Value;
		public static implicit operator AutoGlowingTexture(string asset) => new(asset);
		void IBatchLoadable.Load() {
			texture.LoadAsset();
			glowTexture.LoadAsset();
		}
		void IUnloadable.Unload() {
			texture.Unload();
			glowTexture.Unload();
		}
		void IBatchLoadable.Wait() {
			texture.Wait();
			glowTexture.Wait();
		}
	}
}
