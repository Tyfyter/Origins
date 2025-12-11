using Origins.Dev;
using Origins.Tiles.Other;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Defiled {
	public class Defiled_Stone : ComplexFrameTile, IDefiledTile, ITileWithItem {
		public ModItem Item { get; private set; }
		public override void Load() {
			Mod.AddContent(Item = new TileItem(this).WithExtraStaticDefaults(static item => {
				item.ResearchUnlockCount = 100;
				ItemTrader.ChlorophyteExtractinator.AddOption_FromAny(ItemID.StoneBlock, item.type);
			}));
			Chambersite_Ore.Create(this, Item, () => Defiled_Wastelands.DefaultTileDust, hitSound: () => Origins.Sounds.DefiledIdle);
		}
		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)TileType<Defiled_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)TileType<Defiled_Foliage>(), 0, 6));
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			TileID.Sets.Stone[Type] = true;
			TileID.Sets.Conversion.Stone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeClearedDuringOreRunner[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.Stone];
			Main.tileMerge[Type][TileID.Stone] = true;
			for (int i = 0; i < TileLoader.TileCount; i++) {
				Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Stone];
				if (TileID.Sets.Grass[i] || TileID.Sets.GrassSpecial[i] || Main.tileSand[i]) {
					Main.tileMerge[Type][i] = true;
					Main.tileMerge[i][Type] = true;
				}
			}
			AddMapEntry(new Color(200, 200, 200));
			mergeID = TileID.Stone;
			MinPick = 65;
			MineResist = 2;
			HitSound = Origins.Sounds.DefiledIdle;
			DustType = Defiled_Wastelands.DefaultTileDust;
		}
		protected override IEnumerable<TileOverlay> GetOverlays() {
			yield return new TileMergeOverlay(merge + "Mud_Overlay", TileID.Sets.Mud);
		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid && Main.rand.NextBool(250)) {
				above.SetToType((ushort)TileType<Soulspore>(), Main.tile[i, j].TileColor);
				WorldGen.TileFrame(i, j - 1);
			}
		}
	}
}
