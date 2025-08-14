using AlchemistNPCLite.NPCs;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables;
using Origins.Items.Other.LootBags;
using Origins.NPCs;
using Origins.Questing;
using Origins.Tiles;
using Origins.Tiles.Ashen;
using Origins.Tiles.Dawn;
using Origins.Tiles.Defiled;
using Origins.Tiles.Endowood;
using Origins.Tiles.Limestone;
using Origins.Tiles.Marrowick;
using Origins.Tiles.MusicBoxes;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using static Origins.NPCs.Boss_Tracker;

namespace Origins.CrossMod.AlchemistNPC.NPCs {
	[ExtendsFromMod(nameof(AlchemistNPCLite))]
	public class OAlchemistGlobalNPC : GlobalNPC {
		public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.ModNPC?.Mod is AlchemistNPCLite.AlchemistNPCLite;
		public override void ModifyShop(NPCShop shop) {
			if (shop.NpcType == NPCType<Musician>()) {
				if (shop.Name == Musician.Sh5) {
					shop.Add(new Item(Music_Box.ItemType<Music_Box_DW>()) { shopCustomPrice = 100000 }, Condition.Hardmode);
					shop.Add(new Item(Music_Box.ItemType<Music_Box_DC>()) { shopCustomPrice = 100000 }, Condition.Hardmode);
					shop.Add(new Item(Music_Box.ItemType<Music_Box_RH>()) { shopCustomPrice = 100000 }, Condition.Hardmode);
					shop.Add(new Item(Music_Box.ItemType<Music_Box_BP>()) { shopCustomPrice = 100000 }, Condition.Hardmode);
					shop.Add(new Item(Music_Box.ItemType<Music_Box_FU>()) { shopCustomPrice = 100000 }, Condition.Hardmode);
					shop.Add(new Item(Music_Box.ItemType<Music_Box_TD>()) { shopCustomPrice = 100000 }, Condition.Hardmode);
					shop.Add(new Item(Music_Box.ItemType<Otherworldly_Music_Box_DW>()) { shopCustomPrice = 100000 }, Condition.Hardmode);
					shop.Add(new Item(Music_Box.ItemType<Ancient_Music_Box_DW>()) { shopCustomPrice = 100000 }, Condition.Hardmode);
					shop.Add(new Item(Music_Box.ItemType<Ancient_Music_Box_RH>()) { shopCustomPrice = 100000 }, Condition.Hardmode);
					shop.Add(new Item(Music_Box.ItemType<Ancient_Music_Box_BP>()) { shopCustomPrice = 100000 }, Quest.QuestCondition<Old_Brine_Music_Box_Quest>());
				}
			}
			if (shop.NpcType == NPCType<Alchemist>()) {
				if (shop.Name == Alchemist.BaseShop) {
					shop.InsertAfter(ItemID.Ichor, new Item(ItemType<Black_Bile>()) { shopCustomPrice = 7500 }, Condition.Hardmode);
					shop.InsertAfter(ItemType<Black_Bile>(), new Item(ItemType<Alkahest>()) { shopCustomPrice = 7500 }, Condition.Hardmode);
					shop.InsertAfter(ItemType<Alkahest>(), new Item(ItemType<Respyrite>()) { shopCustomPrice = 7500 }, Condition.Hardmode);
				}
				if (shop.Name == Alchemist.PlantShop) {
					shop.InsertAfter(ItemID.Deathweed, new Item(ItemType<Wilting_Rose_Item>()) { shopCustomPrice = 2500 });
					shop.InsertAfter(ItemType<Wilting_Rose_Item>(), new Item(ItemType<Wrycoral_Item>()) { shopCustomPrice = 2500 });
					shop.InsertAfter(ItemType<Wrycoral_Item>(), new Item(ItemType<Surveysprout_Item>()) { shopCustomPrice = 2500 });
					shop.InsertAfter(ItemID.ViciousMushroom, new Item(ItemType<Soulspore_Item>()) { shopCustomPrice = 1000 }, Condition.DownedEowOrBoc);
					shop.InsertAfter(ItemType<Soulspore_Item>(), new Item(ItemType<Acetabularia_Item>()) { shopCustomPrice = 1000 }, Condition.DownedEowOrBoc);
					shop.InsertAfter(ItemType<Acetabularia_Item>(), new Item(ItemType<Fungarust_Item>()) { shopCustomPrice = 1000 }, Condition.DownedEowOrBoc);
				}
			}
			if (shop.NpcType == NPCType<Operator>()) {
				switch (shop.Name) {
					case Operator.ModMaterialShop:
					shop.Add(new Item(ItemType<Lost_Ore_Item>()) { shopCustomPrice = 1500 });
					shop.Add(new Item(ItemType<Undead_Chunk>()) { shopCustomPrice = 10000 });
					shop.Add(new Item(ItemType<Strange_String>()) { shopCustomPrice = 10000 });
					shop.Add(new Item(ItemType<Encrusted_Ore_Item>()) { shopCustomPrice = 1500 });
					shop.Add(new Item(ItemType<Riven_Carapace>()) { shopCustomPrice = 10000 });
					shop.Add(new Item(ItemType<Bud_Barnacle>()) { shopCustomPrice = 10000 });
					shop.Add(new Item(ItemType<Sanguinite_Ore_Item>()) { shopCustomPrice = 1500 });
					shop.Add(new Item(ItemType<NE8>()) { shopCustomPrice = 10000 });
					shop.Add(new Item(ItemType<Biocomponent10>()) { shopCustomPrice = 10000 });
					shop.Add(new Item(ItemType<Nova_Fragment>()) { shopCustomPrice = 100000 }, Condition.DownedMoonLord);
					break;

					case Operator.Bags2Shop:
					shop.Add(new Item(ItemType<Defiled_Amalgamation_Bag>()) { shopCustomPrice = 500000 }, Condition.DownedEowOrBoc.And(Condition.InExpertMode));
					shop.Add(new Item(ItemType<World_Cracker_Bag>()) { shopCustomPrice = 500000 }, Condition.DownedEowOrBoc.And(Condition.InExpertMode));
					//shop.Add(new Item(ItemType<Trenchmaker_Bag>()) { shopCustomPrice = 500000 }, Condition.DownedEowOrBoc.And(Condition.InExpertMode)); // for ashen update
					shop.Add(new Item(ItemType<Fiberglass_Weaver_Bag>()) { shopCustomPrice = 1000000 }, Conditions[nameof(Boss_Tracker.downedFiberglassWeaver)]);
					shop.Add(new Item(ItemType<Shimmer_Construct_Bag>()) { shopCustomPrice = 1650000 }, Conditions[nameof(Boss_Tracker.downedShimmerConstruct)]);
					shop.Add(new Item(ItemType<Lost_Diver_Bag>()) { shopCustomPrice = 1500000 }, Conditions[nameof(Boss_Tracker.downedLostDiver)]);
					break;
				}
			}
			if (shop.NpcType == NPCType<Architect>()) {
				if (shop.Name == Architect.Filler) {
					shop.InsertAfter(ItemID.Shadewood, new Item(ItemType<Endowood_Item>()) { shopCustomPrice = 10 });
					shop.InsertAfter(ItemType<Endowood_Item>(), new Item(ItemType<Marrowick_Item>()) { shopCustomPrice = 10 });
					//shop.InsertAfter(ItemType<Marrowick_Item>(), new Item(ItemType<>) { shopCustomPrice = 10}); // the wood of the ashen
					//shop.InsertAfter(ItemID.AshWood, new Item(ItemType<Eden_Wood_Item>()) { shopCustomPrice = 5 }); // when the dawn is added
					shop.InsertAfter(ItemID.CrimstoneBlock, new Item(ItemType<Defiled_Stone_Item>()) { shopCustomPrice = 5 });
					shop.InsertAfter(ItemType<Defiled_Stone_Item>(), new Item(ItemType<Riven_Flesh_Item>()) { shopCustomPrice = 5 });
					//shop.InsertAfter(ItemType<Riven_Flesh_Item>(), new Item(ItemType<>) { shopCustomPrice = 5}); // the stone of the ashen
					shop.InsertAfter(ItemID.CrimsandBlock, new Item(ItemType<Defiled_Sand_Item>()) { shopCustomPrice = 5 });
					shop.InsertAfter(ItemType<Defiled_Sand_Item>(), new Item(ItemType<Silica_Item>()) { shopCustomPrice = 5 });
					//shop.InsertAfter(ItemType<Silica_Item>(), new Item(ItemType<>()) { shopCustomPrice = 5 }); // the sand of the ashen
					shop.InsertAfter(ItemID.Granite, new Item(ItemType<Limestone_Item>()) { shopCustomPrice = 50 });
					//shop.InsertAfter(ItemID.RainCloud, new Item(ItemType<Angelic_Cloud_Item>()) { shopCustomPrice = 100}); // when the dawn is added
				}
				if (shop.Name == Architect.Building) {
					//shop.InsertAfter(ItemID.GrayBrick, new Item(ItemType<Harmony_Brick_Item>()) { shopCustomPrice = 40 }); // when the dawn is added
					/* shop.InsertAfter(ItemID.CrimstoneBrick, new Item(ItemType<>()) { shopCustomPrice = 10}); // Defiled Brick
					shop.InsertAfter(ItemType<>(), new Item(ItemType<>()) { shopCustomPrice = 10 }); // Riven Brick
					//shop.InsertAfter(ItemType<>(), new Item(ItemType<>()) { shopCustomPrice = 10 }); // Ashen Brick
					*/
				}
				if (shop.Name == Architect.Torch) {
					shop.InsertAfter(ItemID.YellowTorch, new Item(ItemType<Shadow_Torch>()) { shopCustomPrice = 300 }, Condition.Hardmode);
					shop.InsertAfter(ItemID.IchorTorch, new Item(ItemType<Bile_Torch>()) { shopCustomPrice = 300 });
					shop.InsertAfter(ItemType<Bile_Torch>(), new Item(ItemType<Alkahest_Torch>()) { shopCustomPrice = 300 });
					//shop.InsertAfter(ItemType<Alkahest_Torch>(), new Item(ItemType<Respyrite_Torch>()) { shopCustomPrice = 300 }); // the ashen substance torch
					shop.InsertAfter(ItemID.CrimsonTorch, new Item(ItemType<Defiled_Torch>()) { shopCustomPrice = 300 });
					shop.InsertAfter(ItemType<Defiled_Torch>(), new Item(ItemType<Riven_Torch>()) { shopCustomPrice = 300 });
					//shop.InsertAfter(ItemType<Riven_Torch>(), new Item(ItemType<Ashen_Torch>()) { shopCustomPrice = 300 }); // the ashen torch
				}
				if (shop.Name == Architect.Candle) {
					List<int> items = [
						FurnitureSet.Get<Endowood_Furniture, FurnitureSet_Candle>().Item.Type,
						FurnitureSet.Get<Marrowick_Furniture, FurnitureSet_Candle>().Item.Type,
						//FurnitureSet.Get<_Furniture, FurnitureSet_Candle>().Item.Type,
						FurnitureSet.Get<Limestone_Furniture, FurnitureSet_Candle>().Item.Type];

					shop.InsertAfter(ItemID.ShadewoodCandle, new Item(items[0]) { shopCustomPrice = 500 });
					shop.InsertAfter(items[0], new Item(items[1]) { shopCustomPrice = 500 });
					//shop.InsertAfter(items[1], new Item(items[2]) { shopCustomPrice = 500 }); // the ashen wood candle
					shop.InsertAfter(ItemID.GraniteCandle, new Item(items[2]) { shopCustomPrice = 500 });
				}
				if (shop.Name == Architect.Lamp) {
					List<int> items = [
						FurnitureSet.Get<Endowood_Furniture, FurnitureSet_Lamp>().Item.Type,
						FurnitureSet.Get<Marrowick_Furniture, FurnitureSet_Lamp>().Item.Type,
						//FurnitureSet.Get<_Furniture, FurnitureSet_Lamp>().Item.Type,
						FurnitureSet.Get<Limestone_Furniture, FurnitureSet_Lamp>().Item.Type];

					shop.InsertAfter(ItemID.ShadewoodLamp, new Item(items[0]) { shopCustomPrice = 500 });
					shop.InsertAfter(items[0], new Item(items[1]) { shopCustomPrice = 500 });
					//shop.InsertAfter(items[1], new Item(items[2]) { shopCustomPrice = 500 }); // the ashen wood candle
					shop.InsertAfter(ItemID.GraniteLamp, new Item(items[2]) { shopCustomPrice = 500 });
				}
				if (shop.Name == Architect.Lantern) {
					List<int> items = [
						FurnitureSet.Get<Endowood_Furniture, FurnitureSet_Lantern>().Item.Type,
						FurnitureSet.Get<Marrowick_Furniture, FurnitureSet_Lantern>().Item.Type,
						//FurnitureSet.Get<_Furniture, FurnitureSet_Lantern>().Item.Type,
						FurnitureSet.Get<Limestone_Furniture, FurnitureSet_Lantern>().Item.Type];

					shop.InsertAfter(ItemID.ShadewoodLantern, new Item(items[0]) { shopCustomPrice = 500 });
					shop.InsertAfter(items[0], new Item(items[1]) { shopCustomPrice = 500 });
					//shop.InsertAfter(items[1], new Item(items[2]) { shopCustomPrice = 500 }); // the ashen wood candle
					shop.InsertAfter(ItemID.GraniteLantern, new Item(items[2]) { shopCustomPrice = 500 });
				}
				if (shop.Name == Architect.Chandelier) {
					List<int> items = [
						FurnitureSet.Get<Endowood_Furniture, FurnitureSet_Chandelier>().Item.Type,
						FurnitureSet.Get<Marrowick_Furniture, FurnitureSet_Chandelier>().Item.Type,
						//FurnitureSet.Get<_Furniture, FurnitureSet_Chandelier>().Item.Type,
						FurnitureSet.Get<Limestone_Furniture, FurnitureSet_Chandelier>().Item.Type];

					shop.InsertAfter(ItemID.ShadewoodChandelier, new Item(items[0]) { shopCustomPrice = 1200 });
					shop.InsertAfter(items[0], new Item(items[1]) { shopCustomPrice = 1200 });
					//shop.InsertAfter(items[1], new Item(items[2]) { shopCustomPrice = 1200 }); // the ashen wood candle
					shop.InsertAfter(ItemID.GraniteChandelier, new Item(items[2]) { shopCustomPrice = 1200 });
				}
				if (shop.Name == Architect.Candelabra) {
					List<int> items = [
						FurnitureSet.Get<Endowood_Furniture, FurnitureSet_Candelabra>().Item.Type,
						FurnitureSet.Get<Marrowick_Furniture, FurnitureSet_Candelabra>().Item.Type,
						//FurnitureSet.Get<_Furniture, FurnitureSet_Candelabra>().Item.Type,
						FurnitureSet.Get<Limestone_Furniture, FurnitureSet_Candelabra>().Item.Type];

					shop.InsertAfter(ItemID.ShadewoodCandelabra, new Item(items[0]) { shopCustomPrice = 500 });
					shop.InsertAfter(items[0], new Item(items[1]) { shopCustomPrice = 500 });
					//shop.InsertAfter(items[1], new Item(items[2]) { shopCustomPrice = 500 }); // the ashen wood candle
					shop.InsertAfter(ItemID.GraniteCandelabra, new Item(items[2]) { shopCustomPrice = 500 });
				}
			}
			if (shop.NpcType == NPCType<Jeweler>()) {
				if (shop.Name == Jeweler.Other) {
					shop.InsertAfter(ItemID.BandofStarpower, new Item(ItemType<Dim_Starlight>()) { shopCustomPrice = 30000 }, Condition.DownedEowOrBoc);
					shop.InsertAfter(ItemID.BandofRegeneration, new Item(ItemType<Bomb_Charm>()) { shopCustomPrice = 50000 }, Condition.DownedEowOrBoc);
					shop.InsertAfter(ItemType<Bomb_Charm>(), new Item(ItemType<Lightning_Ring>()) { shopCustomPrice = 55000 }, Conditions[nameof(Boss_Tracker.downedShimmerConstruct)]);
					shop.InsertAfter(ItemID.Diamond, new Item(ItemType<Chambersite_Item>()) { shopCustomPrice = 9000 }, Condition.Hardmode);
				}
				if (shop.Name == Jeweler.Arena) { 
					shop.InsertAfter(ItemID.DartTrap, new Item(ItemType<Bomb_Trap_Item>()) { shopCustomPrice = 30000 }, Condition.DownedSkeletron);
				}
			}
			if (shop.NpcType == NPCType<YoungBrewer>()) {
				if (shop.Name == YoungBrewer.Shop2) {
					shop.InsertAfter(ItemID.FlaskofCursedFlames, new Item(ItemType<Bile_Flask>()) { shopCustomPrice = 25000 }, Condition.Hardmode);
					shop.InsertAfter(ItemType<Bile_Flask>(), new Item(ItemType<Salt_Flask>()) { shopCustomPrice = 25000 }, Condition.Hardmode);
					//shop.InsertAfter(ItemType<Salt_Flask>(), new Item(ItemType<>()) { shopCustomPrice = 25000 }, Condition.Hardmode); // the ashen substance flask
				}
			}
			if (shop.NpcType == NPCType<Brewer>()) {
				switch (shop.Name) {
					case Brewer.SHOP_2: {
						shop.Add(new Item(ItemType<Absorption_Potion>()) { shopCustomPrice = 10000 }, OriginGlobalNPC.PeatSoldCondition(350));
						shop.Add(new Item(ItemType<Fervor_Potion>()) { shopCustomPrice = 25000 }, Condition.DownedEowOrBoc);
						shop.Add(new Item(ItemType<Protean_Potion>()) { shopCustomPrice = 25000 }, Condition.DownedEowOrBoc);
						shop.Add(new Item(ItemType<Ambition_Potion>()) { shopCustomPrice = 25000 }, Condition.DownedEowOrBoc);
						shop.Add(new Item(ItemType<Fervor_Potion>()) { shopCustomPrice = 25000 }, Condition.DownedEowOrBoc);
						shop.Add(new Item(ItemType<Antisolve_Potion>()) { shopCustomPrice = 25000 }, Condition.Hardmode);
						shop.Add(new Item(ItemType<Focus_Potion>()) { shopCustomPrice = Item.buyPrice(gold: 2, silver: 50) }, Condition.Hardmode);
						shop.Add(new Item(ItemType<Greater_Summoning_Potion>()) { shopCustomPrice = 25000 }, OriginsModIntegrations.NotAprilFools.And(Condition.Hardmode));
						shop.Add(new Item(ItemType<Greater_Summoning_Potato>()) { shopCustomPrice = 25000 }, OriginsModIntegrations.AprilFools.And(Condition.Hardmode));
						shop.Add(new Item(ItemType<Purification_Potion>()) { shopCustomPrice = Item.buyPrice(gold: 4, silver: 50) }, Condition.DownedGolem);
						shop.Add(new Item(ItemType<Voidsight_Potion>()) { shopCustomPrice = 25000 }, Condition.Hardmode);
						break;
					}
				}
			}
			if (shop.NpcType == NPCType<Tinkerer>()) {
				if (shop.Name == Tinkerer.Shop1) {
					shop.InsertAfter(ItemID.ShinyRedBalloon, new Item(ItemType<Feathery_Crest>()) { shopCustomPrice = 50000 });
					shop.InsertAfter(ItemType<Feathery_Crest>(), new Item(ItemType<Superjump_Cape>()) { shopCustomPrice = 50000 });
					shop.Add(new Item(ItemType<Plasma_Bag>()) { shopCustomPrice = 45000 });
					shop.Add(new Item(ItemType<Fallacious_Vase>()) { shopCustomPrice = 150000 });
				}
				if (shop.Name == Tinkerer.Shop2) {
					shop.InsertBefore(ItemID.WhiteString, new Item(ItemType<Comb>()) { shopCustomPrice = 15000 });
					shop.InsertAfter(ItemID.SummonerEmblem, new Item(ItemType<Exploder_Emblem>()) { shopCustomPrice = 250000 }, Condition.DownedMechBossAll);
					shop.InsertAfter(ItemID.TitanGlove, new Item(ItemType<Resizing_Glove>()) { shopCustomPrice = 250000 }, Condition.Hardmode);
					shop.InsertAfter(ItemID.MagmaStone, new Item(ItemType<Messy_Leech>()) { shopCustomPrice = 150000 }, Condition.DownedQueenBee);
					shop.InsertAfter(ItemType<Messy_Leech>(), new Item(ItemType<Symbiote_Skull>()) { shopCustomPrice = 50000 }, Condition.DownedEowOrBoc);
					shop.InsertAfter(ItemType<Symbiote_Skull>(), new Item(ItemType<Venom_Fang>()) { shopCustomPrice = 15000 }, Conditions[nameof(Boss_Tracker.downedLostDiver)]);
					shop.InsertAfter(ItemID.NecromanticScroll, new Item(ItemType<Priority_Mail>()) { stack = 2, shopCustomPrice = 100000 }, Condition.Hardmode);
					shop.Add(new Item(ItemType<Mildew_Heart>()) { shopCustomPrice = 200000 }, Conditions[nameof(Boss_Tracker.downedLostDiver)]);
				}
			}
		}
	}
}