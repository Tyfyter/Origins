using Microsoft.Xna.Framework;
using Origins.Items.Accessories;
using Origins.Items.Weapons.Ranged;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Riven_Pigron : Glowing_Mod_NPC, IRivenEnemy {
		public override Color? GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.PigronCrimson];
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new Vector2(10f, 5f),
				PortraitPositionXOverride = 0f,
				PortraitPositionYOverride = -12f
			});
		}
		public override void SetDefaults() {
			NPC.CloneDefaults(NPCID.PigronCrimson);
			AnimationType = NPCID.PigronCrimson;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive_Ice_Biome>().Type
			];
			Banner = Item.NPCtoBanner(NPCID.PigronCorruption);
			BannerItem = ItemID.PigronBanner;
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Food(ItemID.Bacon, 15));
			npcLoot.Add(ItemDropRule.Common(ItemID.PigronMinecart, 100));
			npcLoot.Add(new ItemDropWithConditionRule(ItemID.KitePigron, 25, 1, 1, new Conditions.WindyEnoughForKiteDrops()));
			npcLoot.Add(new ItemDropWithConditionRule(ItemID.HamBat, 10, 1, 1, new Conditions.DontStarveIsUp()));
			npcLoot.Add(new ItemDropWithConditionRule(ItemID.HamBat, 25, 1, 1, new Conditions.DontStarveIsNotUp()));
		}
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.AddTags(
				new FlavorTextBestiaryInfoElement("CommonBestiaryFlavor.Pigron")
			);
		}
		public override void HitEffect(NPC.HitInfo hit) {
			if (Main.netMode == NetmodeID.Server) return;
			if (NPC.life < 0) {
				for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
			} else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
			}
		}
	}
}
