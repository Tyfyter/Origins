using Microsoft.Xna.Framework;
using Origins.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Riven {
	public class Weak_Riven_Flesh : OriginTile, RivenTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			TileID.Sets.Conversion.Stone[Type] = true;
			TileID.Sets.CanBeClearedDuringGeneration[Type] = true;
			/*Main.tileMergeDirt[Type] = true;
            Main.tileMerge[Type] = Main.tileMerge[TileID.Stone];
            Main.tileMerge[Type][TileID.Stone] = true;
            for(int i = 0; i < TileLoader.TileCount; i++) {
                Main.tileMerge[i][Type] = Main.tileMerge[i][TileID.Stone];
            }*/
			//AddMapEntry(new Color(200, 125, 100));
			AddMapEntry(new Color(160, 100, 80));
			//SetModTree(Defiled_Tree.Instance);
			mergeID = TileID.Stone;
			//soundType = SoundID.NPCKilled;
		}
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) {
			fail = false;
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
			if (Main.tile[i, j].HasTile) {
				int adj = OriginSystem.GetAdjTileCount(i, j);
				if (adj < 4) {
					//Main.tile[i, j].active(false);
					GetInstance<OriginSystem>().QueueKillTile(i, j);
				} else if (adj > 7) {
					Main.tile[i, j].ResetToType((ushort)TileType<Riven_Flesh>());
				}
			}
			return base.TileFrame(i, j, ref resetFrame, ref noBreak);
		}
	}
}
