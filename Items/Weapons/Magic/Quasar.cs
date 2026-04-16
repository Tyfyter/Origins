using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Items.Accessories;
using Origins.Items.Other.Dyes;
using Origins.NPCs;
using PegasusLib.Graphics;
using PegasusLib.Networking;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Quasar : ModItem {
		public override string Texture => $"Terraria/Images/NPC_1";
		public static int ShaderID { get; private set; }
		public override void SetStaticDefaults() {
			GameShaders.Armor.BindShader(Type, new ArmorShaderData(
				Mod.Assets.Request<Effect>("Effects/Overbrighten"),
				"Quasar"
			)).UseOpacity(2);
			ShaderID = GameShaders.Armor.GetShaderIdFromItemId(Type);
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 28;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 44;
			Item.height = 44;
			Item.useTime = 37;
			Item.useAnimation = 37;
			Item.shoot = ModContent.ProjectileType<Quasar_P>();
			Item.shootSpeed = 1f;
			Item.mana = 8;
			Item.knockBack = 3f;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item67;
			Item.autoReuse = false;
			Item.channel = true;
		}
	}
	public class Quasar_P : ModProjectile, IShadedProjectile {
		readonly static Sound sound = EnvironmentSounds.Register<Sound>();
		public override string Texture => $"Terraria/Images/NPC_0";
		public float ManaMultiplier => 1;
		public float ChargeTime => Projectile.ai[0] * 2;
		public bool Charged => Projectile.ai[2] >= ChargeTime;
		public int Shader => Quasar.ShaderID;
		Quasar_P() : base() => _ = sound;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = 1600 + 64;
			Origins.HomingEffectivenessMultiplier[Type] = 10;
			On_Main.UpdateAudio += sound.On_SoundPlayer_Update;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.localNPCHitCooldown = 4;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_ItemUse { Player: Player player }) {
				Projectile.ai[0] = player.itemTimeMax;
			}
		}
		public override bool ShouldUpdatePosition() => false;
		public Vector2 TargetPos {
			get => new(Projectile.localAI[0], Projectile.localAI[1]);
			set => (Projectile.localAI[0], Projectile.localAI[1]) = value;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (!Lunatics_Rune.CheckMana(player, player.HeldItem, ManaMultiplier / 60f, pay: true)) {
				Projectile.Kill();
				return;
			}
			if (Projectile.velocity.X != 0) player.ChangeDir(Math.Sign(Projectile.velocity.X));

			//SoundEngine.SoundPlayer.Play(SoundID.Item158.WithPitch(Projectile.ai[2] / 10).WithVolume(0.24f), player.position);
			//SoundEngine.SoundPlayer.Play(SoundID.Item132.WithPitch(Projectile.ai[2] / 10).WithVolume(0.24f), player.position);
			Projectile.position = Main.GetPlayerArmPosition(Projectile);
			if (player.mount?.Active ?? false) Projectile.position.Y -= player.mount.PlayerOffset;
			Projectile.position = player.RotatedRelativePoint(Projectile.position);
			if (Projectile.IsLocallyOwned()) {
				if (!player.channel) {
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
						Min(ref diff, 0.02f);
						Projectile.velocity = Projectile.velocity.RotatedBy(diff * dir);
						Projectile.netUpdate = true;
					}
				}
			}
			player.ChangeDir(Projectile.direction); // Change the player's direction based on the projectile's own
			player.heldProj = Projectile.whoAmI; // We tell the player that the drill is the held projectile, so it will draw in their hand
			player.SetDummyItemTime(2); // Make sure the player's item time does not change while the projectile is out
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			player.itemRotation = (Projectile.velocity * Projectile.direction).ToRotation();

			Projectile.ai[1] = CollisionExt.Raymarch(Projectile.position, Projectile.velocity, ProjectileID.Sets.DrawScreenCheckFluff[Type] - 64) + 8;
			Vector2 newTarget = Projectile.position + Projectile.velocity * Projectile.ai[1];
			TargetPos = newTarget;
			Projectile.ai[2]++;
			if (Projectile.IsLocallyOwned() && Charged && Projectile.ai[2] % 5 == 0) {
				Vector2 perp = Projectile.velocity.Perpendicular();
				int type = ModContent.ProjectileType<Quasar_Offshoot_P>();
				for (int i = Main.rand.Next((int)Math.Ceiling(0.0075f * Projectile.ai[1])); i > 0; i--) {
					float dist = Main.rand.NextFloat(Projectile.ai[1]);
					int dir = Main.rand.NextBool().ToDirectionInt();
					Projectile.SpawnProjectile(null,
						Projectile.position + Projectile.velocity * dist,
						perp.RotatedBy(dir * (Main.rand.NextFloat(0.5f) + 0.25f)) * 16 * dir,
						type,
						Projectile.damage,
						Projectile.knockBack,
						dist
					);
				}
			}
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (!Charged) return false;
			float collisionPoint = 1;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.position, TargetPos, 64, ref collisionPoint);
		}
		public override void OnKill(int timeLeft) { }
		public override bool PreDraw(ref Color lightColor) {
			sound.TrySetNearest(Projectile.position);
			if (!Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), Projectile.position, TargetPos)) return false;
			SpriteBatchState state = Main.spriteBatch.GetState();
			Main.spriteBatch.Restart(state, samplerState: SamplerState.LinearWrap);
			Vector2 diff = TargetPos - Projectile.position;
			Vector2 position = Projectile.position;
			position -= Main.screenPosition;
			float rotation = diff.ToRotation();
			float dist = diff.Length();
			const float scale = 1f / 256f;
			Color color = new(80, 0, 255, 0);
			float progress = Projectile.ai[2] / ChargeTime;
			Min(ref progress, 1);
			Rectangle frame = new(256 - (int)((Projectile.ai[2] * 24) % 256), 0, (int)(dist), 256);
			DrawData data = new(
				TextureAssets.Extra[ExtrasID.RainbowRodTrailErosion].Value,
				position,
				frame,
				color * progress,
				rotation,
				Vector2.UnitY * 128,
				new Vector2(1, 128 * scale),
				0
			);
			Main.EntitySpriteDraw(data);
			data.color = color * progress * progress * progress;
			Vector2 offset = (rotation + MathHelper.PiOver2).ToRotationVector2() * (1 - progress) * 32;
			data.position = position + offset;
			frame.Width = (int)CollisionExt.Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist * 1.15f + 16) + 8;
			data.sourceRect = frame;
			Main.EntitySpriteDraw(data);
			data.position = position - offset;
			frame.Width = (int)CollisionExt.Raymarch(data.position + Main.screenPosition, Projectile.velocity, dist * 1.15f + 16) + 8;
			data.sourceRect = frame;
			Main.EntitySpriteDraw(data);
			Main.spriteBatch.Restart(state);
			return false;
		}
		class Sound : AEnvironmentSound {
			SlotId droning;
			SoundStyle sound = new("Origins/Sounds/Custom/Black_Hole", SoundType.Sound) {
				IsLooped = true
			};
			float drownOutMultiplier = 1;
			public override void UpdateSound(Vector2 position) {
				float volume = 0;
				droning.PlaySoundIfInactive(sound, position, playingSound => {
					if (GetPosition() is Vector2 pos) {
						playingSound.Position = pos;
						MathUtils.LinearSmoothing(ref volume, 2f / float.Max(pos.DistanceSQ(Main.Camera.Center) / (16 * 20 * 16 * 20), 1), 1f / 30);
					} else if (MathUtils.LinearSmoothing(ref volume, 0, 1f / 15)) {
						return false;
					}
					drownOutMultiplier = Utils.Remap(volume, 0, 2, 1, 1f / 4);
					playingSound.Volume = volume / drownOutMultiplier;
					return true;
				});
			}
			public override void UpdateSoundInactive() => drownOutMultiplier = 1;
			internal void On_SoundPlayer_Update(On_Main.orig_UpdateAudio orig, Main self) {
				using ScopedOverride<float> s = Main.soundVolume.ScopedOverride(Main.soundVolume * drownOutMultiplier);
				using ScopedOverride<float> a = Main.ambientVolume.ScopedOverride(Main.ambientVolume * drownOutMultiplier);
				using ScopedOverride<float> m = Main.musicVolume.ScopedOverride(Main.musicVolume * drownOutMultiplier);
				orig(self);
			}
		}
	}
	public class Quasar_Offshoot_P : ModProjectile {
		public override string Texture => typeof(Blast_Furnace).GetDefaultTMLName();
		public virtual float FadeFrames => 20f;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 15;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.timeLeft = 35;
			Projectile.extraUpdates = 1;
			Projectile.penetrate = -1;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool ShouldUpdatePosition() => false;
		public record struct Wave(float Frequency, float Amplitude, float Phase) {
			public readonly double Sample(double position) => Math.Sin(position * Frequency + Phase) * Amplitude;
			public override readonly string ToString() => $"sin(x * {Frequency} + {Phase}) * {Amplitude}";
		}
		public Vector2 originalDirection;
		public NPC target;
		public Wave[] waves;
		public Vector2 HeadOffset {
			get => new(Projectile.ai[0], Projectile.ai[1]);
			set => (Projectile.ai[0], Projectile.ai[1]) = value;
		}
		Attachment attachment;
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent { Entity: Projectile parent }) {
				attachment.Projectile = parent;
				attachment.Dist = Projectile.ai[0];
			}
			Projectile.ai[0] = 0;
			waves = new Wave[Main.rand.Next(13, 21)];
			for (int i = 0; i < waves.Length; i++) {
				waves[i] = new(Main.rand.NextFloat(0.1f, 0.5f), Main.rand.NextFloat(0.02f, 0.1f), Main.rand.NextFloat(MathHelper.TwoPi));
			}
			originalDirection = Projectile.velocity.Normalized(out _);
			float distanceFromTarget = 16 * 5f;
			distanceFromTarget *= distanceFromTarget;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.CanBeChasedBy()) {
					Vector2 pos = Main.MouseWorld;
					float between = pos.Clamp(npc.Hitbox).DistanceSQ(pos);
					if (distanceFromTarget > between) {
						distanceFromTarget = between;
						target = npc;
					}
				}
			}
		}
		public override void AI() {
			Projectile.Opacity = Projectile.timeLeft / FadeFrames;
			attachment.Apply(Projectile);
			if (++Projectile.ai[2] >= Projectile.oldPos.Length) return;
			Projectile.ai[0] += Projectile.velocity.X;
			Projectile.ai[1] += Projectile.velocity.Y;
			ProcessTick();
			double value = 0;
			for (int i = 0; i < waves.Length; i++) value += waves[i].Sample(Projectile.ai[2]);
			Projectile.velocity = Projectile.velocity.RotatedBy(value);
			if (target is not null) {
				if (Projectile.localNPCImmunity[target.whoAmI] != 0 || !target.CanBeChasedBy(Projectile)) target = null;
			}
			float speed = Projectile.velocity.Length();
			Vector2? targetDir = target?.DirectionFrom(Projectile.position + HeadOffset);
			Projectile.velocity = (Vector2.Lerp(
				(targetDir ?? originalDirection) * speed,
				Projectile.velocity,
				float.Clamp(Vector2.Dot((targetDir ?? originalDirection), HeadOffset.Normalized(out _)) * 2, 0, 1)
			) + (targetDir ?? Vector2.Zero) * speed * 0.25f).Normalized(out _) * speed;
			SetHitboxCache();
			void ProcessTick() {
				Projectile.oldPos[(int)Projectile.ai[2]] = HeadOffset;
				Projectile.oldRot[(int)Projectile.ai[2]] = Projectile.velocity.ToRotation() + MathHelper.Pi;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (polygonCache is null) return false;
			targetHitbox.Offset((-Projectile.position).ToPoint());
			return CollisionExtensions.PolygonIntersectsRect(polygonCache, targetHitbox);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Daybreak, Main.rand.Next(60, 181));
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.WriteVector2(originalDirection);
			writer.Write((short)(target?.whoAmI ?? -1));
			writer.Write((byte)waves.Length);
			for (int i = 0; i < waves.Length; i++) {
				writer.Write(waves[i].Frequency);
				writer.Write(waves[i].Amplitude);
				writer.Write(waves[i].Phase);
			}
			attachment.Write(writer);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			originalDirection = reader.ReadVector2();
			target = Main.npc.GetIfInRange(reader.ReadInt16());
			waves = new Wave[reader.ReadByte()];
			for (int i = 0; i < waves.Length; i++) {
				waves[i] = new Wave(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
			}
			attachment.Read(reader);
		}
		(Vector2 start, Vector2 end)[] polygonCache;
		private void SetHitboxCache() {
			Vector2 width = new(BladeWidth, 0);
			Vector2 rot = width.RotatedBy(Projectile.rotation);
			Vector2 lastPos0 = -rot;
			Vector2 lastPos1 = rot;
			Vector2 nextPos0 = default, nextPos1 = default;
			int count = int.Min((int)Projectile.ai[2], Projectile.oldPos.Length);
			polygonCache = new (Vector2 start, Vector2 end)[count * 2 + 2];
			int lineIndex = 0;
			polygonCache[lineIndex++] = (lastPos1, lastPos0);
			for (int i = 0; i < count; i++) {
				Vector2 nextPos = Projectile.oldPos[i];
				rot = width.RotatedBy(Projectile.oldRot[i]);
				nextPos0 = nextPos - rot;
				nextPos1 = nextPos + rot;

				polygonCache[lineIndex++] = (lastPos0, nextPos0);
				polygonCache[lineIndex++] = (nextPos1, lastPos1);
				lastPos0 = nextPos0;
				lastPos1 = nextPos1;
			}
			polygonCache[lineIndex++] = (nextPos0, nextPos1);
		}
		private static readonly VertexStrip _vertexStrip = new();
		public override bool PreDraw(ref Color lightColor) {
			Vector2[] oldPos = new Vector2[int.Min((int)Projectile.ai[2], Projectile.oldPos.Length)];
			for (int i = 0; i < oldPos.Length; i++) {
				oldPos[i] = Projectile.oldPos[i] + Projectile.position;
			}
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:LaserBlade"];
			miscShaderData.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
			miscShaderData.UseImage0(TextureAssets.Extra[ExtrasID.FlameLashTrailShape]);
			miscShaderData.Shader.Parameters["uAlphaMatrix0"].SetValue(new Vector4(1, 1, 1, 0));
			miscShaderData.UseSaturation(-1);
			miscShaderData.UseOpacity(2);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(oldPos, Projectile.oldRot, BladeSecondaryColors, BladeWidth, -Main.screenPosition, true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();

			miscShaderData.UseSaturation(0.5f);
			miscShaderData.Apply();
			_vertexStrip.PrepareStripWithProceduralPadding(oldPos, Projectile.oldRot, BladeColors, BladeWidth, -Main.screenPosition, true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
			float BladeWidth(float progressOnStrip) => this.BladeWidth;
			return false;
		}
		public virtual Color BladeColors(float progressOnStrip) => new Color(56, 0, 255, 0) * Projectile.Opacity;
		public virtual Color BladeSecondaryColors(float progressOnStrip) => new Color(182, 32, 218, 0) * Projectile.Opacity;
		public virtual int BladeWidth => 12;
		record struct Attachment(Projectile Projectile, float Dist) {
			public readonly void Write(BinaryWriter writer) {
				if (Projectile is null) {
					writer.Write((byte)Main.maxPlayers);
					return;
				}
				writer.Write((byte)Projectile.owner);
				writer.Write((ushort)Projectile.identity);
				writer.Write(Dist);
			}
			public void Read(BinaryReader reader) {
				byte owner = reader.ReadByte();
				if (owner == Main.maxPlayers) return;
				ushort identity = reader.ReadUInt16();
				foreach (Projectile projectile in Main.ActiveProjectiles) {
					if (projectile.owner == owner && projectile.identity == identity) {
						Projectile = projectile;
						break;
					}
				}
				Dist = reader.ReadSingle();
			}
			public readonly void Apply(Projectile applyTo) {
				if (Projectile is null) return;
				applyTo.position = Projectile.position + Projectile.velocity * Dist;
				if (Dist > Projectile.ai[1]) applyTo.timeLeft -= 2;
			}
		}
	}
}
