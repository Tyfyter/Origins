using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Tools.Liquids;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Origins.Items;
class Item_Lubrication : GlobalItem {
	static AutoLoadingTexture texture = "Origins/UI/Item_Oiled_Overlay";
	public override bool AppliesToEntity(Item entity, bool lateInstantiation) => !lateInstantiation && entity.ModItem is IOilableItem;
	public override bool CanRightClick(Item item) {
		if (Main.mouseRightRelease && Main.mouseItem.ModItem is Oil_Bucket bucket) {
			bucket.ConsumeOil(Main.LocalPlayer);
			ApplyOil((IOilableItem)item.ModItem);
			Main.mouseRightRelease = false;
			return false;
		}
		return base.CanRightClick(item);
	}
	static readonly Regex afterLineRegex = new("^(Tooltip\\d+|Placeable|Ammo|Consumable|Material|Wireable)$", RegexOptions.Compiled);
	public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
		IOilableItem oilable = (IOilableItem)item.ModItem;
		for (int i = tooltips.Count - 1; i >= 0; i--) {
			if (afterLineRegex.IsMatch(tooltips[i].Name)) {
				tooltips.Insert(i + 1, new(Mod, "Oilable",
					oilable.OilCount > 0 ?
					Language.GetTextValue("Mods.Origins.Items.GenericTooltip.Oiled", oilable.OilCount, oilable.MaxOilCount) :
					Language.GetTextValue("Mods.Origins.Items.GenericTooltip.Oilable")
				));
				break;
			}
		}
	}
	public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
		if (((IOilableItem)item.ModItem).OilApplied()) DrawOilOverlay(item, spriteBatch, position);
	}
	public static void DrawOilOverlay(Item item, SpriteBatch spriteBatch, Vector2 position) {
		Texture2D texture = Item_Lubrication.texture;
		Rectangle frame = texture.Frame();
		Vector2 offset = new(14, 12);
		if (item.ModItem is Oil_Bucket) offset -= Vector2.One * 2;
		spriteBatch.Draw(texture, position + offset * Main.inventoryScale, frame, Color.White, 0f, frame.Size() / 2f, Main.inventoryScale * 0.85f, SpriteEffects.None, 0f);
	}
	public override void SaveData(Item item, TagCompound tag) {
		tag[nameof(IOilableItem.OilCount)] = ((IOilableItem)item.ModItem).OilCount;
	}
	public override void LoadData(Item item, TagCompound tag) {
		((IOilableItem)item.ModItem).OilCount = tag.SafeGet(nameof(IOilableItem.OilCount), 0);
	}
	static void ApplyOil(IOilableItem item) => item.OilCount = item.MaxOilCount;
}
public interface IOilableItem {
	public int MaxOilCount { get; }
	public int OilCount { get; set; }
}
public static class OilExtensions {
	public static bool OilApplied(this IOilableItem item) {
		if (((ModItem)item).Item.prefix == Oily_Prefix.ID) item.OilCount = item.MaxOilCount;
		return item.OilCount > 0;
	}

	public static void ConsumeOil(this IOilableItem item, int count = 1) {
		item.OilCount = Math.Max(item.OilCount - count, 0);
		if (((ModItem)item).Item.prefix == Oily_Prefix.ID) item.OilCount = item.MaxOilCount;
	}
}
