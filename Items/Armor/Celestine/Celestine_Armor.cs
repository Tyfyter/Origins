using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Celestine {
    [AutoloadEquip(EquipType.Head)]
	public class Celestine_Helmet : ModItem {
        public override string Texture => "Origins/Items/Armor/Felnum/Felnum_Helmet";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Celestine Helmet");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.defense = 21;
		}
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Celestine_Breastplate>() && legs.type == ModContent.ItemType<Celestine_Greaves>();
        }
	}
    [AutoloadEquip(EquipType.Body)]
	public class Celestine_Breastplate : ModItem {
        public override string Texture => "Origins/Items/Armor/Felnum/Felnum_Breastplate";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Celestine Breastplate");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.defense = 32;
		}
	}
    [AutoloadEquip(EquipType.Legs)]
	public class Celestine_Greaves : ModItem {
        public override string Texture => "Origins/Items/Armor/Felnum/Felnum_Greaves";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Celestine Greaves");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.defense = 25;
		}
    }
}
