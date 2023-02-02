using Microsoft.Xna.Framework;
using Origins.Items.Other.Consumables;
using Origins.Items.Weapons.Summoner;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Flagellant : Glowing_Mod_NPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Flagellant");
			Main.npcFrameCount[NPC.type] = 4;
			SpawnModBiomes = new int[] {
				ModContent.GetInstance<Riven_Hive>().Type
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.BloodJelly);
			NPC.lifeMax = 380;
			NPC.defense = 20;
			NPC.damage = 70;
			NPC.width = 56;
			NPC.height = 60;
			NPC.frame.Height = 58;
			NPC.value = 500;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("A gentle swimmer in the amoeba-infested waters. It is flimsy and fragile, so travelers should be weary when approaching it."),
			});
		}
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			NPC.frameCounter += 1.0;
			if (NPC.frameCounter >= 24.0) {
				NPC.frameCounter = 0.0;
			}
			NPC.frame.Y = 60 * (int)(NPC.frameCounter / 6.0);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Jam_Sandwich>(), 17));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Flagellash>(), 25));
		}
		public override void OnHitPlayer(Player target, int damage, bool crit) {
			OriginPlayer.InflictTorn(target, 1800, 180, 0.47f);
		}
		public override void HitEffect(int hitDirection, double damage) {
			if (NPC.life < 0) {
				for (int i = 0; i < 3; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4)));
			} else {
				Gore.NewGore(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, Mod.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
			}
		}
	}
}
