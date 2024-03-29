﻿using Origins.Dev;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOff)]
    public class Bug_Trapper : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "Vitality"
        };
        static short glowmask;
        public override void SetStaticDefaults() {
            glowmask = Origins.AddGlowMask(this);
        }
        public override void SetDefaults() {
            Item.DefaultToAccessory(38, 20);
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Blue;
            Item.glowMask = glowmask;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Shackle);
            recipe.AddIngredient(ModContent.ItemType<Primordial_Soup>());
            recipe.AddTile(TileID.TinkerersWorkbench);
			recipe.Register();
        }
        public override void UpdateAccessory(Player player, bool isHidden) {
            player.GetModPlayer<OriginPlayer>().bugZapper = true;
        }
	}
}
