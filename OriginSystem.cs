using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using static Origins.Origins;
using static Tyfyter.Utils.UITools;

namespace Origins {
    public partial class OriginSystem : ModSystem {
        public static OriginSystem instance { get; private set;}
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
            //this hook is supposed to be used for adding recipes,
            //but since it also runs after a lot of other stuff I tend to use it for a lot of unrelated stuff
            Origins.instance.LateLoad();
        }
        public override void AddRecipeGroups() {
            RecipeGroup group = new RecipeGroup(() => "Gem Staves", new int[] {
                ItemID.AmethystStaff,
                ItemID.TopazStaff,
                ItemID.SapphireStaff,
                ItemID.EmeraldStaff,
                ItemID.RubyStaff,
                ItemID.DiamondStaff
            });
            RecipeGroup.RegisterGroup("Origins:Gem Staves", group);
        }
        public override void PostAddRecipes() {
            int l = Main.recipe.Length;
            Recipe r;
            Recipe recipe;
            int roseID = ModContent.ItemType<Wilting_Rose_Item>();
            for (int i = 0; i < l; i++) {
                r = Main.recipe[i];
                if (!r.requiredItem.ToList().Exists((ing) => ing.type == ItemID.Deathweed)) {
                    continue;
                }
                recipe = r.Clone();
                recipe.requiredItem = recipe.requiredItem.Select((it) => it.type == ItemID.Deathweed ? new Item(roseID) : it.CloneByID()).ToList();
                Mod.Logger.Info("adding procedural recipe: " + recipe.Stringify());
                recipe.Create();
            }
        }
        public override void ModifyLightingBrightness(ref float scale) {
            if (Main.LocalPlayer.GetModPlayer<OriginPlayer>().plagueSightLight) {
                scale *= 1.03f;
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
                        setBonusUI?.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI) { Active = Main.playerInventory }
                );
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                    "Origins: Journal UI",
                    delegate {
                        journalUI?.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI) { Active = Main.playerInventory }
                );
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
