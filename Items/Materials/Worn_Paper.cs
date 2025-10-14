using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Materials {
	public abstract class Worn_Paper : ModItem {
		public override string Texture => typeof(Worn_Paper).GetDefaultTMLName();
		public override void SetDefaults() {
			Item.rare = ItemRarityID.Gray;
			Item.width = 16;
			Item.height = 16;
			Item.maxStack = Item.CommonMaxStack;
		}
	}
	public class Worn_Paper_Loose_Wheel : Worn_Paper { }
	public class Worn_Paper_Self_Preservation : Worn_Paper { }
	public class Worn_Paper_Smog_Test : Worn_Paper { }
	public class Worn_Paper_The_Packing_Slip : Worn_Paper { }
	public class Worn_Paper_They_Found_Us : Worn_Paper { }
}
