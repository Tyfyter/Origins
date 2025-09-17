using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Terraria;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Ashen {
	public class Surveysprout : OriginTile, IAshenTile {
		private const int FrameWidth = 18; // A constant for readability and to kick out those magic numbers
        public string[] Categories => [
            "Plant"
        ];
        public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileObsidianKill[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			TileID.Sets.ReplaceTileBreakUp[Type] = true;
			TileID.Sets.IgnoredInHouseScore[Type] = true;
			TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
			TileID.Sets.TileCutIgnore.IgnoreDontHurtNature[Type] = true;
			TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]); // Make this tile interact with golf balls in the same way other plants do

			LocalizedText name = CreateMapEntryName();
			AddMapEntry(new Color(128, 128, 128), name);

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
			/*TileObjectData.newTile.AnchorValidTiles = new int[] {
				TileType<Sootgrass>(),
				TileType<Compact_Scrap>()
			};*/
			TileObjectData.newTile.AnchorAlternateTiles = [
				TileID.ClayPot,
				TileID.PlanterBox
			];
			TileObjectData.addTile(Type);

			HitSound = SoundID.Grass;
			DustType = DustID.Ash;
		}

		/*public override bool CanPlace(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j); // Safe way of getting a tile instance

			if (tile.HasTile) {
				int tileType = tile.TileType;
				if (tileType == Type) {
					int stage = GetStage(i, j); // The current stage of the herb

					// Can only place on the same herb again if it's blooming
					return stage >= 2;
				} else {
					// Support for vanilla herbs/grasses:
					if (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType] || tileType == TileID.WaterDrip || tileType == TileID.LavaDrip || tileType == TileID.HoneyDrip || tileType == TileID.SandDrip) {
						bool foliageGrass = tileType == TileID.Plants || tileType == TileID.Plants2;
						bool moddedFoliage = tileType >= TileID.Count && (Main.tileCut[tileType] || TileID.Sets.BreakableWhenPlacing[tileType]);
						bool harvestableVanillaHerb = Main.tileAlch[tileType] && WorldGen.IsHarvestableHerbWithSeed(tileType, tile.TileFrameX / 18);

						if (foliageGrass || moddedFoliage || harvestableVanillaHerb) {
							WorldGen.KillTile(i, j);
							if (!tile.HasTile && Main.netMode == NetmodeID.MultiplayerClient) {
								NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 0, i, j);
							}
							return true;
						}
					}
					return false;
				}
			}
			return true;
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 0) {
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
		}

		public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
			offsetY = -2; // This is -1 for tiles using StyleAlch, but vanilla sets to -2 for herbs, which causes a slight visual offset between the placement preview and the placed tile. 
		}
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			int stage = GetStage(i, j);

			if (stage < 1) {
				// Do not drop anything when just planted
				yield break;
			}

			Vector2 worldPosition = new Vector2(i, j).ToWorldCoordinates();
			Player nearestPlayer = Main.player[Player.FindClosest(worldPosition, 16, 16)];

			int herbItemType = ItemType<Wilting_Rose_Item>();
			int herbItemStack = 1;

			int seedItemType = 27;//ModContent.ItemType<ExampleHerbSeeds>();
			int seedItemStack = 1;

			if (nearestPlayer.active && nearestPlayer.HeldItem.type == ItemID.StaffofRegrowth) {
				// Increased yields with Staff of Regrowth, even when not fully grown
				herbItemStack = Main.rand.Next(1, 3);
				seedItemStack = Main.rand.Next(1, 6);
			} else if (stage >= 2) {
				// Default yields, only when fully grown
				herbItemStack = 1;
				seedItemStack = Main.rand.Next(1, 4);
			}

			if (herbItemStack > 0) {
				yield return new Item(herbItemType, herbItemStack);
			}

			if (seedItemStack > 0) {
				yield return new Item(seedItemType, seedItemStack);
			}
		}

		public override bool IsTileSpelunkable(int i, int j) {
			int stage = GetStage(i, j);

			// Only glow if the herb is grown
			return stage >= 2;
		}

		public override void RandomUpdate(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			int stage = GetStage(i, j);

			// Only grow to the next stage if there is a next stage. We don't want our tile turning pink!
			if (stage < 2) {
				// Increase the x frame to change the stage
				tile.TileFrameX += FrameWidth;

				// If in multiplayer, sync the frame change
				if (Main.netMode != NetmodeID.SinglePlayer) {
					NetMessage.SendTileSquare(-1, i, j, 1);
				}
			}
		}

		// A helper method to quickly get the current stage of the herb (assuming the tile at the coordinates is our herb)
		private static int GetStage(int i, int j) {
			Tile tile = Framing.GetTileSafely(i, j);
			return tile.TileFrameX / FrameWidth;
		}
	}*/
	}
	public class Surveysprout_Item : MaterialItem {
		public override int Value => Item.sellPrice(copper: 20);
		public override bool Hardmode => false;
	}
}
