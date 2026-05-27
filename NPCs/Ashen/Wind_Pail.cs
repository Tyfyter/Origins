using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Events;
using Origins.Items.Armor.Ashen;
using Origins.Items.Other.Consumables.Food;
using Origins.LootConditions;
using ReLogic.Utilities;
using System.Numerics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.Ashen {
	public class Wind_Pail : Glowing_Mod_NPC, IAshenEnemy {
		public bool IsOpaqueEnoughToAttack => NPC.Opacity > 0.5f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 1;
			NPCID.Sets.DoesntDespawnToInactivityAndCountsNPCSlots[Type] = true;
			GetInstance<Smog_Storm.SpawnRates>().AddSpawn(Type, BiomeSpawnChance);
			NPCID.Sets.SpecificDebuffImmunity[Type][Slow_Debuff.ID] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][Blind_Debuff.ID] = true;
		}
		public override void SetDefaults() {
			NPC.lifeMax = 240;
			NPC.defense = 6;
			NPC.damage = 54;
			NPC.width = 40;
			NPC.height = 184;
			NPC.value = Item.buyPrice(0, 0, 2);
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			NPC.knockBackResist = 0.5f;
			SpawnModBiomes = [
				GetInstance<Smog_Storm>().Type,
			];
			NPC.alpha = 255;
			NPC.setFrameSize = true;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemType<BBQ_Skewer>(), 19));
			npcLoot.Add(ScavengerBonus.Scrap(amountDroppedMinimum: 26, amountDroppedMaximum: 52));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Greaves>(), 525));
		}
		SlotId attackSound;
		public override void AI() {
			NPC.dontTakeDamage = NPC.Opacity <= 0;
			if (Main.rand.NextBool(7000)) SoundEngine.PlaySound(Origins.Sounds.WindPail.WithVolume(0.5f), Vector2.Lerp(NPC.Center, Main.Camera.Center, 0.5f));
			const int attack_range = 16 * 15;
			if (!NPC.HasValidTarget || (Main.player[NPC.target].Center.X - NPC.Center.X) * NPC.direction < 1 || !Main.player[NPC.target].InModBiome<Smog_Storm>()) {
				NPC.target = -1;
				NPC.ai[1] = 60;
				NPC.aiAction = 0;
				if (LinearSmoothing(ref NPC.alpha, 255, 255 / 60)) AI_TryTeleport();
				return;
			} else {
				LinearSmoothing(ref NPC.alpha, 0, 255 / 60);
				if (!IsOpaqueEnoughToAttack) return;
			}
			Rectangle attackHitbox = NPC.Hitbox;
			attackHitbox.Width += attack_range;
			NPC.velocity = Vector2.UnitY * 4;
			if (NPC.direction < 0) attackHitbox.X -= attack_range;
			switch (NPC.aiAction) {
				case 1:
				if (NPC.ai[2] >= 6) NPC.target = -1;
				if (!NPC.HasValidTarget) {
					NPC.aiAction = 2;
					return;
				}
				if (LinearSmoothing(ref NPC.ai[0], 1, 1f / 15)) {
					Player target = Main.player[NPC.target];
					target.AddBuff(ModContent.BuffType<Wind_Pail_Debuff>(), 15);
					target.OriginPlayer().windPailPosition = NPC.Center.X;
					Rectangle dustHitbox = attackHitbox;
					dustHitbox.X += NPC.direction * 16 * 4;
					for (int i = 0; i < 2; i++) {
						Dust dust = Dust.NewDustDirect(
							dustHitbox.TopLeft(),
							dustHitbox.Width,
							dustHitbox.Height,
							ModContent.DustType<Wind_Pail_Dust>(),
							Alpha: 255
						);
						dust.velocity.Y = 2f + Main.rand.NextFloat() * 0.2f;
						dust.velocity.Y *= dust.scale;
						dust.velocity.Y *= 0.35f;
						dust.velocity.X = NPC.direction * -5f + Main.rand.NextFloat();
						dust.velocity.X += NPC.direction * 0.7f * -10f;
						dust.fadeIn += 25;
						dust.velocity *= 1f + 0.35f * 0.5f;
						dust.velocity *= 1f + 0.35f;
					}
					attackSound.PlaySoundIfInactive(SoundID.Zombie4.WithPitch(-1.5f).WithPitchVarience(1).WithVolume(5f), NPC.Center);
					if (!attackHitbox.Intersects(Main.player[NPC.target].Hitbox)) NPC.aiAction = 2;
				}
				break;
				case 2:
				if (NPC.ai[0].Cooldown(rate: 1f / 15)) {
					NPC.aiAction = 0;
					NPC.ai[1] = 60;
				}
				break;
				default: {
					if (!LinearSmoothing(ref NPC.ai[1], 0, 1)) break;
					if (attackHitbox.Intersects(Main.player[NPC.target].Hitbox)) {
						NPC.ai[2] = 0;
						attackSound = SoundEngine.PlaySound(SoundID.Zombie4.WithPitch(-1.5f).WithPitchVarience(1).WithVolume(5f), NPC.Center);
						SoundEngine.PlaySound(Origins.Sounds.WindPail, NPC.Center);
						NPC.aiAction = 1;
					}
					break;
				}
			}
		}
		public override bool CheckActive() {
			if (NPC.dontTakeDamage) {
				if (--NPC.timeLeft <= 0) NPC.Despawn();
			} else {
				NPC.timeLeft = NPC.activeTime;
			}
			return base.CheckActive();
		}
		public void AI_TryTeleport() {
			const float min_dist = 16 * 15;
			const float max_dist = 16 * 40;
			static float GetDist() => Main.rand.NextBool().ToDirectionInt() * Main.rand.NextFloat(min_dist, max_dist);
			Rectangle hitbox = NPC.Hitbox;
			float distSQ = float.PositiveInfinity;
			Vector2? targetPos = null;
			int targetIndex = 0;
			foreach (Player player in Main.ActivePlayers) {
				float currentDistSQ = player.DistanceSQ(NPC.Bottom);
				if (player.InModBiome<Smog_Storm>() && distSQ > currentDistSQ && TryGetPosition(player) is Vector2 pos) {
					distSQ = currentDistSQ;
					targetPos = pos;
					targetIndex = player.whoAmI;
				}
			}
			if (!targetPos.HasValue) return;
			NPC.target = targetIndex;
			NPC.targetRect = Main.player[NPC.target].Hitbox;
			NPC.Bottom = targetPos.Value;
			NPC.FaceTarget();
			bool posIsValidPlayers(Vector2 pos) {
				foreach (Player player in Main.ActivePlayers) {
					if (pos.WithinRange(player.Center, min_dist)) return false;
				}
				return true;
			}
			Vector2? TryGetPosition(Player player) {
				int tries = 0;
				while (++tries < 100) {
					Vector2 testPos = player.Center + new Vector2(GetDist(), GetDist() * 0.5f);
					bool wasBurried = false;
					bool wasAirborne = false;
					retry:
					hitbox.X = (int)(testPos.X - (float)hitbox.Width * 0.5f);
					hitbox.Y = (int)testPos.Y - hitbox.Height;
					bool notInGround = !hitbox.OverlapsAnyTiles();
					bool nearGround = hitbox.Add(Vector2.UnitY * 16).OverlapsAnyTiles();
					if (notInGround && nearGround && posIsValidPlayers(testPos)) return testPos;
					if (!notInGround) {
						testPos.Y -= 16;
						wasBurried = true;
						if (wasAirborne) continue;
						goto retry;
					}
					if (!nearGround) {
						testPos.Y += 16;
						wasAirborne = true;
						if (wasBurried) continue;
						goto retry;
					}
				}
				return null;
			}
		}
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => IsOpaqueEnoughToAttack;
		public override bool CanHitNPC(NPC target) => IsOpaqueEnoughToAttack;
		public override void HitEffect(NPC.HitInfo hit) {
			NPC.ai[2] += hit.Knockback;
			NPC.velocity = default;
		}
		//Temp
		static bool LinearSmoothing<T>(ref T smoothed, T target, T rate) where T : INumberBase<T>, IComparisonOperators<T, T, bool> {
			if (target != smoothed) {
				if (T.Abs(target - smoothed) < rate) {
					smoothed = target;
				} else {
					if (target > smoothed) {
						smoothed += rate;
					} else if (target < smoothed) {
						smoothed -= rate;
					}
				}
			}
			return smoothed == target;
		}
		public static float BiomeSpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			return Smog_Storm.SpawnRates.WindPail;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void DrawBehind(int index) {
			if (NPC.frame.Width.TrySet(92)) NPC.frame.X = 94 * Main.rand.Next(3);
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			SpriteEffects spriteEffects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			spriteBatch.DrawGlowingNPCPart(
				TextureAssets.Npc[Type].Value,
				GlowTexture,
				NPC.Bottom - screenPos,
				NPC.frame,
				drawColor * NPC.Opacity,
				NPC.GetTintColor(Color.White) * NPC.Opacity * (NPC.ai[0] * 0.5f + 0.5f),
				0,
				NPC.frame.Size() * new Vector2(0.5f, 1),
				NPC.scale,
				spriteEffects
			);
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) { }
	}
	public class Wind_Pail_Debuff : ModBuff {
		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
			On_Player.HorizontalMovement += On_Player_HorizontalMovement;
		}
		static void On_Player_HorizontalMovement(On_Player.orig_HorizontalMovement orig, Player player) {
			bool windPailPushed = player.OriginPlayer().windPailPushed;
			if (windPailPushed) player.windPushed = false;
			orig(player);
			if (windPailPushed) {
				float windStrength = float.CopySign(0.24f, player.OriginPlayer().windPailPosition - player.Center.X);
				if (player.velocity.Y != 0f) windStrength *= 1.35f;
				player.velocity.X += windStrength;
			}
		}
		public override void Update(Player player, ref int buffIndex) {
			player.windPushed = true;
			player.OriginPlayer().windPailPushed = true;
		}
	}
	public class Wind_Pail_Dust : ModDust {
		public override string Texture => "Terraria/Images/Dust";
		public override void OnSpawn(Dust dust) {
			dust.frame.X = 10 * DustID.Silt;
			dust.frame.Y = 10 * Main.rand.Next(3);
			dust.frame.Width = 8;
			dust.frame.Height = 8;
		}
		public override bool Update(Dust dust) {
			dust.position += dust.velocity;
			if (!dust.noGravity) dust.velocity.Y += 0.1f;
			dust.velocity.X *= 0.99f;
			dust.rotation += dust.velocity.X * 0.5f;
			if (dust.fadeIn > 0f && dust.alpha > 0) {
				if (dust.alpha.Cooldown(0, Main.rand.RandomRound(dust.fadeIn))) dust.fadeIn = 0;
			} else if (dust.noGravity) {
				dust.velocity *= 0.92f;
				if (dust.fadeIn == 0f) dust.scale -= 0.04f;
			}
			if (dust.position.Y > Main.screenPosition.Y + Main.screenHeight) {
				dust.active = false;
			}
			return false;
		}
	}
}
