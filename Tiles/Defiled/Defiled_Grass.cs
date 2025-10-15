using Microsoft.Xna.Framework;
using Origins.Dev;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Defiled_Grass : OriginTile, IDefiledTile {
        public string[] Categories => [
            WikiCategories.Grass
        ];
        public override void SetStaticDefaults() {
			TileID.Sets.Grass[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.Conversion.Grass[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.Grass];
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			Main.tileMerge[Type][TileID.Mud] = true;
			Main.tileMerge[TileID.Mud][Type] = true;
			Origins.TileTransformsOnKill[Type] = true;
			HitSound = Origins.Sounds.DefiledIdle;
			for (int i = 0; i < TileLoader.TileCount; i++) {
				if (TileID.Sets.Grass[i] || TileID.Sets.GrassSpecial[i]) {
					Main.tileMerge[Type][i] = true;
					Main.tileMerge[i][Type] = true;
				}
			}
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(200, 200, 200));
			//SetModTree(Defiled_Tree.Instance);
			AddDefiledTile();
			DustType = Defiled_Wastelands.DefaultTileDust;
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (fail && (!effectOnly || WorldGen.genRand.NextBool(3))) {
				Framing.GetTileSafely(i, j).TileType = TileID.Dirt;
				WorldGen.SquareTileFrame(i, j);
			}
			OriginSystem originWorld = ModContent.GetInstance<OriginSystem>();
			if (originWorld is not null) {
				originWorld.defiledAltResurgenceTiles ??= [];
				originWorld.defiledAltResurgenceTiles.Add((i, j, Type, TileID.Dirt));
			}
		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid) {
				if (WorldGen.genRand.NextBool(250)) {
					above.SetToType((ushort)ModContent.TileType<Soulspore>(), Main.tile[i, j].TileColor);
				} else {
					if (WorldGen.genRand.NextBool(200)) {
						ushort bramble = (ushort)ModContent.TileType<Tangela_Bramble>();
						if (TileObject.CanPlace(i, j - 1, bramble, 0, 1, out TileObject objectData, onlyCheck: false)) {
							TileObjectData tileData = TileObjectData.GetTileData(bramble, objectData.style);
							int left = i - tileData.Origin.X;
							int top = (j - 1) - tileData.Origin.Y;
							for (int y = 0; y < tileData.Height; y++) {
								for (int x = 0; x < tileData.Width; x++) {
									Tile tileSafely = Framing.GetTileSafely(left + x, top + y);
									if (tileSafely.HasTile || tileSafely.LiquidAmount > 0) goto fail;
								}
							}
							if (TileObject.Place(objectData)) WorldGen.SquareTileFrame(i, j - 1);
							return;
							fail:;
						}
					}
					above.SetToType((ushort)ModContent.TileType<Defiled_Foliage>(), Main.tile[i, j].TileColor);
				}
				WorldGen.TileFrame(i, j - 1);
			}
		}
	}
	public class Defiled_Jungle_Grass : OriginTile, IDefiledTile {
		public override void SetStaticDefaults() {
			if (ModLoader.HasMod("InfectedQualities")) {
				TileID.Sets.JungleBiome[Type] = 1;
				TileID.Sets.RemixJungleBiome[Type] = 1;
			}
			TileID.Sets.GrassSpecial[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.Conversion.JungleGrass[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			Main.tileMerge[Type] = Main.tileMerge[TileID.JungleGrass];
			Main.tileMerge[Type][TileID.Dirt] = true;
			Main.tileMerge[TileID.Dirt][Type] = true;
			Main.tileMerge[Type][TileID.Mud] = true;
			Main.tileMerge[TileID.Mud][Type] = true;
			Origins.TileTransformsOnKill[Type] = true;
			HitSound = Origins.Sounds.DefiledIdle;
			for (int i = 0; i < TileLoader.TileCount; i++) {
				if (TileID.Sets.Grass[i] || TileID.Sets.GrassSpecial[i]) {
					Main.tileMerge[Type][i] = true;
					Main.tileMerge[i][Type] = true;
				}
			}
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(new Color(180, 180, 180));
			//SetModTree(Defiled_Tree.Instance);
			AddDefiledTile();
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (fail && !effectOnly) {
				Framing.GetTileSafely(i, j).TileType = TileID.Mud;
			}
			OriginSystem originWorld = ModContent.GetInstance<OriginSystem>();
			if (originWorld is not null) {
				originWorld.defiledAltResurgenceTiles ??= [];
				originWorld.defiledAltResurgenceTiles.Add((i, j, Type, TileID.Mud));
			}
		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid) {
				if (WorldGen.genRand.NextBool(250)) {
					above.SetToType((ushort)ModContent.TileType<Soulspore>(), Main.tile[i, j].TileColor);
				} else {
					if (WorldGen.genRand.NextBool(200)) {
						ushort bramble = (ushort)ModContent.TileType<Tangela_Bramble>();
						if (TileObject.CanPlace(i, j - 1, bramble, 0, 1, out TileObject objectData, onlyCheck: false)) {
							TileObjectData tileData = TileObjectData.GetTileData(bramble, objectData.style);
							int left = i - tileData.Origin.X;
							int top = (j - 1) - tileData.Origin.Y;
							for (int y = 0; y < tileData.Height; y++) {
								for (int x = 0; x < tileData.Width; x++) {
									Tile tileSafely = Framing.GetTileSafely(left + x, top + y);
									if (tileSafely.HasTile || tileSafely.LiquidAmount > 0) goto fail;
								}
							}
							if (TileObject.Place(objectData)) WorldGen.SquareTileFrame(i, j - 1);
							return;
							fail:;
						}
					}
					above.SetToType((ushort)ModContent.TileType<Defiled_Foliage>(), Main.tile[i, j].TileColor);
				}
				WorldGen.TileFrame(i, j - 1);
			}
		}
	}
	public class Defiled_Grass_Seeds : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.GrassSeeds[Type] = true;
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CorruptSeeds);
		}
		public override bool ConsumeItem(Player player) {
			ref ushort tileType = ref Main.tile[Player.tileTargetX, Player.tileTargetY].TileType;
			switch (tileType) {
				case TileID.CorruptGrass:
				tileType = (ushort)ModContent.TileType<Defiled_Grass>();
				break;
				case TileID.CorruptJungleGrass:
				tileType = (ushort)ModContent.TileType<Defiled_Jungle_Grass>();
				break;
			}
			if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, Player.tileTargetX, Player.tileTargetY, tileType, 0);
			return true;
		}
	}
}
