using Microsoft.Xna.Framework;
using Origins.Items.Armor.Riven;
using Origins.Items.Other.Consumables.Food;
using Origins.World.BiomeData;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Events;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.NPCs.Riven {
	public class Trijaw_Shark : ModNPC{
        public override void SetStaticDefaults() {
            Main.npcFrameCount[NPC.type] = 4;
        }
        public override void SetDefaults() {
            NPC.CloneDefaults(NPCID.SandsharkCrimson);
            NPC.lifeMax = 450;
            NPC.defense = 23;
            NPC.damage = 56;
            NPC.width = 86;
            NPC.height = 32;
            NPC.frame.Height = 38;
            NPC.value = 400;
			SpawnModBiomes = [
				ModContent.GetInstance<Riven_Hive>().Type,
				ModContent.GetInstance<Riven_Hive_Ocean>().Type
			];
		}
        public override void ModifyNPCLoot(NPCLoot npcLoot) {
            npcLoot.Add(ItemDropRule.Common(ItemID.SharkFin, 8));
            npcLoot.Add(ItemDropRule.Common(ItemID.Nachos, 30));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Jam_Sandwich>(), 16));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Mask>(), 25));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Coat>(), 25));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Riven2_Pants>(), 25));
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (!spawnInfo.Water) {
				if (!Sandstorm.Happening || !spawnInfo.Player.ZoneSandstorm || !TileID.Sets.Conversion.Sand[spawnInfo.SpawnTileType] || !NPC.Spawning_SandstoneCheck(spawnInfo.SpawnTileX, spawnInfo.SpawnTileY)) {
					return 0f;
				}
			}
            return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo) * Riven_Hive.SpawnRates.Shark1;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                this.GetBestiaryFlavorText("This menacing shark has adapted to the Hive's ecosystem quite well, taking on a triple-mandible design used for crushing and cracking nautili caught in its path."),
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Sandstorm
            });
        }
        public override void AI() {
            if (++NPC.frameCounter > 5) {
                NPC.frame = new Rectangle(0, (NPC.frame.Y + 40) % 160, 96, 38);
                NPC.frameCounter = 0;
            }
        }
        public override void HitEffect(NPC.HitInfo hit) {
            if (NPC.life < 0) {
                for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
            } else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
            }
        }
    }
}
