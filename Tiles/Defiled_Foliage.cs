using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Origins.Tiles {
    public class Defiled_Foliage : ModTile {
		public override void SetDefaults(){
			Main.tileFrameImportant[Type] = true;
			Main.tileCut[Type] = true;
			Main.tileNoFail[Type] = true;
			AddMapEntry(new Color(175, 175, 175));

			TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);

			TileObjectData.newTile.AnchorValidTiles = new int[]{
				ModContent.TileType<Defiled_Grass>()
			};

			TileObjectData.addTile(Type);
		}

		public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects){
			if (i % 2 == 1)
				spriteEffects = SpriteEffects.FlipHorizontally;
		}

		public override bool Drop(int i, int j){
			return false;
		}
    }
    public class Defiled_Grass_Seeds : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Defiled Seeds");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.GrassSeeds);
            //item.createTile;
		}
    }
}
