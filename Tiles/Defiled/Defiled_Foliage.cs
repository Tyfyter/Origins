using Microsoft.Xna.Framework.Graphics;
using Origins.NPCs.Critters;
using Origins.Tiles.Ashen;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
	public class Defiled_Foliage : ModTile, IDefiledTile {
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			TileID.Sets.IgnoredByGrowingSaplings[Type] = true;
			AddMapEntry(new Color(175, 175, 175));
			HitSound = Origins.Sounds.DefiledIdle;

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
			int[] validTiles = [
				ModContent.TileType<Defiled_Grass>(),
				ModContent.TileType<Defiled_Stone>(),
				ModContent.TileType<Defiled_Jungle_Grass>()
			];

			TileObjectData.newTile.AnchorValidTiles = [..validTiles,
				TileID.Stone,
				TileID.Grass
			];

			TileObjectData.addTile(Type);

			PileConversionGlobal.AddConversion(TileID.SmallPiles, [0, 1, 2, 3, 4, 5], Type, [..validTiles]);
			HitSound = Origins.Sounds.DefiledHurt.WithVolume(0.25f);
			DustType = Defiled_Wastelands.DefaultTileDust;
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) {
			if (i % 2 == 1) spriteEffects = SpriteEffects.FlipHorizontally;
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			Main.tile[i, j].TileFrameX = (short)(WorldGen.genRand.Next(6) * 18);
			ushort anchorType = Main.tile[i, j + 1].TileType;
			if (!TileObjectData.GetTileData(Main.tile[i, j]).isValidTileAnchor(anchorType)) {
				if (TileID.Sets.Conversion.Grass[anchorType]) {
					switch (anchorType) {
						case TileID.Grass:
						Main.tile[i, j].TileType = TileID.Plants;
						return true;
						case TileID.CorruptGrass:
						Main.tile[i, j].TileType = TileID.CorruptPlants;
						return true;
						case TileID.CrimsonGrass:
						Main.tile[i, j].TileType = TileID.CrimsonPlants;
						return true;
						case TileID.HallowedGrass:
						Main.tile[i, j].TileType = TileID.HallowedPlants;
						return true;
						default:
						if (anchorType == ModContent.TileType<Riven_Grass>()) {
							Main.tile[i, j].TileType = (ushort)ModContent.TileType<Riven_Foliage>();
							return true;
						}
						if (anchorType == ModContent.TileType<Ashen_Grass>()) {
							Main.tile[i, j].TileType = (ushort)ModContent.TileType<Ashen_Foliage>();
							return true;
						}
						break;
					}
				}
				WorldGen.KillTile(i, j, noItem: WorldGen.gen);
			}
			return false;
		}
		public override bool CanDrop(int i, int j) => true;
		public override IEnumerable<Item> GetItemDrops(int i, int j) {
			int critterType = ModContent.NPCType<Cicada_3301>();
			int chance = 150, _ = 0;
			TileLoader.DropCritterChance(i, j, Type, ref _, ref chance, ref _);
			if (chance > 0 && NPC.CountNPCS(critterType) < 5 && WorldGen.genRand.NextBool(chance)) {
				NPC npc = NPC.NewNPCDirect(new EntitySource_TileBreak(i, j), i * 16 + 10, j * 16, critterType);
				npc.TargetClosest();
				npc.velocity.Y = WorldGen.genRand.Next(-50, -21) * 0.1f;
				npc.velocity.X = WorldGen.genRand.Next(0, 26) * -0.1f * npc.direction;
				npc.direction *= -1;
				npc.netUpdate = true;
			}
			yield break;
		}
	}
}
