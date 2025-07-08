using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.World.BiomeData;
using System.IO;
using System;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using PegasusLib;
using Terraria.Audio;
using Origins.Projectiles;
using Origins.Gores.NPCs;
using Origins.Buffs;
using Origins.Dev;
using Newtonsoft.Json.Linq;

namespace Origins.NPCs.Riven {
	public class Pustule_Jelly : Glowing_Mod_NPC, IRivenEnemy, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 32, 42);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override Color GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public AssimilationAmount? Assimilation => 0.08f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 8;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new();
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BloodJelly);
			NPC.aiStyle = -1;
			NPC.lifeMax = 380;
			NPC.defense = 20;
			NPC.damage = 70;
			NPC.width = 32;
			NPC.height = 42;
			NPC.frame.Height = 40;
			NPC.value = 800f;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type,
				ModContent.GetInstance<Riven_Hive_Ocean>().Type,
			];
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
			bestiaryEntry.Icon = new NPCExtensions.MultipleUnlockableNPCEntryIcon(Type, [0, 0, 0, 0], [0, 0, 0, 1]);
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!spawnInfo.Water) return 0f;
			return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.BlisterBoi;
		}
		public override void AI() {
			const int fuse_duration = 60;
			const float lunge_threshold = 0.2f;
			Vector3 glow = Color.Cyan.ToVector3() * 0.25f;
			if (NPC.ai[3] < 1) {
				glow *= 1 + NPC.ai[3] * 0.5f;
			} else {
				glow *= 0.5f;
			}
			NPC.DoJellyfishAI(lunge_threshold, glowColor: glow, canDoZappy: false);
			if (NPC.ai[3] < 1 && (NPC.ai[3] > 0 || (NPC.HasValidTarget && (Main.player[NPC.target].wet || !NPC.wet)))) {
				bool shouldExplode;
				if (NPC.ai[3] > 0) {
					shouldExplode = true;
				} else {
					Vector2 position = NPC.Center;
					Vector2 velocity = NPC.velocity;
					int steps = (int)MathF.Ceiling(MathF.Log(lunge_threshold / NPC.velocity.Length(), 0.98f));
					for (int i = 0; i < steps && i < fuse_duration; i++) {
						position += velocity * 0.5f;
						velocity *= 0.98f;
					}
					Rectangle hitbox = new(0, 0, Pustule_Jelly_Explosion.size, Pustule_Jelly_Explosion.size);
					hitbox.Inflate(-8, -8);// so it doesn't trigger if the player would barely be in range
					hitbox.X = (int)(position.X - hitbox.Width / 2);
					hitbox.Y = (int)(position.Y - hitbox.Width / 2);
					shouldExplode = Main.player[NPC.target].Hitbox.Intersects(hitbox);
				}
				if (shouldExplode) {
					NPC.ai[3] += 1f / fuse_duration;
					if (NPC.ai[3] >= 1) {
						SoundEngine.PlaySound(SoundID.NPCDeath26, NPC.Center);
						SoundEngine.PlaySound(SoundID.NPCDeath63, NPC.Center);
						if (Main.netMode != NetmodeID.MultiplayerClient) {
							Projectile.NewProjectile(
								NPC.GetSource_FromAI(),
								NPC.Center,
								(NPC.rotation - MathHelper.PiOver2).ToRotationVector2(),
								ModContent.ProjectileType<Pustule_Jelly_Explosion>(),
								NPC.damage,
								8
							);
						}
					}
				}
			}
		}
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			NPC.frameCounter += 1.0;
			if (NPC.frameCounter >= 24.0) {
				NPC.frameCounter = 0.0;
			}
			NPC.frame.Y = 42 * (int)(NPC.frameCounter / 6.0);
			if (NPC.ai[3] >= 1) {
				NPC.frame.Y += 168;
			}
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 1, 1, 3));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
			} else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			}
		}
	}
	public class Pustule_Jelly_Explosion : ExplosionProjectile {
		public const int size = 128;
		public const float assimilation_amount = 0.12f;
		public AssimilationAmount Assimilation = assimilation_amount;
		public override DamageClass DamageType => DamageClasses.Explosive;
		public override int Size => size;
		public override bool Hostile => true;
		public override SoundStyle? Sound => null;
		public override void SetStaticDefaults() {
			this.AddAssimilation<Riven_Assimilation>(Assimilation);
		}
		public override bool ShouldUpdatePosition() => false;
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				Projectile.ai[0] = 1;
				for (int i = 0; i < 8; i++) {
					Vector2 direction = Projectile.velocity * Main.rand.NextFloat(4, 6) + Main.rand.NextVector2CircularEdge(1, 1) * Main.rand.NextFloat(2, 3);
					Gore.NewGoreDirect(
						Projectile.GetSource_FromThis(),
						Projectile.Center + direction.SafeNormalize(default) * 12,
						direction,
						Main.rand.Next(R_Effect_Blood1.GoreIDs),
						1f
					);
				}
				for (int i = Main.rand.Next(48, 60); i-- > 0;) {
					Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(2f) * Main.rand.NextFloat(3, 5) + Main.rand.NextVector2Circular(1, 1), ModContent.GoreType<R_Effect_Blood1_Small>(), 2);
				}
			}
			if (!Hostile && DealsSelfDamage) ExplosiveGlobalProjectile.DealSelfDamage(Projectile, SelfDamageCooldownCounter);
		}
	}
}
