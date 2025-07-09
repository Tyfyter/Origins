using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace Origins.Tiles.Limestone {
	public class Limestone_Furniture : FurnitureSet<Limestone_Item> {
		public override Color MapColor => new(180, 172, 134);
		public override int DustType => DustID.Sand;
	}
}
