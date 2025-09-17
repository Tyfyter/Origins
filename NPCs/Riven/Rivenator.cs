using Newtonsoft.Json.Linq;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	/// <summary>
	/// TODO: use <see cref="Worm"/>
	/// </summary>
	public class Rivenator_Head : Rivenator, ICustomWikiStat {
		string ICustomWikiStat.CustomSpritePath => WikiPageExporter.GetWikiImagePath("UI/Rivenator_Preview");
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				CustomTexturePath = "Origins/UI/Rivenator_Preview", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(50f, 32f),
				PortraitPositionXOverride = 4,
				PortraitPositionYOverride = 8
			};
			ModContent.GetInstance<Riven_Hive.SpawnRates>().AddSpawn(Type, SpawnChance);
		}
		public bool? Hardmode => true;
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerHead);
			NPC.width = 22;
			NPC.height = 22;
			NPC.lifeMax = 634;
			NPC.defense = 18;
			NPC.damage = 52;
			NPC.scale = 0.9f;
			NPC.value = 1000;
			SpawnModBiomes = [
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public new static float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.SpawnTileY <= Main.worldSurface || spawnInfo.PlayerSafe || spawnInfo.DesertCave) return 0;
			return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo, true) * Riven_Hive.SpawnRates.Worm;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				this.GetBestiaryFlavorText()
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Vitamins, 100));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Alkahest>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ameballoon>(), 1, 38, 72));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Seam_Beam>(), 40));
		}
		public override void AI() {
			if (Main.netMode != NetmodeID.MultiplayerClient && NPC.localAI[0] == 0) {
				NPC.localAI[0] = 1;
				NPC.spriteDirection = Main.rand.NextBool() ? 1 : -1;
				NPC.ai[3] = NPC.whoAmI;
				int current;
				int last = NPC.whoAmI;
				int type = ModContent.NPCType<Rivenator_Body>();
				NPC.netUpdate = true;
				for (int k = 0; k < 17; k++) {
					current = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI);
					Main.npc[current].ai[3] = NPC.whoAmI;
					Main.npc[current].realLife = NPC.whoAmI;

					Main.npc[current].ai[1] = last;
					Main.npc[current].spriteDirection = Main.rand.NextBool() ? 1 : -1;
					Main.npc[last].ai[0] = current;

					last = current;
					Main.npc[current].netUpdate = true;
				}
				current = NPC.NewNPC(NPC.GetSource_NaturalSpawn(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Rivenator_Tail>(), NPC.whoAmI);
				Main.npc[current].ai[3] = NPC.whoAmI;
				Main.npc[current].realLife = NPC.whoAmI;
				Main.npc[current].ai[1] = last;
				Main.npc[current].spriteDirection = Main.rand.NextBool() ? 1 : -1;
				Main.npc[last].ai[0] = current;
				Main.npc[current].netUpdate = true;
			}
			Lighting.AddLight(NPC.Center, Riven_Hive.ColoredGlow(0.04f));
		}
		public override bool SpecialOnKill() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return false;
			NPC current = NPC;
			int bodyType = ModContent.NPCType<Rivenator_Body>();
			HashSet<int> indecies = [];
			while (current.ai[0] != 0) {
				if (!indecies.Add(current.whoAmI)) break;
				if (Main.rand.NextBool(9)) {
					NPC.NewNPC(NPC.GetSource_Death(), (int)current.position.X + Main.rand.Next(current.width), (int)current.position.Y + Main.rand.Next(current.height), ModContent.NPCType<Cleaver_Head>());
				}
				current = Main.npc[(int)current.ai[0]];
				if (current.type != bodyType) break;
			}
			return false;
		}
		public void ModifyWikiStats(JObject data) {
			data["SpriteWidth"] = 354;
		}
	}

	internal class Rivenator_Body : Rivenator, ICustomWikiStat {
		string ICustomWikiStat.CustomStatPath => ModContent.GetInstance<NPCWikiProvider>().PageName(this) + "_Body";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerBody);
		}
	}

	internal class Rivenator_Tail : Rivenator, ICustomWikiStat {
		string ICustomWikiStat.CustomStatPath => ModContent.GetInstance<NPCWikiProvider>().PageName(this) + "_Tail";
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerTail);
		}
	}

	public abstract class Rivenator : ModNPC, IRivenEnemy {
		public AssimilationAmount? Assimilation => 0.06f;
		public override void AI() {
			if (NPC.realLife > -1) NPC.life = Main.npc[NPC.realLife].active ? NPC.lifeMax : 0;
			NPC.oldVelocity = NPC.position - NPC.oldPosition;
			Lighting.AddLight(NPC.Center, Riven_Hive.ColoredGlow(0.04f));
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life <= 0) {
				NPC current = Main.npc[NPC.realLife > -1 ? NPC.realLife : NPC.whoAmI];
				int bodyType = ModContent.NPCType<Rivenator_Body>();
				int tailType = ModContent.NPCType<Rivenator_Tail>();
				HashSet<int> indecies = [];
				while (current.active && current.ai[0] != 0) {
					if (!indecies.Add(current.whoAmI)) break;
					current.velocity = current.oldVelocity;
					deathEffect(current);
					current = Main.npc[(int)current.ai[0]];
					if (current.type != bodyType && current.type != tailType) break;
				}
			}
		}
		protected static void deathEffect(NPC npc) {
			for (int i = 0; i < 5; i++) Origins.instance.SpawnGoreByName(npc.GetSource_Death(), npc.position, npc.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 480, targetSeverity: 1f - 0.67f);
		}
		public override Color? GetAlpha(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
	}
}