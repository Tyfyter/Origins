using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Fiberglass {
    [AutoloadEquip(EquipType.Head)]
	public class Fiberglass_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Helmet");
			Tooltip.SetDefault("This doesn't seem very protective");
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.defense = 5;
		}
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Fiberglass_Body>() && legs.type == ModContent.ItemType<Fiberglass_Legs>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "Weapon damage increased by 4";
            player.GetModPlayer<OriginPlayer>().fiberglassSet = true;
            int inv = player.FindBuffIndex(BuffID.Invisibility);
            if(inv>-1)player.buffTime[inv]++;
        }
	}
    [AutoloadEquip(EquipType.Body)]
	public class Fiberglass_Body : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Pauldrons");
			Tooltip.SetDefault("These don't seem very protective");
			ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = false;
			ArmorIDs.Body.Sets.HidesBottomSkin[Item.bodySlot] = false;
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.defense = 6;
		}
        public override void UpdateVanity(Player player) {
            player.GetModPlayer<OriginPlayer>().drawShirt = true;
        }
	}
    [AutoloadEquip(EquipType.Legs)]
	public class Fiberglass_Legs : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Boots");
			Tooltip.SetDefault("These don't seem very protective");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.defense = 5;
		}
        public override void UpdateVanity(Player player) {
            player.GetModPlayer<OriginPlayer>().drawPants = true;
        }
	}
}
