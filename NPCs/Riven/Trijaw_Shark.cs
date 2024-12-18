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
	public class Trijaw_Shark : Glowing_Mod_NPC, ICustomCollisionNPC {
		public override Color? GetGlowColor(Color drawColor) => Riven_Hive.GetGlowAlpha(drawColor);
		public bool IsSandshark => true;
		public override void Load() => this.AddBanner();
		public override void SetStaticDefaults() {
            Main.npcFrameCount[NPC.type] = 4;
			NPCID.Sets.NPCBestiaryDrawOffset[Type] = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				Position = new(28, 0),
				PortraitPositionXOverride = 0,
				PortraitPositionYOverride = 8
			};
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
			AnimationType = NPCID.SandsharkCrimson;
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
            return Riven_Hive.SpawnRates.FlyingEnemyRate(spawnInfo, true) * Riven_Hive.SpawnRates.Shark1;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
            bestiaryEntry.Info.AddRange([
                this.GetBestiaryFlavorText(),
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Sandstorm
            ]);
        }
        public override void HitEffect(NPC.HitInfo hit) {
            if (NPC.life < 0) {
                for (int i = 0; i < 3; i++) Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Meat" + Main.rand.Next(2, 4));
            } else {
				Origins.instance.SpawnGoreByName(NPC.GetSource_Death(), NPC.position + new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height)), NPC.velocity, "Gores/NPCs/R_Effect_Blood" + Main.rand.Next(1, 4));
            }
        }
		public void PreUpdateCollision() { }
		public void PostUpdateCollision() { }
	}
}
