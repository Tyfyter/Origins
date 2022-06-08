using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Cryosten {
    [AutoloadEquip(EquipType.Head)]
	public class Cryosten_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cryosten Helmet");
			Tooltip.SetDefault("Increased life regeneration");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
		public override void SetDefaults() {
            Item.defense = 2;
		}
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().cryostenHelmet = true;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Cryosten_Breastplate>() && legs.type == ModContent.ItemType<Cryosten_Greaves>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "Life restoration from hearts increased";
            player.GetModPlayer<OriginPlayer>().cryostenSet = true;
            if(player.HasBuff(BuffID.Chilled))player.buffTime[player.FindBuffIndex(BuffID.Chilled)]--;
            if(player.HasBuff(BuffID.Frozen))player.buffTime[player.FindBuffIndex(BuffID.Frozen)]--;
            if(player.HasBuff(BuffID.Frostburn))player.buffTime[player.FindBuffIndex(BuffID.Frostburn)]--;
        }
	}
    [AutoloadEquip(EquipType.Body)]
	public class Cryosten_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cryosten Breastplate");
			Tooltip.SetDefault("20% increased maximum health");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
		public override void SetDefaults() {
            Item.defense = 3;
		}
        public override void UpdateEquip(Player player) {
            player.statLifeMax2+=player.statLifeMax2/5;
        }
	}
    [AutoloadEquip(EquipType.Legs)]
	public class Cryosten_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cryosten Greaves");
			Tooltip.SetDefault("5% increased movement speed\nincreased movement speed on ice");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
		public override void SetDefaults() {
            Item.defense = 2;
		}
        public override void UpdateEquip(Player player) {
            player.moveSpeed+=0.05f;
            player.iceSkate = true;
        }
    }
}
