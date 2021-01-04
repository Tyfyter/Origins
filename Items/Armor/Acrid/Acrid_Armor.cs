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
            item.defense = 10;
        }
        public override void UpdateEquip(Player player) {
            Lighting.AddLight(player.Center, new Vector3(0, 1, (float)Math.Abs(Math.Sin(Main.GameUpdateCount/60f))));
        }
    }
    [AutoloadEquip(EquipType.Body)]
	public class Acrid_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Breastplate");
			Tooltip.SetDefault("Increases life regeneration");
		}
        public override void SetDefaults() {
            item.defense = 18;
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
            item.defense = 14;
        }
        public override void UpdateEquip(Player player) {
            player.swimTime = 2;
        }
    }
}
