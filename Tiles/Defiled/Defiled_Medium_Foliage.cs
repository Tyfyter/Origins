using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles.Defiled {
    public class Defiled_Medium_Foliage : ModTile {
		public override void SetStaticDefaults(){
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new Color(175, 175, 175));

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);

			TileObjectData.newTile.AnchorValidTiles = new int[]{
				ModContent.TileType<Defiled_Grass>()
			};

			TileObjectData.addTile(Type);
            //soundType = SoundID.Grass;
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects){
			if (i % 2 == 1)spriteEffects = SpriteEffects.FlipHorizontally;
		}

        public override bool Drop(int i, int j){
			return false;
		}
    }
}
