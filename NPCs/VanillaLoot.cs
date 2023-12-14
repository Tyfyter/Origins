using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Pets;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.LootConditions;
using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
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
			switch (npc.netID) {
				case NPCID.CaveBat:
				case NPCID.GiantBat:
				case NPCID.IceBat:
				case NPCID.IlluminantBat:
				case NPCID.JungleBat:
				case NPCID.VampireBat:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bat_Hide>(), 3, 1, 3));
				break;
				case NPCID.SkeletonSniper: //Tiny skeleton sniper
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Tiny_Sniper>(), 24));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bread>(), 5));
                break;
				case NPCID.Snatcher:
				case NPCID.JungleSlime:
				case NPCID.SpikedJungleSlime:
				case NPCID.MossHornet:
				case NPCID.BigMossHornet:
				case NPCID.GiantMossHornet:
				case NPCID.LittleMossHornet:
				case NPCID.TinyMossHornet:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Peat_Moss>(), 2));
				break;
				case NPCID.AngryBones:
				case NPCID.AngryBonesBig:
				case NPCID.AngryBonesBigMuscle:
				case NPCID.AngryBonesBigHelmet:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bolt_Gun>(), 50));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Longbone>(), 50));
				break;
				case NPCID.SkeletronPrime:
				case NPCID.TheDestroyer:
				case NPCID.TheDestroyerBody:
				case NPCID.TheDestroyerTail:
				case NPCID.Retinazer:
				case NPCID.Spazmatism:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Busted_Servo>(), 1, 8, 37));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Power_Core>(), 1, 1, 2));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Rotor>(), 1, 5, 22));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_Power_Up>(), 50));
				break;
				case NPCID.GoblinArcher:
				case NPCID.GoblinPeon:
				case NPCID.GoblinScout:
				case NPCID.GoblinSorcerer:
				case NPCID.GoblinWarrior:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Harpoon_Gun>(), 200));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Harpoon>(), 2, 1, 2));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Potato>(), 34));
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
				case NPCID.WyvernHead:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Startillery>(), 12));
				break;
				case NPCID.Clown:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Happy_Bomb>(), 1, 69));
				break;
				case NPCID.PurpleSlime:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Plasma_Phial>(), 2));
				break;
				case NPCID.AnglerFish:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Rebreather>(), 20));
				break;
				case NPCID.BloodCrawler:
				case NPCID.BloodCrawlerWall:
				case NPCID.FaceMonster:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Explosive_Artery>(), 87));
				break;
				case NPCID.UndeadMiner:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<IWTPA_Standard>(), 4));
				break;
				case NPCID.SporeSkeleton:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Irish_Cheddar>(), 3));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bread>(), 5));
				break;
				case NPCID.GiantTortoise:
				npcLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Rocodile>(), 17, 1, 1, new LootConditions.DownedPlantera()));
				break;
				case NPCID.Skeleton:
				case NPCID.SkeletonAlien:
				case NPCID.SkeletonArcher:
				case NPCID.SkeletonAstonaut:
				case NPCID.SkeletonCommando:
				case NPCID.SkeletonMerchant:
				case NPCID.SkeletonTopHat:
				case NPCID.ArmoredSkeleton:
				case NPCID.BigHeadacheSkeleton:
				case NPCID.BigMisassembledSkeleton:
				case NPCID.BigPantlessSkeleton:
				case NPCID.BigSkeleton:
				case NPCID.BoneThrowingSkeleton:
				case NPCID.BoneThrowingSkeleton2:
				case NPCID.BoneThrowingSkeleton3:
				case NPCID.BoneThrowingSkeleton4:
				case NPCID.GreekSkeleton:
				case NPCID.HeadacheSkeleton:
				case NPCID.HeavySkeleton:
				case NPCID.MisassembledSkeleton:
				case NPCID.PantlessSkeleton:
				case NPCID.SmallHeadacheSkeleton:
				case NPCID.SmallMisassembledSkeleton:
				case NPCID.SmallPantlessSkeleton:
				case NPCID.SmallSkeleton:
				case NPCID.TacticalSkeleton:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bread>(), 5));
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
				case NPCID.TheGroom:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Comb>()));
				break;
				case NPCID.MoonLordCore:
				npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Third_Eye>(), 4));
				break;
				default:
				break;
			}
			switch (npc.type) {
				case NPCID.DemonEye:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Eyeball_Staff>(), 63));
				break;
			}
		}
		public override void OnKill(NPC npc) {
			switch (npc.type) {
				case NPCID.SkeletronHead:
				if (!NPC.downedBoss3) {
					GenFelnumOre();
					OriginSystem.Instance.forceThunderstormDelay = Main.rand.Next(600, (int)(Main.dayLength / 2));
				}
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
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Dawn_Key>(), 2500, 1, 1, new LootConditions.Dawn_Key_Condition()));
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Defiled_Key>(), 2500, 1, 1, new LootConditions.Defiled_Key_Condition()));
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Dusk_Key>(), 2500, 1, 1, new LootConditions.Dusk_Key_Condition()));
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Hell_Key>(), 2500, 1, 1, new LootConditions.Hell_Key_Condition()));
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Mushroom_Key>(), 2500, 1, 1, new LootConditions.Mushroom_Key_Condition()));
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Ocean_Key>(), 2500, 1, 1, new LootConditions.Ocean_Key_Condition()));
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Riven_Key>(), 2500, 1, 1, new LootConditions.Riven_Key_Condition()));
		}

		static void GenFelnumOre() {
			string text = "The clouds have been blessed with Felnum!";
			if (Main.netMode == NetmodeID.SinglePlayer) {
				Main.NewText(text, Colors.RarityGreen);
			} else if (Main.netMode == NetmodeID.Server) {
				ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), Colors.RarityGreen);
			}
			if (!Main.gameMenu && Main.netMode != NetmodeID.MultiplayerClient) {
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
					while (type != TileID.Cloud && type != TileID.Dirt && type != TileID.Grass && type != TileID.Stone && type != TileID.RainCloud) {
						x = WorldGen.genRand.Next(0, Main.maxTilesX);
						y = WorldGen.genRand.Next(90, (int)OriginSystem.worldSurfaceLow - 5);
						tile = Framing.GetTileSafely(x, y);
						type = tile.HasTile ? tile.TileType : TileID.BlueDungeonBrick;
						if (++tries >= 150) {
							if (++fails % 2 == 0) k--;
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
