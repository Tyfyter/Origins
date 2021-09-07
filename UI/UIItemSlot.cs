using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.UI;

namespace Origins.UI {
    public class RefItemSlot : UIElement {
		internal Ref<Item> item;
		internal readonly int _context;
		internal readonly int color;
		private readonly float _scale;
		internal Func<Item, bool> ValidItemFunc;
        protected internal int index = -1;
		public RefItemSlot(int colorContext = ItemSlot.Context.CraftingMaterial, int context = ItemSlot.Context.InventoryItem, float scale = 1f, Ref<Item> _item = null) {
			color = colorContext;
            _context = context;
			_scale = scale;
			item = _item;
			Width.Set(Main.inventoryBack9Texture.Width * scale, 0f);
			Height.Set(Main.inventoryBack9Texture.Height * scale, 0f);
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			float oldScale = Main.inventoryScale;
			Main.inventoryScale = 0.85f * _scale;
			Rectangle rectangle = GetDimensions().ToRectangle();

			if (ContainsPoint(Main.MouseScreen) && !PlayerInput.IgnoreMouseInterface) {
				Main.LocalPlayer.mouseInterface = true;
				if (ValidItemFunc == null || ValidItemFunc(Main.mouseItem)) {
                    ItemSlot.Handle(ref item.Value, _context);
				}
			}
			// Draw draws the slot itself and Item. Depending on context, the color will change, as will drawing other things like stack counts.
			ItemSlot.Draw(spriteBatch, ref item.Value, color, rectangle.TopLeft());
			Main.inventoryScale = oldScale;
		}
	}
}