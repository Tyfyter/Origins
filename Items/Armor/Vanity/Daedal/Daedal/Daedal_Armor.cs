//remove the asterisk below when the item gets a sprite
/*/
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.Daedal.Daedal{
    [AutoloadEquip(EquipType.Head)]
	public class Daedal_Mask : ModItem {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("*insert name here*");
			Tooltip.SetDefault("Great for impersonating Origins devs!'");
		}
        public override void SetDefaults() {
            item.vanity = true;
        }
	}
    [AutoloadEquip(EquipType.Body)]
	public class Daedal_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("*insert name here*");
			Tooltip.SetDefault("Great for impersonating Origins devs!'");
		}
        public override void SetDefaults() {
            item.vanity = true;
        }
	}
    [AutoloadEquip(EquipType.Legs)]
	public class Daedal_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("*insert name here*");
			Tooltip.SetDefault("Great for impersonating Origins devs!'");
		}
        public override void SetDefaults() {
            item.vanity = true;
        }
    }
}
//*/