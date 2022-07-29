using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Armor.Vanity.Terlet.PlagueTexan{
    [AutoloadEquip(EquipType.Head)]
	public class Plague_Texan_Mask : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plague Texan's Visage");
			Tooltip.SetDefault("Great for impersonating Origins devs!'\n");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.vanity = true;
            Item.rare = ItemRarityID.Expert;
        }
	}
    [AutoloadEquip(EquipType.Body)]
	public class Plague_Texan_Jacket : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plague Texan's Surprisingly Affordable Style");
			Tooltip.SetDefault("Great for impersonating Origins devs!'\n");
            ArmorIDs.Body.Sets.HidesHands[Item.bodySlot] = false;
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.vanity = true;
            Item.rare = ItemRarityID.Expert;
        }
    }
    [AutoloadEquip(EquipType.Legs)]
	public class Plague_Texan_Jeans : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plague Texan's Jeans");
			Tooltip.SetDefault("Great for impersonating Origins devs!'\n");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.vanity = true;
            Item.rare = ItemRarityID.Expert;
        }
    }
	public class Plague_Texan_Sight : ModItem {
        public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plague Texan's Gift");
			Tooltip.SetDefault("Great for impersonating Origins devs!'\nForesight is '20/20'");
            SacrificeTotal = 1;
            ID = Item.type;
		}
        public override void SetDefaults() {
            Item.accessory = true;
            Item.rare = ItemRarityID.Expert;
        }
        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.dangerSense = true;
            player.detectCreature = true;
            player.GetModPlayer<OriginPlayer>().plagueSightLight = true;
            if (!hideVisual) {
                ApplyVisuals(player);
            }
        }
        public static void ApplyVisuals(Player player) {
            player.GetModPlayer<OriginPlayer>().plagueSight = true;
            Color color = Color.Gold;
			if (OriginExtensions.IsDevName(player.name, 1)) {
                color = new Color(43, 185, 255);
			}
            Lighting.AddLight(player.Center+new Vector2(3*player.direction,-6), color.ToVector3()/3f);
        }
    }
}
