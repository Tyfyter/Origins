using Origins.Items.Materials;
using Origins.Items.Weapons.Magic;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Rivenator_Head : Rivenator {
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerHead);
			NPC.lifeMax = 634;
			NPC.defense = 18;
			NPC.damage = 52;
			NPC.value = 1000;
        }
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo, true) * Riven_Hive.SpawnRates.Worm;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				this.GetBestiaryFlavorText("A common spreading agent of the Riven parasite. The Mitoworm can reproduce very rapidly if a threat is being persistent."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.StatusImmunityItem(ItemID.Vitamins, 100));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Alkahest>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Seam_Beam>(), 40));
		}
		public override void AI() { }
		public override void OnSpawn(IEntitySource source) {
			NPC.spriteDirection = Main.rand.NextBool() ? 1 : -1;
			NPC.ai[3] = NPC.whoAmI;
			NPC.realLife = NPC.whoAmI;
			int current = NPC.whoAmI;
			int last = NPC.whoAmI;
			int type = ModContent.NPCType<Rivenator_Body>();
			NPC.netUpdate = true;
			for (int k = 0; k < 17; k++) {
				current = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, type, NPC.whoAmI);
				Main.npc[current].ai[3] = NPC.whoAmI;
				Main.npc[current].realLife = NPC.whoAmI;

				Main.npc[current].ai[1] = last;
				Main.npc[current].spriteDirection = Main.rand.NextBool() ? 1 : -1;
				Main.npc[last].ai[0] = current;

				last = current;
				Main.npc[current].netUpdate = true;
			}
			current = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Rivenator_Tail>(), NPC.whoAmI);
			Main.npc[current].ai[3] = NPC.whoAmI;
			Main.npc[current].realLife = NPC.whoAmI;
			Main.npc[current].ai[1] = last;
			Main.npc[current].spriteDirection = Main.rand.NextBool() ? 1 : -1;
			Main.npc[last].ai[0] = current;
			Main.npc[current].netUpdate = true;
		}
		public override bool SpecialOnKill() {
			NPC current = NPC;
			int bodyType = ModContent.NPCType<Rivenator_Body>();
			HashSet<int> indecies = new();
			while (current.ai[0] != 0) {
				Mod.Logger.Info("Wormed the worm");
				if (indecies.Contains(NPC.whoAmI)) break;
				indecies.Add(NPC.whoAmI);
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
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new() {
				Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			});
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerBody);
		}
	}

	internal class Rivenator_Tail : Rivenator {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new() {
				Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			});
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.DiggerTail);
		}
	}

	public abstract class Rivenator : Glowing_Mod_NPC, IRivenEnemy {
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
			for (int i = 0; i < 5; i++) Gore.NewGore(npc.GetSource_Death(), npc.position, npc.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
            //for(int i = 0; i < 3; i++)
        }
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			OriginPlayer.InflictTorn(target, 480, targetSeverity: 1f - 0.67f);
		}
	}
}