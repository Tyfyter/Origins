using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Graphics;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Weapons.Magic;
using Origins.World.BiomeData;
using PegasusLib;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Defiled {
	public class Defiled_Broadcaster : Glowing_Mod_NPC, IDefiledEnemy, ITangelaHaver {
		public AssimilationAmount? Assimilation => 0.01f;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 5;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new(-5, -5),
				PortraitPositionYOverride = -10,
				Velocity = 1f
			};
			ModContent.GetInstance<Defiled_Wastelands.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.aiStyle = -1;
			NPC.lifeMax = 48;
			NPC.damage = 12;
			NPC.defense = 0;
			NPC.width = 40;
			NPC.height = 48;
			NPC.hide = true;
			NPC.HitSound = Origins.Sounds.DefiledHurt;
			NPC.DeathSound = Origins.Sounds.DefiledKill;
			NPC.knockBackResist = 0.75f;
			NPC.value = 76;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			SpawnModBiomes = [
				ModContent.GetInstance<Defiled_Wastelands>().Type
			];
			this.CopyBanner<Defiled_Banner_NPC>();
		}
		public int MaxMana => 35;
		public int MaxManaDrain => 5;
		public float Mana { get; set; }
		public Vector2 CarryPosition => NPC.Center + Vector2.UnitY * 12;
		public void Regenerate(out int lifeRegen) {
			int factor = 64 / Math.Max(NPC.life / 6, 1);
			lifeRegen = factor;
			Mana -= factor / 240f;
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!NPC.downedBoss1 && !NPC.downedBoss2 && !NPC.downedBoss3) return 0;
			if (spawnInfo.Player.ZoneSkyHeight) return 0;
			return Defiled_Wastelands.SpawnRates.FlyingEnemyRate(spawnInfo) * Defiled_Wastelands.SpawnRates.Broadcaster;
		}
		public override int SpawnNPC(int tileX, int tileY) {
			tileY = OriginGlobalNPC.GetAerialSpawnPosition(tileX, tileY, this);
			if (tileY == -1) return Main.maxNPCs;
			return NPC.NewNPC(null, tileX * 16 + 8, tileY * 16, NPC.type);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags([
				this.GetBestiaryFlavorText()
			]);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_String>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Latchkey>(), 6, 3, 6));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Krunch_Mix>(), 19));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Nerve_Flan>(), 44));
		}
		public override void AI() {
			if (Main.rand.NextBool(900)) SoundEngine.PlaySound(Origins.Sounds.DefiledIdle.WithPitchRange(1f, 1.1f), NPC.Center);
			//TargetSearchFlag searchFlag
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.Players);
			if (searchResults.FoundTarget) {
				NPC.target = searchResults.NearestTargetIndex;
				NPC.targetRect = searchResults.NearestTargetHitbox;
				if (NPC.ShouldFaceTarget(ref searchResults)) {
					NPC.FaceTarget();
				}
			}
			if (NPC.ai[2] == 0) {
				NPC.DoFrames(7, 0..4);
				if (NPC.ai[0] >= 180) {
					NPCAimedTarget playerTarget = NPC.GetTargetData();
					searchResults = SearchForTarget(NPC, TargetSearchFlag.NPCs, null, (other) => {
						if (!other.noGravity && other.ModNPC is IDefiledEnemy) {
							//return true;
							return NPC.ai[1] == 0 ?
								other.position.Y > playerTarget.Position.Y + playerTarget.Height + 16 * 7 :
								NPC.ai[1] == other.WhoAmIToTargettingIndex;
						}
						return false;
					});
					if (searchResults.FoundTarget) {
						NPC.ai[1] = searchResults.NearestTargetIndex;
						NPC.target = searchResults.NearestTargetIndex;
						NPC.targetRect = searchResults.NearestTargetHitbox;
						if (NPC.ShouldFaceTarget(ref searchResults)) {
							NPC.FaceTarget();
						}
					} else {
						NPC.ai[0] = 0;
						NPC.ai[1] = 0;
						if (!playerTarget.Invalid) {
							Vector2 spawnPos = (NPC.Center + new Vector2(0 * NPC.direction, 16));
							Projectile.NewProjectile(
								NPC.GetSource_FromAI(),
								spawnPos,
								((playerTarget.Center + playerTarget.Velocity * 15) - spawnPos).SafeNormalize(default) * Nerve_Flan_P.tick_motion,
								ModContent.ProjectileType<Defiled_Broadcaster_Flan>(),
								30,
								1
							);
						}
					}
				} else {
					NPC.ai[0]++;
					NPC.ai[1] = 0;
				}
				NPCAimedTarget target = NPC.GetTargetData();
				if (target.Type == NPCTargetType.NPC) {
					if (NPC.ai[2] == 0) {
						Vector2 direction = (target.Position + Vector2.UnitX * target.Width * 0.5f) - CarryPosition;
						if (direction.IsWithin(Vector2.Zero, 8)) {
							NPC.ai[2] = 1;
							NPC.ai[0] = 0;
							NPC.velocity = Collision.AnyCollision(target.Position, NPC.velocity, target.Width, target.Height);
						} else {
							NPC.velocity += direction.SafeNormalize(default) * 0.5f;
						}
					}
				} else if (target.Type != NPCTargetType.None) {
					Vector2 direction = target.Center - NPC.Center;
					float distSQ = direction.LengthSquared();
					if (distSQ > 16 * 16 * 20 * 20) {
						NPC.velocity += direction.SafeNormalize(default) * 0.5f;
					} else if (distSQ < 16 * 16 * 9 * 9) {
						NPC.velocity -= direction.SafeNormalize(default) * 0.5f;
					}
				} else {

				}
			} else if (Main.npc.IndexInRange((int)NPC.ai[1] - 300)) { 
				NPC carriedNPC = Main.npc[(int)NPC.ai[1] - 300];
				if (!carriedNPC.TryGetGlobalNPC(out DefiledGlobalNPC global)) {
					NPC.ai[2] = 0;
					return;
				}
				NPCAimedTarget playerTarget = NPC.GetTargetData();
				Vector2 targetPos = playerTarget.Position + Vector2.UnitX * playerTarget.Width * 0.5f - Vector2.UnitY * 16 * 15;
				carriedNPC.Top = CarryPosition;
				NPC.ai[0]++;
				if (NPC.ai[2] == 1) {
					if (NPC.ai[0] > 60 && AI_AttemptToFindTeleportSpot(ref targetPos)) {
						Vector2 diff = targetPos - NPC.Center;
						float dist = diff.Length();
						Projectile.NewProjectile(
							NPC.GetSource_FromAI(),
							NPC.Center,
							(diff / dist).SafeNormalize(default) * Nerve_Flan_P.tick_motion,
							ModContent.ProjectileType<Defiled_Broadcaster_Teleport_Flan>(),
							10,
							1,
							ai1: dist / Nerve_Flan_P.tick_motion
						);
						NPC.Teleport(targetPos - Vector2.UnitY * (12), -1);
						NPC.ai[2] = 2;
					} else {
						NPC.velocity += (targetPos - NPC.Center).SafeNormalize(default) * 0.5f;
					}
				} else if (NPC.ai[0] < 180) {
					float diff = playerTarget.Center.X - NPC.Center.X;
					int dir = Math.Sign(diff);
					Vector2 direction = Vector2.Zero;
					if ((diff * dir) > 32) {
						direction.X = dir;
					}
					if (targetPos.Y < NPC.Center.Y) {
						direction.Y = -1;
					}
					NPC.velocity += direction.SafeNormalize(default) * 0.5f;
				} else {
					NPC.ai[2] = 0;
					NPC.ai[0] = 0;
				}
				NPC.velocity = Collision.AnyCollision(carriedNPC.position, NPC.velocity, carriedNPC.width, carriedNPC.height);
				global.broadcasterHoldingThisNPC = NPC;
				carriedNPC.velocity = NPC.velocity;
			}
			NPC.velocity *= 0.9f;
		}
		public bool AI_AttemptToFindTeleportSpot(ref Vector2 pos) {
			const float rangeFromTargetTile = 10;
			NPC carriedNPC = Main.npc[(int)NPC.ai[1] - 300];
			Rectangle carriedRect = carriedNPC.Hitbox;
			bool posIsValid(Vector2 pos) {
				carriedRect.X = (int)(pos.X - carriedRect.Width * 0.5f);
				carriedRect.Y = (int)pos.Y;
				return !carriedRect.OverlapsAnyTiles();
			}
			if (posIsValid(pos)) {
				return true;
			}
			int tries = 0;
			while (++tries < 100) {
				Vector2 testPos = pos + new Vector2(Main.rand.NextBool().ToDirectionInt() * Main.rand.NextFloat(rangeFromTargetTile * 16), Main.rand.NextBool().ToDirectionInt() * Main.rand.NextFloat(rangeFromTargetTile * 16));
				if (posIsValid(testPos)) {
					pos = testPos;
					return true;
				}
			}
			return false;
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void FindFrame(int frameHeight) {
			if (NPC.ai[2] == 0 && NPC.ai[0] < 120) {
				NPC.frame.Y = NPC.frame.Height * 4;
			}
		}
		public override void DrawBehind(int index) {
			Main.instance.DrawCacheNPCProjectiles.Add(index);
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			Rectangle spawnbox = projectile.Hitbox.MoveToWithin(NPC.Hitbox);
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), Main.rand.NextVectorIn(spawnbox), projectile.velocity, "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			int halfWidth = NPC.width / 2;
			int baseX = player.direction > 0 ? 0 : halfWidth;
			for (int i = Main.rand.Next(3); i-- > 0;) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(baseX + Main.rand.Next(halfWidth), Main.rand.Next(NPC.height)), hit.GetKnockbackFromHit(yMult: -0.5f), "Gores/NPCs/DF_Effect_Small" + Main.rand.Next(1, 4));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 2; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF3_Gore");
				for (int i = 0; i < 6; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/DF_Effect_Medium" + Main.rand.Next(1, 4));
			}
		}
		public int? TangelaSeed { get; set; }
		public AutoLoadingAsset<Texture2D> tangelaTexture = typeof(Defiled_Broadcaster).GetDefaultTMLName() + "_Tangela";
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (NPC.direction == 1) {
				spriteEffects |= SpriteEffects.FlipHorizontally;
			}
			Vector2 halfSize = new(GlowTexture.Width * 0.5f, (GlowTexture.Height / Main.npcFrameCount[NPC.type]) * 0.5f);
			Vector2 position = NPC.Center + Vector2.UnitY * NPC.gfxOffY - screenPos;
			Vector2 origin = new(halfSize.X * 0.75f, halfSize.Y * 0.9f);
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
				position,
				NPC.frame,
				drawColor,
				NPC.rotation,
				origin,
				NPC.scale,
				spriteEffects,
			0);
			spriteBatch.Draw(
				GlowTexture,
				position,
				NPC.frame,
				Color.White,
				NPC.rotation,
				origin,
				NPC.scale,
				spriteEffects,
			0);

			TangelaVisual.DrawTangela(
				this,
				tangelaTexture,
				position,
				NPC.frame,
				NPC.rotation,
				origin,
				new(NPC.scale),
				spriteEffects
			);
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }
	}
	public class Defiled_Broadcaster_Flan : Nerve_Flan_P {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
		}
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.extraUpdates = 3;
			startupDelay = 0;
			Projectile.localAI[1] = float.PositiveInfinity;
		}
		public override void AI() {
			SoundEngine.PlaySound(Origins.Sounds.defiledKillAF.WithPitchRange(-1f, -0.2f).WithVolume(0.1f), Projectile.Center);
			SoundEngine.PlaySound(SoundID.Item60.WithPitchRange(-1f, -0.2f).WithVolume(0.3f), Projectile.Center);
			target ??= Projectile.Center + Projectile.velocity * 25 * (10 - Projectile.ai[2]);
			if (++Projectile.localAI[0] > 25) {
				Projectile.localAI[0] = 0;
				if (++Projectile.ai[2] >= 5) {
					Projectile.Kill();
					return;
				}
			}
			if (Projectile.numUpdates == -1) Projectile.extraUpdates = Projectile.extraUpdates == 3 ? 2 : 3;
			if (Projectile.ai[0] != 1) {
				if ((int)Projectile.localAI[0] % 5 == 0 && startupDelay <= 0) {
					float speed = Projectile.velocity.Length();
					if (speed != 0) Projectile.velocity = (target.Value - Projectile.Center).SafeNormalize(Projectile.velocity / speed).RotatedByRandom(randomArcing) * speed;
				}
				if (startupDelay > 0) {
					SoundEngine.PlaySound(Origins.Sounds.defiledKillAF.WithPitchRange(-1f, -0.2f).WithVolume(0.5f), Projectile.Center);
					SoundEngine.PlaySound(SoundID.Item60.WithPitchRange(-1f, -0.2f), Projectile.Center);
					startupDelay--;
				} else {
					if (++Projectile.ai[1] > ProjectileID.Sets.TrailCacheLength[Type] || Projectile.ai[1] > Projectile.localAI[1]) {
						StopMovement();
						Projectile.extraUpdates = 3;
					} else {
						int index = (int)Projectile.ai[1];
						Projectile.oldPos[^index] = Projectile.Center;
						Projectile.oldRot[^index] = Projectile.velocity.ToRotation();
					}
				}
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(Rasterized_Debuff.ID, (int)(30 * (1 + ContentExtensions.DifficultyDamageMultiplier)));
		}
	}
	public class Defiled_Broadcaster_Teleport_Flan : Defiled_Broadcaster_Flan {
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.tileCollide = false;
		}
		public override void AI() {
			base.AI();
			if (Projectile.ai[0] != 1) Projectile.extraUpdates = 25;
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.localAI[1] = Projectile.ai[1];
			Projectile.ai[1] = 0;
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(Projectile.localAI[1]);
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			Projectile.localAI[1] = reader.ReadSingle();
		}
	}
}
