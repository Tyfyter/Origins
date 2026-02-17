using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Other.Consumables.Medicine;
using Origins.Items.Tools.Liquids;
using Origins.Items.Weapons.Melee;
using Origins.Tiles.Ashen;
using Origins.Tiles.Brine;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Origins.OriginsSets.Items;

namespace Origins {
	public partial class OriginSystem : ModSystem {
		public override void AddRecipes() {
			#region Armor
			Recipe.Create(ItemID.MiningHelmet)
			.AddIngredient(ItemID.Glowstick, 4)
			.AddRecipeGroup(RecipeGroupID.IronBar, 7)
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemID.MiningShirt)
			.AddIngredient(ItemID.Leather, 15)
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemID.MiningPants)
			.AddIngredient(ItemID.Leather, 15)
			.AddTile(TileID.WorkBenches)
			.Register();

			Recipe.Create(ItemID.EskimoHood)
			.AddIngredient(ItemID.FlinxFur, 5)
			.AddIngredient(ItemID.Leather, 12)
			.AddTile(TileID.Loom)
			.Register();

			Recipe.Create(ItemID.EskimoCoat)
			.AddIngredient(ItemID.FlinxFur, 5)
			.AddIngredient(ItemID.Leather, 12)
			.AddTile(TileID.Loom)
			.Register();

			Recipe.Create(ItemID.EskimoPants)
			.AddIngredient(ItemID.FlinxFur, 5)
			.AddIngredient(ItemID.Leather, 12)
			.AddTile(TileID.Loom)
			.Register();

			Recipe.Create(ItemID.CrystalNinjaHelmet)
			.AddIngredient(ItemID.CrystalShard, 30)
			.AddIngredient(ItemID.SoulofLight, 5)
			.AddIngredient<Carburite_Item>(15)
			.AddTile(TileID.MythrilAnvil)
			.Register();

			Recipe.Create(ItemID.CrystalNinjaChestplate)
			.AddIngredient(ItemID.CrystalShard, 60)
			.AddIngredient(ItemID.SoulofLight, 7)
			.AddIngredient<Carburite_Item>(30)
			.AddTile(TileID.MythrilAnvil)
			.Register();

			Recipe.Create(ItemID.CrystalNinjaLeggings)
			.AddIngredient(ItemID.CrystalShard, 45)
			.AddIngredient(ItemID.SoulofLight, 3)
			.AddIngredient<Carburite_Item>(23)
			.AddTile(TileID.MythrilAnvil)
			.Register();
			#endregion
			#region Weapons
			Recipe.Create(ItemID.GoldShortsword)
			.AddIngredient(ItemID.EnchantedSword)
			.AddTile(TileID.BewitchingTable)
			.Register();

			Recipe.Create(ItemID.StylistKilLaKillScissorsIWish)
			.AddRecipeGroup(ALRecipeGroups.SilverBars, 2)
			.AddIngredient<Magic_Hair_Spray>(5)
			.AddIngredient<Rubber>(4)
			.AddTile(TileID.Anvils)
			.Register();

			#region Corruption
			Recipe.Create(ItemID.Musket)
			.AddIngredient(ItemID.DemoniteBar, 6)
			.AddIngredient(ItemID.ShadowScale, 4)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.BallOHurt)
			.AddIngredient(ItemID.DemoniteBar, 10)
			.AddIngredient(ItemID.ShadowScale, 5)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.Vilethorn)
			.AddRecipeGroup("Origins:Gem Staves")
			.AddIngredient(ItemID.DemoniteBar, 10)
			.AddIngredient(ItemID.ShadowScale, 6)
			.AddTile(TileID.Anvils)
			.Register();
			#endregion
			#region Crimson
			Recipe.Create(ItemID.TheUndertaker)
			.AddIngredient(ItemID.CrimtaneBar, 6)
			.AddIngredient(ItemID.TissueSample, 4)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.TheRottedFork)
			.AddIngredient(ItemID.CrimtaneBar, 9)
			.AddIngredient(ItemID.TissueSample, 5)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.CrimsonRod)
			.AddRecipeGroup("Origins:Gem Staves")
			.AddIngredient(ItemID.CrimtaneBar, 10)
			.AddIngredient(ItemID.TissueSample, 6)
			.AddTile(TileID.Anvils)
			.Register();
			#endregion
			#endregion
			#region Explosives
			Recipe.Create(ItemID.ScarabBomb, 3)
			.AddIngredient(ItemID.Bomb, 3)
			.AddIngredient(ItemID.FossilOre)
			.Register();

			Recipe.Create(ItemID.Beenade, 6)
			.AddIngredient(ItemID.BeeWax)
			.AddIngredient(ItemID.Grenade, 6)
			.Register();
			#endregion
			#region Tools
			Recipe.Create(ItemID.SpelunkerGlowstick, 200)
			.AddIngredient(ItemID.SpelunkerPotion)
			.AddIngredient(ItemID.Glowstick, 200)
			.Register();
			#endregion
			#region Misc
			Recipe.Create(ItemID.Coal)
			.AddIngredient<Peat_Moss_Item>()
			.DisableDecraft()
			.Register();

			Recipe.Create(ItemID.Torch, 5)
			.AddIngredient(ItemID.Coal)
			.AddIngredient(ItemID.Wood)
			.Register();
			#region Nova Fragmnt
			Recipe.Create(ItemID.CelestialSigil)
			.AddIngredient(ItemID.FragmentNebula, 12)
			.AddIngredient(ItemID.FragmentSolar, 12)
			.AddIngredient(ItemID.FragmentStardust, 12)
			.AddIngredient(ItemID.FragmentVortex, 12)
			.AddIngredient<Nova_Fragment>(12)
			.Register();

			Recipe.Create(ItemID.LunarHook)
			.AddIngredient(ItemID.FragmentNebula, 6)
			.AddIngredient(ItemID.FragmentSolar, 6)
			.AddIngredient(ItemID.FragmentStardust, 6)
			.AddIngredient(ItemID.FragmentVortex, 6)
			.AddIngredient<Nova_Fragment>(6)
			.Register();

			Recipe.Create(ItemID.SuperHealingPotion, 5)
			.AddIngredient(ItemID.FragmentNebula)
			.AddIngredient(ItemID.FragmentSolar)
			.AddIngredient(ItemID.FragmentStardust)
			.AddIngredient(ItemID.FragmentVortex)
			.AddIngredient<Nova_Fragment>()
			.Register();

			Recipe.Create(ItemID.FragmentNebula)
			.AddIngredient(ItemID.FragmentSolar)
			.AddIngredient(ItemID.FragmentStardust)
			.AddIngredient(ItemID.FragmentVortex)
			.AddIngredient<Nova_Fragment>()
			.Register();

			Recipe.Create(ItemID.FragmentSolar)
			.AddIngredient(ItemID.FragmentNebula)
			.AddIngredient(ItemID.FragmentStardust)
			.AddIngredient(ItemID.FragmentVortex)
			.AddIngredient<Nova_Fragment>()
			.Register();

			Recipe.Create(ItemID.FragmentStardust)
			.AddIngredient(ItemID.FragmentNebula)
			.AddIngredient(ItemID.FragmentSolar)
			.AddIngredient(ItemID.FragmentVortex)
			.AddIngredient<Nova_Fragment>()
			.Register();

			Recipe.Create(ItemID.FragmentVortex)
			.AddIngredient(ItemID.FragmentNebula)
			.AddIngredient(ItemID.FragmentSolar)
			.AddIngredient(ItemID.FragmentStardust)
			.AddIngredient<Nova_Fragment>()
			.Register();

			Recipe.Create(ItemID.Megaphone)
			.AddIngredient(ItemID.WhiteString)
			.AddIngredient(ItemID.Squirrel)
			.AddIngredient(ItemID.Megaphone)
			.AddCondition(OriginsModIntegrations.AprilFools)
			.Register();
			#endregion
			#region Oil
			Recipe.Create(ItemID.AsphaltBlock, 20)
			.AddIngredient(ItemID.StoneBlock, 20)
			.AddIngredient<Oil_Bucket>()
			.AddTile(TileID.Blendomatic)
			.AddOnCraftCallback(CraftingCallbacks.BucketCrafting<Oil_Bucket>)
			.Register();

			Recipe.Create(ItemID.AsphaltBlock, 20)
			.AddIngredient(ItemID.StoneBlock, 20)
			.AddIngredient<Oil_Bottomless_Bucket>()
			.AddTile(TileID.Blendomatic)
			.AddOnCraftCallback(CraftingCallbacks.NoConsumeCrafting<Oil_Bottomless_Bucket>)
			.Register();

			Recipe.Create(ItemID.Torch, 20)
			.AddRecipeGroup(RecipeGroupID.Wood, 5)
			.AddIngredient<Oil_Bucket>()
			.AddTile(TileID.Blendomatic)
			.AddOnCraftCallback(CraftingCallbacks.BucketCrafting<Oil_Bucket>)
			.Register();

			Recipe.Create(ItemID.Torch, 20)
			.AddRecipeGroup(RecipeGroupID.Wood, 5)
			.AddIngredient<Oil_Bottomless_Bucket>()
			.AddTile(TileID.Blendomatic)
			.AddOnCraftCallback(CraftingCallbacks.NoConsumeCrafting<Oil_Bottomless_Bucket>)
			.Register();

			Recipe.Create(ItemID.MushroomTorch, 20)
			.AddIngredient(ItemID.GlowingMushroom, 5)
			.AddIngredient<Oil_Bucket>()
			.AddTile(TileID.Blendomatic)
			.AddOnCraftCallback(CraftingCallbacks.BucketCrafting<Oil_Bucket>)
			.Register();

			Recipe.Create(ItemID.MushroomTorch, 20)
			.AddIngredient(ItemID.GlowingMushroom, 5)
			.AddIngredient<Oil_Bottomless_Bucket>()
			.AddTile(TileID.Blendomatic)
			.AddOnCraftCallback(CraftingCallbacks.NoConsumeCrafting<Oil_Bottomless_Bucket>)
			.Register();
			#endregion
			#endregion

			//this hook is supposed to be used for adding recipes,
			//but since it also runs after a lot of other stuff I tend to use it for a lot of unrelated stuff
			Origins.instance.LateLoad();
			OriginsModIntegrations.AddRecipes();
		}
		public static int GemStaffRecipeGroupID { get; private set; }
		public static int GemPhasebladeRecipeGroupID { get; private set; }
		public static int DeathweedRecipeGroupID { get; private set; }
		public static int RottenChunkRecipeGroupID { get; private set; }
		public static int ShadowScaleRecipeGroupID { get; private set; }
		public static int CursedFlameRecipeGroupID { get; private set; }
		public static int EvilBoomerangRecipeGroupID { get; private set; }
		public static int GolfBallsRecipeGroupID { get; private set; }
		public static RecipeGroup EvilGunMagazineRecipeGroup { get; private set; } = new RecipeGroup(() => Language.GetOrRegister("Mods.Origins.RecipeGroups.EvilGunMagazines").Value, ItemID.MagicQuiver);
		public static RecipeGroup LampRecipeGroup { get; private set; } = new RecipeGroup(() => Language.GetOrRegister("Mods.Origins.RecipeGroups.Lamps").Value, ItemID.PalmWoodLamp);
		public override void AddRecipeGroups() {
			GemStaffRecipeGroupID = RecipeGroup.RegisterGroup("Origins:Gem Staves", new RecipeGroup(() => Language.GetOrRegister("Mods.Origins.RecipeGroups.GemStaves").Value, [
				ItemID.AmberStaff,
				ItemID.AmethystStaff,
				ItemID.DiamondStaff,
				ItemID.EmeraldStaff,
				ItemID.RubyStaff,
				ItemID.SapphireStaff,
				ItemID.TopazStaff
			]));
			GemPhasebladeRecipeGroupID = RecipeGroup.RegisterGroup("Origins:Gem Phaseblades", new RecipeGroup(() => Language.GetOrRegister("Mods.Origins.RecipeGroups.GemPhaseblades").Value, [
				ItemID.OrangePhaseblade,
				ItemID.PurplePhaseblade,
				ItemID.WhitePhaseblade,
				ItemID.GreenPhaseblade,
				ItemID.RedPhaseblade,
				ItemID.BluePhaseblade,
				ItemID.YellowPhaseblade
			]));
			EvilBoomerangRecipeGroupID = RecipeGroup.RegisterGroup("Origins:Evil Boomerangs", new RecipeGroup(() => Language.GetOrRegister("Mods.Origins.RecipeGroups.EvilBoomerangs").Value, [
				ModContent.ItemType<Dark_Spiral>(),
				ModContent.ItemType<Hemorang>(),
				ModContent.ItemType<Krakram>(),
				ModContent.ItemType<Riverang>(),
				ModContent.ItemType<Orbital_Saw>(),
			]));
			GolfBallsRecipeGroupID = RecipeGroup.RegisterGroup("Origins:GolfBalls", new RecipeGroup(() => Language.GetOrRegister("Mods.Origins.RecipeGroups.GolfBalls").Value, [
				ItemID.GolfBall
			]));
			for (int i = ItemID.GolfBallDyedBlack; i <= ItemID.GolfBallDyedYellow; i++) RecipeGroup.recipeGroups[GolfBallsRecipeGroupID].ValidItems.Add(i);
			EvilGunMagazineRecipeGroup.ValidItems.Remove(ItemID.MagicQuiver);
			RecipeGroup.RegisterGroup("Origins:Evil Gun Magazines", EvilGunMagazineRecipeGroup);
			RecipeGroup.RegisterGroup("Origins:Lamps", LampRecipeGroup);
			RecipeGroup.RegisterGroup("Origins:Any Different Advanced Medicines", AnyDifferentMedicine.RecipeGroup);
			DeathweedRecipeGroupID = ALRecipeGroups.Deathweed.RegisteredId;
			RottenChunkRecipeGroupID = ALRecipeGroups.RottenChunks.RegisteredId;
			ShadowScaleRecipeGroupID = ALRecipeGroups.ShadowScales.RegisteredId;
			CursedFlameRecipeGroupID = ALRecipeGroups.CursedFlames.RegisteredId;
			static void AddItemsToGroup(RecipeGroup group, params int[] items) {
				for (int i = 0; i < items.Length; i++) {
					group.ValidItems.Add(items[i]);
				}
			}
			AddItemsToGroup(RecipeGroup.recipeGroups[RecipeGroupID.Sand],
				ModContent.ItemType<Hardened_Defiled_Sand_Item>(),
				ModContent.ItemType<Defiled_Sand_Item>(),
				ModContent.ItemType<Brittle_Quartz_Item>(),
				ModContent.ItemType<Silica_Item>(),
				ModContent.ItemType<Hardened_Sootsand_Item>(),
				ModContent.ItemType<Sootsand_Item>()
			);
			AddItemsToGroup(RecipeGroup.recipeGroups[RecipeGroupID.Wood],
				ModContent.ItemType<Endowood_Item>(),
				ModContent.ItemType<Marrowick_Item>(),
				ModContent.ItemType<Artifiber_Item>()
			);
			AddItemsToGroup(RecipeGroup.recipeGroups[RecipeGroupID.Fruit],
				ModContent.ItemType<Bileberry>(),
				ModContent.ItemType<Pawpaw>(),
				ModContent.ItemType<Periven>(),
				ModContent.ItemType<Prickly_Pear>(),
				ModContent.ItemType<Sour_Apple>()
			);

			OriginsModIntegrations.AddRecipeGroups();
		}
		public override void PostAddRecipes() {
			int l = Main.recipe.Length;
			Recipe r;
			//Recipe recipe;
			for (int i = 0; i < l; i++) {
				r = Main.recipe[i];
				if (r.requiredItem.ToList().Exists((ing) => ing.type == ItemID.Deathweed)) {
					r.acceptedGroups.Add(DeathweedRecipeGroupID);
				}
				//example use of Recipe.Matches extension method because I just realized that I don't know which recipes you're trying to disable:
				//this would match any recipe which creates any number of potato chips, is crafted at pots, and has exactly the ingredients: any number of potato chips, 7 potions of return
				if (r.Matches((ItemID.PotatoChips, null), [TileID.Pots], (ItemID.PotatoChips, null), (ItemID.PotionOfReturn, 7))) {
					r.DisableRecipe();
				}

				if (r.Matches((ItemID.ScarabBomb, null), null, (ItemID.Bomb, 1), (ItemID.FossilOre, 1))) {
					r.DisableRecipe();
				}

				if (r.Matches((ItemID.Beenade, null), null, (ItemID.Grenade, 1), (ItemID.BeeWax, 1))) {
					r.DisableRecipe();
				}

				if (r.Matches((ItemID.UltraAbsorbantSponge, null), null, (ItemID.SuperAbsorbantSponge, null), (ItemID.LavaAbsorbantSponge, null), (ItemID.HoneyAbsorbantSponge, null))) {
					foreach (SpongeBase sponge in ModContent.GetContent<SpongeBase>()) {
						if (SpongeBase.UltraSpongeIngredients[sponge.Type]) r.AddIngredient(sponge.Type);
					}
				}

				/*if (r.Matches((ItemID.CelestialShell, null), new int[] { TileID.TinkerersWorkbench }, (ItemID.CelestialStone, 1), (ItemID.MoonShell, 1))) {
					r.DisableRecipe();
				} only uncomment when Ornament of Metamorphosis is implemented */

				//Everything below this needs the corresponding recipe in the Nova Fragment class when the Nova Pillar is implemented
				/*if (r.Matches((ItemID.CelestialSigil, null), new int[] { TileID.LunarCraftingStation }, (ItemID.FragmentNebula, 12), (ItemID.FragmentSolar, 12), (ItemID.FragmentStardust, 12), (ItemID.FragmentVortex, 12))) {
					r.DisableRecipe();
				}
				if (r.Matches((ItemID.LunarHook, null), new int[] { TileID.LunarCraftingStation }, (ItemID.FragmentNebula, 6), (ItemID.FragmentSolar, 6), (ItemID.FragmentStardust, 6), (ItemID.FragmentVortex, 6))) {
					r.DisableRecipe();
				}
				if (r.Matches((ItemID.FragmentNebula, null), new int[] { TileID.LunarCraftingStation }, (ItemID.FragmentSolar, 1), (ItemID.FragmentStardust, 1), (ItemID.FragmentVortex, 1))) {
					r.DisableRecipe();
				}
				if (r.Matches((ItemID.FragmentSolar, null), new int[] { TileID.LunarCraftingStation }, (ItemID.FragmentNebula, 1), (ItemID.FragmentStardust, 1), (ItemID.FragmentVortex, 1))) {
					r.DisableRecipe();
				}
				if (r.Matches((ItemID.FragmentStardust, null), new int[] { TileID.LunarCraftingStation }, (ItemID.FragmentNebula, 1), (ItemID.FragmentSolar, 1), (ItemID.FragmentVortex, 1))) {
					r.DisableRecipe();
				}
				if (r.Matches((ItemID.FragmentVortex, null), new int[] { TileID.LunarCraftingStation }, (ItemID.FragmentNebula, 1), (ItemID.FragmentSolar, 1), (ItemID.FragmentStardust, 1))) {
					r.DisableRecipe();
				}
				if (r.Matches((ItemID.SuperHealingPotion, null), new int[] { TileID.Bottles }, (ItemID.FragmentNebula, 1), (ItemID.FragmentSolar, 1), (ItemID.FragmentStardust, 1), (ItemID.FragmentVortex, 1), (ItemID.GreaterHealingPotion, 1)) {
					r.DisableRecipe();
				}*/

				//recipe = r.Clone();
				//recipe.requiredItem = recipe.requiredItem.Select((it) => it.type == ItemID.Deathweed ? new Item(roseID) : it.CloneByID()).ToList();
				//Mod.Logger.Info("adding procedural recipe: " + recipe.Stringify());
				//recipe.Create();

			}
			foreach (AbstractNPCShop shop in NPCShopDatabase.AllShops) {
				if (shop is NPCShop npcShop) {
					foreach (NPCShop.Entry item in npcShop.Entries) PaintingsNotFromVendor[item.Item.type] = false;
				}
			}

			OriginsModIntegrations.PostAddRecipes();
		}
	}
	public static class CraftingCallbacks {
		public static void BucketCrafting<TBucket>(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) where TBucket : ModItem {
			for (int i = 0; i < consumedItems.Count; i++) {
				if (consumedItems[i].type == ModContent.ItemType<TBucket>()) {
					Main.LocalPlayer.GetItem(Main.myPlayer, new Item(ItemID.EmptyBucket, consumedItems[i].stack), new GetItemSettings(NoText: true, CanGoIntoVoidVault: true));
				}
			}
		}
		public static void NoConsumeCrafting<TItem>(Recipe recipe, Item item, List<Item> consumedItems, Item destinationStack) where TItem : ModItem {
			for (int i = 0; i < consumedItems.Count; i++) {
				if (consumedItems[i].type == ModContent.ItemType<TItem>()) {
					Main.LocalPlayer.GetItem(Main.myPlayer, consumedItems[i], new GetItemSettings(NoText: true, CanGoIntoVoidVault: true));
				}
			}
		}
	}
}
