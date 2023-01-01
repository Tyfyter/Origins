using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Pets;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Dungeon;
using Origins.Items.Weapons.Explosives;
using Origins.Items.Weapons.Other;
using Origins.Items.Weapons.Summon;
using Origins.LootConditions;
using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Origins.World;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.NPCs {
    public partial class OriginGlobalNPC : GlobalNPC {
		internal static int woFEmblemsCount = 4;
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
			List<IItemDropRule> dropRules = npcLoot.Get(false);
			var def = new IsWorldEvil(OriginSystem.evil_wastelands);
			var riv = new IsWorldEvil(OriginSystem.evil_riven);
			var defExp = new IsWorldEvilAndNotExpert(OriginSystem.evil_wastelands);
			var rivExp = new IsWorldEvilAndNotExpert(OriginSystem.evil_riven);
			LootFixers.WorldEvilFixer(dropRules, (rule, isExpert) => {
				switch (rule.itemId) {
					case ItemID.DemoniteOre:
					npcLoot.Add(ItemDropRule.ByCondition(
						isExpert ? defExp : def,
						ModContent.ItemType<Defiled_Ore_Item>(),
						rule.chanceDenominator,
						rule.amountDroppedMinimum,
						rule.amountDroppedMaximum,
						rule.chanceNumerator
					));
					npcLoot.Add(ItemDropRule.ByCondition(
						isExpert ? rivExp : riv,
						ModContent.ItemType<Infested_Ore_Item>(),
						rule.chanceDenominator,
						rule.amountDroppedMinimum,
						rule.amountDroppedMaximum,
						rule.chanceNumerator
					));
					break;
					case ItemID.CorruptSeeds:
					npcLoot.Add(ItemDropRule.ByCondition(
						isExpert ? defExp : def,
						ModContent.ItemType<Defiled_Grass_Seeds>(),
						rule.chanceDenominator,
						rule.amountDroppedMinimum,
						rule.amountDroppedMaximum,
						rule.chanceNumerator
					));
					break;
				}
			});
			switch (npc.type) {
                case NPCID.CaveBat:
                case NPCID.GiantBat:
                case NPCID.IceBat:
                case NPCID.IlluminantBat:
                case NPCID.JungleBat:
                case NPCID.VampireBat:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bat_Hide>(), 3, 1, 3));
                break;
                case NPCID.ArmoredSkeleton:
                case NPCID.SkeletonArcher:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Tiny_Sniper>(), 50));
                break;
				case NPCID.Snatcher:
				case NPCID.JungleSlime:
				case NPCID.SpikedJungleSlime:
				case NPCID.MossHornet:
                case NPCID.BigMossHornet:
                case NPCID.GiantMossHornet:
                case NPCID.LittleMossHornet:
                case NPCID.TinyMossHornet:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Peat_Moss>(), 3));
                break;
				case NPCID.AngryBones:
				case NPCID.AngryBonesBig:
				case NPCID.AngryBonesBigMuscle:
				case NPCID.AngryBonesBigHelmet:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bolter>(), 41));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Longbone>(), 41));
				break;
				case NPCID.RedDevil:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Burning_Ember>(), 15));
				break;
				case NPCID.EaterofWorldsHead:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Rotting_Worm_Staff>(), 43));
				break;
				case NPCID.BrainofCthulhu:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Brainy_Staff>(), 20));
				break;
				case NPCID.DemonEye:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Eyeball_Staff>(), 60));
				break;
				case NPCID.SkeletronPrime:
				case NPCID.TheDestroyer:
				case NPCID.Retinazer:
				case NPCID.Spazmatism:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Busted_Servo>(), 1, 8, 37));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Power_Core>(), 4, 1, 2));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Rotor>(), 2, 5, 22));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_Power_Up>(), 106));
				break;
				case NPCID.GoblinArcher:
				case NPCID.GoblinPeon:
				case NPCID.GoblinScout:
				case NPCID.GoblinSorcerer:
				case NPCID.GoblinWarrior:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Harpoon_Gun>(), 200));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Harpoon>(), 2, 1, 2));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Potato>(), 50));
				break;
				case NPCID.Zombie:
				case NPCID.Harpy:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Potato>(), 13));
				break;
				case NPCID.Nymph:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Potato>()));
				break;
				case NPCID.Wolf:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Tasty_Vanilla_Shake>(), 21));
				break;
				case NPCID.RainbowSlime:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<One_Kind_Cookie>(), 20, 1, 2));
				break;
				case NPCID.WallofFlesh:
				IEnumerable<IItemDropRule> rules = dropRules.Where((r) => 
				r is LeadingConditionRule conditionRule &&
				conditionRule.ChainedRules.Any() &&
				conditionRule.ChainedRules[0].RuleToChain is OneFromOptionsNotScaledWithLuckDropRule dropRule &&
				dropRule.dropIds.Contains(ItemID.WarriorEmblem));
				if (rules.Any()) {
					OneFromOptionsNotScaledWithLuckDropRule rule = rules.First().ChainedRules[0].RuleToChain as OneFromOptionsNotScaledWithLuckDropRule;
					if (rule is not null) {
						Array.Resize(ref rule.dropIds, rule.dropIds.Length + 1);
						rule.dropIds[^1] = ModContent.ItemType<Exploder_Emblem>();
						woFEmblemsCount = rule.dropIds.Length;
					} else {
						Origins.instance.Logger.Warn("Emblem drop rule not present on WoF");
					}
				} else {
					Origins.instance.Logger.Warn("Emblem drop rule not present on WoF");
				}
				rules = dropRules.Where((r) =>
				r is LeadingConditionRule conditionRule &&
				conditionRule.ChainedRules.Any() &&
				conditionRule.ChainedRules[0].RuleToChain is OneFromOptionsNotScaledWithLuckDropRule dropRule &&
				dropRule.dropIds.Contains(ItemID.BreakerBlade));
				if (rules.Any()) {
					OneFromOptionsNotScaledWithLuckDropRule rule = rules.First().ChainedRules[0].RuleToChain as OneFromOptionsNotScaledWithLuckDropRule;
					if (rule is not null) {
						Array.Resize(ref rule.dropIds, rule.dropIds.Length + 1);
						rule.dropIds[^1] = ModContent.ItemType<Thermite_Launcher>();
					} else {
						Origins.instance.Logger.Warn("Emblem drop rule not present on WoF");
					}
				} else {
					Origins.instance.Logger.Warn("Emblem drop rule not present on WoF");
				}
				break;
				default:
                break;
            }
        }
		public override void OnKill(NPC npc) {
            switch(npc.type) {
                case NPCID.SkeletronHead:
                if(!NPC.downedBoss3) GenFelnumOre();
                break;
                default:
                break;
            }
        }
		public override void ModifyGlobalLoot(GlobalLoot globalLoot) {
			foreach (var rule in globalLoot.Get()) {
				if ((rule is ItemDropWithConditionRule conditionalRule) && conditionalRule.condition is Conditions.SoulOfNight) {
					conditionalRule.condition = new LootConditions.SoulOfNight();
				}
			}
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Defiled_Key>(), 2500, 1, 1, new LootConditions.Defiled_Key_Condition()));
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Riven_Key>(), 2500, 1, 1, new LootConditions.Riven_Key_Condition()));
		}

		static void GenFelnumOre() {
            string text = "The clouds have been blessed with Felnum.";
			if (Main.netMode == NetmodeID.SinglePlayer) {
                Main.NewText(text, Colors.RarityPurple);
			}else if (Main.netMode == NetmodeID.Server) {
                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), Colors.RarityPurple);
			}
            if(!Main.gameMenu && Main.netMode != NetmodeID.MultiplayerClient) {
                int x = 0, y = 0;
                int felnumOre = ModContent.TileType<Felnum_Ore>();
                int type;
                Tile tile;
                int fails = 0;
                int success = 0;
				int maxFeln = (int)((Main.maxTilesX * Main.maxTilesY) * (Main.expertMode ? 6E-06 : 4E-06));
				for (int k = 0; k < maxFeln; k++) {
                    int tries = 0;
                    type = TileID.BlueDungeonBrick;
                    while(type!=TileID.Cloud&&type!=TileID.Dirt&&type!=TileID.Grass&&type!=TileID.Stone&&type!=TileID.RainCloud) {
				        x = WorldGen.genRand.Next(0, Main.maxTilesX);
						y = WorldGen.genRand.Next(90, (int)OriginSystem.worldSurfaceLow - 5);
                        tile = Framing.GetTileSafely(x, y);
                        type = tile.HasTile?tile.TileType:TileID.BlueDungeonBrick;
                        if(++tries >= 150) {
                            if(++fails%2==0)k--;
                            success--;
                            type = TileID.Dirt;
                        }
                    }
                    success++;
				    GenRunners.FelnumRunner(x, y, WorldGen.genRand.Next(2, 6), WorldGen.genRand.Next(2, 6), felnumOre);
			    }
                //Main.NewText($"generation complete, ran {runCount} times with {fails} fails");
            }
        }
    }
}
