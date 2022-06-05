using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Acrid{
    [AutoloadEquip(EquipType.Head)]
	public class Acrid_Helmet : ModItem {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Helmet");
			Tooltip.SetDefault("Emits light when worn");
		}
        public override void SetDefaults() {
            Item.defense = 10;
        }
        public override void UpdateEquip(Player player) {
            if(player.wet){
                player.nightVision = true;
            }
            Lighting.AddLight(player.Center, new Vector3(0, 1, (float)Math.Abs(Math.Sin(Main.GameUpdateCount/60f))));
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Acrid_Breastplate>() && legs.type == ModContent.ItemType<Acrid_Greaves>();
        }
        public override void UpdateArmorSet(Player player) {
            player.buffImmune[BuffID.Poisoned] = true;
            player.buffImmune[BuffID.Venom] = true;
            player.breathMax += (int)(player.breathMax*1.8f);
            if (Main.GameUpdateCount%4 != 0) {
                player.ignoreWater = true;
            }
        }
    }
    [AutoloadEquip(EquipType.Body)]
	public class Acrid_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Breastplate");
			Tooltip.SetDefault("Increases life regeneration");
		}
        public override void SetDefaults() {
            Item.defense = 18;
        }
        public override void UpdateEquip(Player player) {
            player.lifeRegenCount+=2;
        }
	}
    [AutoloadEquip(EquipType.Legs)]
	public class Acrid_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Greaves");
			Tooltip.SetDefault("Grants the ability to swim");
		}
        public override void SetDefaults() {
            Item.defense = 14;
        }
        public override void UpdateEquip(Player player) {
            player.accFlipper = true;
        }
    }
}
