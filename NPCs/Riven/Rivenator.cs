using Microsoft.Xna.Framework;
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
	public class Rivenator_Head : Rivenator {
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				CustomTexturePath = "Origins/UI/Rivenator_Preview", // If the NPC is multiple parts like a worm, a custom texture for the Bestiary is encouraged.
				Position = new Vector2(4f, 8f)
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerHead);
			NPC.lifeMax = 634;
			NPC.defense = 18;
			NPC.damage = 52;
			NPC.value = 1000;
			SpawnModBiomes = [
				ModContent.GetInstance<Underground_Riven_Hive_Biome>().Type
			];
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
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
		}
		public override bool SpecialOnKill() {
			NPC current = NPC;
			int bodyType = ModContent.NPCType<Rivenator_Body>();
			HashSet<int> indecies = new();
			while (current.ai[0] != 0) {
				Mod.Logger.Info("Wormed the worm");
				if (!indecies.Add(current.whoAmI)) break;
				if (Main.rand.NextBool(9)) {
					NPC.NewNPC(NPC.GetSource_Death(), (int)NPC.position.X + Main.rand.Next(NPC.width), (int)NPC.position.Y + Main.rand.Next(NPC.height), ModContent.NPCType<Cleaver_Head>());
				}
				current = Main.npc[(int)current.ai[0]];
				if (current.type != bodyType) break;
			}
			return false;
		}
	}

	internal class Rivenator_Body : Rivenator {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerBody);
		}
	}

	internal class Rivenator_Tail : Rivenator {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, NPCExtensions.HideInBestiary);
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerTail);
		}
	}

	public abstract class Rivenator : Glowing_Mod_NPC, IRivenEnemy {
		public AssimilationAmount? Assimilation => 0.06f;
		public override string GlowTexturePath => Texture;
		public override void AI() {
			if (NPC.realLife > -1) NPC.life = Main.npc[NPC.realLife].active ? NPC.lifeMax : 0;
			NPC.oldVelocity = NPC.position - NPC.oldPosition;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0) {
				NPC current = Main.npc[NPC.realLife > -1 ? NPC.realLife : NPC.whoAmI];
				while (current.ai[0] != 0) {
					current.velocity = current.oldVelocity;
					deathEffect(current);
					current = Main.npc[(int)current.ai[0]];
				}
			}
		}
		protected static void deathEffect(NPC npc) {
			//Gore.NewGore(NPC.GetSource_Death(), npc.position, npc.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/DF_Effect_Medium"+Main.rand.Next(1,4)));
			for (int i = 0; i < 5; i++) Origins.instance.SpawnGoreByName(npc.GetSource_Death(), npc.position, npc.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
            //for(int i = 0; i < 3; i++)
        }
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 480, targetSeverity: 1f - 0.67f);
		}
	}
}