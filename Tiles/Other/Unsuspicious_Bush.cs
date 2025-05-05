using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.NPCs.Defiled;
using Origins.NPCs.MiscE;
using Origins.NPCs.Riven;
using Origins.World.BiomeData;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Other {
	public class Unsuspicious_Bush : ModTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLavaDeath[Type] = true;
			TileID.Sets.HasOutlines[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 3, 0);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.CoordinateHeights = [16, 18];
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(100, 157, 6));
			DustType = DustID.GrassBlades;
		}
		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => false;
		public override bool RightClick(int i, int j) {
			OriginSystem.Instance.ForceAF ^= true;
			return true;
		}
	}
	public class Unsuspicious_Bush_Item : ModItem {
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Unsuspicious_Bush>());
			Item.value = Item.sellPrice(gold: 0);
			Item.rare = ItemRarityID.Green;
			Item.placeStyle = 0;
		}
	}
}
