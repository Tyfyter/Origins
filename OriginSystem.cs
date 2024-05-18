using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Projectiles;
using Origins.Questing;
using Origins.Reflection;
using Origins.Tiles.Brine;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using Origins.Tiles.Riven;
using Origins.UI;
using Origins.UI.SetBonus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using static Tyfyter.Utils.UITools;
using ALRecipeGroups = AltLibrary.Common.Systems.RecipeGroups;

namespace Origins {
    public partial class OriginSystem : ModSystem {
		static OriginSystem instance;
		public static OriginSystem Instance => instance ??= ModContent.GetInstance<OriginSystem>();
		public UserInterface setBonusInventoryUI;
		UserInterface setBonusHUDInterface;
		UserInterface itemUseHUDInterface;
		public State_Switching_UI SetBonusHUD { get; private set; } = new();
		public State_Switching_UI ItemUseHUD { get; private set; } = new();
		public UserInterfaceWithDefaultState journalUI;
		internal static List<SwitchableUIState> queuedUIStates = [];
		public override void Load() {
			setBonusInventoryUI = new UserInterface();
			setBonusHUDInterface = new UserInterface();
			setBonusHUDInterface.SetState(SetBonusHUD);
			itemUseHUDInterface = new UserInterface();
			itemUseHUDInterface.SetState(ItemUseHUD);
			journalUI = new UserInterfaceWithDefaultState() {
				DefaultUIState = new Journal_UI_Button()
			};
		}
		public override void SetStaticDefaults() {
			for (int i = 0; i < queuedUIStates.Count; i++) {
				queuedUIStates[i].AddToList();
			}
			queuedUIStates = null;
		}
		public override void Unload() {
			instance = null;
			queuedUIStates = null;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(ItemID.MiningHelmet);
			recipe.AddIngredient(ItemID.Glowstick, 4);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 7);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();

			recipe = Recipe.Create(ItemID.MiningShirt);
			recipe.AddIngredient(ItemID.Leather, 15);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();

			recipe = Recipe.Create(ItemID.MiningPants);
			recipe.AddIngredient(ItemID.Leather, 15);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();

			recipe = Recipe.Create(ItemID.GoldShortsword);
			recipe.AddIngredient(ItemID.EnchantedSword);
			recipe.AddTile(TileID.BewitchingTable);
			recipe.Register();

			recipe = Recipe.Create(ItemID.SpelunkerGlowstick, 200);
			recipe.AddIngredient(ItemID.SpelunkerPotion);
			recipe.AddIngredient(ItemID.Glowstick, 200);
			recipe.Register();

			recipe = Recipe.Create(ItemID.CrystalNinjaHelmet);
			recipe.AddIngredient(ItemID.CrystalShard, 30);
			recipe.AddIngredient(ItemID.SoulofLight, 5);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 15);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();

			recipe = Recipe.Create(ItemID.CrystalNinjaChestplate);
			recipe.AddIngredient(ItemID.CrystalShard, 60);
			recipe.AddIngredient(ItemID.SoulofLight, 7);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 30);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();

			recipe = Recipe.Create(ItemID.CrystalNinjaLeggings);
			recipe.AddIngredient(ItemID.CrystalShard, 45);
			recipe.AddIngredient(ItemID.SoulofLight, 3);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 23);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();

			recipe = Recipe.Create(ItemID.StylistKilLaKillScissorsIWish);
			recipe.AddIngredient(ItemID.SilverBar, 2);
			recipe.AddIngredient(ModContent.ItemType<Magic_Hair_Spray>(), 5);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 4);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(ItemID.StylistKilLaKillScissorsIWish);
			recipe.AddIngredient(ItemID.TungstenBar, 2);
			recipe.AddIngredient(ModContent.ItemType<Magic_Hair_Spray>(), 5);
			recipe.AddIngredient(ModContent.ItemType<Rubber>(), 4);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

            recipe = Recipe.Create(ItemID.BallOHurt);
            recipe.AddIngredient(ItemID.DemoniteBar, 10);
            recipe.AddIngredient(ItemID.ShadowScale, 5);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = Recipe.Create(ItemID.BloodButcherer);
			recipe.AddIngredient(ItemID.CrimtaneBar, 6);
			recipe.AddIngredient(ItemID.TissueSample, 3);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

            recipe = Recipe.Create(ItemID.CrimsonRod);
            recipe.AddIngredient(ItemID.CrimtaneBar, 10);
            recipe.AddIngredient(ItemID.TissueSample, 6);
            recipe.AddRecipeGroup("Origins:Gem Staves");
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = Recipe.Create(ItemID.ScarabBomb, 3);
            recipe.AddIngredient(ItemID.Bomb, 3);
            recipe.AddIngredient(ItemID.FossilOre);
            recipe.Register();

            recipe = Recipe.Create(ItemID.Beenade, 6);
            recipe.AddIngredient(ItemID.BeeWax);
            recipe.AddIngredient(ItemID.Grenade, 6);
            recipe.Register();

            recipe = Recipe.Create(ItemID.TheRottedFork);
			recipe.AddIngredient(ItemID.CrimtaneBar, 9);
			recipe.AddIngredient(ItemID.TissueSample, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(ItemID.TheUndertaker);
			recipe.AddIngredient(ItemID.CrimtaneBar, 6);
			recipe.AddIngredient(ItemID.TissueSample, 4);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(ItemID.Vilethorn);
			recipe.AddIngredient(ItemID.DemoniteBar, 10);
			recipe.AddIngredient(ItemID.ShadowScale, 6);
			recipe.AddRecipeGroup("Origins:Gem Staves");
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(ItemID.Coal);
			recipe.AddIngredient(ModContent.ItemType<Peat_Moss_Item>());
			recipe.Register();

			recipe = Recipe.Create(ItemID.Torch, 5);
			recipe.AddIngredient(ItemID.Coal);
			recipe.AddIngredient(ItemID.Wood);
			recipe.Register();

            recipe = Recipe.Create(ItemID.CelestialSigil);
            recipe.AddIngredient(ItemID.FragmentNebula, 12);
            recipe.AddIngredient(ItemID.FragmentSolar, 12);
            recipe.AddIngredient(ItemID.FragmentStardust, 12);
            recipe.AddIngredient(ItemID.FragmentVortex, 12);
            recipe.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 12);
            recipe.Register();

            recipe = Recipe.Create(ItemID.LunarHook);
            recipe.AddIngredient(ItemID.FragmentNebula, 6);
            recipe.AddIngredient(ItemID.FragmentSolar, 6);
            recipe.AddIngredient(ItemID.FragmentStardust, 6);
            recipe.AddIngredient(ItemID.FragmentVortex, 6);
            recipe.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 6);
            recipe.Register();

            recipe = Recipe.Create(ItemID.SuperHealingPotion, 5);
            recipe.AddIngredient(ItemID.FragmentNebula);
            recipe.AddIngredient(ItemID.FragmentSolar);
            recipe.AddIngredient(ItemID.FragmentStardust);
            recipe.AddIngredient(ItemID.FragmentVortex);
            recipe.AddIngredient(ModContent.ItemType<Nova_Fragment>());
            recipe.Register();

            recipe = Recipe.Create(ItemID.FragmentNebula);
            recipe.AddIngredient(ItemID.FragmentSolar);
            recipe.AddIngredient(ItemID.FragmentStardust);
            recipe.AddIngredient(ItemID.FragmentVortex);
            recipe.AddIngredient(ModContent.ItemType<Nova_Fragment>());
            recipe.Register();

            recipe = Recipe.Create(ItemID.FragmentSolar);
            recipe.AddIngredient(ItemID.FragmentNebula);
            recipe.AddIngredient(ItemID.FragmentStardust);
            recipe.AddIngredient(ItemID.FragmentVortex);
            recipe.AddIngredient(ModContent.ItemType<Nova_Fragment>());
            recipe.Register();

            recipe = Recipe.Create(ItemID.FragmentStardust);
            recipe.AddIngredient(ItemID.FragmentNebula);
            recipe.AddIngredient(ItemID.FragmentSolar);
            recipe.AddIngredient(ItemID.FragmentVortex);
            recipe.AddIngredient(ModContent.ItemType<Nova_Fragment>());
            recipe.Register();

            recipe = Recipe.Create(ItemID.FragmentVortex);
            recipe.AddIngredient(ItemID.FragmentNebula);
            recipe.AddIngredient(ItemID.FragmentSolar);
            recipe.AddIngredient(ItemID.FragmentStardust);
            recipe.AddIngredient(ModContent.ItemType<Nova_Fragment>());
            recipe.Register();

            //this hook is supposed to be used for adding recipes,
            //but since it also runs after a lot of other stuff I tend to use it for a lot of unrelated stuff
            Origins.instance.LateLoad();
		}
		public override void PostUpdateInput() {
		}
		public override void PostUpdateTime() {
			foreach (var quest in Quest_Registry.NetQuests) {
				quest.CheckSync();
			}
		}
		public static int GemStaffRecipeGroupID { get; private set; }
		public static int DeathweedRecipeGroupID { get; private set; }
		public static int RottenChunkRecipeGroupID { get; private set; }
		public static int ShadowScaleRecipeGroupID { get; private set; }
		public static int CursedFlameRecipeGroupID { get; private set; }
		public override void AddRecipeGroups() {
			GemStaffRecipeGroupID = RecipeGroup.RegisterGroup("Origins:Gem Staves", new RecipeGroup(() => $"{Lang.misc[37].Value} Gem Staff", new int[] {
				ItemID.AmethystStaff,
				ItemID.TopazStaff,
				ItemID.SapphireStaff,
				ItemID.EmeraldStaff,
				ItemID.RubyStaff,
				ItemID.DiamondStaff
			}));
			DeathweedRecipeGroupID = ALRecipeGroups.Deathweed.RegisteredId;
			RottenChunkRecipeGroupID = ALRecipeGroups.RottenChunks.RegisteredId;
			ShadowScaleRecipeGroupID = ALRecipeGroups.ShadowScales.RegisteredId;
			CursedFlameRecipeGroupID = ALRecipeGroups.CursedFlames.RegisteredId;
			RecipeGroup sandGroup = RecipeGroup.recipeGroups[RecipeGroupID.Sand];
			sandGroup.ValidItems.Add(ModContent.ItemType<Defiled_Sand_Item>());
			sandGroup.ValidItems.Add(ModContent.ItemType<Silica_Item>());
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
				if (r.Matches((ItemID.PotatoChips, null), new int[] { TileID.Pots }, (ItemID.PotatoChips, null), (ItemID.PotionOfReturn, 7))) {
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
				if (journalUI?.CurrentState is not null) {

				}
			}
			setBonusHUDInterface.Update(gameTime);
			itemUseHUDInterface.Update(gameTime);
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
				layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
					 "Origins: Held Item HUD",
					 delegate {
						 itemUseHUDInterface?.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
						 return true;
					 },
					 InterfaceScaleType.UI)
				);
				layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
					 "Origins: Set Bonus HUD",
					 delegate {
						 setBonusHUDInterface?.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
						 return true;
					 },
					 InterfaceScaleType.UI)
				);
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
		}
		public override void OnLocalizationsLoaded() {
			Dictionary<string, LocalizedText> texts = LocalizationMethods._localizedTexts.GetValue(LanguageManager.Instance);
			texts["Riven"] = texts["Mods.Origins.Generic.Riven"];
			texts["Riven_Hive"] = texts["Mods.Origins.Generic.Riven_Hive"];
			texts["Dusk"] = texts["Mods.Origins.Generic.Dusk"];
			texts["Defiled"] = texts["Mods.Origins.Generic.Defiled"];
			texts["Defiled_Wastelands"] = texts["Mods.Origins.Generic.Defiled_Wastelands"];
			texts["The_Defiled_Wastelands"] = texts["Mods.Origins.Generic.The_Defiled_Wastelands"];
			if (OriginsModIntegrations.CheckAprilFools()) {
				foreach (var text in texts.ToList()) {
					if (text.Key.StartsWith("Mods.Origins.AprilFools")) {
						string key = text.Key.Replace("AprilFools.", "");
						if (texts.TryGetValue(key, out LocalizedText targetText)) {
							LocalizationMethods._value.SetValue(targetText, text.Value.Value);
							LocalizationMethods._hasPlurals.SetValue(targetText, LocalizationMethods._hasPlurals.GetValue(text.Value));
							LocalizationMethods.BoundArgs.SetValue(targetText, text.Value.BoundArgs);
						} else {
							Mod.Logger.Warn($"Adding April Fools text instead of replacing existing text: {text.Key}");
							texts[key] = text.Value;
						}
					}
				}
			}
			Regex substitutionRegex = new Regex("{§(.*?)}", RegexOptions.Compiled);
			foreach (var text in texts.ToList()) {
				Match subMatch = substitutionRegex.Match(text.Value.Value);
				while (subMatch.Success) {
					LocalizationMethods._value.SetValue(text.Value, text.Value.Value.Replace(subMatch.Groups[0].Value, Language.GetTextValue(subMatch.Groups[1].Value)));
					subMatch = substitutionRegex.Match(text.Value.Value);
				}
			}
		}
		public override void PreUpdateProjectiles() {
			for (int i = 0; i < Main.maxProjectiles; i++) {
				if (Main.projectile[i].TryGetGlobalProjectile(out OriginGlobalProj global) && global.isFromMitosis) {
					Main.player[Main.projectile[i].owner].ownedProjectileCounts[Main.projectile[i].type]--;
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
			}else if (forceThunderstormDelay > 0) {
				if (--forceThunderstormDelay <= 0) forceThunderstorm = true;
			}
		}
		bool hasLoggedPUP = false;
		public override void PreUpdatePlayers() {
			OriginPlayer.LocalOriginPlayer = Main.LocalPlayer.TryGetModPlayer(out OriginPlayer localPlayer) ? localPlayer : null;
			if (OriginPlayer.playersByGuid is null) OriginPlayer.playersByGuid = new();
			else OriginPlayer.playersByGuid.Clear();
			if (!hasLoggedPUP) {
				hasLoggedPUP = true;
				Mod.Logger.Info($"Running {nameof(PreUpdatePlayers)} in netmode {Main.netMode}");
			}
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
