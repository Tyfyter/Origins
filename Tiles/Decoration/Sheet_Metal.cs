using Origins.World.BiomeData;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Decoration {
	public class Sheet_Metal : OriginTile {
		public override void Load() {
			Mod.AddContent(new TileItem(this).WithExtraStaticDefaults(i => RegisterItemDrop(i.type, -1)));
		}
		public override void SetStaticDefaults() {
			// Properties
			TileID.Sets.CanBeSloped[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileID.Sets.HasOutlines[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			HitSound = SoundID.Tink;

			// Names
			AddMapEntry(new Color(21, 28, 25));

			// Placement
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3Wall);
			TileObjectData.newTile.RandomStyleRange = 4;
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.addTile(Type);
			DustType = Ashen_Biome.DefaultTileDust;
		}
	}
}
