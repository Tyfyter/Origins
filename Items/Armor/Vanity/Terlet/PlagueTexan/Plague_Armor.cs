using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Vanity.Terlet.PlagueTexan{
    [AutoloadEquip(EquipType.Head)]
	public class Plague_Texan_Mask : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plague Texan's Visage");
			Tooltip.SetDefault("Great for impersonating Origins devs!'\n");
		}
        public override void SetDefaults() {
            item.vanity = true;
        }
	}
    [AutoloadEquip(EquipType.Body)]
	public class Plague_Texan_Jacket : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plague Texan's Surprisingly Affordable Style");
			Tooltip.SetDefault("Great for impersonating Origins devs!'\n");
		}
        public override void SetDefaults() {
            item.vanity = true;
        }
	}
    [AutoloadEquip(EquipType.Legs)]
	public class Plague_Texan_Jeans : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plague Texan's Jeans");
			Tooltip.SetDefault("Great for impersonating Origins devs!'\n");
		}
        public override void SetDefaults() {
            item.vanity = true;
        }
    }
	public class Plague_Texan_Sight : ModItem {
        internal static int id;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plague Texan's Gift");
			Tooltip.SetDefault("Great for impersonating Origins devs!'\nForesight is '20/20'");
            id = item.type;
		}
        public override void SetDefaults() {
            item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.dangerSense = true;
            player.detectCreature = true;
            if(!hideVisual) {
                ApplyVisuals(player);
            }
        }
        public static void ApplyVisuals(Player player) {
            player.GetModPlayer<OriginPlayer>().PlagueSight = true;
            Lighting.AddLight(player.Center+new Vector2(3*player.direction,-6), Color.Gold.ToVector3()/3f);
        }
    }
}
