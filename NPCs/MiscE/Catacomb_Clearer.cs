using Origins.Projectiles;
using Origins.Tiles.Limestone;
using Origins.World;
using PegasusLib;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
	public class Catacomb_Clearer : ModNPC {
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 20;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = NPCExtensions.BestiaryWalkLeft;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.AngryBones);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 81;
			NPC.defense = 10;
			NPC.damage = 33;
			NPC.width = 20;
			NPC.height = 38;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath24.WithPitch(0.6f);
			NPC.knockBackResist = 0.5f;
			NPC.value = 90;
			SpawnModBiomes = [
				ModContent.GetInstance<Limestone_Cave>().Type
			];
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.PlayerInTown) return 0;
			int limestoneTile = ModContent.TileType<Limestone>();
			for (int i = 0; i < 10; i++) {
				if (Framing.GetTileSafely(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY + i).TileIsType(limestoneTile)) {
					return 0.5f;
				}
			}
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText(),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(new LeadingConditionRule(new Conditions.NotExpert()).WithOnSuccess(
				ItemDropRule.Common(ItemID.Dynamite, 9)
				.WithOnFailedRoll(
					ItemDropRule.Common(ItemID.Bomb, 4, minimumDropped: 3, maximumDropped: 6)
				)
			));
			npcLoot.Add(new LeadingConditionRule(new Conditions.IsExpert()).WithOnSuccess(
				ItemDropRule.Common(ItemID.Dynamite, 6, minimumDropped: 1, maximumDropped: 3)
				.WithOnFailedRoll(
					new CommonDrop(ItemID.Bomb, 5, amountDroppedMinimum: 3, amountDroppedMaximum: 6, 2)
				)
			));
		}
		public override bool PreAI() {
			switch (NPC.aiAction) {
				case 0:
				if (NPC.collideY && NPC.HasValidTarget) {
					const int index = 0;
					NPC.localAI[index] += Main.rand.NextFloat(0.5f, 1);
					if (NPC.localAI[index] > 180) {
						NPC.aiAction = 1;
						NPC.localAI[index] = 0;
					}
				}
				break;
				case 1:
				int frame = (int)NPC.localAI[0]++;
				const int timePerFrame = 6;
				if (frame == 4 * timePerFrame && Main.netMode != NetmodeID.MultiplayerClient) {
					Vector2 pos = NPC.Center + new Vector2(8 * NPC.direction, -4);
					Vector2 velocity;
					bool dynamite = Main.rand.NextBool(3);
					float speed = dynamite ? 7 : 6;
					if (GeometryUtils.AngleToTarget(NPC.GetTargetData().Center - pos, speed, 0.2f, dynamite) is float angle) {
						velocity = angle.ToRotationVector2() * speed;
					} else {
						float val = 0.70710678119f;
						velocity = new Vector2(val * NPC.direction, -val) * speed;
					}
					Projectile.NewProjectile(
						NPC.GetSource_FromAI(),
						pos,
						velocity,
						dynamite ? ModContent.ProjectileType<Catacomb_Clearer_Dynamite>() : ModContent.ProjectileType<Catacomb_Clearer_Bomb>(),
						30 + (int)(20 * ContentExtensions.DifficultyDamageMultiplier),
						8
					);
				}
				if (NPC.collideY) NPC.velocity *= 0.95f;
				if (frame / timePerFrame > 5) {
					NPC.localAI[0] = 0;
					NPC.aiAction = 0;
					return true;
				}
				NPC.frame.Y = NPC.frame.Height * (frame / timePerFrame + 14);
				NPC.frameCounter = 0;
				return false;
			}
			return true;
		}
		public override void AI() {
			NPC.TargetClosest();
			if (NPC.HasPlayerTarget) {
				NPC.spriteDirection = NPC.direction;
			}
		}
		public override void FindFrame(int frameHeight) {
			if (NPC.aiAction != 1) {
				if (NPC.velocity.Y == 0) {
					NPC.DoFrames(6, 1..14);
				} else {
					NPC.DoFrames(6, 0..1);
				}
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life > 0) {
				for (int i = (int)(hit.Damage / (float)NPC.lifeMax * 50f); i-->0;) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, hit.HitDirection, -1f);
				}
			} else {
				for (int i = 0; i < 20; i++) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone, 2.5f * hit.HitDirection, -2.5f);
				}

				Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, 42, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 43, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 44, NPC.scale);
			}
		}
	}
	public class Catacomb_Clearer_Bomb : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Bomb;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Bomb);
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.timeLeft = 60;
		}
		public override void AI() {
			const int HalfSpriteWidth = 22 / 2;
			const int HalfSpriteHeight = 30 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
		}
		public override void PrepareBombToBlow() {
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.Resize(128, 128);
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item14);
		}
	}
	public class Catacomb_Clearer_Dynamite : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Dynamite;
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Dynamite);
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.timeLeft = 70;
		}
		public override void AI() {
			const int HalfSpriteWidth = 10 / 2;
			const int HalfSpriteHeight = 32 / 2;

			int HalfProjWidth = Projectile.width / 2;
			int HalfProjHeight = Projectile.height / 2;

			// Vanilla configuration for "hitbox in middle of sprite"
			DrawOriginOffsetX = 0;
			DrawOffsetX = -(HalfSpriteWidth - HalfProjWidth);
			DrawOriginOffsetY = -(HalfSpriteHeight - HalfProjHeight);
		}
		public override void PrepareBombToBlow() {
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.Resize(250, 250);
			ExplosiveGlobalProjectile.ExplosionVisual(Projectile, true, sound: SoundID.Item14);
		}
	}
}
