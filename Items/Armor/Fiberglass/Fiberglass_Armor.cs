using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Armor.Fiberglass {
    [AutoloadEquip(EquipType.Head)]
	public class Fiberglass_Helmet : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Helmet");
			Tooltip.SetDefault("This doesn't seem very protective");
			ArmorIDs.Head.Sets.DrawHatHair[Item.headSlot] = true;
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
            Item.defense = 5;
		}
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Fiberglass_Body>() && legs.type == ModContent.ItemType<Fiberglass_Legs>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "Weapon damage increased by 4";
			player.GetDamage(DamageClass.Default).Flat += 4;
			player.GetDamage(DamageClass.Generic).Flat += 4;
			//player.GetModPlayer<OriginPlayer>().fiberglassSet = true;
            int inv = player.FindBuffIndex(BuffID.Invisibility);
            if(inv>-1)player.buffTime[inv]++;
        }
	}
    [AutoloadEquip(EquipType.Body)]
	public class Fiberglass_Body : ModItem {
		public static int SlotID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Pauldrons");
			Tooltip.SetDefault("These don't seem very protective");
			//ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = true;
			//ArmorIDs.Body.Sets.HidesBottomSkin[Item.bodySlot] = true;
			SacrificeTotal = 1;
			SlotID = Item.bodySlot;
		}
		public override void SetDefaults() {
            Item.defense = 6;
		}
	}
    [AutoloadEquip(EquipType.Legs)]
	public class Fiberglass_Legs : ModItem {
		public static int SlotID { get; private set; }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Fiberglass Boots");
			Tooltip.SetDefault("These don't seem very protective");
			SacrificeTotal = 1;
			//ArmorIDs.Legs.Sets.HidesTopSkin[Item.legSlot] = true;
			//ArmorIDs.Legs.Sets.HidesBottomSkin[Item.legSlot] = true;
			SlotID = Item.legSlot;
		}
		public override void SetDefaults() {
            Item.defense = 5;
		}
	}
}
