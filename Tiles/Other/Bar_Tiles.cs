using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
	[Autoload(false)]
	public class Bar_Tile : OriginTile {
		public readonly string name;
		public readonly LocalizedText displayName;
		public readonly Color? color;
		public override string Name => name;
		public override string Texture => base.Texture + "_Tile";
		public Bar_Tile() {
			name = base.Name;
		}
		public Bar_Tile(string name, LocalizedText displayName, Color? color) {
			this.name = name;
			this.displayName = displayName;
			this.color = color;
		}
		public override void SetStaticDefaults() {
			Main.tileShine[Type] = 1100;
			Main.tileSolid[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileFrameImportant[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.addTile(Type);
			if (color.HasValue) {
				AddMapEntry(color.Value, displayName);
			}
		}
		public static int AddBarTile(ModItem item, Color? color = null) {
			Bar_Tile tile = new(item.Name, item.DisplayName, color);
			if (!ModContent.HasAsset(tile.Texture)) {
				item.Mod.Logger.Error($"Tried to add bar tile with texture \"{tile.Texture}\", but that no texture exists at that path");
				return -1;
			}
			item.Mod.AddContent(tile);
			return tile.Type;
		}
	}
}
