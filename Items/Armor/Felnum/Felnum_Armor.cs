using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Felnum {
    [AutoloadEquip(EquipType.Head)]
	public class Felnum_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Helmet");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.defense = 5;
		}
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Felnum_Breastplate>() && legs.type == ModContent.ItemType<Felnum_Greaves>();
        }
        public override void UpdateArmorSet(Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            player.setBonus = "Weapon damage increased by 4";
            originPlayer.felnumSet = true;
            if(player.velocity.Length()>4) {
                originPlayer.felnumShock+=player.velocity.Length()/4;
            } else if (originPlayer.felnumShock>0){
                originPlayer.felnumShock--;
            }
            if(player.HasBuff(BuffID.Electrified)) {
                int id = BuffID.BeetleMight1;
                if(player.HasBuff(BuffID.BeetleMight1)) {
                    id = BuffID.BeetleMight2;
                } else if(player.HasBuff(BuffID.BeetleMight2)) {
                    id = BuffID.BeetleMight3;
                } else if(player.HasBuff(BuffID.BeetleMight3)) {
                    id = BuffID.BeetleEndurance3;
                }
                player.buffType[player.FindBuffIndex(BuffID.Electrified)] = id;
            }
        }
	}
    [AutoloadEquip(EquipType.Body)]
	public class Felnum_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Breastplate");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.defense = 6;
		}
	}
    [AutoloadEquip(EquipType.Legs)]
	public class Felnum_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Felnum Greaves");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.defense = 5;
		}
	}
}
