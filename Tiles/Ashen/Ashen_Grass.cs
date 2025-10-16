using Origins.Dev;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Tiles.Ashen {
	public class Ashen_Grass : OriginTile, IAshenTile {
		public string[] Categories => [
			WikiCategories.Grass
		];
		public override void SetStaticDefaults() {
			TileID.Sets.Grass[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.Conversion.Grass[Type] = true;
			TileID.Sets.Conversion.MergesWithDirtInASpecialWay[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeDugByShovel[Type] = true;
			Main.tileMerge[Type][ModContent.TileType<Murky_Sludge>()] = true;
			Main.tileMerge[ModContent.TileType<Murky_Sludge>()][Type] = true;
			Origins.TileTransformsOnKill[Type] = true;
			Main.tileBrick[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(FromHexRGB(0x5a4e6d));
			DustType = DustID.Demonite;
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (fail && !effectOnly) {
				Framing.GetTileSafely(i, j).TileType = TileID.Dirt;
			}
		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid) {
				if (WorldGen.genRand.NextBool(250)) above.SetToType((ushort)ModContent.TileType<Fungarust>(), Main.tile[i, j].TileColor);
				else above.SetToType((ushort)ModContent.TileType<Ashen_Foliage>(), Main.tile[i, j].TileColor);

				WorldGen.TileFrame(i, j - 1);
			}
		}
	}
	public class Ashen_Jungle_Grass : OriginTile, IAshenTile {
		public override void SetStaticDefaults() {
			if (ModLoader.HasMod("InfectedQualities")) {
				TileID.Sets.JungleBiome[Type] = 1;
				TileID.Sets.RemixJungleBiome[Type] = 1;
			}
			Origins.PotType.Add(Type, ((ushort)ModContent.TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)ModContent.TileType<Ashen_Foliage>(), 0, 6));
			TileID.Sets.GrassSpecial[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.Conversion.JungleGrass[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeDugByShovel[Type] = true;
			Origins.TileTransformsOnKill[Type] = true;
			Main.tileBrick[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(FromHexRGB(0x5a4e6d));
			DustType = DustID.Demonite;
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (fail && !effectOnly) Framing.GetTileSafely(i, j).TileType = TileID.Mud;
		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid) {
				if (WorldGen.genRand.NextBool(250)) above.SetToType((ushort)ModContent.TileType<Fungarust>(), Main.tile[i, j].TileColor);
				else above.SetToType((ushort)ModContent.TileType<Ashen_Foliage>(), Main.tile[i, j].TileColor);

				WorldGen.TileFrame(i, j - 1);
			}
		}
	}
	public class Ashen_Murky_Sludge_Grass : OriginTile, IAshenTile {
		public string[] Categories => [
			WikiCategories.Grass
		];
		public override void SetStaticDefaults() {
			Origins.PotType.Add(Type, ((ushort)ModContent.TileType<Ashen_Pot>(), 0, 0));
			Origins.PileType.Add(Type, ((ushort)ModContent.TileType<Ashen_Foliage>(), 0, 6));
			TileID.Sets.Grass[Type] = true;
			TileID.Sets.NeedsGrassFraming[Type] = true;
			TileID.Sets.ChecksForMerge[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			TileID.Sets.CanBeDugByShovel[Type] = true;
			Origins.TileTransformsOnKill[Type] = true;
			Main.tileBrick[Type] = true;
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			AddMapEntry(FromHexRGB(0x5a4e6d));
			DustType = DustID.Demonite;
			HitSound = SoundID.Item167.WithPitchOffset(-2f);
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (fail && !effectOnly) {
				Framing.GetTileSafely(i, j).TileType = (ushort)ModContent.TileType<Murky_Sludge>();
			}
		}
		public override void RandomUpdate(int i, int j) {
			Tile above = Framing.GetTileSafely(i, j - 1);
			if (!above.HasTile && Main.tile[i, j].BlockType == BlockType.Solid) {
				if (WorldGen.genRand.NextBool(250)) above.SetToType((ushort)ModContent.TileType<Fungarust>(), Main.tile[i, j].TileColor);
				else above.SetToType((ushort)ModContent.TileType<Ashen_Foliage>(), Main.tile[i, j].TileColor);

				WorldGen.TileFrame(i, j - 1);
			}
		}
		public override void FloorVisuals(Player player) {
			player.AddBuff(ModContent.BuffType<Murky_Sludge_Debuff>(), 2);
		}
		public override bool HasWalkDust() {
			return Main.rand.NextBool(3, 25);
		}
		public override void WalkDust(ref int dustType, ref bool makeDust, ref Color color) {
			dustType = DustID.Snow;
			color = Main.rand.NextBool() ? FromHexRGB(0x5a4e6d) : FromHexRGB(0x2c212a);
		}
	}
	public class Ashen_Grass_Seeds : ModItem {
		public override void SetStaticDefaults() {
			ItemID.Sets.GrassSeeds[Type] = true;
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.CorruptSeeds);
		}
		public override void Load() {
			On_SmartCursorHelper.Step_GrassSeeds += On_SmartCursorHelper_Step_GrassSeeds;
		}
		public override void Unload() {
			On_SmartCursorHelper.Step_GrassSeeds -= On_SmartCursorHelper_Step_GrassSeeds;
		}
		private void On_SmartCursorHelper_Step_GrassSeeds(On_SmartCursorHelper.orig_Step_GrassSeeds orig, object providedInfo, ref int focusedX, ref int focusedY) {
			Player player = Main.LocalPlayer;
			if (player.HeldItem.type == ModContent.ItemType<Ashen_Grass_Seeds>()) {
				List<Tuple<int, int>> _targets = [];
				int tileTargetX = Player.tileTargetX;
				int tileTargetY = Player.tileTargetY;
				int tileRangeX = Player.tileRangeX;
				int tileRangeY = Player.tileRangeY;
				int tileBoost = player.HeldItem.tileBoost;
				int reachableStartX = (int)(player.position.X / 16f) - tileRangeX - tileBoost + 1;
				int reachableEndX = (int)((player.position.X + (float)player.width) / 16f) + tileRangeX + tileBoost - 1;
				int reachableStartY = (int)(player.position.Y / 16f) - tileRangeY - tileBoost + 1;
				int reachableEndY = (int)((player.position.Y + (float)player.height) / 16f) + tileRangeY + tileBoost - 2;
				reachableStartX = Utils.Clamp(reachableStartX, 10, Main.maxTilesX - 10);
				reachableEndX = Utils.Clamp(reachableEndX, 10, Main.maxTilesX - 10);
				reachableStartY = Utils.Clamp(reachableStartY, 10, Main.maxTilesY - 10);
				reachableEndY = Utils.Clamp(reachableEndY, 10, Main.maxTilesY - 10);

				_targets.Clear();
				for (int i = reachableStartX; i <= reachableEndX; i++) {
					for (int j = reachableStartY; j <= reachableEndY; j++) {
						Tile tile = Main.tile[i, j];
						bool flag = !Main.tile[i - 1, j].HasTile || !Main.tile[i, j + 1].HasTile || !Main.tile[i + 1, j].HasTile || !Main.tile[i, j - 1].HasTile;
						bool flag2 = !Main.tile[i - 1, j - 1].HasTile || !Main.tile[i - 1, j + 1].HasTile || !Main.tile[i + 1, j + 1].HasTile || !Main.tile[i + 1, j - 1].HasTile;
						if (tile.HasTile && !tile.IsActuated && (flag || flag2)) {
							if (tile.TileIsType(ModContent.TileType<Murky_Sludge>()) || tile.TileIsType(TileID.Dirt) || tile.TileIsType(TileID.Mud))
								_targets.Add(new Tuple<int, int>(i, j));
						}
					}
				}

				if (_targets.Count > 0) {
					float num = -1f;
					Tuple<int, int> tuple = _targets[0];
					for (int k = 0; k < _targets.Count; k++) {
						float num2 = Vector2.Distance(new Vector2(_targets[k].Item1, _targets[k].Item2) * 16f + Vector2.One * 8f, Main.MouseWorld);
						if (num == -1f || num2 < num) {
							num = num2;
							tuple = _targets[k];
						}
					}

					if (Collision.InTileBounds(tuple.Item1, tuple.Item2, reachableStartX, reachableStartY, reachableEndX, reachableEndY)) {
						focusedX = tuple.Item1;
						focusedY = tuple.Item2;
					}
				}

				_targets.Clear();
			}
			orig(providedInfo, ref focusedX, ref focusedY);
		}

		public override bool? UseItem(Player player) {
			if (Main.tile[Player.tileTargetX, Player.tileTargetY].TileType == ModContent.TileType<Murky_Sludge>()) return true;
			return null;
		}
		public override bool ConsumeItem(Player player) {
			ref ushort tileType = ref Main.tile[Player.tileTargetX, Player.tileTargetY].TileType;
			switch (tileType) {
				case TileID.CorruptGrass:
				tileType = (ushort)ModContent.TileType<Ashen_Grass>();
				break;
				case TileID.CorruptJungleGrass:
				tileType = (ushort)ModContent.TileType<Ashen_Jungle_Grass>();
				break;
			}
			if (tileType == ModContent.TileType<Murky_Sludge>()) tileType = (ushort)ModContent.TileType<Ashen_Murky_Sludge_Grass>();
			if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, Player.tileTargetX, Player.tileTargetY, tileType, 0);
			return true;
		}
	}
}
