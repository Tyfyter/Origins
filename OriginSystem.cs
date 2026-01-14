using Origins.Core;
using Origins.Core.Structures;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Other.Consumables.Food;
using Origins.Items.Other.Consumables.Medicine;
using Origins.Items.Tools;
using Origins.Items.Weapons.Melee;
using Origins.NPCs.Brine;
using Origins.NPCs.MiscB.Shimmer_Construct;
using Origins.NPCs.MiscE.Quests;
using Origins.Projectiles;
using Origins.Questing;
using Origins.Reflection;
using Origins.Tiles.Ashen;
using Origins.Tiles.Ashen.Hanging_Scrap;
using Origins.Tiles.Brine;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.UI;
using Origins.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using static Origins.OriginsSets.Items;
using static Tyfyter.Utils.UITools;

namespace Origins {
	public partial class OriginSystem : ModSystem {
		public static OriginSystem Instance => ModContent.GetInstance<OriginSystem>();
		public UserInterface setBonusInventoryUI;
		public StateSwitchingInterface SetBonusHUD { get; } = new("Origins: Set Bonus HUD");
		public StateSwitchingInterface AccessoryHUD { get; } = new("Origins: Accessory HUD", true);
		public StateSwitchingInterface ItemUseHUD { get; } = new("Origins: Held Item HUD");
		public StateSwitchingInterface EventHUD { get; } = new("Origins: Event HUD");
		public SpacePirateEyeInterface SpacePirateEyeUI { get; } = new();
		public UserInterfaceWithDefaultState journalUI;
		internal static List<SwitchableUIState> queuedUIStates = [];
		public static bool HasSetupAllContent { get; private set; }
		public override void Load() {
			HasSetupAllContent = false;
			setBonusInventoryUI = new UserInterface();
			journalUI = new UserInterfaceWithDefaultState() {
				DefaultUIState = new Journal_UI_Button()
			};
		}
		public override void ModifyGameTipVisibility(IReadOnlyList<GameTipData> gameTips) {
			HasSetupAllContent = true;
		}
		public override void SetStaticDefaults() {
			for (int i = 0; i < queuedUIStates.Count; i++) {
				queuedUIStates[i].AddToList();
			}
			queuedUIStates = null;
			DamageClasses.Patch();
#if DEBUG
			Task.Run(() => {
				foreach (string structure in Mod.GetFileNames()) {
					if (!structure.StartsWith("World/Structures/")) continue;
					DeserializedStructure.Load($"{nameof(Origins)}/{structure}");
				}
			});
#endif
		}
		public override void Unload() {
			queuedUIStates = null;
			EvilGunMagazineRecipeGroup = null;
		}
		public override void AddRecipes() {
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

			Recipe.Create(ItemID.GoldShortsword)
			.AddIngredient(ItemID.EnchantedSword)
			.AddTile(TileID.BewitchingTable)
			.Register();

			Recipe.Create(ItemID.SpelunkerGlowstick, 200)
			.AddIngredient(ItemID.SpelunkerPotion)
			.AddIngredient(ItemID.Glowstick, 200)
			.Register();

			Recipe.Create(ItemID.CrystalNinjaHelmet)
			.AddIngredient(ItemID.CrystalShard, 30)
			.AddIngredient(ItemID.SoulofLight, 5)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 15)
			.AddTile(TileID.MythrilAnvil)
			.Register();

			Recipe.Create(ItemID.CrystalNinjaChestplate)
			.AddIngredient(ItemID.CrystalShard, 60)
			.AddIngredient(ItemID.SoulofLight, 7)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 30)
			.AddTile(TileID.MythrilAnvil)
			.Register();

			Recipe.Create(ItemID.CrystalNinjaLeggings)
			.AddIngredient(ItemID.CrystalShard, 45)
			.AddIngredient(ItemID.SoulofLight, 3)
			.AddIngredient(ModContent.ItemType<Carburite_Item>(), 23)
			.AddTile(TileID.MythrilAnvil)
			.Register();

			Recipe.Create(ItemID.StylistKilLaKillScissorsIWish)
			.AddIngredient(ItemID.SilverBar, 2)
			.AddIngredient(ModContent.ItemType<Magic_Hair_Spray>(), 5)
			.AddIngredient(ModContent.ItemType<Rubber>(), 4)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.StylistKilLaKillScissorsIWish)
			.AddIngredient(ItemID.TungstenBar, 2)
			.AddIngredient(ModContent.ItemType<Magic_Hair_Spray>(), 5)
			.AddIngredient(ModContent.ItemType<Rubber>(), 4)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.BallOHurt)
			.AddIngredient(ItemID.DemoniteBar, 10)
			.AddIngredient(ItemID.ShadowScale, 5)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.BloodButcherer)
			.AddIngredient(ItemID.CrimtaneBar, 6)
			.AddIngredient(ItemID.TissueSample, 3)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.CrimsonRod)
			.AddRecipeGroup("Origins:Gem Staves")
			.AddIngredient(ItemID.CrimtaneBar, 10)
			.AddIngredient(ItemID.TissueSample, 6)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.ScarabBomb, 3)
			.AddIngredient(ItemID.Bomb, 3)
			.AddIngredient(ItemID.FossilOre)
			.Register();

			Recipe.Create(ItemID.Beenade, 6)
			.AddIngredient(ItemID.BeeWax)
			.AddIngredient(ItemID.Grenade, 6)
			.Register();

			Recipe.Create(ItemID.TheRottedFork)
			.AddIngredient(ItemID.CrimtaneBar, 9)
			.AddIngredient(ItemID.TissueSample, 5)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.TheUndertaker)
			.AddIngredient(ItemID.CrimtaneBar, 6)
			.AddIngredient(ItemID.TissueSample, 4)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.Vilethorn)
			.AddRecipeGroup("Origins:Gem Staves")
			.AddIngredient(ItemID.DemoniteBar, 10)
			.AddIngredient(ItemID.ShadowScale, 6)
			.AddTile(TileID.Anvils)
			.Register();

			Recipe.Create(ItemID.Coal)
			.AddIngredient(ModContent.ItemType<Peat_Moss_Item>())
			.DisableDecraft()
			.Register();

			Recipe.Create(ItemID.Torch, 5)
			.AddIngredient(ItemID.Coal)
			.AddIngredient(ItemID.Wood)
			.Register();

			Recipe.Create(ItemID.CelestialSigil)
			.AddIngredient(ItemID.FragmentNebula, 12)
			.AddIngredient(ItemID.FragmentSolar, 12)
			.AddIngredient(ItemID.FragmentStardust, 12)
			.AddIngredient(ItemID.FragmentVortex, 12)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 12)
			.Register();

			Recipe.Create(ItemID.LunarHook)
			.AddIngredient(ItemID.FragmentNebula, 6)
			.AddIngredient(ItemID.FragmentSolar, 6)
			.AddIngredient(ItemID.FragmentStardust, 6)
			.AddIngredient(ItemID.FragmentVortex, 6)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 6)
			.Register();

			Recipe.Create(ItemID.SuperHealingPotion, 5)
			.AddIngredient(ItemID.FragmentNebula)
			.AddIngredient(ItemID.FragmentSolar)
			.AddIngredient(ItemID.FragmentStardust)
			.AddIngredient(ItemID.FragmentVortex)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>())
			.Register();

			Recipe.Create(ItemID.FragmentNebula)
			.AddIngredient(ItemID.FragmentSolar)
			.AddIngredient(ItemID.FragmentStardust)
			.AddIngredient(ItemID.FragmentVortex)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>())
			.Register();

			Recipe.Create(ItemID.FragmentSolar)
			.AddIngredient(ItemID.FragmentNebula)
			.AddIngredient(ItemID.FragmentStardust)
			.AddIngredient(ItemID.FragmentVortex)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>())
			.Register();

			Recipe.Create(ItemID.FragmentStardust)
			.AddIngredient(ItemID.FragmentNebula)
			.AddIngredient(ItemID.FragmentSolar)
			.AddIngredient(ItemID.FragmentVortex)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>())
			.Register();

			Recipe.Create(ItemID.FragmentVortex)
			.AddIngredient(ItemID.FragmentNebula)
			.AddIngredient(ItemID.FragmentSolar)
			.AddIngredient(ItemID.FragmentStardust)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>())
			.Register();

			Recipe.Create(ItemID.Megaphone)
			.AddIngredient(ItemID.WhiteString)
			.AddIngredient(ItemID.Squirrel)
			.AddIngredient(ItemID.Megaphone)
			.AddCondition(OriginsModIntegrations.AprilFools)
			.Register();

			//this hook is supposed to be used for adding recipes,
			//but since it also runs after a lot of other stuff I tend to use it for a lot of unrelated stuff
			Origins.instance.LateLoad();
			OriginsModIntegrations.AddRecipes();
		}
		public override void PostUpdateInput() {
		}
		int lastHour = -1;
		public override void PostUpdateTime() {
			foreach (Quest quest in Quest_Registry.NetQuests) {
				quest.CheckSync();
			}
			Tax_Collector_Ghosts_Quest.GetTime(out int hour, out _);
			if (hour != lastHour && ModContent.GetInstance<Tax_Collector_Ghosts_Quest>()?.Stage is int stage) {
				switch (hour) {
					case 0 or 1:
					if (stage == hour + 1) {
						foreach (NPC other in Main.ActiveNPCs) {
							if (other.type == NPCID.TaxCollector) {
								int ghost;
								if (hour == 0) {
									ghost = ModContent.NPCType<Jacob_Marley>();
								} else {
									ghost = ModContent.NPCType<Spirit_Of_Christmas_Past>();
								}
								NPC.NewNPC(Entity.GetSource_None(), (int)other.position.X + Main.rand.Next(64, 96) * Main.rand.NextBool().ToDirectionInt(), (int)other.position.Y - Main.rand.Next(16), ghost, ai3: -1000);
								break;
							}
						}
					}
					break;
					case 2:
					if (stage == 3 && Spirit_Of_Christmas_Present.FindTeleportPosition(out int posX, out int posY, out _, out _)) {
						posX *= 16;
						posY *= 16;
						NPC.NewNPC(Entity.GetSource_None(), posX, posY, ModContent.NPCType<Spirit_Of_Christmas_Present>(), ai3: -1000);
						NPC.NewNPC(Entity.GetSource_None(), posX, posY, ModContent.NPCType<Spirit_Of_Christmas_Present_Tax_Collector>());
					}
					break;
				}
			}
			lastHour = hour;
			if (NetmodeActive.MultiplayerClient) {
				int shelf = ModContent.TileEntityType<Shelf_Coral_TE>();
				TileEntity.UpdateStart();
				foreach (TileEntity value in TileEntity.ByID.Values) {
					if (value.type == shelf) value.Update();
				}
				TileEntity.UpdateEnd();
			}
		}
		public override void PostUpdateEverything() {
			Debugging.LogFirstRun(PostUpdateEverything);
			for (int i = 0; i < Origins.tickers.Count; i++) {
				Origins.tickers[i].Tick();
			}
			Debugging.LogFirstRun(nameof(PostUpdateEverything) + " (passed tickers)");
			Debugging.firstUpdate = false;
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
		public override void ModifyLightingBrightness(ref float scale) {
			OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
			if (originPlayer.plagueSightLight) {
				scale *= 1.03f;
			}
			if (originPlayer.sonarVisor) {
				scale *= 0.9f;
			}
		}
		public override void UpdateUI(GameTime gameTime) {
			if (Main.playerInventory) {
				if (setBonusInventoryUI?.CurrentState is Eyndum_Core_UI eyndumCoreUIState) {
					OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
					if (eyndumCoreUIState?.itemSlot?.item == originPlayer.eyndumCore) {
						if (!originPlayer.eyndumSet) {
							if (eyndumCoreUIState?.itemSlot?.item?.Value?.IsAir ?? true) {
								setBonusInventoryUI.SetState(null);
							} else {
								eyndumCoreUIState.hasSetBonus = false;
								setBonusInventoryUI.Update(gameTime);
							}
						} else {
							setBonusInventoryUI.Update(gameTime);
						}
					} else {
						setBonusInventoryUI.SetState(null);
					}
				} else if (setBonusInventoryUI?.CurrentState is Mimic_Selection_UI) {
					OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
					if (originPlayer.mimicSet) {
						setBonusInventoryUI.Update(gameTime);
					} else {
						setBonusInventoryUI.SetState(null);
					}
				}
			}
			ItemUseHUD.Update(gameTime);
			AccessoryHUD.Update(gameTime);
			SetBonusHUD.Update(gameTime);
			EventHUD.Update(gameTime);
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1) {//error prevention & null check
				layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
					"Origins: Set Bonus Inventory UI",
					delegate {
						setBonusInventoryUI?.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
						return true;
					},
					InterfaceScaleType.UI) { Active = Main.playerInventory }
				);
				ItemUseHUD.Insert(layers);
				AccessoryHUD.Insert(layers);
				SetBonusHUD.Insert(layers);
				EventHUD.Insert(layers);
				SpacePirateEyeUI.Insert(layers);
				if (Main.LocalPlayer.GetModPlayer<OriginPlayer>().journalUnlocked) {
					layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
						"Origins: Journal UI",
						delegate {
							journalUI?.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
							return true;
						},
						InterfaceScaleType.UI) { Active = Main.playerInventory }
					);
				}
			}
			Strange_Computer.drawingStrangeLine = false;
			layers.Insert(0, new LegacyGameInterfaceLayer(
				"Origins: Strange Computer Line",
				delegate {
					Strange_Computer.DrawStrangeLine();
					return true;
				},
				InterfaceScaleType.Game) { Active = Main.LocalPlayer.GetModPlayer<OriginPlayer>().strangeComputer }
			);
			inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Hotbar"));
			if (inventoryIndex != -1 && layers[inventoryIndex].Active && Main.LocalPlayer.HasBuff<Lunatics_Rune_Attacks_Buff>()) {
				layers[inventoryIndex] = new LegacyGameInterfaceLayer(
					"Vanilla: Hotbar",
					LunaticsRuneAttack.DrawSlots,
					InterfaceScaleType.UI
				) {
					Active = !Main.playerInventory
				};
			}
		}
		public override void OnLocalizationsLoaded() {
			Dictionary<string, LocalizedText> texts = LocalizationMethods._localizedTexts.GetValue(LanguageManager.Instance);
			/*texts["Riven"] = texts["Mods.Origins.Generic.Riven"];
			texts["Riven_Hive"] = texts["Mods.Origins.Generic.Riven_Hive"];
			texts["Dusk"] = texts["Mods.Origins.Generic.Dusk"];
			texts["Defiled"] = texts["Mods.Origins.Generic.Defiled"];
			texts["Defiled_Wastelands"] = texts["Mods.Origins.Generic.Defiled_Wastelands"];
			texts["the_Defiled_Wastelands"] = texts["Mods.Origins.Generic.the_Defiled_Wastelands"];
			texts["The_Defiled_Wastelands"] = texts["Mods.Origins.Generic.The_Defiled_Wastelands"];*/
			bool isAprilFools = OriginsModIntegrations.CheckAprilFools();
			foreach (KeyValuePair<string, LocalizedText> text in texts.ToList()) {
				if (isAprilFools && text.Key.StartsWith("Mods.Origins.AprilFools")) {
					string key = text.Key.Replace("AprilFools.", "");
					if (texts.TryGetValue(key, out LocalizedText targetText)) {
						LocalizationMethods._value.SetValue(targetText, text.Value.Value);
						LocalizationMethods._hasPlurals.SetValue(targetText, LocalizationMethods._hasPlurals.GetValue(text.Value));
						LocalizationMethods.BoundArgs.SetValue(targetText, text.Value.BoundArgs);
					} else {
						Mod.Logger.Warn($"Adding April Fools text instead of replacing existing text: {text.Key}");
						texts[key] = text.Value;
					}
				} else if (text.Key.StartsWith("Mods.Origins.Generic")) {
					string key = text.Key.Replace("Mods.Origins.Generic.", "");
					texts[key] = text.Value;
				}
			}
			Regex substitutionRegex = new("{§(.*?)}", RegexOptions.Compiled);
			foreach (KeyValuePair<string, LocalizedText> text in texts.ToList()) {
				Match subMatch = substitutionRegex.Match(text.Value.Value);
				while (subMatch.Success) {
					LocalizationMethods._value.SetValue(text.Value, text.Value.Value.Replace(subMatch.Groups[0].Value, Language.GetTextValue(subMatch.Groups[1].Value)));
					subMatch = substitutionRegex.Match(text.Value.Value);
				}
			}
			Time_Radices.Refresh();
		}
		public static Projectile[,] projectilesByOwnerAndID = new Projectile[Main.maxPlayers + 1, Main.maxProjectiles];
		public override void PreUpdateProjectiles() {
			Debugging.LogFirstRun(PreUpdateProjectiles);
			OriginsGlobalBiome.isConversionFromProjectile = true;
			Array.Clear(projectilesByOwnerAndID);
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile projectile = Main.projectile[i];
				projectilesByOwnerAndID[projectile.owner, projectile.identity] = projectile;
				if (projectile.TryGetGlobalProjectile(out OriginGlobalProj global) && global.isFromMitosis) {
					Main.player[Main.projectile[i].owner].ownedProjectileCounts[Main.projectile[i].type]--;
				}
			}
			OriginExtensions.SwapClear(ref ExplosiveGlobalProjectile.nextExplodingProjectiles, ref ExplosiveGlobalProjectile.explodingProjectiles);
			OriginExtensions.SwapClear(ref Mitosis_P.nextMitosises, ref Mitosis_P.mitosises);
			OriginExtensions.SwapClear(ref The_Bird_Swing.nextReflectors, ref The_Bird_Swing.reflectors);
			Brine_Pool_NPC.Ripples.Clear();
		}
		public override void PostUpdateProjectiles() {
			OriginsGlobalBiome.isConversionFromProjectile = false;
			foreach (Projectile projectile in Main.ActiveProjectiles) {
				if ((projectile.wet || (projectile.ignoreWater && Collision.WetCollision(projectile.position, projectile.width, projectile.height))) && ProjectileID.Sets.CanDistortWater[projectile.type] && !ProjectileID.Sets.NoLiquidDistortion[projectile.type]) {
					Brine_Pool_NPC.Ripples.Add((projectile.Center, projectile.velocity.Length()));
				}
			}
		}
		FastStaticFieldInfo<Main, float> _minWind;
		FastStaticFieldInfo<Main, float> _maxWind;
		public override void PreUpdateEntities() {
			if (!NPC.downedBoss3 && Main.raining) {
				float minWind = Math.Abs((float)(_minWind ??= new("_minWind", BindingFlags.NonPublic, true))) - 0.001f;
				Main.windSpeedTarget = MathHelper.Clamp(Main.windSpeedTarget, -minWind, minWind);
			} else if (forceThunderstorm) {
				float maxWind = Math.Abs((float)(_maxWind ??= new("_maxWind", BindingFlags.NonPublic, true)));
				if (Main.IsItRaining && Math.Abs(Main.windSpeedTarget) >= MathHelper.Lerp(maxWind, 0.8f, 0.5f)) {
					forceThunderstorm = false;
				} else {
					if (!Main.IsItRaining && Main.rand.NextBool(6)) {
						Main.numClouds += 1;
						if (Main.numClouds > 100) {
							Main.StartRain();
						}
					}
					if (Math.Abs(Main.windSpeedTarget) < maxWind && Main.rand.NextBool(4)) {
						Main.windSpeedTarget += Main.rand.Next(5, 26) * 0.001f * (Main.windSpeedTarget < 0 ? -1 : 1);
						Main.windSpeedTarget = MathHelper.Clamp(Main.windSpeedTarget, -0.8f, 0.8f);
					}
				}
			} else if (forceThunderstormDelay > 0) {
				if (--forceThunderstormDelay <= 0) forceThunderstorm = true;
			}
			foreach (Player player in Main.ActivePlayers) {
				if (player.OriginPlayer() is OriginPlayer originPlayer) {
					originPlayer.oldNearbyActiveNPCs = player.nearbyActiveNPCs;
				}
			}
			Main.tileSolid[Broken_Catwalk.ID] = false;
			Main.tileSolidTop[Broken_Catwalk.ID] = false;
		}
		bool hasLoggedPUP = false;
		public int laserTagActiveTeams = 0;
		public int laserTagActivePlayers = 0;
		public Laser_Tag_Rules laserTagRules = new();
		public int laserTagTimeLeft = 0;
		public int[] laserTagTeamPoints = new int[6];
		public int[] laserTagTeamHits = new int[6];
		public int[] laserTagTeamGems = new int[6];
		public int[] laserTagTeamPlayers = new int[6];
		public override void PreUpdatePlayers() {
			Debugging.LogFirstRun(PreUpdatePlayers, true);
			OriginPlayer.LocalOriginPlayer = Main.LocalPlayer.TryGetModPlayer(out OriginPlayer localPlayer) ? localPlayer : null;
			if (OriginPlayer.playersByGuid is null) OriginPlayer.playersByGuid = [];
			else OriginPlayer.playersByGuid.Clear();
			Laser_Tag_Console.ProcessLaserTag();
			if (!hasLoggedPUP) {
				hasLoggedPUP = true;
				//Mod.Logger.Info($"Running {nameof(PreUpdatePlayers)} in netmode {Main.netMode}");
			}
		}
		static Stack<Point> QueuedTileFrames { get; } = new();
		static Stack<Point> queuedSpecialTileFrames = new();
		static Stack<Point> workingQueuedSpecialTileFrames = new();
		static bool isFramingQueuedTiles = false;
		public static void QueueTileFrames(int i, int j) {
			if (!isFramingQueuedTiles) QueuedTileFrames.Push(new(i, j));
		}
		public static void QueueSpecialTileFrames(int i, int j) {
			workingQueuedSpecialTileFrames.Push(new(i, j));
		}
		public override void PostUpdatePlayers() {
			try {
				isFramingQueuedTiles = true;
				while (QueuedTileFrames.TryPop(out Point pos)) WorldGen.TileFrame(pos.X, pos.Y);
			} finally {
				isFramingQueuedTiles = false;
			}
			Utils.Swap(ref queuedSpecialTileFrames, ref workingQueuedSpecialTileFrames);
			if (queuedSpecialTileFrames.Count > 10000) queuedSpecialTileFrames.Clear();
			while (queuedSpecialTileFrames.TryPop(out Point pos)) {
				if (TileLoader.GetTile(Main.tile[pos].TileType) is not ISpecialFrameTile specialTile) continue;
				specialTile.SpecialFrame(pos.X, pos.Y);
			}
		}
		public override void PreUpdateNPCs() {
			Debugging.LogFirstRun(PreUpdateNPCs);
		}
		public override void PreUpdateGores() {
			Debugging.LogFirstRun(PreUpdateGores);
		}
		public override void PreUpdateItems() {
			Debugging.LogFirstRun(PreUpdateItems);
		}
		public override void PreUpdateDusts() {
			Debugging.LogFirstRun(PreUpdateDusts);
		}
		public override void PreUpdateTime() {
			Debugging.LogFirstRun(PreUpdateTime);
		}
		public override void PreUpdateWorld() {
			Debugging.LogFirstRun(PreUpdateWorld);
		}
		public override void PreUpdateInvasions() {
			Debugging.LogFirstRun(PreUpdateInvasions);
		}
		public override void PostUpdateInvasions() {
			Debugging.LogFirstRun(PostUpdateInvasions);
		}
		public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform) {
			SC_Phase_Three_Underlay.minLightAreas.Clear();
			foreach (Projectile proj in Main.ActiveProjectiles) {
				if (proj.ModProjectile is IPreDrawSceneProjectile preDrawer) preDrawer.PreDrawScene();
			}
		}
		internal static List<IPreDrawAnything> assetSwitchers = [];
		public override void ModifyScreenPosition() {
			for (int i = 0; i < assetSwitchers.Count; i++) assetSwitchers[i].PreDrawAnything();
		}
		public override void PostDrawTiles() {
			SpecialTilePreviewOverlay.ForceActive();
			Players_Behind_Tiles_Overlay.ForceActive();
			Hanging_Scrap_Overlay.ForceActive();
			Gas_Mask_Overlay.ForceActive();
		}
	}
	public class TempleBiome : ModBiome {
		public override string Name => "Bestiary_Biomes.TheTemple";
		public override bool IsBiomeActive(Player player) {
			return player.ZoneLihzhardTemple;
		}
	}
	public class SpaceBiome : ModBiome {
		public override string Name => "Bestiary_Biomes.Space";
		public override bool IsBiomeActive(Player player) {
			return player.ZoneSkyHeight;
		}
	}
}
