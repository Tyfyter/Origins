using Microsoft.Xna.Framework.Graphics;
using Origins.Core;
using Origins.Core.Shaders;
using Origins.Dev;
using Origins.Gores;
using Origins.Graphics;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Journal;
using Origins.NPCs.Ashen.Boss;
using Origins.World.BiomeData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.NPCs.Ashen.Boss.Fire_Lasers_State;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.Ashen {
	public class CM_17 : Glowing_Mod_NPC, IWikiNPC, IAshenEnemy, IBroken {
		public Rectangle DrawRect => new(0, 0, 142, 90);
		public int AnimationFrames => 6;
		public int FrameDuration => 8;
		public AutoLoadingTexture drillBit = typeof(CM_17).GetDefaultTMLName() + "_Drillbit";
		public AutoLoadingTexture lowerArm = typeof(CM_17).GetDefaultTMLName() + "_Lower";
		public AutoLoadingTexture upperArm = typeof(CM_17).GetDefaultTMLName() + "_Upper";
		protected SpriteEffects SpriteEffects => NPC.direction == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
		public Vector2 HeadPos => NPC.Center + new Vector2(-59, -5).Apply(SpriteEffects, default);

		public static string BrokenReason => "remove debug info after balance testing";

		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 9;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with {
				Position = new Vector2(50f, 26f),
				PortraitPositionXOverride = 20,
				PortraitPositionYOverride = 0
			};
			GetInstance<Ashen_Biome.SpawnRates>().AddSpawn(Type, BiomeSpawnChance);
		}
		public override void SetDefaults() {
			NPC.lifeMax = 3000;
			NPC.defense = 29;
			NPC.damage = 42;
			NPC.width = 142;
			NPC.height = 88;
			NPC.value = Item.buyPrice(0, 0, 6);
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			NPC.knockBackResist = 0.1f;
			SpawnModBiomes = [
				GetInstance<Underground_Ashen_Biome>().Type,
			];
		}
		public override bool? CanFallThroughPlatforms() => NPC.targetRect.Bottom > NPC.position.Y + NPC.height + NPC.velocity.Y;
		public static int TimeToSpawnWatchlings => 2 * 60;
		public static int LaserDamage => (int)(16 * ContentExtensions.DifficultyDamageMultiplier);
		public override void AI() {
			const int MaxWatchlings = 10; // desired max subtracted by 2
			float accel = 0.15f;
			NPCAimedTarget target = NPC.GetTargetData();
			bool targetInvalid = target.Invalid;
			int currentMoveDirection = float.Sign(NPC.velocity.X);
			if (!NPC.collideY && NPC.velocity.Y == 0) {
				NPC.collideY = Collision.GetTilesIn(NPC.BottomLeft + Vector2.UnitY, NPC.BottomRight + Vector2.UnitY * 16).Any(pos => Framing.GetTileSafely(pos).HasSolidTile());
			}

			Vector2 targetDirection = targetInvalid ? default : NPC.DirectionTo(NPC.targetRect.Center());
			int targetMoveDirection = targetInvalid ? NPC.direction : float.Sign(targetDirection.X);
			Rectangle detectRange = NPC.Hitbox;
			Rectangle fleeRange = NPC.Hitbox;
			detectRange.Inflate(20 * 16, 15 * 16);
			fleeRange.Inflate(8 * 16, 5 * 16); // for debugging
			detectRange.DrawDebugOutline(); // for debugging
			fleeRange.DrawDebugOutline();
			void AttemptRetarget() {
				if (NPC.localAI[3] == 0) accel = 0;
				NPC.ai[0] = 0;
				NPC.TargetClosest(false);
			}
			bool HasMaxWatchings() {
				int count = 0;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (npc?.ModNPC is Watchling { OwnerID: int OwnerID } && OwnerID == NPC.whoAmI) {
						count++;
					}
					if (count >= MaxWatchlings) break;
				}
				return count >= MaxWatchlings;
			}
			if (!targetInvalid && (target.Hitbox.Intersects(detectRange) || NPC.localAI[3] == 1)) {
				NPC.localAI[3] = 1;
				bool canSpawnWatchling = true;
				switch (NPC.aiAction) {
					case 0:
					targetMoveDirection = Math.Sign(target.Center.X - NPC.Center.X);
					if ((NPC.ai[2].Cooldown() || NPC.ai[2] == 0) && !HasMaxWatchings() && canSpawnWatchling) {
						NPC.ai[1] = 30;
						NPC.aiAction = 1;
						NPC.netUpdate = true;
					}
					if (NPC.aiAction != 0) break;
					if ((NPC.ai[1].Cooldown() || NPC.ai[1] == 0) && NPC.Center.IsWithin(target.Center, 35 * 16) && !target.Hitbox.Intersects(fleeRange)) {
						NPC.ai[0] = 0;
						NPC.ai[3] = (target.Center - NPC.Center).ToRotation();
						NPC.aiAction = 2;
						NPC.netUpdate = true;
					}
					break;

					case 1:
					Vector2 pos = NPC.Center + new Vector2(55, -4).Apply(SpriteEffects, default);
					Dust.QuickDust(pos, Color.White); // for debugging
					if (NPC.ai[0]++ == TimeToSpawnWatchlings * 0.5f) {
						for (int i = 0; i < 3; i++) {
							NPC watchling = NPC.SpawnNPC(null, (int)pos.X, (int)pos.Y, NPCType<Watchling>());
							watchling.velocity = new Vector2(-targetMoveDirection * 2, -2) + Main.rand.NextVector2Circular(3, 3);
						}
					} else if (NPC.ai[0] >= TimeToSpawnWatchlings) {
						NPC.ai[0] = 0;
						NPC.ai[2] = 1 * 60;
						NPC.aiAction = 0;
						NPC.netUpdate = true;
					}
					if (target.Hitbox.Intersects(fleeRange)) {
						targetMoveDirection = -Math.Sign(target.Center.X - NPC.Center.X);
						if (NPC.ai[0] < TimeToSpawnWatchlings * 0.5f) NPC.ai[0].Cooldown(rate: 2);
						else accel = 0;
					} else accel = 0;
					break;

					case 2: {
						Vector2 diff = target.Center - HeadPos;
						accel *= 0.35f * float.Pow(NPC.ai[1] / ChargeTime, 2) + 1.3f / diff.Length();
						NPC.ai[1].Cooldown();
						canSpawnWatchling = false;
						switch (NPC.ai[0]) {
							case 0: {
								Vector2 dir = NPC.ai[3].ToRotationVector2();
								NPC.SpawnProjectile(null,
									HeadPos + dir * 16,
									dir,
									ProjectileType<CM_17_Laser>(),
									ShotDamage,
									1
								);
								NPC.ai[0] = 1;
								NPC.ai[1] = ChargeTime;
								break;
							}
							case 1: {
								GeometryUtils.AngularSmoothing(ref NPC.ai[3], diff.ToRotation(), 0.04f * float.Pow(NPC.ai[1] / ChargeTime, 2) + 1.3f / diff.Length());
								if (NPC.ai[1] == 0) {
									NPC.ai[0] = 2;
									NPC.ai[1] = ActiveTime;
								}
								break;
							}
							case 2: {
								GeometryUtils.AngularSmoothing(ref NPC.ai[3], diff.ToRotation(), 1.8f / diff.Length());
								if (NPC.ai[1] < ActiveTime - 16) canSpawnWatchling = true;
								if (NPC.ai[1] == 0) {
									NPC.ai[0] = 0;
									NPC.ai[1] = 90;
									NPC.aiAction = 0;
									NPC.netUpdate = true;
								}
								break;
							}
						}
						goto case 0;
					}
				}
			} else AttemptRetarget();
			HasMaxWatchings(); // for debugging

			if (currentMoveDirection != targetMoveDirection) accel *= 0.25f;
			if (NPC.direction == 0) NPC.direction = -1;
			if (!NPC.collideY) accel *= 0.25f;
			NPC.velocity.X += accel * targetMoveDirection;
			NPC.velocity.X *= NPC.collideY ? 0.93f : 0.98f;
			float preStepOffY = NPC.gfxOffY;
			if (NPC.collideY) {
				Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
				Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			}
			if (accel != 0) NPC.direction = targetMoveDirection;
			if (NPC.collideY && NPC.velocity.Y == 0) {
				bool shouldJump = false;
				if (NPC.collideX && preStepOffY == NPC.gfxOffY) shouldJump = true;
				else if (!targetInvalid) {
					if (Math.Abs(NPC.Center.Y - target.Center.Y) <= 8.5f * 16 && Math.Abs(NPC.Center.X - target.Center.X) <= 4) {
						NPC.velocity.X *= 0.2f;
						shouldJump = true;
					} else if (target.Position.Y + target.Height < NPC.position.Y && !NPC.Hitbox.Add(new Vector2(NPC.width * 0.5f, 16)).OverlapsAnyTiles(false)) {
						shouldJump = true;
					}
				}
				if (shouldJump) NPC.velocity.Y -= 8;
			}
			NPC.spriteDirection = NPC.direction;
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			NPC.localAI[3] = 1;
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			if (!projectile.npcProj) NPC.localAI[3] = 1;
		}
		public static float BiomeSpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			if (spawnInfo.SpawnTileY < Main.rockLayer) return 0;
			return Ashen_Biome.SpawnRates.Watcher * (Main.hardMode ? 1 : 0.5f);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.CustomBestiaryName(Type, this.GetLocalizationKey("FullName"));
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void FindFrame(int frameHeight) {
			NPC.localAI[0] = (NPC.localAI[0] + 0.15f) % 2f;
			if (NPC.aiAction == 1 && NPC.ai[0] > 0) {
				int tmp = (int)(NPC.ai[0] / TimeToSpawnWatchlings * 2) + 6;
				NPC.frame.Y = Math.Min(tmp, 8) * frameHeight;
				return;
			}
			NPC.DoFrames(10, 0..6, Math.Abs(NPC.velocity.X));
			if (Math.Abs(NPC.velocity.X) < 0.3f) NPC.frame.Y = 0;
			if (!NPC.collideY && !NPC.IsABestiaryIconDummy && Math.Abs(NPC.velocity.Y) != 0) NPC.DoFrames(1, 5..6);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.ByCondition(new Conditions.IsHardmode(), ItemType<Phoenum>(), 1, 1, 3));
			npcLoot.Add(new CommonDrop(ItemType<Exo_Legs>(), 300, 1, 1, 11));
			npcLoot.Add(ItemDropRule.ByCondition(new Journal_Entry_Condition(Journal_Registry.GetJournalEntryByTextKey(GetInstance<Worn_Paper_Smog_Test>().PaperName)), ItemType<Worn_Paper_Smog_Test>(), 40));
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Vector2[] offsets = new Vector2[Main.npcFrameCount[Type]];
			offsets[1] = new(0, 2);
			offsets[3] = new(0, 2);
			offsets[4] = new(0, 2);
			offsets[6] = new(0, 4);
			offsets[8] = new(0, 4);
			SpriteEffects effects = SpriteEffects;
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			base.PostDraw(spriteBatch, screenPos, drawColor);

			void SetDrawData(Texture2D texture, Vector2 offset, float rotation = 0f, Rectangle? frame = null) {
				DrawData data = new(texture, default, frame, drawColor, rotation, texture.Size() * 0.5f, NPC.scale, effects);
				if (frame is not null) {
					data.origin = frame.Value.Size() * 0.5f;
				}
				data.position = NPC.Center - screenPos + (offset.Apply(effects, default) + new Vector2(0, NPC.gfxOffY) * NPC.scale);
				data.Draw(spriteBatch);
			}

			SetDrawData(upperArm.Value, new Vector2(13, -15) + offsets[NPC.frame.Y / 90], NPC.rotation);

			Rectangle lowerFrame = lowerArm.Frame(1, 5, 0, NPC.frame.Y / 90);
			if (NPC.frame.Y / 90 > 4) lowerFrame.Y = 0;

			SetDrawData(lowerArm.Value, new Vector2(-28, -38) + offsets[NPC.frame.Y / 90], NPC.rotation, lowerFrame);

			SetDrawData(drillBit.Value, new Vector2(-98, -26), NPC.rotation, drillBit.Frame(1, 2, 0, (int)NPC.localAI[0]));

			// for debugging
			NPCAimedTarget target = NPC.GetTargetData();
			bool targetInvalid = target.Invalid || NPC.localAI[3] != 1;
			spriteBatch.DrawDebugTextAbove(
				$"{NPC.direction} {NPC.spriteDirection}, {NPC.aiAction}, {TimeToSpawnWatchlings}, {TimeToSpawnWatchlings * 0.5f}\n" +
				$"{NPC.ai[0]}, {NPC.ai[1]}, {NPC.ai[2]}, {NPC.ai[3]}\n" +
				$"{NPC.localAI[0]}, {NPC.localAI[1]}, {NPC.localAI[2]}, {NPC.localAI[3]}\n" +
				$"{target.Type}, {targetInvalid}",
				NPC.Top - screenPos);
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(NPC.aiAction);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			NPC.aiAction = reader.ReadInt32();
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[0]);
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[1]);
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[2]);
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[3]);
				for (int i = 0; i < 7; i++) {
					OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic);
				}
			} else if (Main.rand.NextBool(5)) {
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic);
			}
		}
	}
	public class CM_17_Laser : ModProjectile {
		public override string Texture => typeof(Fire_Lasers_State.Trenchmaker_Laser_P).GetDefaultTMLName();
		static readonly AdvancedMiscShaderData hitAOEShader = new(Request<Effect>("Origins/Effects/Radial"), "TrenchmakerLaserHit", [
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
			if (source is EntitySource_Parent { Entity: NPC owner }) {
				Projectile.ai[2] = owner.whoAmI;
			}
		}
		public static int ChargeTime => 65;
		public static int ActiveTime => 55;
		public override void AI() {
			if (Main.npc.GetIfInRange((int)Projectile.ai[2]) is not NPC { active: true } owner || owner.ModNPC is not CM_17 cm17 || owner.aiAction != 2) {
				Projectile.Kill();
				return;
			}
			IsActive = owner.ai[0] == 2;
			Vector2 gunPos = cm17.HeadPos;
			Projectile.localAI[1] = owner.ai[1];
			Projectile.velocity = owner.ai[3].ToRotationVector2();
			Projectile.position = gunPos;
			Vector2 targetPos = Projectile.position + Projectile.velocity * Raymarch(Projectile.position, Projectile.velocity, ProjectileID.Sets.DrawScreenCheckFluff[Type] - 64);
			if (IsActive) {
				SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(2.7f).WithVolume(0.5f), Projectile.Center);
				SoundEngine.SoundPlayer.Play(SoundID.Item72.WithVolume(0.5f), Projectile.Center);
				Dust.NewDust(targetPos - Vector2.One * 2, 4, 4, DustID.AmberBolt);
			}
			Projectile.localAI[2] += 1f / 60;
			TargetPos = targetPos;
			float pitch = owner.ai[1] + 1;
			SoundEngine.SoundPlayer.Play(SoundID.Item158.WithPitch(pitch / 10).WithVolume(0.5f), Projectile.Center);
			SoundEngine.SoundPlayer.Play(Origins.Sounds.RivenBass.WithPitch(pitch / 20).WithVolume(0.5f), Projectile.Center);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (!IsActive) return false;
			if (targetHitbox.IsWithin(TargetPos, 16 * 5)) return true;
			return targetHitbox.Contains(targetHitbox.Center().SnapToLine(Projectile.position, TargetPos, radius: 12));
		}
		public override bool PreDraw(ref Color lightColor) {
			if (!TargetPos.IsWithin(TargetPos.Clamp(Main.screenPosition, Main.screenPosition + Main.ScreenSize.ToVector2()), 64) && !Collision.CheckAABBvLineCollision(Main.screenPosition, Main.ScreenSize.ToVector2(), Projectile.position, TargetPos)) return false;
			using GraphicsExt.SpritebatchOverride _ = Main.spriteBatch.OverrideState(SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap);
			float reduce = IsActive ? 0 : -(Projectile.localAI[1] / ChargeTime);
			hitAOEShader.UseImage1(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]).Apply(null,
				uImageOffset1 with { Value = new Vector2(Projectile.localAI[2], Projectile.localAI[2] * -0.5f) },
				uColorMatrix0 with {
					Value = Matrix.Identity with {
						M14 = reduce,
						M24 = reduce,
						M34 = reduce
					}
				},
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
	public class Watchling : Glowing_Mod_NPC, IWikiNPC, IAshenEnemy, IBroken {
		public Rectangle DrawRect => new(0, 0, 32, 26);
		public int AnimationFrames => 6;
		public static string BrokenReason => "Balance test";
		public List<int> ConnectedWatchlings = [];
		public int OwnerID = -1;
		public int SpawnAnimCounter;
		public static int SpawnAnimCounterMax => 60;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 6;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
			NPCID.Sets.PositiveNPCTypesExcludedFromDeathTally[Type] = true;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.width = 28;
			NPC.height = 28;
			SetSharedDefaults();
		}
		public void SetSharedDefaults() {
			NPC.lifeMax = 81;
			NPC.defense = 10;
			NPC.damage = 33;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			this.CopyBanner<CM_17>();
			SpawnModBiomes = [
				GetInstance<Underground_Ashen_Biome>().Type
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent { Entity: NPC { ModNPC: CM_17, whoAmI: int owner } }) OwnerID = owner;
		}
		public override bool PreAI() {
			if (!NPC.collideY && NPC.velocity.Y == 0) {
				NPC.collideY = Collision.GetTilesIn(NPC.BottomLeft + Vector2.UnitY, NPC.BottomRight + Vector2.UnitY * 16).Any(pos => Framing.GetTileSafely(pos).HasSolidTile());
			}
			if ((SpawnAnimCounter > 0 || NPC.collideY) && SpawnAnimCounter.Warmup(SpawnAnimCounterMax)) NPC.netUpdate = true;
			if (SpawnAnimCounter < SpawnAnimCounterMax) {
				if (NPC.collideY) NPC.velocity.X *= 0.8f;
				return false;
			}
			return base.PreAI();
		}
		public void Transform<TNPC>() where TNPC : Watchling {
			int frame = NPC.frame.Y / NPC.frame.Height;
			double frameCounter = NPC.frameCounter;
			NPC.Transform(NPCType<TNPC>());
			NPC.frame.Y = frame * NPC.frame.Height;
			NPC.frameCounter = frameCounter;
			TNPC watch = (TNPC)NPC.ModNPC;
			watch.OwnerID = OwnerID;
			watch.SpawnAnimCounter = SpawnAnimCounter;
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) NPC.spriteDirection = NPC.direction;

			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			if (NPC.velocity.Y == 0f && NPC.NPCCanStickToWalls()) Transform<Watchling_Wall>();
			SharedAI();
		}
		public static float LaserRange => 8 * 16;
		public void SharedAI() {
			ConnectedWatchlings.Clear();
			for (int i = NPC.whoAmI - 1; i >= 0; i--) {
				NPC target = Main.npc[i];
				if (!target.active) continue;
				if (NPC.Center.WithinRange(target.Center, LaserRange) &&
					target?.ModNPC is Watchling { SpawnAnimCounter: int counter }
					&& counter >= SpawnAnimCounterMax) {
					ConnectedWatchlings.Add(target.whoAmI);
				}
			}
		}
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			for (int i = 0; i < ConnectedWatchlings.Count; i++) {
				if (Collision.CheckAABBvLineCollision(victimHitbox.TopLeft(), victimHitbox.Size(), NPC.Center, Main.npc[ConnectedWatchlings[i]].Center)) {
					npcHitbox = victimHitbox;
					break;
				}
			}
			return true;
		}
		public override void FindFrame(int frameHeight) {
			if (SpawnAnimCounter < SpawnAnimCounterMax && !NPC.IsABestiaryIconDummy) NPC.frame.Y = (SpawnAnimCounter * 3) / SpawnAnimCounterMax * frameHeight;
			else if (NPC.collideY || NPC.IsABestiaryIconDummy) NPC.DoFrames(4, 3..);
			else NPC.DoFrames(1, 4..5);
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(SpawnAnimCounter);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			SpawnAnimCounter = reader.ReadInt32();
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			for (int i = 0; i < ConnectedWatchlings.Count; i++) {
				NPC connected = Main.npc[ConnectedWatchlings[i]];
				if (connected.Center.HasNaNs()) continue;
				using GraphicsExt.SpritebatchOverride _ = Main.spriteBatch.OverrideState(SpriteSortMode.Immediate, samplerState: SamplerState.PointWrap);
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();
				Vector2 diff = connected.Center - NPC.Center;
				Vector2 position = NPC.Center;
				position -= screenPos;
				float rotation = diff.ToRotation();
				float dist = diff.Length();
				const float scale = 1f / 256f;
				DrawData data = new(
					TextureAssets.Extra[ExtrasID.RainbowRodTrailShape].Value,
					position,
					null,
					new Color(255, 40, 0, 0),
					rotation,
					Vector2.UnitY * 128,
					new Vector2(dist * scale, 24 * scale),
					0
				);
				data.Draw(Main.spriteBatch);
				Rectangle frame = new(256 - (int)(Main.timeForVisualEffects * 5 % 256), 0, (int)dist, 256);
				data.scale.X = 1;
				data.scale.Y *= 2;
				data.texture = TextureAssets.Extra[ExtrasID.MagicMissileTrailShape].Value;
				data.position = position;
				data.sourceRect = frame;
				data.Draw(Main.spriteBatch);
				data.position = connected.Center - screenPos;
				data.rotation += MathHelper.Pi;
				data.sourceRect = frame;
				data.Draw(Main.spriteBatch);
			}
			return true;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[0]);
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[1]);
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[2]);
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic[3]);
				for (int i = 0; i < 7; i++) {
					OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic);
				}
			} else if (Main.rand.NextBool(5)) {
				OriginExtensions.SpawnGoreByType(NPC.GetSource_Death(), Main.rand.NextVector2FromRectangle(NPC.Hitbox), NPC.velocity, GoreCache.Ashen_Generic);
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.ByCondition(new Conditions.PlayerNeedsHealing(), ItemID.Heart, 4));
		}
	}
	public class Watchling_Wall : Watchling, ICustomWikiStat {
		bool ICustomWikiStat.CanExportStats => false;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.HideInBestiary;
		}
		public override void SetDefaults() {// could not add stats because 
			NPC.CloneDefaults(NPCID.WallCreeperWall);
			NPC.aiStyle = NPCAIStyleID.Spider;
			NPC.width = 28;
			NPC.height = 28;
			SetSharedDefaults();
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) { }
		public override bool PreAI() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return true;
			if (!NPC.NPCCanStickToWalls()) Transform<Watchling>();
			return true;
		}
		public override void AI() {
			NPC.rotation += MathHelper.Pi;
			SharedAI();
			Vector2 targetDir = NPC.DirectionTo(Main.player[NPC.target].Center);
			const float min_dist = 16 * 5;
			Vector2 totalSeparation = default;
			foreach (NPC other in Main.ActiveNPCs) {
				if (other.type != Type) continue;
				if (other == NPC) continue;
				Vector2 diff = NPC.Center - other.Center;
				if (diff.LengthSquared() >= min_dist * min_dist) continue;
				diff -= targetDir * Vector2.Dot(targetDir, diff);
				float distSq = diff.LengthSquared();
				if (distSq == 0) continue;
				totalSeparation += diff / (distSq / min_dist);
			}
			if (totalSeparation != default && !totalSeparation.HasNaNs()) {
				float speed = NPC.velocity.Length();
				NPC.velocity += totalSeparation.Normalized(out _) * 0.25f;
				if (NPC.velocity != default) NPC.velocity = NPC.velocity.Normalized(out _) * speed;
			}
		}
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(4, 3..);
		}
	}
}
