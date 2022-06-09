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
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.vanity = true;
        }
	}
    [AutoloadEquip(EquipType.Body)]
	public class Plague_Texan_Jacket : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plague Texan's Surprisingly Affordable Style");
			Tooltip.SetDefault("Great for impersonating Origins devs!'\n");
            ArmorIDs.Body.Sets.HidesHands[Item.bodySlot] = false;
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.vanity = true;
        }
    }
    [AutoloadEquip(EquipType.Legs)]
	public class Plague_Texan_Jeans : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plague Texan's Jeans");
			Tooltip.SetDefault("Great for impersonating Origins devs!'\n");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.vanity = true;
        }
    }
	public class Plague_Texan_Sight : ModItem {
        public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Plague Texan's Gift");
			Tooltip.SetDefault("Great for impersonating Origins devs!'\nForesight is '20/20'");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            ID = Item.type;
		}
        public override void SetDefaults() {
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual) {
            player.dangerSense = true;
            player.detectCreature = true;
            if(!hideVisual) {
                ApplyVisuals(player);
            }
        }
        public static void ApplyVisuals(Player player) {
            player.GetModPlayer<OriginPlayer>().plagueSight = true;
            Lighting.AddLight(player.Center+new Vector2(3*player.direction,-6), Color.Gold.ToVector3()/3f);
        }
    }
}
