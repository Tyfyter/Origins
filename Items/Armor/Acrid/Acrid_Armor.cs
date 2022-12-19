using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Origins.Items.Materials;
using Origins.Items.Accessories;

namespace Origins.Items.Armor.Acrid{
    [AutoloadEquip(EquipType.Head)]
	public class Acrid_Helmet : ModItem {
        public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Helmet");
			Tooltip.SetDefault("Emits light when worn");
            if (Main.netMode != NetmodeID.Server) {
                Origins.AddHelmetGlowmask(Item.headSlot, "Items/Armor/Acrid/Acrid_Helmet_Head_Glow");
            }
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.defense = 10;
            Item.rare = ItemRarityID.LightRed;
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
            player.setBonus = "we actually just didn't have set bonus text for it, it wasn't that it wasn't displaying it";
            player.buffImmune[BuffID.Poisoned] = true;
            player.buffImmune[BuffID.Venom] = true;
            player.breathMax += (int)(player.breathMax*1.8f);
            if (Main.GameUpdateCount%4 != 0) {
                player.ignoreWater = true;
            }
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Acrid_Bar>(), 15);
            recipe.AddIngredient(ModContent.ItemType<Rubber>(), 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
    [AutoloadEquip(EquipType.Body)]
	public class Acrid_Breastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Breastplate");
			Tooltip.SetDefault("Increased life regeneration");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.defense = 18;
            Item.rare = ItemRarityID.LightRed;
        }
        public override void UpdateEquip(Player player) {
            player.lifeRegenCount+=2;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Acrid_Bar>(), 25);
            recipe.AddIngredient(ModContent.ItemType<Rubber>(), 30);
            recipe.AddIngredient(ModContent.ItemType<Air_Pack>());
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
    [AutoloadEquip(EquipType.Legs)]
	public class Acrid_Greaves : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Acrid Greaves");
			Tooltip.SetDefault("Grants the ability to swim");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.defense = 14;
            Item.rare = ItemRarityID.LightRed;
        }
        public override void UpdateEquip(Player player) {
            player.accFlipper = true;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Acrid_Bar>(), 20);
            recipe.AddIngredient(ModContent.ItemType<Rubber>(), 20);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
