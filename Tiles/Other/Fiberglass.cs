using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles {
    //current sprites are very unfinished
    public class Fiberglass : OriginTile {
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
            Main.tileLighted[Type] = false;
            Main.tileMergeDirt[Type] = false;
			ItemDrop = ItemType<Fiberglass_Item>();
			ModTranslation name = CreateMapEntryName();
			AddMapEntry(new Color(42, 116, 160), name);
		}
		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak) {
            OriginSystem originWorld = OriginSystem.instance;
            originWorld.AddFiberglassFrameTile(i, j);
			return true;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
        }
    }
    public class Fiberglass_Item : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Fiberglass");
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Glass);
            Item.createTile = TileType<Fiberglass>();
            Item.rare = ItemRarityID.Green;
		}
    }
}
