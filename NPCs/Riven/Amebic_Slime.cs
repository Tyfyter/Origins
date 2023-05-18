using Origins.Items.Weapons.Demolitionist;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
    public class Amebic_Slime : Glowing_Mod_NPC {
		public override string GlowTexturePath => Texture;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Amebic Slime");
			Main.npcFrameCount[NPC.type] = 2;
			SpawnModBiomes = new int[] {
				ModContent.GetInstance<Riven_Hive>().Type
			};
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Crimslime);
			NPC.aiStyle = NPCAIStyleID.Slime;
			NPC.lifeMax = 60;
			NPC.defense = 5;
			NPC.damage = 30;
			NPC.width = 32;
			NPC.height = 24;
			NPC.friendly = false;
			NPC.value = 40;
		}
		public override void FindFrame(int frameHeight) {
			NPC.CloneFrame(NPCID.Crimslime, frameHeight);
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				new FlavorTextBestiaryInfoElement("With its viscosity unchanged, it behaves like most slimes... the only difference is that it tends to easily digest whatever it absorbs."),
			});
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Gel, 1, 2, 4));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Ameballoon>(), 10));
		}
		public override void OnHitPlayer(Player target, int damage, bool crit) {
			OriginPlayer.InflictTorn(target, 180, 180, 0.9f);
		}
		public override void HitEffect(int hitDirection, double damage) {
			if (NPC.life < 0) {
				for (int i = 0; i < 5; i++) Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Origins.instance.GetGoreSlot("Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4)));
			}
		}
	}
}
