using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using System;
using System.Linq;


namespace Origins.UI {
    public class Eyndum_Core_UI : UIState {
        public RefItemSlot itemSlot = null;
        protected internal Queue<Action> itemSlotQueue = new Queue<Action>() { };
        public override void OnInitialize() {
            int slotX = Main.screenWidth - 64 - 28;
            int slotY = (int)((float)(174 + (!Main.mapFullscreen && Main.mapStyle == 1 ? 256 : 0)) + (float)(1 * 56) * 0.85f);

            // Ensures that the player's core slot item is not null, then adds the slot
            ref Ref<Item> item = ref Main.LocalPlayer.GetModPlayer<OriginPlayer>().eyndumCore;
            if (item is null) {
                item = new Ref<Item>(new Item());
                item.Value.SetDefaults(0);
            }
            AddItemSlot(item, new Vector2(slotX, slotY));
        }
        /// <summary>
        /// Passes the parameters to an action to be added in Update
        /// </summary>
        public void SafeAddItemSlot(Ref<Item> item, Vector2 position, bool usePercent = false, Func<Item, bool> ValidItemFunc = null, int colorContext = ItemSlot.Context.CraftingMaterial, int context = ItemSlot.Context.InventoryItem, float slotScale = 1f) {
            if (item.Value is null) {
                item.Value = new Item();
                item.Value.SetDefaults(0);
            }
            itemSlotQueue.Enqueue(() => AddItemSlot(item, position, usePercent, ValidItemFunc, colorContext, context, slotScale));
        }
        /// <summary>
        /// Adds a reference-based item slot to the ui state
        /// </summary>
        /// <param name="item"> the item that should be referenced by the new slot</param>
        /// <param name="_position">the position of the slot, leave as null to automatically place the slot</param>
        /// <param name="usePercent">ignored if position is null</param>
        /// <param name="_ValidItemFunc">passed to RefItemSlot constructor</param>
        /// <param name="colorContext">passed to RefItemSlot constructor</param>
        /// <param name="context">passed to RefItemSlot constructor</param>
        /// <param name="slotScale">passed to RefItemSlot constructor</param>
        public void AddItemSlot(Ref<Item> item, Vector2 position, bool usePercent = false, Func<Item, bool> _ValidItemFunc = null, int colorContext = ItemSlot.Context.CraftingMaterial, int context = ItemSlot.Context.InventoryItem, float slotScale = 1f) {
            RefItemSlot itemSlot = new RefItemSlot(_item: item, colorContext: colorContext, context: context, scale: slotScale) {
                ValidItemFunc = _ValidItemFunc ?? (i => true),
            };
            if (usePercent) {
                itemSlot.Left = new StyleDimension { Percent = position.X };
                itemSlot.Top = new StyleDimension { Percent = position.Y };
            } else {
                itemSlot.Left = new StyleDimension { Pixels = position.X };
                itemSlot.Top = new StyleDimension { Pixels = position.Y };
            }
            RemoveChild(itemSlot);
            this.itemSlot = itemSlot;
            Append(itemSlot);
        }
        public override void Update(GameTime gameTime) {
            if (itemSlotQueue.Count > 0) {
                itemSlotQueue.Dequeue()();
            }
            int slotX = Main.screenWidth - 64 - 28 - 142;
            int slotY = (int)((float)(174 + (!Main.mapFullscreen && Main.mapStyle == 1 ? 256 : 0)) + (float)(1 * 56) * 0.85f);
            itemSlot.Left = new StyleDimension { Pixels = slotX };
            itemSlot.Top = new StyleDimension { Pixels = slotY };
            base.Update(gameTime);
        }
    }
}