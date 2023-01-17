﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Face)]
    public class Powerflower : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Powerflower");
            Tooltip.SetDefault("Chance for mana stars to fall from critical hits\n8% reduced mana cost\nAutomatically use mana potions when needed");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            sbyte slot = Item.faceSlot;
            Item.CloneDefaults(ItemID.ManaFlower);
            Item.accessory = true;
            Item.faceSlot = slot;
            Item.rare = ItemRarityID.Pink;
            Item.value = Item.sellPrice(gold: 3);
        }
        public override void UpdateEquip(Player player) {
            OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
            originPlayer.dimStarlight = true;
            player.manaFlower = true;
            float light = 0.2f+(originPlayer.dimStarlightCooldown/1000f);
            Lighting.AddLight(player.Center,0.3f,0.3f,0f);
        }
    }
}