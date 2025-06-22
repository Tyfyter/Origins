using Microsoft.Xna.Framework;
using Origins.World.BiomeData;
using System;
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
		public static int AddBarTile(ModItem item, Color? color = null, int dust = -1, string displayName = "MapObject.MetalBar") {
			Bar_Tile tile = new(item.Name, Language.GetText(displayName), color);
			if (!Main.dedServ && !ModContent.HasAsset(tile.Texture)) {
				throw new System.Exception($"Tried to add bar tile with texture \"{tile.Texture}\", but no texture exists at that path");
			}
			if (dust != -1) tile.DustType = dust;
			item.Mod.AddContent(tile);
			return tile.Type;
		}
	}
}
