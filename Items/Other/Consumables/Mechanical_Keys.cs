using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public abstract class Mechanical_Key_Base : ModItem {
		public abstract int Value { get; }
		public abstract int Rare { get; }
		public override void SetDefaults() {
			Item.value = Value;
			Item.rare = Rare;
		}
	}
	public class Mechanical_Key_Blue : Mechanical_Key_Base {
		public override int Value => Item.sellPrice(copper: 40);
		public override int Rare => ItemRarityID.Blue;
	}
	public class Mechanical_Key_Green : Mechanical_Key_Base {
		public override int Value => Item.sellPrice(copper: 40);
		public override int Rare => ItemRarityID.Blue;
	}
	public class Mechanical_Key_Orange : Mechanical_Key_Base {
		public override int Value => Item.sellPrice(copper: 40);
		public override int Rare => ItemRarityID.Blue;
	}
	public class Mechanical_Key_Purple : Mechanical_Key_Base {
		public override int Value => Item.sellPrice(copper: 40);
		public override int Rare => ItemRarityID.Blue;
	}
	public class Mechanical_Key_Yellow : Mechanical_Key_Base {
		public override int Value => Item.sellPrice(copper: 40);
		public override int Rare => ItemRarityID.Blue;
	}
}