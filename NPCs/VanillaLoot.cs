using Microsoft.Xna.Framework.Audio;
using Origins.Buffs;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Pets;
using Origins.Items.Weapons;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.Tiles.Brine;
using Origins.Tiles.Other;
using Origins.World;
using PegasusLib;
using System;
using System.Collections.Generic;
using System.Data;
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
		static OneFromOptionsDropRule _eaterOfWorldsWeaponDrops;
		public static OneFromOptionsDropRule EaterOfWorldsWeaponDrops => _eaterOfWorldsWeaponDrops ??=  new(1, 1, ModContent.ItemType<Rotting_Worm_Staff>(), ModContent.ItemType<Eaterboros>());
		public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot) {
			static LocalizedText GetWarningText(string key) => Language.GetText("Mods.Origins.Warnings." + key);
			List<IItemDropRule> dropRules = npcLoot.Get(false);
			switch (npc.netID) {
				case NPCID.BrainofCthulhu:
				npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Weakpoint_Analyzer>(), 4));
				break;
				case NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail:
				npcLoot.Add(new LeadingConditionRule(new Conditions.LegacyHack_IsABoss())).WithOnSuccess(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Forbidden_Voice>(), 4));
				npcLoot.Add(new LeadingConditionRule(new Conditions.LegacyHack_IsBossAndNotExpert()).WithOnSuccess(EaterOfWorldsWeaponDrops));
				break;
				case NPCID.KingSlime:
				npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Cursed_Crown>(), 4));
				break;
				case NPCID.EyeofCthulhu:
				npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Strange_Tooth>(), 4));
				break;
				case NPCID.SkeletronHead:
				npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Terrarian_Voodoo_Doll>(), 4));
				break;
				case NPCID.QueenBee: {
					npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Emergency_Bee_Canister>(), 4));
					if (!AddToOneFromOptionsRule(dropRules, ItemID.BeeGun, ModContent.ItemType<Bee_Afraid_Incantation>())) {
						Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Weapon"), Lang.GetNPCName(npc.netID)));
					}
					break;
				}
				case NPCID.Deerclops:
				npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Blizzardwalkers_Jacket>(), 4));
				break;
				case NPCID.SkeletronPrime:
				case NPCID.TheDestroyer:
				case NPCID.TheDestroyerBody:
				case NPCID.TheDestroyerTail:
				case NPCID.Retinazer:
				case NPCID.Spazmatism:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Busted_Servo>(), 1, 8, 37));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Power_Core>(), 1, 1, 3));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Rotor>(), 1, 5, 22));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Strange_Power_Up>(), 50));
				switch (npc.type) {
					case NPCID.SkeletronPrime:
					npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Retool_Arm_Cannon>(), 4));
					break;
				}
				break;
				case NPCID.HallowBoss:
				npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Glitter_Glue>(), 4));
				break;
				case NPCID.CultistBoss:
				npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Lunatics_Rune>(), 4));
				break;
				case NPCID.MoonLordCore:
				npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Third_Eye>(), 4));
				break;
				case NPCID.WallofFlesh: {
					npcLoot.Add(ItemDropRule.MasterModeDropOnAllPlayers(ModContent.ItemType<Scribe_of_the_Meat_God>(), 4));
					if (!AddToOneFromOptionsRule(dropRules, ItemID.WarriorEmblem, ModContent.ItemType<Exploder_Emblem>())) {
						Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Emblem"), Lang.GetNPCName(npc.netID)));
					}
					if (!AddToOneFromOptionsRule(dropRules, ItemID.BreakerBlade, ModContent.ItemType<Thermite_Launcher>())) {
						Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Weapon"), Lang.GetNPCName(npc.netID)));
					}
					break;
				}
				case NPCID.DukeFishron: {
					if (!AddToOneFromOptionsRule(dropRules, ItemID.BubbleGun, ModContent.ItemType<Sharknade_O>())) {
						Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Weapon"), Lang.GetNPCName(npc.netID)));
					}
					if (!AddToOneFromOptionsRule(dropRules, ItemID.AquaScepter, ModContent.ItemType<Sharknade_O>())) {
						Origins.LogLoadingWarning(GetWarningText("MissingDropRule").WithFormatArgs(GetWarningText("DropRuleType.Weapon"), Lang.GetNPCName(npc.netID)));
					}
					break;
				}

				case NPCID.CaveBat:
				case NPCID.GiantBat:
				case NPCID.IceBat:
				case NPCID.IlluminantBat:
				case NPCID.JungleBat:
				case NPCID.VampireBat:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bat_Hide>(), 1, 2, 4));
				break;
				case NPCID.SkeletonSniper: //Tiny skeleton sniper
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Tiny_Sniper>(), 24));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bread>(), 10));
				break;
				case NPCID.Snatcher:
				case NPCID.JungleSlime:
				case NPCID.SpikedJungleSlime:
				case NPCID.MossHornet:
				case NPCID.BigMossHornet:
				case NPCID.GiantMossHornet:
				case NPCID.LittleMossHornet:
				case NPCID.TinyMossHornet:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Peat_Moss_Item>(), 1, 3, 7));
				break;
				case NPCID.AngryBones:
				case NPCID.AngryBonesBig:
				case NPCID.AngryBonesBigMuscle:
				case NPCID.AngryBonesBigHelmet:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bolt_Gun>(), 50));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bread>(), 10));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Longbone>(), 50));
				break;
				case NPCID.Harpy:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Potato>(), 13));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Feathery_Crest>(), 30));
				break;
				case NPCID.LostGirl:
				case NPCID.Nymph:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Potato>()));
				break;
				case NPCID.Wolf:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Vanilla_Shake>(), 21));
				break;
				case NPCID.WyvernHead:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Startillery>(), 12));
				break;
				case NPCID.Clown:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Happy_Bomb>(), 1, 69, 69));
				break;
				case NPCID.PurpleSlime:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Plasma_Bag>(), 10));
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
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bread>(), 10));
				break;
				case NPCID.SporeSkeleton:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Irish_Cheddar>(), 6));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bread>(), 10));
				break;
				case NPCID.GiantTortoise:
				npcLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Rocodile>(), 17, 1, 1, new LootConditions.DownedPlantera()));
				break;
				case NPCID.Skeleton:
				case NPCID.SkeletonAlien:
				case NPCID.SkeletonArcher:
				case NPCID.SkeletonAstonaut:
				case NPCID.SkeletonCommando:
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
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bread>(), 10));
				break;
				case NPCID.SkeletonMerchant:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Dysfunctional_Endless_Explosives_Bag>(), 5));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Bread>(), 10));
				break;
				case NPCID.TheGroom:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Comb>()));
				break;
				case NPCID.ZombieSuperman:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Superjump_Cape>(), 3));
				//npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Well_Gelled_Heroes_Hair>(), 3));
				break;
				case NPCID.MaggotZombie:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Grave_Danger>(), 20));
				break;
				case NPCID.LunarTowerNebula or NPCID.LunarTowerSolar or NPCID.LunarTowerStardust or NPCID.LunarTowerVortex: {
					DropOneByOne.Parameters normalParameters = default;
					normalParameters.MinimumItemDropsCount = 4;
					normalParameters.MaximumItemDropsCount = 6;
					normalParameters.ChanceNumerator = 1;
					normalParameters.ChanceDenominator = 1;
					normalParameters.MinimumStackPerChunkBase = 1;
					normalParameters.MaximumStackPerChunkBase = 3;
					normalParameters.BonusMinDropsPerChunkPerPlayer = 0;
					normalParameters.BonusMaxDropsPerChunkPerPlayer = 0;

					DropOneByOne.Parameters expertParameters = normalParameters;
					expertParameters.BonusMinDropsPerChunkPerPlayer = 0;
					expertParameters.BonusMaxDropsPerChunkPerPlayer = 1;
					expertParameters.MinimumStackPerChunkBase = (int)(expertParameters.MinimumStackPerChunkBase * 1.25f);
					expertParameters.MaximumStackPerChunkBase = (int)(expertParameters.MaximumStackPerChunkBase * 1.25f);
					int itemType = ModContent.ItemType<Nova_Fragment>();
					npcLoot.Add(new DropBasedOnExpertMode(new DropOneByOne(itemType, normalParameters), new DropOneByOne(itemType, expertParameters)));
				}
				break;
			}
			bool alreadyAddedHandDrop = false;
			switch (NPCID.FromNetId(npc.netID)) {
				case NPCID.Zombie or NPCID.FemaleZombie or NPCID.BaldZombie or NPCID.PincushionZombie or NPCID.SlimedZombie or NPCID.SwampZombie or NPCID.TwiggyZombie or NPCID.TorchZombie:
				case NPCID.ZombieDoctor or NPCID.ZombieSuperman or NPCID.ZombiePixie or NPCID.ZombieXmas or NPCID.ZombieSweater:
				case NPCID.ZombieRaincoat or NPCID.ZombieEskimo or NPCID.ZombieMushroom or NPCID.ZombieMushroomHat or NPCID.BloodZombie:
				case NPCID.MaggotZombie:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Potato>(), 13));
				if (!alreadyAddedHandDrop) npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Inanimate_Zombie_Hand>(), 4000));
				break;

				case NPCID.ArmedTorchZombie or NPCID.ArmedZombie or NPCID.ArmedZombieCenx or NPCID.ArmedZombieEskimo or NPCID.ArmedZombiePincussion or NPCID.ArmedZombieSlimed or NPCID.ArmedZombieSwamp or NPCID.ArmedZombieTwiggy:
				alreadyAddedHandDrop = true;
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Inanimate_Zombie_Hand>(), 3000));
				goto case NPCID.MaggotZombie;

				case NPCID.TheBride or NPCID.TheGroom or NPCID.DoctorBones or NPCID.Eyezor:
				alreadyAddedHandDrop = true;
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Inanimate_Zombie_Hand>(), 100));
				goto case NPCID.MaggotZombie;

				case NPCID.DemonEye:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Eyeball_Staff>(), 63));
				break;

				case NPCID.Vampire:
				case NPCID.VampireBat:
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Vampire_Grenade>(), 5, 20, 30));
				break;
			}
			CommonDrop harpoonRule = null;
			foreach (IItemDropRule rule in npcLoot.Get(includeGlobalDrops: false)) {
				List<DropRateInfo> drops = [];
				DropRateInfoChainFeed ratesInfo = new();
				rule.ReportDroprates(drops, ratesInfo);
				if (drops.Count != 0 && drops[0].itemId == ItemID.Harpoon && rule is CommonDrop harp) {
					harpoonRule = harp;
				}
				if (harpoonRule is not null) break;//add any further replacements with &&
			}
			if (harpoonRule is not null) {
				harpoonRule.itemId = ModContent.ItemType<Harpoon_Gun>();
				harpoonRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Harpoon>(), 1, 15, 99));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Potato>(), 34));
			}
		}
		public static bool AddToOneFromOptionsRule(List<IItemDropRule> dropRules, int targetContains, params int[] items) {
			{
				if (dropRules.FindDropRule<OneFromOptionsDropRule>(dropRule => dropRule.dropIds?.Contains(targetContains) ?? false) is OneFromOptionsDropRule rule) {
					Array.Resize(ref rule.dropIds, rule.dropIds.Length + items.Length);
					for (int i = 0; i < items.Length; i++) {
						rule.dropIds[^(i + 1)] = items[i];
					}
					return true;
				}
			}
			{
				if (dropRules.FindDropRule<OneFromOptionsNotScaledWithLuckDropRule>(dropRule => dropRule.dropIds?.Contains(targetContains) ?? false) is OneFromOptionsNotScaledWithLuckDropRule rule) {
					Array.Resize(ref rule.dropIds, rule.dropIds.Length + items.Length);
					for (int i = 0; i < items.Length; i++) {
						rule.dropIds[^(i + 1)] = items[i];
					}
					return true;
				}
			}
			if (ModLoader.HasMod("CalamityMod") && AddToOneFromOptionsRuleCalamity(dropRules, targetContains, items)) return true;
			return false;
		}
		static Predicate<IItemDropRule> DropsItem(int itemType) => dropRule => {
			List<DropRateInfo> dropRates = [];
			dropRule.ReportDroprates(dropRates, new DropRateInfoChainFeed(1));
			for (int i = 0; i < dropRates.Count; i++) {
				if (dropRates[i].itemId == itemType) return true;
			}
			return false;
		};
		[JITWhenModsEnabled("CalamityMod")]
		static bool AddToOneFromOptionsRuleCalamity(List<IItemDropRule> dropRules, int targetContains, params int[] items) {
			if (dropRules.FindDropRule<CalamityMod.DropHelper.AllOptionsAtOnceWithPityDropRule>(DropsItem(targetContains)) is CalamityMod.DropHelper.AllOptionsAtOnceWithPityDropRule rule) {
				Array.Resize(ref rule.stacks, rule.stacks.Length + items.Length);
				for (int i = 0; i < items.Length; i++) {
					rule.stacks[^(i + 1)] = items[i];
				}
				return true;
			}
			return false;
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
			int shrapnelIndex = npc.FindBuffIndex(Impeding_Shrapnel_Debuff.ID);
			if (shrapnelIndex > -1) {
				Impeding_Shrapnel_Debuff.SpawnShrapnel(npc, npc.buffTime[shrapnelIndex]);
			}
		}
		public override void ModifyGlobalLoot(GlobalLoot globalLoot) {
			foreach (ItemDropWithConditionRule rule in globalLoot.Get().FindDropRules<ItemDropWithConditionRule>(rule => rule.condition is Conditions.SoulOfNight)) {
				rule.condition = new LootConditions.SoulOfNight();
			}
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Ashen_Key>(), 2500, 1, 1, new LootConditions.Ashen_Key_Condition()));
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Defiled_Key>(), 2500, 1, 1, new LootConditions.Defiled_Key_Condition()));
			//globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Hell_Key>(), 2500, 1, 1, new LootConditions.Hell_Key_Condition()));
			//globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Mushroom_Key>(), 2500, 1, 1, new LootConditions.Mushroom_Key_Condition()));
			//globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Ocean_Key>(), 2500, 1, 1, new LootConditions.Ocean_Key_Condition()));
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Riven_Key>(), 2500, 1, 1, new LootConditions.Riven_Key_Condition()));
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Brine_Key>(), 2500, 1, 1, new LootConditions.Brine_Key_Condition()));
			globalLoot.Add(new ItemDropWithConditionRule(ModContent.ItemType<Lost_Picture_Frame>(), 25, 1, 1, new LootConditions.Lost_Picture_Frame_Condition()));
			globalLoot.Add(ItemDropRule.Common(ModContent.ItemType<Generic_Weapon>(), 50000));
			globalLoot.Add(ItemDropRule.ByCondition(OriginsModIntegrations.AprilFools.ToDropCondition(ShowItemDropInUI.WhenConditionSatisfied), ModContent.ItemType<Silicon_Sword>(), 5000));
		}

		static void GenFelnumOre() {
			string text = Language.GetTextValue("Mods.Origins.Status_Messages.Felnum_Spawn");
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
						y = WorldGen.genRand.Next(90, (int)OriginSystem.WorldSurfaceLow - 5);
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
