using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Armor.Riven;
using Origins.Items.Materials;
using Origins.Items.Weapons.Magic;
using Origins.World.BiomeData;
using PegasusLib;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Barnacleback : Glowing_Mod_NPC, IRivenEnemy, IWikiNPC {
		public Rectangle DrawRect => new(0, 0, 36, 50);
		public int AnimationFrames => 24;
		public int FrameDuration => 1;
		public NPCExportType ImageExportType => NPCExportType.Bestiary;
		public override Color GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public AssimilationAmount? Assimilation => 0.05f;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 5;
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Drippler);
			NPC.lifeMax = 40;
			NPC.defense = 0;
			NPC.damage = 14;
			NPC.width = 24;
			NPC.height = 47;
			NPC.friendly = false;
			NPC.HitSound = SoundID.NPCHit13;
			NPC.DeathSound = SoundID.NPCDeath15;
			NPC.knockBackResist = 0.75f;
			NPC.value = 76;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public new float SpawnChance(NPCSpawnInfo spawnInfo) {
			float rate = Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.BarnBack;
			if (rate == 0) return 0; // skip counting other barnaclebacks if it's already not going to spawn
			int count = 1;
			int maxCount = 2;
			float bonusCount = 0;
			if (!Main.expertMode) bonusCount += 0.5f;
			if (!Main.masterMode) {
				bonusCount += 0.5f;
				maxCount = 1;
			}
			foreach (NPC npc in Main.ActiveNPCs) {
				if (npc.type == Type) count++;
				if (count + 1 > maxCount) return 0;
			}
			return rate / (count + bonusCount);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bud_Barnacle>(), 1, 2, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Avulsion>(), 37));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Mask>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Coat>(), 525));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Pants>(), 525));
		}
		public override void AI() {
			const float maxDistTiles2 = 60f * 16;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC currentTarget = Main.npc[i];
				if (currentTarget.CanBeChasedBy() && currentTarget.ModNPC is IRivenEnemy) {
					float distSquared = (currentTarget.Center - NPC.Center).LengthSquared();
					if (distSquared < maxDistTiles2 * maxDistTiles2) {
						currentTarget.AddBuff(Barnacled_Buff.ID, 5);
						if (Main.rand.NextBool(6)) {
							Vector2 pos = Main.rand.NextVector2FromRectangle(currentTarget.Hitbox);
							Vector2 dir = (NPC.Center - pos).SafeNormalize(default) * 4;
							Dust dust = Dust.NewDustPerfect(
								pos,
								DustID.Clentaminator_Cyan,
								dir,
								120,
								Color.LightCyan,
								1f
							);
							dust.noGravity = true;
							if (Main.rand.NextBool(2)) {
								dust.noGravity = false;
								dust.scale *= 0.7f;
							}
						}
					}
				}
			}

			if (!NetmodeActive.MultiplayerClient && ++NPC.ai[3] > 60 * 1) {
				NPC.ai[3] = 0;
				List<(Vector2 a, Vector2 b)> directions = new(8);
				const int minTileRange = 160;
				const int maxTileRange = 160 * 2;
				Vector2 pos = NPC.Center;
				foreach (NPC other in Main.ActiveNPCs) {
					if (other.ModNPC is Goo_Wall gooWall && (other.WithinRange(pos, maxTileRange) || gooWall.Anchor1.WithinRange(pos, maxTileRange) || gooWall.Anchor2.WithinRange(pos, maxTileRange))) {
						return;
					}
				}
				const int rays_to_cast = 7;
				float betweenRays = MathHelper.ToRadians(60) / rays_to_cast;
				for (int i = -rays_to_cast; i < rays_to_cast; i++) {
					Vector2 dir = Vector2.UnitY.RotatedBy(i * betweenRays);
					float ray1 = CollisionExt.Raymarch(pos, dir, float.BitIncrement(maxTileRange));
					float ray2 = CollisionExt.Raymarch(pos, -dir, float.BitIncrement(maxTileRange));

					if (ray1 + ray2 >= minTileRange && ray1 + ray2 <= maxTileRange) {
						Vector2 ray1Pos = (dir * (ray1 + 1)) + pos;
						Vector2 ray2Pos = (-dir * (ray2 + 1)) + pos;
						if (Main.tile[ray1Pos.ToTileCoordinates()].HasFullSolidTile() && Main.tile[ray2Pos.ToTileCoordinates()].HasFullSolidTile()) {
							bool invalid = false;
							Vector2 anchor = ray1Pos;
							Vector2 position = ray2Pos;
							Vector2 perp = (position - anchor).RotatedBy(MathHelper.PiOver2).Normalized(out _) * 26;

							Vector2 vert1 = position + perp;
							Vector2 vert2 = anchor + perp;
							Vector2 vert3 = anchor - perp;
							Vector2 vert4 = position - perp;

							(Vector2 start, Vector2 end)[] lines = [(vert1, vert2), (vert2, vert3), (vert3, vert4), (vert4, vert1)];
							foreach (Player player in Main.ActivePlayers) {
								if (invalid) break;
								invalid = CollisionExtensions.PolygonIntersectsRect(lines, player.Hitbox);
							}
							if (invalid) continue;
							directions.Add((ray1Pos, ray2Pos));
						}
					}
				}
				if (directions.Count > 0) {
					(Vector2 start, Vector2 end) = Main.rand.Next(directions);
					pos = Vector2.Lerp(start, end, 0.5f);
					NPC.NewNPC(
						NPC.GetSource_FromAI(),
						(int)pos.X,
						(int)(pos.Y + ModContent.GetInstance<Goo_Wall>().NPC.height / 2),
						ModContent.NPCType<Goo_Wall>(),
						ai1: end.X,
						ai2: end.Y
					);
				}
			}
		}
		public override void FindFrame(int frameHeight) {
			if (++NPC.frameCounter > 7) {
				NPC.frame = new Rectangle(0, (NPC.frame.Y + 50) % 250, 36, 50);
				NPC.frameCounter = 0;
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(1, 4));
			}
			NPC.frameCounter = 0;
		}
	}
	public class Barnacled_Buff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_32";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<OriginGlobalNPC>().barnacleBuff = true;
		}
	}
}
