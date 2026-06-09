using Microsoft.Xna.Framework.Graphics;
using Origins.Events;
using Origins.Items.Armor.Ashen;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.LootConditions;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.NPCs.Ashen {
	public abstract class Cartwheeler : ModNPC, IAshenEnemy {
		protected abstract float SpawnRate { get; }
		public virtual float Speed => 4f + MathHelper.Lerp(0.6f, 1f, Math.Abs(Main.windSpeedTarget)) * 3f;
		public virtual float Acceleration => 0.05f;
		static AutoLoadingTexture eyeTexture = typeof(Cartwheeler).GetDefaultTMLName("_Eye");
		static AutoLoadingTexture eyeGlowTexture = typeof(Cartwheeler).GetDefaultTMLName("_Eye_Glow");
		Player Target => Main.player[NPC.target];
		ref float TurnaroundTimer => ref NPC.ai[3];
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 1;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with { Direction = 1 };
			GetInstance<Smog_Storm.SpawnRates>().AddSpawn(Type, BiomeSpawnChance(SpawnRate));
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Tumbleweed);
			NPC.aiStyle = NPCAIStyleID.ActuallyNone;
			NPC.HitSound = SoundID.NPCHit4.WithPitchOffset(-1.2f);
			NPC.DeathSound = SoundID.NPCDeath44;
			SpawnModBiomes = [
				GetInstance<Smog_Storm>().Type,
			];
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemType<BBQ_Skewer>(), 19));
			npcLoot.Add(ScavengerBonus.Scrap(amountDroppedMinimum: 9, amountDroppedMaximum: 17));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Helmet>(), 525));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Breastplate>(), 525));
			npcLoot.Add(ItemDropRule.Common(ItemType<Ashen2_Greaves>(), 525));
		}
		public override void AI() {
			const int min_turnaround_time = 30;
			bool turnAround = false;
			bool walled = false;
			if (NPC.velocity.Y == 0f && NPC.velocity.X * NPC.direction < 0) {
				turnAround = true;
				TurnaroundTimer += 1f;
			}
			bool grounded = NPC.velocity.Y == 0f;
			foreach (NPC other in Main.ActiveNPCs) {
				if (other == NPC || other.ModNPC is not Cartwheeler) continue;
				if (Math.Abs(NPC.position.X - other.position.X) + Math.Abs(NPC.position.Y - other.position.Y) < (NPC.width + other.width) * 0.5f) {
					if (NPC.position.X < other.position.X) {
						NPC.velocity.X -= 0.05f;
					} else {
						NPC.velocity.X += 0.05f;
					}
					if (NPC.position.Y < other.position.Y) {
						NPC.velocity.Y -= 0.05f;
					} else {
						NPC.velocity.Y += 0.05f;
					}
				}
			}
			if (grounded) NPC.velocity.Y = 0f;
			if (NPC.position.X == NPC.oldPosition.X || TurnaroundTimer >= min_turnaround_time || turnAround) {
				TurnaroundTimer += 1f;
				walled = true;
			} else if (TurnaroundTimer > 0f) {
				TurnaroundTimer -= 1f;
			}
			if (TurnaroundTimer > min_turnaround_time * 4) TurnaroundTimer = 0f;
			if (NPC.justHit) TurnaroundTimer = 0f;
			if (TurnaroundTimer == min_turnaround_time) NPC.netUpdate = true;
			if (!walled && Target.Bottom.WithinRange(NPC.Center, 200f)) TurnaroundTimer = 0f;

			if (NPC.velocity.Y == 0f && Math.Abs(NPC.velocity.X) > 3f && (NPC.Center.X - Target.Center.X) * NPC.velocity.X < 0f) {
				NPC.velocity.Y -= 4f;
				SoundEngine.PlaySound(SoundID.NPCHit11.WithPitchOffset(-0.5f), NPC.Center);
			}
			if (!Target.InModBiome<Smog_Storm>()) {
				NPC.TargetClosest(false);
				if (Target.InModBiome<Smog_Storm>()) {
					NPC.FaceTarget();
				} else {
					NPC.EncourageDespawn(10);
					TurnaroundTimer = min_turnaround_time;
				}
			}
			if (TurnaroundTimer < min_turnaround_time) {
				NPC.TargetClosest();
			} else {
				if (NPC.velocity.X == 0f) {
					if (NPC.velocity.Y == 0f) {
						NPC.ai[0] += 1f;
						if (NPC.ai[0] >= 2f) {
							NPC.direction *= -1;
							NPC.spriteDirection = NPC.direction;
							NPC.ai[0] = 0f;
						}
					}
				} else {
					NPC.ai[0] = 0f;
				}
				NPC.directionY = -1;
				if (NPC.direction == 0) NPC.direction = 1;
			}

			if (NPC.velocity.Y == 0f || NPC.wet || NPC.velocity.X * NPC.direction >= 0f) {
				if (Math.Sign(NPC.velocity.X) != NPC.direction) NPC.velocity.X *= 0.92f;
				float speed = Target.InModBiome<Smog_Storm>() ? Speed : 4;
				if (Math.Abs(NPC.velocity.X) > speed) {
					if (NPC.velocity.Y == 0f) NPC.velocity *= 0.8f;
				} else if (NPC.velocity.X < speed && NPC.direction == 1) {
					NPC.velocity.X += Acceleration;
					Min(ref NPC.velocity.X, speed);
				} else if (NPC.velocity.X > -speed && NPC.direction == -1) {
					NPC.velocity.X -= Acceleration;
					Max(ref NPC.velocity.X, -speed);
				}
			}
			if (NPC.velocity.Y >= 0f) {
				Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
				Collision.StepDown(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
			}
			if (NPC.velocity.Y == 0f) {
				bool fullyGrounded = true;
				int bottom = (int)(NPC.position.Y - 7f) / 16;
				int left = (int)(NPC.position.X - 7f) / 16;
				int right = (int)(NPC.position.X + NPC.width + 7f) / 16;
				for (int m = left; fullyGrounded && m <= right; m++) {
					if (Main.tile[m, bottom].HasSolidTile()) fullyGrounded = false;
				}
				if (fullyGrounded && NPC.velocity.X * NPC.spriteDirection < 0) {
					int i = (int)((NPC.position.X + NPC.width / 2 + (NPC.width / 2 + 2) * NPC.direction + NPC.velocity.X * 5f) / 16f);
					int j = (int)((NPC.position.Y + NPC.height - 15f) / 16f);
					if (Main.tile[i, j - 2].HasSolidTile()) {
						if (Main.tile[i, j - 3].HasSolidTile()) {
							NPC.velocity.Y = -8.5f;
							NPC.netUpdate = true;
						} else {
							NPC.velocity.Y = -7.5f;
							NPC.netUpdate = true;
						}
					} else if (Main.tile[i, j - 1].HasSolidTile() && !Main.tile[i, j - 1].TopSlope) {
						NPC.velocity.Y = -7f;
						NPC.netUpdate = true;
					} else if (NPC.position.Y + NPC.height - j * 16 > 20f && Main.tile[i, j].HasSolidTile() && !Main.tile[i, j].TopSlope) {
						NPC.velocity.Y = -6f;
						NPC.netUpdate = true;
					} else if ((NPC.directionY < 0 || Math.Abs(NPC.velocity.X) > 3) && !Main.tile[i, j + 2].HasSolidTile() && !Main.tile[i + NPC.direction, j + 3].HasSolidTile()) {
						NPC.velocity.Y = -8f;
						NPC.netUpdate = true;
					}
				}
			}
			NPC.rotation += NPC.velocity.X * 0.05f;
			NPC.spriteDirection = -NPC.direction;
		}
		public static Func<NPCSpawnInfo, float> BiomeSpawnChance(float rate) => spawnInfo => (!spawnInfo.PlayerInTown).Mul(rate);
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
			bestiaryEntry.Icon = new NPCExtensions.RotatingUnlockableNPCEntryIcon(Type, -(Acceleration * 5));
		}
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			drawColor = NPC.GetNPCColorTintedByBuffs(drawColor);
			DrawData data = new(
				eyeTexture,
				NPC.Center - screenPos,
				null,
				drawColor,
				0,
				eyeTexture.Value.Size() * 0.5f + Vector2.UnitX * NPC.width * 0.4f * NPC.spriteDirection,
				NPC.scale,
				NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
			data.Draw(spriteBatch);
			data.texture = eyeGlowTexture;
			data.color = NPC.GetTintColor(Color.White);
			data.Draw(spriteBatch);
		}
	}
	public class Cartwheeler_Large : Cartwheeler {
		public override void Load() => this.AddBanner(nameOverride: "Cartwheeler");
		protected override float SpawnRate => Smog_Storm.SpawnRates.Cartwheeler_Large;
		public override float Speed => base.Speed;
		public override float Acceleration => 0.03f;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft with {
				Direction = 1,
				Position = new(10),
				PortraitPositionXOverride = 0,
				PortraitPositionYOverride = 0
			};
		}
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.lifeMax = 160;
			NPC.defense = 14;
			NPC.damage = 34;
			NPC.value = Item.buyPrice(0, 0, 2);
			NPC.width = NPC.height = 76;
			NPC.knockBackResist = 0.4f;
		}
	}
	public class Cartwheeler_Medium : Cartwheeler {
		protected override float SpawnRate => Smog_Storm.SpawnRates.Cartwheeler_Medium;
		public override float Speed => base.Speed;
		public override float Acceleration => 0.04f;
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.lifeMax = 120;
			NPC.defense = 8;
			NPC.damage = 38;
			NPC.value = Item.buyPrice(0, 0, 2);
			NPC.width = NPC.height = 52;
			NPC.knockBackResist = 0.6f;
			this.CopyBanner<Cartwheeler_Large>();
		}
	}
	public class Cartwheeler_Small : Cartwheeler {
		protected override float SpawnRate => Smog_Storm.SpawnRates.Cartwheeler_Small;
		public override float Speed => base.Speed;
		public override float Acceleration => 0.06f;
		public override void SetDefaults() {
			base.SetDefaults();
			NPC.lifeMax = 80;
			NPC.defense = 6;
			NPC.damage = 42;
			NPC.value = Item.buyPrice(0, 0, 2);
			NPC.width = NPC.height = 46;
			NPC.knockBackResist = 0.8f;
			this.CopyBanner<Cartwheeler_Large>();
		}
	}
}
