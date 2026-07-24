using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Tools.Liquids;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items;
class Item_Lubrication : GlobalItem {
	static AutoLoadingTexture texture = "Origins/UI/Item_Oiled_Overlay";
	public override bool AppliesToEntity(Item entity, bool lateInstantiation) => !lateInstantiation && entity.ModItem is IOilableItem;
	public override bool CanRightClick(Item item) {
		if (Main.mouseRightRelease && Main.mouseItem.ModItem is Oil_Bucket bucket) {
			if (!bucket.Endless) {
				if (Main.mouseItem.stack == 1) {
					Main.mouseItem.ChangeItemType(ItemID.EmptyBucket);
				} else {
					Main.LocalPlayer.GetItem(Main.myPlayer, new Item(ItemID.EmptyBucket, 1), new GetItemSettings(NoText: true, CanGoIntoVoidVault: true));
				}
			}
			((IOilableItem)item.ModItem).OilApplied = true;
			Main.mouseRightRelease = false;
			return false;
		}
		return base.CanRightClick(item);
	}
	public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
		if (((IOilableItem)item.ModItem).OilApplied) DrawOilOverlay(item, spriteBatch, position);
	}

	public static void DrawOilOverlay(Item item, SpriteBatch spriteBatch, Vector2 position) {
		Texture2D texture = Item_Lubrication.texture;
		Rectangle frame = texture.Frame();
		Vector2 offset = new(14, 12);
		if (item.ModItem is Oil_Bucket) offset -= Vector2.One * 2;
		spriteBatch.Draw(texture, position + offset * Main.inventoryScale, frame, Color.White, 0f, frame.Size() / 2f, Main.inventoryScale * 0.85f, SpriteEffects.None, 0f);
	}
}
public interface IOilableItem {
	public bool OilApplied { get; set; }
}
