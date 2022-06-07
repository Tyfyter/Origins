using Microsoft.Xna.Framework;
using Origins.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using static Origins.Origins;

namespace Origins {
    public partial class OriginSystem : ModSystem {
        public static OriginSystem instance { get; private set;}
        public UserInterface setBonusUI;
        public override void Load() {
            instance = this;
            setBonusUI = new UserInterface();
        }
        public override void Unload() {
            instance = null;
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
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1) {//error prevention & null check
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
                    "Origins: Eyndum Core UI",
                    delegate {
                        setBonusUI?.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI) { Active = Main.playerInventory }
                );
            }
        }

    }
}
