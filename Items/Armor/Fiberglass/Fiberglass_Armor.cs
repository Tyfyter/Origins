using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Fiberglass {
    [AutoloadEquip(EquipType.Head)]
	public class Fiberglass_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Helmet");
			Tooltip.SetDefault("This doesn't seem very protective");
		}
		public override void SetDefaults() {
            item.defense = 5;
		}
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Fiberglass_Body>() && legs.type == ModContent.ItemType<Fiberglass_Legs>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "Weapon damage increased by 4";
            player.GetModPlayer<OriginPlayer>().fiberglassSet = true;
            if(player.HasBuff(BuffID.Invisibility))player.buffTime[player.FindBuffIndex(BuffID.Invisibility)]++;
        }
        public override void DrawHair(ref bool drawHair, ref bool drawAltHair) { drawAltHair = true; }
	}
    [AutoloadEquip(EquipType.Body)]
	public class Fiberglass_Body : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Pauldrons");
			Tooltip.SetDefault("These don't seem very protective");
		}
		public override void SetDefaults() {
            item.defense = 6;
		}
        public override void UpdateVanity(Player player, EquipType equipType) {
            player.GetModPlayer<OriginPlayer>().DrawShirt = true;
        }
	}
    [AutoloadEquip(EquipType.Legs)]
	public class Fiberglass_Legs : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Boots");
			Tooltip.SetDefault("These don't seem very protective");
		}
		public override void SetDefaults() {
            item.defense = 5;
		}
        public override void UpdateVanity(Player player, EquipType equipType) {
            player.GetModPlayer<OriginPlayer>().DrawPants = true;
        }
	}
}
