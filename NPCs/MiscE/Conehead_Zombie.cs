using Microsoft.Xna.Framework;
using Origins.Tiles.Other;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.MiscE {
    public class Conehead_Zombie : ModNPC {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 3;
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.Zombie);
			NPC.aiStyle = NPCAIStyleID.Fighter;
			NPC.lifeMax = 45;
			NPC.defense = 14;
			NPC.damage = 14;
			NPC.width = 19;
			NPC.height = 28;
			NPC.friendly = false;
		}
		public override void FindFrame(int frameHeight) {
			NPC.CloneFrame(NPCID.Zombie, frameHeight);
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if ((spawnInfo.Player.ZoneGraveyard || !Main.dayTime) && spawnInfo.Player.ZoneForest) {
				return 0.085f;
			}
			return 0;
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				new FlavorTextBestiaryInfoElement("Conehead Zombie shuffled mindlessly forward like every other zombie. But something made him stop, made him pick up a traffic cone and stick it on his head. Oh yeah. He likes to party."),
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime
			);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemID.Diamond, 20));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Traffic_Cone_Item>(), 3));
        }
		public override void HitEffect(NPC.HitInfo hit) {
			if (NPC.life < 0 || OriginsModIntegrations.CheckAprilFools()) {
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 20f), NPC.velocity, 4);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
				Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y + 34f), NPC.velocity, 5);
			}
		}
	}
}
