using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.Items.Armor.Riven;
using Origins.Items.Materials;
using Origins.Items.Weapons.Magic;
using Origins.World.BiomeData;
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
