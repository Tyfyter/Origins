using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Buffs;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Armor.Defiled;
using Origins.Items.Armor.Felnum;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Summoner;
using Origins.Tiles.Other;
using PegasusLib;
using ReLogic.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.Utilities.NPCUtils;

namespace Origins.NPCs.Felnum {
	public class Felnum_Guardian : ModNPC, IWikiNPC {
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Felnum_Guardian).GetDefaultTMLName() + "_Glow";
		public override void Load() => this.AddBanner();
		public static HashSet<int> FriendlyNPCTypes { get; private set; } = [];
		public Rectangle DrawRect => new(-14, 4, 72, 70);
		public int AnimationFrames => 16;
		public int FrameDuration => 1;
		public override void SetStaticDefaults() {
			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.FairyCritterBlue;
			Main.npcFrameCount[NPC.type] = 4;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			FriendlyNPCTypes.Add(Type);
			FriendlyNPCTypes.Add(ModContent.NPCType<Felnum_Ore_Slime>());
			FriendlyNPCTypes.Add(ModContent.NPCType<Felnum_Einheri>());
			FriendlyNPCTypes.Add(ModContent.NPCType<Cloud_Elemental>());
			NPCID.Sets.SpecificDebuffImmunity[Type][Static_Shock_Debuff.ID] = true;
		}
		public override void Unload() => FriendlyNPCTypes = null;
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 120;
			NPC.defense = 14;
			NPC.damage = 15;
			NPC.width = 44;
			NPC.height = 40;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.noGravity = true;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			if (spawnInfo.Player.ZoneSkyHeight && NPC.downedBoss3) return 0.4f;
			return 0;
		}
		public override void FindFrame(int frameHeight) {
			NPC.DoFrames(4);
		}
		public override bool? CanFallThroughPlatforms() => true;
		SlotId soundSlot;
		public static bool ShouldChaseNPC(NPC npc, Vector2 center, ref NPC dummyTarget) {
			if (OriginsSets.NPCs.TargetDummies[npc.type]) {
				if (npc.DistanceSQ(center) < (dummyTarget?.DistanceSQ(center) ?? float.PositiveInfinity)) dummyTarget = npc;
				return false;
			}
			if (!FriendlyNPCTypes.Contains(npc.type) && npc.chaseable) return true;
			return false;
		}
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => NPC.playerInteraction[target.whoAmI] || !target.OriginPlayer().felnumEnemiesFriendly;
		public override void AI() {
			NPC dummyTarget = null;
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.All,
				player => NPC.playerInteraction[player.whoAmI] || !player.OriginPlayer().felnumEnemiesFriendly,
				npc => ShouldChaseNPC(npc, NPC.Center, ref dummyTarget)
			);
			NPC.target = searchResults.NearestTargetIndex;
			if (searchResults.FoundTarget) {
				if (searchResults.NearestTargetHitbox.Center().IsWithin(NPC.Center, 16 * 100)) {
					NPC.targetRect = searchResults.NearestTargetHitbox;
					if (NPC.ShouldFaceTarget(ref searchResults)) {
						NPC.FaceTarget();
					}
				} else {
					NPC.target = -1;
				}
			} else if (dummyTarget is not null) {
				NPC.target = dummyTarget.WhoAmIToTargettingIndex;
				NPC.targetRect = dummyTarget.Hitbox;
			}
			if (NPC.HasValidTarget) {
				NPCAimedTarget target = NPC.GetTargetData();
				float speed = 12f;
				float inertia = 32f;
				const int charge_time = 9 * 3;
				Vector2 vectorToTargetPosition = target.Center - NPC.Center;
				if (NPC.confused) vectorToTargetPosition *= -1;
				NPC.spriteDirection = Math.Sign(vectorToTargetPosition.X);
				float dist = vectorToTargetPosition.Length();
				vectorToTargetPosition /= dist;
				const float hover_range = 16 * 13;
				if (dist < hover_range - 32) {
					speed *= -1;
				} else if (dist < hover_range + 32) {
					speed = 0;
					NPC.velocity += (dist - hover_range) * vectorToTargetPosition * 0.01f * -Vector2.Dot(NPC.velocity.SafeNormalize(default), vectorToTargetPosition);
					if (NPC.velocity.LengthSquared() < 0.05f) {
						// If there is a case where it's not moving at all, give it a little "poke"
						NPC.velocity += vectorToTargetPosition.RotatedBy(Main.rand.NextBool() ? -MathHelper.PiOver2 : MathHelper.PiOver2) * 2;
					}
				}
				if (NPC.ai[0] != 120 - charge_time || CollisionExt.CanHitRay(NPC.Center, target.Center)) {
					NPC.ai[0]++;
				}
				if (NPC.ai[0] > 120 - charge_time) {
					float chargeFactor = (120 - NPC.ai[0]) / charge_time;
					GeometryUtils.AngularSmoothing(ref NPC.rotation, vectorToTargetPosition.ToRotation(), (chargeFactor + 0.05f) * 0.1f);
					speed *= chargeFactor;
					NPC.velocity *= (chargeFactor + 149) / 150;
					float diameter = NPC.width * 0.75f;
					Vector2 offset = Main.rand.NextVector2CircularEdge(diameter, diameter) * Main.rand.NextFloat(0.9f, 1f);
					Dust dust = Dust.NewDustPerfect(
						NPC.Center - offset,
						DustID.Electric,
						offset * 0.125f
					);
					dust.scale -= chargeFactor;
					dust.velocity += NPC.velocity;
					dust.noGravity = true;
					if (SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound sound)) {
						sound.Pitch = 1 - chargeFactor * 1.5f;
						sound.Position = NPC.Center;
					} else {
						soundSlot = SoundEngine.PlaySound(Origins.Sounds.LightningCharging, NPC.Center, soundInstance => NPC.active && NPC.life > 0);
					}
					if (NPC.ai[0] > 120) {
						NPC.ai[0] = 0;
						if (Main.netMode != NetmodeID.MultiplayerClient) {
							Projectile.NewProjectile(
								NPC.GetSource_FromAI(),
								NPC.Center,
								GeometryUtils.Vec2FromPolar(8, NPC.rotation),
								ModContent.ProjectileType<Felnum_Guardian_P>(),
								(int)(40 * ContentExtensions.DifficultyDamageMultiplier),
								4
							);
						}
						SoundEngine.PlaySound(Main.rand.Next(Origins.Sounds.LightningSounds), NPC.Center);
					}
				} else {
					NPC.rotation = vectorToTargetPosition.ToRotation();
					if (SoundEngine.TryGetActiveSound(soundSlot, out ActiveSound sound)) sound.Stop();
				}
				vectorToTargetPosition *= speed;
				NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			} else {
				NPC.velocity *= 0.94f;
				NPC.velocity.X += (NPC.velocity.X < 0 ? -1 : 1) * 0.25f;
				NPC.rotation = NPC.velocity.ToRotation();
				NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			}
			Vector2 nextVel = Collision.TileCollision(NPC.position, NPC.velocity, NPC.width, NPC.height, true, true);
			if (nextVel.X != NPC.velocity.X) NPC.velocity.X *= -0.7f;
			if (nextVel.Y != NPC.velocity.Y) NPC.velocity.Y *= -0.7f;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Sky
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Felnum_Ore_Item>(), 1, 7, 14));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Guardian_Rod>(), 40));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ancient_Felnum_Helmet>(), 66));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ancient_Felnum_Breastplate>(), 66));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ancient_Felnum_Greaves>(), 66));
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Vector2 origin = new(22, 39);
			SpriteEffects spriteEffects = SpriteEffects.None;
			float rotation = NPC.rotation;
			if (NPC.IsABestiaryIconDummy) NPC.rotation = NPC.velocity.ToRotation();
			if (NPC.spriteDirection != -1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
				origin.X = texture.Width - origin.X;
			} else {
				rotation += MathHelper.Pi;
			}
			spriteBatch.Draw(
				texture,
				NPC.Center - screenPos,
				NPC.frame,
				drawColor,
				rotation,
				origin,
				NPC.scale,
				spriteEffects,
			0);
			spriteBatch.Draw(
				glowTexture,
				NPC.Center - screenPos,
				NPC.frame,
				Color.White * (drawColor.A / 255f),
				rotation,
				origin,
				NPC.scale,
				spriteEffects,
			0);
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				SoundEngine.PlaySound(SoundID.NPCHit37, NPC.Center);
				Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Felnum_Enemy_Death_Dust>());
			} else {
				SoundEngine.PlaySound(SoundID.NPCHit34, NPC.Center);
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			Static_Shock_Debuff.Inflict(target, 60);
		}
	}
	public class Felnum_Guardian_P : Magnus_P {
		HashSet<int> hostilePlayers = [];
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.friendly = true;
			Projectile.hostile = true;
			startupDelay = 0;
		}
		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_Parent parentSource && parentSource.Entity is NPC npc) {
				foreach (Player player in Main.ActivePlayers) {
					if (npc.playerInteraction[player.whoAmI] && player.OriginPlayer().felnumEnemiesFriendly) hostilePlayers.Add(player.whoAmI);
				}
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Felnum_Guardian.FriendlyNPCTypes.Contains(target.type)) return false;
			return null;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			Static_Shock_Debuff.Inflict(target, Main.rand.Next(120, 210));
			int index = Math.Min((int)++Projectile.ai[1], Projectile.oldPos.Length);
			Projectile.oldPos[^index] = Projectile.Center + Projectile.velocity;
			Projectile.oldRot[^index] = Projectile.velocity.ToRotation();
			StopMovement();
		}
		public override bool CanHitPlayer(Player target) {
			if (hostilePlayers.Contains(target.whoAmI)) return true;
			return !target.OriginPlayer().felnumEnemiesFriendly;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.SourceDamage *= 0.5f;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Static_Shock_Debuff.Inflict(target, Main.rand.Next(240, 300));
		}
		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)hostilePlayers.Count);
			foreach (int player in hostilePlayers) {
				writer.Write((byte)player);
			}
		}
		public override void ReceiveExtraAI(BinaryReader reader) {
			for (int i = reader.ReadByte() - 1; i >= 0; i--) {
				hostilePlayers.Add(reader.ReadByte());
			}
		}
	}
}
