using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Graphics;
using Origins.Layers;
using Origins.NPCs;
using Origins.Projectiles;
using Origins.Reflection;
using ReLogic.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Space_Pirates_Eye : ModItem, IRightClickableAccessory {
        static AutoLoadingTexture irisTexture = typeof(Space_Pirates_Eye).GetDefaultTMLName("_Iris");
        public static List<PirateEyeMode> Colors { get; } = [];
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask(EquipType.Face, Item.faceSlot,
				$"{Texture}_{EquipType.Face}_Glow",
				player => Colors.GetIfInRange(player.OriginPlayer().SpacePirateEyeVisualSelection)?.Color ?? Color.Transparent//(Main.timeForVisualEffects % 30 < 15 ? Color.Black : Color.Magenta)
			);
			ArmorIDs.Face.Sets.DrawInFaceUnderHairLayer[Item.faceSlot] = true;

			Colors.Sort();
			Array.Resize(ref counts, Colors.Count);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(34, 20);
			Item.damage = 60;
			Item.DamageType = DamageClass.Magic;
			Item.knockBack = 3;
			Item.rare = ItemRarityID.LightRed;
			Item.master = true;
			Item.value = Item.sellPrice(gold: 1);
			Item.ForceEnableCrit();
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().spacePirateEye = Item;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            Color irisColor = Colors.GetIfInRange(Main.LocalPlayer.OriginPlayer().SpacePirateEyeVisualSelection)?.Color ?? Color.Transparent;
            spriteBatch.Draw(
                irisTexture,
                position,
                null,
                drawColor.MultiplyRGB(irisColor),
                0,
                origin,
                scale,
                SpriteEffects.None,
                0
            );
		}
		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
            Color irisColor = Colors.GetIfInRange(Main.LocalPlayer.OriginPlayer().SpacePirateEyeVisualSelection)?.Color ?? Color.Transparent;
            spriteBatch.Draw(
                irisTexture,
                Item.Center - Main.screenPosition,
                null,
                lightColor.MultiplyRGB(irisColor),
                rotation,
                Item.Size * 0.5f,
                scale,
                SpriteEffects.None,
                0
            );
        }
		public static void UpdateEye(Player player, int mode) {
			if (mode == -1) return;
			OriginPlayer originPlayer = player.OriginPlayer();
			Item spacePirateEye = originPlayer.spacePirateEye;
			if (spacePirateEye is null) return;
			if (mode < 0) {
				int lowest = GetPlayerCounts(player);
				if (originPlayer.spacePirateEyePreference > -2) {
					if (originPlayer.spacePirateEyePreference == -1 || counts[originPlayer.spacePirateEyePreference] == lowest) {
						originPlayer.spacePirateEyeSelection = originPlayer.spacePirateEyePreference;
					}
					if (originPlayer.spacePirateEyeSelection == -1) return;
				}
				for (int i = 0; i < counts.Length && originPlayer.spacePirateEyeSelection < 0; i++) {
					if (counts[i] == lowest) {
						originPlayer.spacePirateEyeSelection = i;
						break;
					}
				}
				mode = originPlayer.spacePirateEyeSelection;
			}

			if (originPlayer.spacePirateEyeCooldown > 0) return;
			Vector2 position = EyePosition(player);
			PirateEyeMode eyeMode = Colors[mode];
			if (eyeMode.FindTarget(player, position, out Vector2 targetPos, out Entity targetEntity)) {
				originPlayer.spacePirateEyeCooldown = CombinedHooks.TotalUseTime(eyeMode.Cooldown, player, spacePirateEye);
				using ScopedOverride<int> dmg = spacePirateEye.damage.ScopedOverride((int)(spacePirateEye.damage * eyeMode.DamageMult));
				using ScopedOverride<float> kb = spacePirateEye.knockBack.ScopedOverride(spacePirateEye.knockBack * eyeMode.KnockbackMult);
				eyeMode.Shoot(player,
					targetEntity,
					new EntitySource_ItemUse_WithTarget(player, spacePirateEye, targetEntity),
					position,
					eyeMode.GetVelocity(player, targetPos - position, targetEntity),
					eyeMode.Type,
					player.GetWeaponDamage(spacePirateEye),
					player.GetWeaponKnockback(spacePirateEye)
				);
			}
		}

		private static Vector2 EyePosition(Player player) {
			return player.Center + new Vector2(2 * player.direction, (12 - player.height * 0.5f) * player.gravDir);
		}

		internal static int[] counts = [];
		/// <returns>the lowest count</returns>
		public static int GetPlayerCounts(Player forPlayer) {
			Array.Clear(counts);
			foreach (Player player in Main.ActivePlayers) {
				if (player == forPlayer) continue;
				int color = player.OriginPlayer().spacePirateEyeSelection;
				if (color < 0) continue;
				counts[color]++;
			}
			int lowest = int.MaxValue;
			for (int i = 0; i < counts.Length; i++) {
				Min(ref lowest, counts[i]);
			}
			//counts[(int)(Main.timeForVisualEffects / 30) % counts.Length] = 1; //just to demo what it looks like when a color is taken
			return lowest;
		}
		public bool CanRightClickAccessory(Item[] inv, int context, int slot) => Math.Abs(context) != ItemSlot.Context.EquipAccessoryVanity;
		public bool RightClickAccessory(Item[] inv, int context, int slot) {
			OriginSystem.Instance.SpacePirateEyeUI.Activate();
			return false;
		}
		#region attacks
		public abstract class PirateEyeMode : ModProjectile, IComparable<PirateEyeMode> {
			public override string Name => "Pirate_Eye_" + base.Name;
			public sealed override void Load() {
				Colors.Add(this);
				OnLoad();
			}
			public virtual void OnLoad() { }
			public abstract Color Color { get; }
			public virtual float DamageMult => 1;
			public virtual float KnockbackMult => 1;
			public abstract int Cooldown { get; }
			public virtual float Order => Main.rgbToHsl(Color).X;
			public virtual Vector2 GetVelocity(Player player, Vector2 difference, Entity target) => difference.Normalized(out _) * 8;
			public virtual void Shoot(Player player, Entity target, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
				player.SpawnProjectile(source, position, velocity, type, damage, knockback);
			}
			public virtual bool FindTarget(Player player, Vector2 position, out Vector2 targetPos, out Entity targetEntity) => DefaultFindTarget(player, position, out targetPos, out targetEntity);
			protected static bool DefaultFindTarget(Player player, Vector2 position, out Vector2 targetPos, out Entity targetEntity, float maxDist = 16 * 25) {
				Vector2 _targetPos = position;
				Entity _targetEntity = null;
				maxDist *= maxDist;

				bool result = player.DoHoming(FindTarget);
				targetEntity = _targetEntity;
				targetPos = _targetPos;
				return result;
				bool FindTarget(Entity target) {
					Vector2 currentPos = position.Clamp(target.Hitbox);
					if (Math.Sign(position.X - currentPos.X) == player.direction) return false;
					float dist = currentPos.DistanceSQ(position);
					if (dist < maxDist) {
						maxDist = dist;
						_targetEntity = target;
						_targetPos = currentPos;
						return true;
					}
					return false;
				}
			}
			int IComparable<PirateEyeMode>.CompareTo(PirateEyeMode other) => Order.CompareTo(other.Order);
		}
		public class Laser : PirateEyeMode {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.MiniRetinaLaser}";
			public override Color Color => FromHexRGB(0xff0060);//#ff0060
			public override float Order => -2;
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.MiniRetinaLaser);
				Projectile.DamageType = DamageClass.Magic;
				AIType = ProjectileID.MiniRetinaLaser;
			}
		}
		public class Cursed_Flames : PirateEyeMode {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.CursedFlameHostile}";
			public override Color Color => FromHexRGB(0x80ff00);//#80ff00
			public override float Order => -1;
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.CursedFlameFriendly);
				AIType = ProjectileID.CursedFlameHostile;
			}
			public override void AI() {
				base.AI();
			}
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.CursedInferno, Main.rand.Next(120, 301));
		}
		public class Sharp_Tears : PirateEyeMode, IBroken {
			static string IBroken.BrokenReason => "Needs review";
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.SharpTears}";
			public override Color Color => FromHexRGB(0xff0000);
			public override int Cooldown => 120;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.SharpTears);
				Projectile.hide = true;
				ProjectileID.Sets.DontAttachHideToAlpha[Type] = true;
				AIType = ProjectileID.SharpTears;
			}
			public override Vector2 GetVelocity(Player player, Vector2 difference, Entity target) => difference.Normalized(out _);
			public override void Shoot(Player player, Entity target, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc.TryGetGlobalNPC(out Blood_Thorn_Global global) && global.damage <= damage) {
						Vector2 diff = (position.Clamp(npc.Hitbox) - position).Normalized(out float dist);
						if (dist > 16 * 25 || Vector2.Dot(diff, velocity) < 0.5f) continue;
						for (float i = 0f; i < dist; i += 8f) {
							Vector2 speed = Main.rand.NextVector2Circular(2, 2);
							Dust dust = EfficientDust.NewDustDirect(
								position + i * diff,
								0,
								0,
								DustID.Blood,
								speed.X,
								speed.Y,
								Alpha: 100,
								newColor: new(128, 0, 0, 0)
							);
							if (!ChildSafety.Disabled) continue;
							dust.scale = 0.85f;
							dust.fadeIn = i / 32;
							dust.noLightEmittence = false;
						}
						global.time = Cooldown - 30;
						global.damage = damage;
						global.knockback = knockback;
						global.source = source;
					}
				}
			}
			class Blood_Thorn_Global : GlobalNPC {
				public int time;
				public int damage;
				public float knockback;
				public IEntitySource source;
				public override bool InstancePerEntity => true;
				public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => !entity.friendly;
				public override void ResetEffects(NPC npc) {
					if (time.Cooldown()) {
						Projectile.NewProjectile(
							source,
							npc.Bottom,
							Vector2.UnitY * -32,
							ModContent.ProjectileType<Sharp_Tears>(),
							damage / 2,
							knockback / 2,
							ai1: Main.rand.NextFloat() * 0.375f + 0.45f
						);
						damage = 0;
						source = null;
					}
				}
				void Trigger(NPC npc) {
					if (time > 0) {
						Projectile.NewProjectile(
							source,
							npc.Bottom,
							Vector2.UnitY * -32,
							ModContent.ProjectileType<Sharp_Tears>(),
							damage,
							knockback,
							ai1: Main.rand.NextFloat() * 0.5f + 0.6f
						);
						time = 0;
						damage = 0;
						source = null;
					}
				}
				public override void OnHitByItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone) => Trigger(npc);
				public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone) => Trigger(npc);
				public override void AI(NPC npc) {
					if (time <= 0) return;
					Dust dust = Dust.NewDustDirect(
						npc.position,
						npc.width,
						npc.height,
						DustID.Blood,
						Alpha: 100
					);
					dust.velocity.Y -= 0.3f;
					dust.velocity += npc.velocity * 0.2f;
					dust.scale = 1f;
					dust.color = new(128, 0, 0, 0);
					dust.noLightEmittence = false;
				}
			}
			public override bool ShouldUpdatePosition() => false;
			public override void AI() {
				base.AI();
				Lighting.AddLight(Projectile.Center, new Vector3(0.5f, 0.1f, 0.1f) * Projectile.scale);
			}
			public override void CutTiles() {
				Utils.PlotTileLine(Projectile.Center, Projectile.Center + Vector2.UnitY * -200f * Projectile.scale, 22f * Projectile.scale, DelegateMethods.CutTiles);
			}
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetRect) {
				float collisionPoint16 = 0f;
				if (Collision.CheckAABBvLineCollision(targetRect.TopLeft(), targetRect.Size(), Projectile.Center, Projectile.Center + Vector2.UnitY * -200f * Projectile.scale, 22f * Projectile.scale, ref collisionPoint16))
					return true;

				return false;
			}
			public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
				behindNPCsAndTiles.Add(index);
			}
			public override Color? GetAlpha(Color lightColor) => Color.Lerp(lightColor, Color.Black, 0.25f);
			public override bool PreDraw(ref Color lightColor) {
				SpriteEffects dir = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				Main.EntitySpriteDraw(
					TextureAssets.Extra[ExtrasID.SharpTears].Value,
					Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY) - Projectile.velocity * Projectile.scale * 0.5f,
					null,
					Projectile.GetAlpha(lightColor.MultiplyRGBA(new Color(67, 17, 17))),
					Projectile.rotation + MathHelper.PiOver2,
					TextureAssets.Extra[ExtrasID.SharpTears].Value.Size() / 2f,
					Projectile.scale * 0.9f,
					dir
				);
				Texture2D texture = TextureAssets.Projectile[Type].Value;
				Rectangle frame = texture.Frame(1, 6, 0, Projectile.frame);
				Main.EntitySpriteDraw(
					texture,
					Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
					frame,
					Projectile.GetAlpha(lightColor),
					Projectile.rotation,
					new(16f, frame.Height / 2),
					new Vector2(Projectile.scale, Projectile.scale * Utils.GetLerpValue(35f, 35f - 5f, Projectile.ai[0], true)),
					dir
				);
				return false;
			}
			public override void OnKill(int timeLeft) {
				for (float i = 0f; i < 1f; i += 0.025f) {
					Dust dust = Dust.NewDustPerfect(
						Projectile.Center + Main.rand.NextVector2Circular(16f, 16f) * Projectile.scale + Projectile.velocity.SafeNormalize(Vector2.UnitY) * i * 200f * Projectile.scale,
						DustID.Blood,
						Main.rand.NextVector2Circular(3f, 3f),
						Alpha: 100
					);
					dust.velocity.Y -= 0.3f;
					dust.velocity += Projectile.velocity * 0.2f;
					dust.scale = 1f;
				}
			}
		}
		public class Scrap_Laser : PirateEyeMode {
			public override Color Color => FromHexRGB(0xff6000);
			public override int Cooldown => 60;
			public static int ChargeTime => 30;
			public static int ActiveTime => 15;
			public override string Texture => "Terraria/Images/Extra_" + ExtrasID.RainbowRodTrailShape;
			public override void SetStaticDefaults() {
				ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600 + 64;
			}
			public override Vector2 GetVelocity(Player player, Vector2 difference, Entity target) => difference.Normalized(out _);
			public override void SetDefaults() {
				Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Magic];
				Projectile.width = 0;
				Projectile.height = 0;
				Projectile.penetrate = -1;
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
			bool IsActive {
				get => Projectile.localAI[0] != 0;
				set => Projectile.localAI[0] = value.ToInt();
			}
			Entity target = null;
			public override void OnSpawn(IEntitySource source) {
				if (source is IEntitySourceWithTarget sourceWithTarget) {
					target = sourceWithTarget.Target;
				}
				Projectile.rotation = Projectile.velocity.ToRotation();
			}
			public override void AI() {
				Player player = Main.player[Projectile.owner];
				IsActive = ++Projectile.ai[2] >= ChargeTime;
				if (Projectile.ai[2] >= ChargeTime + ActiveTime) {
					Projectile.Kill();
					return;
				}
				Vector2 gunPos = EyePosition(player);
				float pitchFactor = IsActive ? 1 : (1 - Projectile.ai[2] / ChargeTime);
				//SoundEngine.SoundPlayer.Play(SoundID.Item158.WithPitch(pitchFactor).WithVolume(0.8f), gunPos);
				if (Math.Abs(1 - Projectile.velocity.LengthSquared()) > 0.01f) Projectile.velocity = Projectile.rotation.ToRotationVector2();
				if (target is not null && (target.Center.X - gunPos.X) * player.direction < 0) target = null;
				if (target is not null) {
					float direction = (target.Center - gunPos).ToRotation();
					if (IsActive) {
						GeometryUtils.AngularSmoothing(ref Projectile.rotation, direction, 0.03f + 0.02f * (Projectile.ai[2] - ChargeTime) / ActiveTime);
					} else {
						GeometryUtils.AngularSmoothing(ref Projectile.rotation, direction, 0.05f);
					}
					Projectile.velocity = Projectile.rotation.ToRotationVector2();
					if (!target.active || (target is NPC npcTarget && !npcTarget.CanBeChasedBy(Projectile))) target = null;
				}
				Projectile.position = gunPos;
				Vector2 targetPos = Projectile.position + Projectile.velocity * Raymarch(Projectile.position, Projectile.velocity, ProjectileID.Sets.DrawScreenCheckFluff[Type] - 64);
				if (IsActive) Dust.NewDust(targetPos - Vector2.One * 2, 4, 4, DustID.AmberBolt);
				TargetPos = targetPos;
			}
			public override bool? CanHitNPC(NPC target) {
				if (!IsActive) {
					if (OriginsSets.NPCs.Eyes[target.type] || NPCID.Sets.DemonEyes[target.type]) return null;
					switch (target.type) {
						case NPCID.Spazmatism:
						case NPCID.Retinazer:
						if (target.ai[0] >= 3) return false;
						return null;
						case NPCID.MoonLordHand:
						case NPCID.MoonLordHead:
						return null;
					}
					return false;
				}
				return null;
			}
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
				return targetHitbox.Contains(targetHitbox.Center().SnapToLine(Projectile.position, TargetPos, radius: 4));
			}
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
				OriginGlobalNPC.InflictImpedingShrapnel(target, 300);
			}
			public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
				if (!target.immune) modifiers = modifiers with { CooldownCounter = -2 };
				modifiers.Knockback *= 0.85f;
			}
			public override void OnHitPlayer(Player target, Player.HurtInfo info) {
				if (info.CooldownCounter == -2) {
					target.immune = true;
					target.immuneTime = target.longInvince ? 15 : 7;
				}
			}
			public override void SendExtraAI(BinaryWriter writer) {
				writer.Write(Projectile.rotation);
				switch (target) {
					case null:
					writer.Write((sbyte)-1);
					break;
					case NPC:
					writer.Write((sbyte)0);
					writer.Write((ushort)target.whoAmI);
					break;
					case Player:
					writer.Write((sbyte)1);
					writer.Write((ushort)target.whoAmI);
					break;
				}
			}
			public override void ReceiveExtraAI(BinaryReader reader) {
				Projectile.rotation = reader.ReadSingle();
				switch (reader.ReadSByte()) {
					case -1:
					target = null;
					break;
					case 0:
					target = Main.npc[reader.ReadUInt16()];
					break;
					case 1:
					target = Main.player[reader.ReadUInt16()];
					break;
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
					TextureAssets.Projectile[Type].Value,
					position,
					frame,
					new Color(255, IsActive ? 40 : 100, 0, 0),
					rotation,
					Vector2.UnitY * 128,
					new Vector2(1, 8 * scale),
					0
				);
				data.Draw(Main.spriteBatch);
				float progress = Projectile.ai[2] / ChargeTime;
				progress *= progress;
				if (IsActive) progress = 1;
				Min(ref progress, 1);
				data.color *= progress;
				Vector2 offset = (rotation + MathHelper.PiOver2).ToRotationVector2() * (1 - progress) * 8;
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
			public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
				overPlayers.Add(index);
			}
			public float Raymarch(Vector2 position, Vector2 direction, float maxLength = float.PositiveInfinity) {
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
				Player owner = Main.player[Projectile.owner];
				if (owner.hostile) {
					int team = owner.team;
					if (team == 0) team = -1;
					foreach (Player player in Main.ActivePlayers) {
						if (dist < 16) return dist;
						if (player.whoAmI == Projectile.owner) continue;
						if (!player.hostile || player.team == team) continue;
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
		public class Ichor_Spray : PirateEyeMode {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.IchorSplash}";
			public override Color Color => FromHexRGB(0xffbf00);
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.DamageType = DamageClass.Magic;
				Projectile.width = 10;
				Projectile.height = 10;
				Projectile.friendly = true;
				Projectile.alpha = 255;
				Projectile.penetrate = 5;
				Projectile.ignoreWater = true;
				Projectile.extraUpdates = 2;
				Projectile.usesIDStaticNPCImmunity = true;
				Projectile.idStaticNPCHitCooldown = 10;
				Projectile.timeLeft = 20;
			}
			public override void AI() {
				for (float i = 0; i < 1; i += 1f / 2) {
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ichor, 0f, 0f, 100);
					dust.position = (dust.position + Projectile.Center) / 2f + Projectile.velocity * i;
					dust.noGravity = true;
					dust.velocity *= 0.1f;
					dust.scale *= (800f - Projectile.ai[0]) / 800f + 0.1f;
				}
			}
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Ichor, Main.rand.Next(60, 181));
			public override bool FindTarget(Player player, Vector2 position, out Vector2 targetPos, out Entity targetEntity) {
				return DefaultFindTarget(player, position, out targetPos, out targetEntity, 16 * 10);
			}
			public override void Shoot(Player player, Entity target, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
				for (int i = 0; i < 6; i++) player.SpawnProjectile(source, position, velocity + Main.rand.NextVector2Circular(2, 2), type, damage, knockback);
			}
		}
		public class _Temp_Yellow : PirateEyeMode, IBroken {
			static string IBroken.BrokenReason => "Needs idea";
			public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MedusaHeadRay;
			public override Color Color => FromHexRGB(0xdfff00);
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.DamageType = DamageClass.Magic;
				Projectile.width = 0;
				Projectile.height = 0;
				Projectile.timeLeft = 30;
				Projectile.penetrate = -1;
				Projectile.friendly = true;
				Projectile.tileCollide = false;
			}
			public override bool ShouldUpdatePosition() => false;
			Triangle hitTri;
			public override void AI() {
				Player player = Main.player[Projectile.owner];
				Projectile.position = player.Center + new Vector2(2 * player.direction, (12 - player.height * 0.5f) * player.gravDir);
				Vector2 perp = Projectile.velocity.Perpendicular() * 0.5f;
				hitTri = new(
					Projectile.position,
					Projectile.position + Projectile.velocity + perp,
					Projectile.position + Projectile.velocity - perp
				);
			}
			public override Vector2 GetVelocity(Player player, Vector2 difference, Entity target) => difference.Normalized(out _) * 16 * 15;
			public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => hitTri.Intersects(targetHitbox);
			public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Midas, Main.rand.Next(180, 481));
			private readonly VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[3];
			public override bool PreDraw(ref Color lightColor) {

				vertices[0].TextureCoordinate = new Vector2(0, 1);
				vertices[1].TextureCoordinate = new Vector2(0, 0);
				vertices[2].TextureCoordinate = new Vector2(1, 1);
													  
				Color color = Color * Projectile.Opacity * 0.05f;
				vertices[0].Color = color;
				vertices[1].Color = color;
				vertices[2].Color = color;

				short[] dices = [0, 1, 2];
				GameShaders.Misc["Origins:Identity"]
				.UseImage0(TextureAssets.MagicPixel)//Extra[ExtrasID.LightDisc]
				.UseSamplerState(SamplerState.LinearClamp)
				.Apply();
				const int count = 5;
				Vector2 perp = Projectile.velocity.Perpendicular().Normalized(out _);
				for (int i = -count; i <= count; i++) {

					vertices[0].Position = new Vector3(hitTri.a + perp * i - Main.screenPosition, 0);//
					vertices[1].Position = new Vector3(hitTri.b + perp * i + Main.rand.NextVector2Circular(4, 4) - Main.screenPosition, 0);//
					vertices[2].Position = new Vector3(hitTri.c + perp * i + Main.rand.NextVector2Circular(4, 4) - Main.screenPosition, 0);//

					Main.instance.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 3, dices, 0, 2);
				}
				return false;
			}
		}
		public class _Temp_Turquoise : PirateEyeMode, IBroken {
			static string IBroken.BrokenReason => "Needs idea";
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.PoisonFang}";
			public override Color Color => FromHexRGB(0x00ff9f);
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.PoisonFang);
				AIType = ProjectileID.PoisonFang;
			}
		}
		public class Worms : PirateEyeMode, IBroken {
			static string IBroken.BrokenReason => "Needs idea";
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.ElectrosphereMissile}";
			public override Color Color => FromHexRGB(0x00ffff);
			public override int Cooldown => 120;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.ElectrosphereMissile);
				AIType = ProjectileID.ElectrosphereMissile;
			}
			public override Vector2 GetVelocity(Player player, Vector2 difference, Entity target) => difference.Normalized(out _) * 8;
			public override void Shoot(Player player, Entity target, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
				if (player.ownedProjectileCounts[ModContent.ProjectileType<Friendly_Cleaver_Head>()] > 0) {
					player.OriginPlayer().spacePirateEyeCooldown = 5;
					return;
				}
				if (source is EntitySource_ItemUse { Item: Item item }) damage = item.damage;
				player.SpawnProjectile(null,
					position,
					velocity,
					ModContent.ProjectileType<Friendly_Cleaver_Head>(),
					damage,
					knockback
				).originalDamage = damage;
				for (int i = 0; i < 3; i++) {
					player.SpawnProjectile(null,
						position,
						default,
						ModContent.ProjectileType<Friendly_Cleaver_Body>(),
						damage,
						knockback
					).originalDamage = damage;
				}
				player.SpawnProjectile(null,
					position,
					default,
					ModContent.ProjectileType<Friendly_Cleaver_Tail>(),
					damage,
					knockback
				).originalDamage = damage;
			}
			[ReinitializeDuringResizeArrays]
			public abstract class Friendly_Cleaver_Base : WormMinion {
				static readonly bool[] SegmentTypes = ProjectileID.Sets.Factory.CreateBoolSet();
				public override string Texture => $"Origins/NPCs/Riven/Cleaver_{Part}";
				public override float ChildDistance => 16;
				public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= 1f;// use this to adjust damage
				public override Rectangle RestRegion {
					get {
						Rectangle hitbox = Owner.Hitbox.Add(Vector2.UnitX * Owner.direction * -80);
						if (Owner.direction == -1) hitbox.X -= hitbox.Height - hitbox.Width;
						hitbox.Width = hitbox.Height;
						hitbox.Inflate(16, 16);
						return hitbox;
					}
				}
				public override void SetStaticDefaults() {
					base.SetStaticDefaults();
					SegmentTypes[Type] = true;
					Main.projPet[Projectile.type] = true;
				}
				public override void SetDefaults() {
					Projectile.DamageType = DamageClass.Summon;
					Projectile.minion = true;
					Projectile.width = 20;
					Projectile.height = 20;
					Projectile.penetrate = -1;
					Projectile.timeLeft *= 5;
					Projectile.friendly = true;
					Projectile.ignoreWater = true;
					Projectile.tileCollide = false;
					Projectile.netImportant = true;
					Projectile.ContinuouslyUpdateDamageStats = true;
					Projectile.usesLocalNPCImmunity = true;
					Projectile.localNPCHitCooldown = 10;
				}
				public override bool MinionContactDamage() => true;
				public override void MoveTowardsTarget() {
					bool foundTarget = targetingData.TargetID != -1;
					Rectangle targetHitbox = foundTarget ? targetingData.targetHitbox : RestRegion;
					if (foundTarget) targetHitbox.Inflate(-targetHitbox.Width / 8, -targetHitbox.Height / 8);

					Vector2 targetPos = Projectile.Center.Clamp(targetHitbox);
					Vector2 direction = (targetPos - Projectile.Center).Normalized(out float distance);
					if (foundTarget) {
						if (Projectile.Hitbox.OverlapsAnyTiles(false)) {
							float speed = 1.2f * SpeedModifier;
							Projectile.velocity += direction * speed;
							Projectile.velocity = Projectile.velocity.Normalized(out float currentSpeed);
							Min(ref currentSpeed, 12);
							Projectile.velocity *= currentSpeed;
						} else {
							Gravity();
						}
					} else {
						if (Projectile.Hitbox.OverlapsAnyTiles(false)) {
							float speed = distance switch {
								< 300f => 0.3f,
								< 600f => 0.6f,
								_ => 0.9f
							};

							if (direction == default) {
								if (!targetHitbox.Contains(Projectile.oldPosition)) {
									Projectile.rotation += Main.rand.NextFloatDirection() * 1f;
									speed *= 10;
								}
								direction = (Projectile.rotation - MathHelper.PiOver2).ToRotationVector2();
								speed *= 0.1f;
							}

							Projectile.velocity += direction * speed;
							if (Vector2.Dot(Projectile.velocity.Normalized(out _), direction) < 0.25f)
								Projectile.velocity *= 0.8f;

							Projectile.velocity = Projectile.velocity.Normalized(out speed);
							if (speed > 2) speed *= 0.96f;
							Projectile.velocity *= Math.Min(speed, 15);
						} else {
							Gravity();
						}
					}
				}
				public override void AI() {
					if (Projectile.ai[0] != 0) {
						Projectile.timeLeft = 2;
						if (Projectile.velocity == default) {
							Projectile.localAI[2]++;
							if (Projectile.localAI[2] > 5) Projectile.Kill();
						} else {
							Projectile.localAI[2] = 0;
						}
						Gravity();
						Projectile.rotation += Projectile.velocity.X * 0.01f;
						if (Projectile.alpha < 127 && !Projectile.Hitbox.OverlapsAnyTiles()) Projectile.tileCollide = true;
						if (Projectile.tileCollide) Projectile.alpha = 0;
						else if ((Projectile.alpha += Projectile.alpha >= 127 ? 15 : 5) >= 255) Projectile.Kill();
						return;
					}
					bool hasParent = wormData.Parent != -1;
					base.AI();
					if (Part != BodyPart.Head && hasParent && Projectile.localAI[1] == 0) Projectile.ai[0] = 1;
				}
				public void Gravity() {
					Projectile.velocity.Y += 0.3f;
				}
				public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
					if (Part == BodyPart.Head || Projectile.ai[0] != 0) {
						Projectile.Kill();
					}
				}
				public override bool OnTileCollide(Vector2 oldVelocity) {
					Projectile.velocity = oldVelocity;
					Projectile.Kill();
					return false;
				}
				public override void OnKill(int timeLeft) {
					if (Projectile.alpha < 127) OriginExtensions.LerpEquals(
						ref Gore.NewGoreDirect(
							Projectile.GetSource_Death(),
							Projectile.position,
							Projectile.velocity,
							Origins.instance.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4))
						).velocity,
						Projectile.velocity,
						0.5f
					);

					if (Part != BodyPart.Head) return;
					foreach (Projectile segment in WalkWorm()) {
						segment.ai[0] = 1;
						segment.velocity = Projectile.velocity.RotatedBy(segment.rotation - Projectile.rotation);
					}
				}
				bool hasEye = true;
				public override ref bool HasBuff(Player player) {
					if (Projectile.ai[0] != 0) {
						hasEye = true;
						return ref hasEye;
					}
					OriginPlayer originPlayer = Main.player[Projectile.owner].OriginPlayer();
					if (originPlayer.spacePirateEye is null || Colors[originPlayer.spacePirateEyeSelection] is not Worms) hasEye = false;
					return ref hasEye;
				}
				public override bool IsValidParent(Projectile segment) => SegmentTypes[segment.type];
				public override Color? GetAlpha(Color lightColor) {
					lightColor = Color.Lerp(lightColor, Color.White, 0.4f);
					lightColor.A = 150;
					lightColor *= (lightColor.A - Projectile.alpha) / 255f;
					return lightColor;
				}
			}
			public class Friendly_Cleaver_Head : Friendly_Cleaver_Base {
				public override BodyPart Part => BodyPart.Head;
				public override bool CanInsert(Projectile parent, Projectile child) => false;
			}
			public class Friendly_Cleaver_Body : Friendly_Cleaver_Base {
				public override BodyPart Part => BodyPart.Body;
				public override bool CanInsert(Projectile parent, Projectile child) => parent.type == ModContent.ProjectileType<Friendly_Cleaver_Head>();
			}
			public class Friendly_Cleaver_Tail : Friendly_Cleaver_Base {
				public override BodyPart Part => BodyPart.Tail;
				public override bool CanInsert(Projectile parent, Projectile child) => child is null;
			}
		}
		public class _Temp_Light_Blue : PirateEyeMode, IBroken {
			static string IBroken.BrokenReason => "Needs idea";
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.FrostBoltStaff}";
			public override Color Color => FromHexRGB(0x009fff);
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.FrostBoltStaff);
				AIType = ProjectileID.FrostBoltStaff;
			}
		}
		public class _Temp_Blue : PirateEyeMode, IBroken {
			static string IBroken.BrokenReason => "Needs idea";
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.WaterStream}";
			public override Color Color => FromHexRGB(0x2000ff);
			public override float DamageMult => 0.15f;
			public override float KnockbackMult => 0.15f;
			public override int Cooldown => 6;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.WaterStream);
				AIType = ProjectileID.WaterStream;
			}
		}
		public class Witch_Bolt : PirateEyeMode {
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.BeeArrow}";
			public override Color Color => FromHexRGB(0x8000ff);
			public override int Cooldown => 20;
			public static float Range => 16 * 45;
			public override void SetDefaults() {
				Projectile.DamageType = DamageClass.Magic;
				Projectile.width = 1;
				Projectile.height = 1;
				Projectile.friendly = true;
				Projectile.penetrate = -1;
				Projectile.usesLocalNPCImmunity = true;
				Projectile.localNPCHitCooldown = Cooldown * 2;
				Projectile.tileCollide = false;
			}
			public override bool ShouldUpdatePosition() => false;
			public override void OnSpawn(IEntitySource source) {
				NPC target = Main.npc.GetIfInRange((int)Projectile.ai[0]);
				if (target?.CanBeChasedBy(Projectile) != true) Projectile.ai[0] = -1;
				Player player = Main.player[Projectile.owner];
				Projectile.localNPCHitCooldown = CombinedHooks.TotalUseTime(Cooldown, player, player.OriginPlayer().spacePirateEye) * 2;
			}
			public override bool? CanHitNPC(NPC target) => Projectile.ai[0] == target.whoAmI;
			public override void AI() {
				NPC target = Main.npc.GetIfInRange((int)Projectile.ai[0]);
				Player player = Main.player[Projectile.owner];
				if (target?.CanBeChasedBy(Projectile) != true) goto die;
				if (player.OriginPlayer().spacePirateEye is not Item spacePirateEye || Colors[player.OriginPlayer().spacePirateEyeSelection] is not Witch_Bolt) goto die;
				Vector2 eyePosition = EyePosition(player);
				Projectile.position = target.Center;
				if (Projectile.position.DistanceSQ(eyePosition) > Range * Range) goto die;
				if (!CollisionExt.CanHitRay(Projectile.position, eyePosition)) goto die;

				Projectile.localNPCHitCooldown = CombinedHooks.TotalUseTime(Cooldown, player, spacePirateEye) * 2;
				Projectile.timeLeft = 60;
				goto sound;
				die:
				Min(ref Projectile.timeLeft, 7);
				sound:
				Vector2 soundPosition = Main.Camera.Center.SnapToLine(Projectile.position, EyePosition(player));
				SoundEngine.PlaySound(Origins.Sounds.LittleZap, soundPosition);
			}
			public override void Shoot(Player player, Entity target, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
				player.SpawnProjectile(source, position, default, type, damage, knockback, target.whoAmI);
			}
			public override bool FindTarget(Player player, Vector2 position, out Vector2 targetPos, out Entity targetEntity) {
				Vector2 _targetPos = position;
				Entity _targetEntity = null;
				float maxDist = Range;
				maxDist *= maxDist;
				BitArray targets = new(Main.maxNPCs);
				foreach (Projectile projectile in Main.ActiveProjectiles) {
					if (projectile.owner == player.whoAmI && projectile.type == Type) targets[(int)projectile.ai[0]] = true;
				}

				bool result = player.DoHoming(FindTarget);
				targetEntity = _targetEntity;
				targetPos = _targetPos;
				return result;
				bool FindTarget(Entity target) {
					if (targets[target.whoAmI]) return false;
					Vector2 currentPos = position.Clamp(target.Hitbox);
					if (Math.Sign(position.X - currentPos.X) == player.direction) return false;
					if (!CollisionExt.CanHitRay(target.Center, position)) return false;
					float dist = currentPos.DistanceSQ(position);
					if (dist < maxDist) {
						maxDist = dist;
						_targetEntity = target;
						_targetPos = currentPos;
						return true;
					}
					return false;
				}
			}
			public override bool PreDraw(ref Color lightColor) {
				Witch_Bolt_Drawer.Draw(Projectile);
				return false;
			}
			public readonly struct Witch_Bolt_Drawer {
				private static readonly VertexStrip _vertexStrip = new();
				public static void Draw(Projectile proj) {
					MiscShaderData miscShaderData = GameShaders.Misc["Origins:Framed"];
					float uTime = (float)Main.timeForVisualEffects / 44;
					const float normal_dist = 16;
					Vector2 diff = (proj.position - EyePosition(Main.player[proj.owner])).Normalized(out float dist);
					int length = (int)float.Ceiling(dist / normal_dist);
					dist /= length;
					length++;
					float[] rot = new float[length];
					Vector2[] pos = new Vector2[length];
					Array.Fill(rot, diff.ToRotation());
					Vector2 cur = proj.position;
					for (int i = 0; i < length; i++) {
						pos[i] = cur + GeometryUtils.Vec2FromPolar(Main.rand.NextFloat(-6, 6), rot[i] + MathHelper.PiOver2);
						Lighting.AddLight(pos[i], 0.75f, 0.1f, 1f);
						cur -= diff * dist;
					}
					Asset<Texture2D> texture = TextureAssets.Extra[ExtrasID.MagicMissileTrailShape];
					miscShaderData.UseImage0(texture);
					//miscShaderData.UseShaderSpecificData(new Vector4(Main.rand.NextFloat(1), 0, 1, 1));
					miscShaderData.Shader.Parameters["uAlphaMatrix0"]?.SetValue(new Vector4(1, 1, 1, 0));
					miscShaderData.Shader.Parameters["uSourceRect0"]?.SetValue(new Vector4(Main.rand.NextFloat(1), 0, 1, 1));
					miscShaderData.Apply();
					_vertexStrip.PrepareStrip(pos, rot, _ => new Color(0.75f, 0.1f, 1f, 0.4f), _ => 12, -Main.screenPosition, length, includeBacksides: true);
					_vertexStrip.DrawTrail();
					for (int i = 0; i < length / 2; i++) {
						pos[i] = pos[i] + GeometryUtils.Vec2FromPolar(Main.rand.NextFloat(-6, 6), rot[i] + MathHelper.PiOver2);
					}
					_vertexStrip.PrepareStrip(pos, rot, _ => new Color(0.95f, 0.3f, 1f, 0f), _ => 9, -Main.screenPosition, length, includeBacksides: true);
					_vertexStrip.DrawTrail();
					Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				}
			}
		}
		public class _Temp_Magenta : PirateEyeMode, IBroken {
			static string IBroken.BrokenReason => "Needs idea";
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.BeeArrow}";
			public override Color Color => FromHexRGB(0xdf00ff);
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.BeeArrow);
				AIType = ProjectileID.BeeArrow;
			}
		}
		public class _Temp_Hot_Pink : PirateEyeMode, IBroken {
			static string IBroken.BrokenReason => "Needs idea";
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.BeeArrow}";
			public override Color Color => FromHexRGB(0xff00bf);
			public override int Cooldown => 60;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.BeeArrow);
				AIType = ProjectileID.BeeArrow;
			}
		}
		public class _Temp_Pink : PirateEyeMode, IBroken {
			static string IBroken.BrokenReason => "Needs idea";
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.BeeArrow}";
			public override Color Color => FromHexRGB(0xff9ae9);
			public override int Cooldown => 60;
			public override float Order => 1.01f;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.BeeArrow);
				AIType = ProjectileID.BeeArrow;
			}
		}
		public class _Temp_Green : PirateEyeMode, IBroken {
			static string IBroken.BrokenReason => "Needs idea";
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.BeeArrow}";
			public override Color Color => FromHexRGB(0x009700);
			public override int Cooldown => 60;
			public override float Order => 1.02f;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.BeeArrow);
				AIType = ProjectileID.BeeArrow;
			}
		}
		public class _Temp_Brown : PirateEyeMode, IBroken {
			static string IBroken.BrokenReason => "Needs idea";
			public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.BeeArrow}";
			public override Color Color => FromHexRGB(0xa74d00);
			public override int Cooldown => 60;
			public override float Order => 1.03f;
			public override void SetDefaults() {
				Projectile.CloneDefaults(ProjectileID.BeeArrow);
				AIType = ProjectileID.BeeArrow;
			}
		}
		#endregion
	}
	public class SpacePirateEyeInterface : UserInterface {
		readonly LegacyGameInterfaceLayer interactionLayer;
		readonly LegacyGameInterfaceLayer displayLayer;
		bool isActive = false;
		Vector2 position;
		static int lowest = 0;
		public SpacePirateEyeInterface() : base() {
			interactionLayer = new LegacyGameInterfaceLayer(
				"Origins: Space Pirate's Eye UI Interaction",
				delegate {
					lowest = Space_Pirates_Eye.GetPlayerCounts(Main.LocalPlayer);
					EnsureButtons();
					if (!GetBox().Contains(Main.MouseScreen)) {
						if (Main.mouseLeft && Main.mouseLeftRelease) isActive = false;
						return true;
					}
					if (PlayerInput.IgnoreMouseInterface) return true;
					Main.LocalPlayer.mouseInterface = true;
					IgnoreRemainingInterface.Activate();
					for (int i = 0; i < buttons.Length; i++) {
						if (GetButton(i).Contains(Main.MouseScreen) && Main.mouseLeft && Main.mouseLeftRelease && Space_Pirates_Eye.counts[i] == lowest) {
							Main.LocalPlayer.OriginPlayer().spacePirateEyeSelection = i;
							isActive = false;
						}
					}
					{
						if (CloseButton.Contains(Main.MouseScreen) && Main.mouseLeft && Main.mouseLeftRelease) isActive = false;
					}
					{
						if (NoneButton.Contains(Main.MouseScreen) && Main.mouseLeft && Main.mouseLeftRelease) {
							Main.LocalPlayer.OriginPlayer().spacePirateEyeSelection = -1;
							isActive = false;
						}
					}
					return true;
				},
				InterfaceScaleType.UI
			);
			displayLayer = new LegacyGameInterfaceLayer(
				"Origins: Space Pirate's Eye UI",
				delegate {
					EnsureButtons();
					Texture2D texture = TextureAssets.MagicPixel.Value;
					DrawRoundedRetangle(Main.spriteBatch, GetBox(), Color.Gray);
					for (int i = 0; i < buttons.Length; i++) {
						Rectangle button = GetButton(i);
						Color color = Space_Pirates_Eye.Colors[i].Color;
						if (Space_Pirates_Eye.counts[i] != lowest) {
							color = color.Desaturate(0.5f);
						} else if (button.Contains(Main.MouseScreen)) {
							Main.spriteBatch.Draw(texture, button, (color.R * 0.375f + color.G * 0.5f + color.B * 0.125f) > 128 ? Color.Black : Color.White);
							button.Inflate(-2, -2);
						}
						Main.spriteBatch.Draw(texture, button, color);
						if (Space_Pirates_Eye.counts[i] != lowest) {
							color = Color.Black;
							button.Inflate(-3, -3);
							Main.spriteBatch.Draw(texture, button, color);
						}
					}
					{
						Rectangle button = CloseButton;
						Color color = Color.White * 0.5f;
						if (button.Contains(Main.MouseScreen)) color = Color.White;
						Main.spriteBatch.Draw(TextureAssets.Cd.Value, button, color);
					}
					{
						Rectangle button = NoneButton;
						Color color = Color.Black * 0.5f;
						if (button.Contains(Main.MouseScreen)) color = Color.Black;
						Main.spriteBatch.Draw(texture, button, color);
						button.Inflate(-3, -3);
						Main.spriteBatch.Draw(texture, button, Color.Gray);
					}
					return true;
				},
				InterfaceScaleType.UI
			);
		}
		Rectangle[] buttons;
		Rectangle entireBox;
		Rectangle CloseButton {
			get {
				Rectangle button = GetButton(0);
				button.X = GetBox().Right - (button.Width + 6);
				return button;
			}
		}
		Rectangle NoneButton {
			get {
				Rectangle button = GetButton(Space_Pirates_Eye.Colors.Count - 1);
				button.X = GetBox().Right - (button.Width + 6);
				return button;
			}
		}
		void EnsureButtons() {
			const int button_size = 14;
			const int padded_size = button_size + 2;
			if (buttons is not null) return;
			buttons = new Rectangle[Space_Pirates_Eye.Colors.Count];
			Vector2 pos = position - new Vector2(padded_size);
			Vector2 min = new(float.PositiveInfinity);
			Vector2 max = new(float.NegativeInfinity);
			for (int i = 0; i < buttons.Length; i++) {
				Min(ref min.X, pos.X);
				Min(ref min.Y, pos.Y);
				Max(ref max.X, pos.X + button_size);
				Max(ref max.Y, pos.Y + button_size);
				buttons[i] = new((int)pos.X, (int)pos.Y, button_size, button_size);
				pos.X += padded_size;
				if (i % 8 == 7) {
					pos.X = position.X - padded_size;
					pos.Y += padded_size;
				}
			}
			const int box_padding = 6;
			entireBox.X = (int)min.X - box_padding;
			entireBox.Y = (int)min.Y - box_padding;
			entireBox.Width = (int)(max.X - min.X) + box_padding * 2 + padded_size;
			entireBox.Height = (int)(max.Y - min.Y) + box_padding * 2;
		}
		Rectangle GetButton(int i) {
			Rectangle button = buttons[i];
			button.X += Math.Min(Main.screenWidth - entireBox.Right, 0);
			button.Y += MainReflection.currentMapHeight.Value;
			return button;
		}
		Rectangle GetBox() {
			Rectangle box = entireBox;
			box.X += Math.Min(Main.screenWidth - entireBox.Right, 0);
			box.Y += MainReflection.currentMapHeight.Value;
			return box;
		}
		public void Activate() {
			isActive = true;
			position = AccessorySlotLoaderMethods.CurrentSlotPosition;
			position += Vector2.One * 52 * 0.5f * Main.inventoryScale;
			position.Y -= MainReflection.currentMapHeight.Value;
			buttons = null;
		}
		public void Insert(List<GameInterfaceLayer> layers) {
			if (!isActive) return;
			if (!Main.playerInventory || Main.LocalPlayer.OriginPlayer().spacePirateEye is null) isActive = false;
			if (!isActive) return;
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1) {//error prevention & null check
				interactionLayer.ScaleType = InterfaceScaleType.UI;
				layers.Insert(inventoryIndex + 1, displayLayer);
				layers.Insert(inventoryIndex, interactionLayer);
			}
		}
		static void DrawRoundedRetangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, Texture2D texture = null) {
			texture ??= TextureAssets.InventoryBack13.Value;
			Rectangle textureBounds = texture.Bounds;
			foreach (StretchSegment segment in RectangleSegments) {
				spriteBatch.Draw(
					texture,
					segment.GetBounds(rectangle),
					segment.GetBounds(textureBounds),
					color
				);
			}
		}
		struct StretchLength(float PositionFactor, float SizeFactor, float Flat) {
			public static StretchLength Position = new(1, 0, 0);
			public readonly float GetValue(float position, float size) => position * PositionFactor + size * SizeFactor + Flat;
		}
		record struct StretchSegment(StretchLength Left, StretchLength Top, StretchLength Width, StretchLength Height) {
			public readonly Rectangle GetBounds(Rectangle parent) => new(
				(int)Left.GetValue(parent.X, parent.Width),
				(int)Top.GetValue(parent.Y, parent.Height),
				(int)Width.GetValue(parent.X, parent.Width),
				(int)Height.GetValue(parent.Y, parent.Height)
			);
		}
		static StretchSegment[] RectangleSegments { get; set; } = [
			new(StretchLength.Position, StretchLength.Position, new(0, 0, 10), new(0, 0, 10)),
			new(StretchLength.Position, new(1, 1, -10), new(0, 0, 10), new(0, 0, 10)),
			new(new(1, 1, -10), StretchLength.Position, new(0, 0, 10), new(0, 0, 10)),
			new(new(1, 1, -10), new(1, 1, -10), new(0, 0, 10), new(0, 0, 10)),

			new(new(1, 0, 10), StretchLength.Position, new(0, 1, -10 * 2), new(0, 0, 10)),
			new(new(1, 0, 10), new(1, 1, -10), new(0, 1, -10 * 2), new(0, 0, 10)),

			new(StretchLength.Position, new(1, 0, 10), new(0, 0, 10), new(0, 1, -10 * 2)),
			new(new(1, 1, -10), new(1, 0, 10), new(0, 0, 10), new(0, 1, -10 * 2)),

			new(new(1, 0, 10), new(1, 0, 10), new(0, 1, -10 * 2), new(0, 1, -10 * 2))
		];
	}
}
