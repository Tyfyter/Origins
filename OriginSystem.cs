using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Projectiles;
using Origins.Questing;
using Origins.Tiles.Other;
using Origins.UI;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static Tyfyter.Utils.UITools;

namespace Origins {
	public partial class OriginSystem : ModSystem {
		public static OriginSystem instance { get; private set; }
		public UserInterface setBonusUI;
		public UserInterfaceWithDefaultState journalUI;
		public override void Load() {
			instance = this;
			setBonusUI = new UserInterface();
			journalUI = new UserInterfaceWithDefaultState() {
				DefaultUIState = new Journal_UI_Button()
			};
		}
		public override void Unload() {
			instance = null;
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
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();

			recipe = Recipe.Create(ItemID.CrystalNinjaHelmet);
			recipe.AddIngredient(ItemID.CrystalShard, 30);
			recipe.AddIngredient(ItemID.SoulofNight, 5);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 15);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();

			recipe = Recipe.Create(ItemID.CrystalNinjaChestplate);
			recipe.AddIngredient(ItemID.CrystalShard, 60);
			recipe.AddIngredient(ItemID.SoulofNight, 7);
			recipe.AddIngredient(ModContent.ItemType<Carburite_Item>(), 30);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();

			recipe = Recipe.Create(ItemID.CrystalNinjaLeggings);
			recipe.AddIngredient(ItemID.CrystalShard, 45);
			recipe.AddIngredient(ItemID.SoulofNight, 3);
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

			recipe = Recipe.Create(ItemID.BloodButcherer);
			recipe.AddIngredient(ItemID.CrimtaneBar, 6);
			recipe.AddIngredient(ItemID.TissueSample, 3);
			recipe.AddTile(TileID.Anvils);
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

			recipe = Recipe.Create(ItemID.CrimsonRod);
			recipe.AddIngredient(ItemID.CrimtaneBar, 10);
			recipe.AddIngredient(ItemID.TissueSample, 6);
			recipe.AddRecipeGroup("Origins:Gem Staves");
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(ItemID.Vilethorn);
			recipe.AddIngredient(ItemID.DemoniteBar, 10);
			recipe.AddIngredient(ItemID.ShadowScale, 6);
			recipe.AddRecipeGroup("Origins:Gem Staves");
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(ItemID.BallOHurt);
			recipe.AddIngredient(ItemID.DemoniteBar, 10);
			recipe.AddIngredient(ItemID.ShadowScale, 5);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = Recipe.Create(ItemID.Coal);
			recipe.AddIngredient(ModContent.ItemType<Peat_Moss>());
			recipe.Register();

			recipe = Recipe.Create(ItemID.Torch, 5);
			recipe.AddIngredient(ItemID.Coal);
			recipe.AddIngredient(ItemID.Wood);
			recipe.Register();

			recipe = Recipe.Create(ItemID.LesserHealingPotion, 2);
			recipe.AddIngredient(ItemID.Bottle, 2);
			recipe.AddIngredient(ModContent.ItemType<Tree_Sap>());
			recipe.AddTile(TileID.Bottles);
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
			RecipeGroup group = new RecipeGroup(() => $"{Lang.misc[37].Value} Gem Staff", new int[] {
				ItemID.AmethystStaff,
				ItemID.TopazStaff,
				ItemID.SapphireStaff,
				ItemID.EmeraldStaff,
				ItemID.RubyStaff,
				ItemID.DiamondStaff
			});
			GemStaffRecipeGroupID = RecipeGroup.RegisterGroup("Origins:Gem Staves", group);
			group = new RecipeGroup(() => Lang.misc[37].Value + " " + Lang.GetItemName(ItemID.Deathweed), new int[] {
				ItemID.Deathweed,
				ModContent.ItemType<Wilting_Rose_Item>(),
				ModContent.ItemType<Wrycoral_Item>()
			});
			DeathweedRecipeGroupID = RecipeGroup.RegisterGroup("Deathweed", group);
			group = new RecipeGroup(() => Lang.misc[37].Value + " " + Lang.GetItemName(ItemID.RottenChunk), new int[] {
				ItemID.RottenChunk,
				ItemID.Vertebrae,
				ModContent.ItemType<Strange_String>(),
				ModContent.ItemType<Bud_Barnacle>()
			});
			RottenChunkRecipeGroupID = RecipeGroup.RegisterGroup("Rotten Chunk", group);
			group = new RecipeGroup(() => Lang.misc[37].Value + " " + Lang.GetItemName(ItemID.ShadowScale), new int[] {
				ItemID.ShadowScale,
				ItemID.TissueSample,
				ModContent.ItemType<Undead_Chunk>(),
				ModContent.ItemType<Riven_Carapace>()
			});
			ShadowScaleRecipeGroupID = RecipeGroup.RegisterGroup("Shadow Scale", group);
			group = new RecipeGroup(() => Lang.misc[37].Value + " " + Lang.GetItemName(ItemID.CursedFlame), new int[] {
				ItemID.CursedFlame,
				ItemID.Ichor,
				ModContent.ItemType<Alkahest>(),
				ModContent.ItemType<Black_Bile>()
			});
			CursedFlameRecipeGroupID = RecipeGroup.RegisterGroup("Cursed Flames", group);
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
				if (setBonusUI?.CurrentState is Eyndum_Core_UI eyndumCoreUIState) {
					OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
					if (eyndumCoreUIState?.itemSlot?.item == originPlayer.eyndumCore) {
						if (!originPlayer.eyndumSet) {
							if (eyndumCoreUIState?.itemSlot?.item?.Value?.IsAir ?? true) {
								setBonusUI.SetState(null);
							} else {
								eyndumCoreUIState.hasSetBonus = false;
								setBonusUI.Update(gameTime);
							}
						} else {
							setBonusUI.Update(gameTime);
						}
					} else {
						setBonusUI.SetState(null);
					}
				} else if (setBonusUI?.CurrentState is Mimic_Selection_UI) {
					OriginPlayer originPlayer = Main.LocalPlayer.GetModPlayer<OriginPlayer>();
					if (originPlayer.mimicSet) {
						setBonusUI.Update(gameTime);
					} else {
						setBonusUI.SetState(null);
					}
				}
				if (journalUI?.CurrentState is not null) {

				}
			}
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1) {//error prevention & null check
				layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
					"Origins: Set Bonus UI",
					delegate {
						setBonusUI?.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
						return true;
					},
					InterfaceScaleType.UI) { Active = Main.playerInventory }
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
		public override void PreUpdateProjectiles() {
			for (int i = 0; i < Main.maxProjectiles; i++) {
				if (Main.projectile[i].TryGetGlobalProjectile(out OriginGlobalProj global) && global.isFromMitosis) {
					Main.player[Main.projectile[i].owner].ownedProjectileCounts[Main.projectile[i].type]--;
				}
			}
		}
	}
	public class TempleBiome : ModBiome {
		public override string Name => "Bestiary_Biomes.TheTemple";
		public override bool IsBiomeActive(Player player) {
			return player.ZoneLihzhardTemple;
		}
	}
}
