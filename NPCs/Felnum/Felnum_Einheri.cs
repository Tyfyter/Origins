using Microsoft.Xna.Framework.Graphics;
using Origins;
using Origins.Buffs;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Accessories;
using Origins.Items.Armor.Felnum;
using Origins.Reflection;
using Origins.Tiles.Other;
using PegasusLib;
using System;
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
	public class Felnum_Einheri : ModNPC, IWikiNPC {
		AutoLoadingAsset<Texture2D> glowTexture = typeof(Felnum_Einheri).GetDefaultTMLName() + "_Glow";
		AutoLoadingAsset<Texture2D> glowOverTexture = typeof(Felnum_Einheri).GetDefaultTMLName() + "_Glow_Over";
		AutoLoadingAsset<Texture2D> attackTexture = typeof(Felnum_Einheri).GetDefaultTMLName() + "_Attac";
		AutoLoadingAsset<Texture2D> spearTexture = typeof(Felnum_Einheri).GetDefaultTMLName() + "_Spear";
		AutoLoadingAsset<Texture2D> spearGlowTexture = typeof(Felnum_Einheri).GetDefaultTMLName() + "_Spear_Glow";
		public Rectangle DrawRect => new(14, -10, 82, 96);
		public int AnimationFrames => 48;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCID.UndeadViking;
			Main.npcFrameCount[NPC.type] = 6;
			NPCID.Sets.UsesNewTargetting[Type] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][Static_Shock_Debuff.ID] = true;
		}
		public override void SetDefaults() {
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.lifeMax = 210;
			NPC.defense = 14;
			NPC.damage = 18;
			NPC.width = 44;
			NPC.height = 40;
			NPC.HitSound = SoundID.NPCHit4;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.noGravity = true;
		}
		public bool GetAttackTime(out int time, out int frame, out bool finished, Vector2? attackDirection = null) {
			const int charge_time_per_frame = 4;
			const int charge_time = charge_time_per_frame * 4;
			const int delay_time = 30;
			time = (int)NPC.ai[0] - delay_time;
			frame = time / charge_time_per_frame - 1;
			if (time == 1 && attackDirection.HasValue) {
				NPC.ai[1] = attackDirection.Value.ToRotation();
			}
			finished = time > charge_time;
			return time > 0;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			if (spawnInfo.Player.ZoneSkyHeight && NPC.downedBoss3) return 0.4f;
			return 0;
		}
		public override void FindFrame(int frameHeight) {
			if (GetAttackTime(out _, out _, out _)) {
				NPC.DoFrames(5, 3..6);
			} else {
				NPC.DoFrames(5, 0..3);
			}
		}
		public override bool CanHitNPC(NPC target) => !Felnum_Guardian.FriendlyNPCTypes.Contains(target.type);
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => NPC.playerInteraction[target.whoAmI] || !target.OriginPlayer().felnumEnemiesFriendly;
		public override void PostAI() {
			int specialHitSetter = 1;
			float damageMultiplier = 1f;
			Rectangle baseHitbox = NPC.Hitbox;
			foreach (NPC other in Main.ActiveNPCs) {
				if (other == NPC) continue;
				if (other.active && other.immune[255] == 0 && !(other.dontTakeDamage || other.dontTakeDamageFromHostiles || other.immortal)) {
					Rectangle hurtbox = other.Hitbox;
					Rectangle hitbox = baseHitbox;
					NPC.GetMeleeCollisionData(hurtbox, NPC.whoAmI, ref specialHitSetter, ref damageMultiplier, ref hitbox);
					if (NPCLoader.CanHitNPC(NPC, other) && hitbox.Intersects(hurtbox)) {
						int damage = NPC.damage;
						try {
							NPC.damage = (int)(damage * damageMultiplier);
							NPCMethods.BeHurtByOtherNPC(other, NPC);
							other.immune[255] = 15;
						} finally {
							NPC.damage = damage;
						}
						break;
					}
				}
			}
		}
		public override bool? CanFallThroughPlatforms() => true;
		public override void AI() {
			NPC dummyTarget = null;
			TargetSearchResults searchResults = SearchForTarget(NPC, TargetSearchFlag.All,
				player => NPC.playerInteraction[player.whoAmI] || !player.OriginPlayer().felnumEnemiesFriendly,
				npc => Felnum_Guardian.ShouldChaseNPC(npc, NPC.Center, ref dummyTarget)
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
				float speed = 6f;
				float inertia = 16f;
				Vector2 vectorToTargetPosition = target.Center - NPC.Center;
				if (NPC.confused) vectorToTargetPosition *= -1;
				NPC.spriteDirection = Math.Sign(vectorToTargetPosition.X);
				float dist = vectorToTargetPosition.Length();
				vectorToTargetPosition /= dist;
				const float hover_range = 16 * 6;
				bool isInAttack = GetAttackTime(out int atkTime, out _, out bool finishAtk, vectorToTargetPosition);
				if (dist < hover_range - 8) {
					speed *= -2;
					NPC.ai[0]++;
				} else if (dist < hover_range + 32) {
					//speed = 0.005f;
					NPC.velocity += (dist - hover_range) * vectorToTargetPosition * 0.01f * -Vector2.Dot(NPC.velocity.SafeNormalize(default), vectorToTargetPosition);
					NPC.ai[0]++;
				} else if (isInAttack) {
					NPC.ai[0]++;
				} else if (NPC.ai[0] > 0) {
					NPC.ai[0] = 0;
				}
				NPC.velocity.Y += 0.02f;
				if (vectorToTargetPosition.Y < 0) {
					NPC.velocity.Y -= 0.04f;
				}
				if (vectorToTargetPosition.Y > 0.7f) {
					NPC.velocity.X -= Math.Sign(vectorToTargetPosition.X) * 0.3f;
				}

				if (isInAttack) {
					NPC.spriteDirection = NPC.direction = Math.Sign(Math.Cos(NPC.ai[1]));
					if (finishAtk) {
						NPC.ai[0] = 0;
					}
				} else {
					NPC.spriteDirection = Math.Sign(vectorToTargetPosition.X);
				}
				vectorToTargetPosition *= speed;
				NPC.velocity = (NPC.velocity * (inertia - 1) + vectorToTargetPosition) / inertia;
			} else if (GetAttackTime(out _, out _, out bool finishAtk)) {
				NPC.ai[0]++;
				if (finishAtk) {
					NPC.ai[0] = 0;
				}
			} else {
				NPC.velocity *= 0.94f;
				NPC.velocity.X += (NPC.velocity.X < 0 ? -1 : 1) * 0.15f;
				NPC.spriteDirection = Math.Sign(NPC.velocity.X);
			}
			NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.04f, -0.1f, 0.1f);
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
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Lightning_Ring>(), 20));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ancient_Felnum_Helmet>(), 66));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ancient_Felnum_Breastplate>(), 66));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ancient_Felnum_Greaves>(), 66));
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			Vector2 flip = new(-NPC.spriteDirection, 1);
			Vector2 origin = new(25, 36);
			Vector2 sporigin = new(71, 7);
			Vector2 atkOrigin = new(91, 43);
			Vector2 atkPosition = new(18, 9);
			Vector2 spearPosition = atkPosition * flip;
			float actualSpearRotOffset = 0;
			float spearRotation = NPC.rotation;
			if (GetAttackTime(out _, out int atkFrame, out _)) {
				spearRotation = NPC.ai[1];
				if (NPC.spriteDirection == -1) spearRotation += MathHelper.Pi;
				Vector2 spearOffset = default;
				switch (atkFrame) {
					case 0:
					spearOffset = new(-7, -4);
					break;
					case 1:
					spearOffset = new(-60, -8);
					break;
					case 2:
					spearOffset = new(-87, -11);
					break;
					case 3:
					spearOffset = new(-81, -9);
					break;
				}
				spearPosition += (spearOffset * flip).RotatedBy(spearRotation);
				actualSpearRotOffset = -0.576f * NPC.spriteDirection;
			}
			SpriteEffects spriteEffects = SpriteEffects.None;
			float rotation = NPC.rotation;
			if (NPC.spriteDirection != -1) {
				spriteEffects = SpriteEffects.FlipHorizontally;
				origin.X = texture.Width - origin.X;
				sporigin.X = spearTexture.Value.Width - sporigin.X;
				atkOrigin.X = attackTexture.Value.Width - atkOrigin.X;
				atkPosition.X *= -1;
			}
			spriteBatch.Draw(
				TextureAssets.Npc[Type].Value,
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

			spriteBatch.Draw(
				spearTexture,
				(NPC.Center + spearPosition) - screenPos,
				null,
				drawColor,
				spearRotation + actualSpearRotOffset,
				sporigin,
				NPC.scale,
				spriteEffects,
			0);
			spriteBatch.Draw(
				spearGlowTexture,
				(NPC.Center + spearPosition) - screenPos,
				null,
				Color.White * (drawColor.A / 255f),
				spearRotation + actualSpearRotOffset,
				sporigin,
				NPC.scale,
				spriteEffects,
			0);
			spriteBatch.Draw(
				glowOverTexture,
				NPC.Center - screenPos,
				NPC.frame,
				Color.White * (drawColor.A / 255f),
				rotation,
				origin,
				NPC.scale,
				spriteEffects,
			0);
			if (atkFrame >= -1) {
				spriteBatch.Draw(
					attackTexture,
					(NPC.Center + atkPosition) - screenPos,
					attackTexture.Frame(verticalFrames: 4, frameY: Math.Max(atkFrame, 0)),
					Color.White * (drawColor.A / 255f),
					spearRotation,
					atkOrigin,
					NPC.scale,
					spriteEffects,
				0);
			}
			return false;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				SoundEngine.PlaySound(SoundID.NPCHit37, NPC.Center);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-11 * NPC.direction, -20).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Felnum_Einheri_Gore_1")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(-2 * NPC.direction, 9).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Felnum_Einheri_Gore_3")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(7 * NPC.direction, 23).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Felnum_Einheri_Gore_2")
				);
				Gore.NewGore(
					NPC.GetSource_Death(),
					NPC.Center + new Vector2(7 * NPC.direction, -5).RotatedBy(NPC.rotation),
					NPC.velocity,
					Mod.GetGoreSlot("Gores/NPCs/Felnum_Einheri_Gore_4")
				);
				Dust.NewDustPerfect(NPC.Center, ModContent.DustType<Felnum_Enemy_Death_Dust>());
			} else {
				SoundEngine.PlaySound(SoundID.NPCHit34, NPC.Center);
			}
		}
		bool isSpearHit = false;
		public override bool ModifyCollisionData(Rectangle victimHitbox, ref int immunityCooldownSlot, ref MultipliableFloat damageMultiplier, ref Rectangle npcHitbox) {
			isSpearHit = false;
			if (victimHitbox.Intersects(npcHitbox)) return true;
			if (GetAttackTime(out _, out int atkFrame, out _)) {
				Vector2 flip = new(-1, NPC.direction);
				Rectangle rect = new(0, 0, 32, 32);
				Vector2 atkPosition = new(18, 9);
				Vector2 spearPosition = atkPosition * new Vector2(-NPC.direction, 1);
				Vector2 spearOffset = default;
				switch (atkFrame) {
					case 0:
					spearOffset = new(-7, -4);
					break;
					case 1:
					spearOffset = new(-60, -8);
					break;
					case 2:
					spearOffset = new(-87, -11);
					break;
					case 3:
					spearOffset = new(-81, -9);
					break;
					default:
					return true;
				}
				spearPosition += (spearOffset * flip).RotatedBy(NPC.ai[1]) + NPC.Center;
				spearPosition += new Vector2(54, 32).RotatedBy(NPC.ai[1] - 0.576f);
				rect.X = (int)(spearPosition.X - rect.Width * 0.5f);
				rect.Y = (int)(spearPosition.Y - rect.Height * 0.5f);
				if (rect.Intersects(victimHitbox)) {
					damageMultiplier *= 3;
					immunityCooldownSlot = ImmunityCooldownID.DD2OgreKnockback;
					isSpearHit = true;
					npcHitbox = rect;
					return true;
				}
			}
			return true;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			Static_Shock_Debuff.Inflict(target, isSpearHit ? 360 : 120);
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit) {
			Static_Shock_Debuff.Inflict(target, isSpearHit ? 360 : 120);
		}
	}
}
