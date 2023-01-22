using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Riptide {
    [AutoloadEquip(EquipType.Head)]
    public class Riptide_Helmet : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riptide Helm");
            Tooltip.SetDefault("10% increased magic damage\nGreatly extends underwater breathing");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.defense = 4;
            Item.value = Item.buyPrice(silver: 30);
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.GetDamage(DamageClass.Magic) += 0.1f;
            player.breath += 63;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Riptide_Breastplate>() && legs.type == ModContent.ItemType<Riptide_Greaves>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "10% increased magic damage when submerged or in rain\nGrants the ability to dash which releases a tidal wave upon dashing";
            player.dashType = 4;
            player.dashDelay = -1;
            //player.GetModPlayer<OriginPlayer>().riptideSet = true;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.ShellPileBlock, 9);
            recipe.AddIngredient(ItemID.DivingHelmet);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    [AutoloadEquip(EquipType.Body)]
    public class Riptide_Breastplate : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riptide Breastplate");
            Tooltip.SetDefault("Emit a small aura of light\n+60 mana capacity");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.defense = 5;
            Item.value = Item.buyPrice(silver: 30);
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.statManaMax2 += 60;
            Lighting.AddLight(player.Center, new Vector3(0, 1, 1));
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.ShellPileBlock, 17);
            recipe.AddIngredient(ItemID.BlueJellyfish, 3);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.ShellPileBlock, 17);
            recipe.AddIngredient(ItemID.GreenJellyfish, 3);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();

            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.ShellPileBlock, 17);
            recipe.AddIngredient(ItemID.PinkJellyfish, 3);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    [AutoloadEquip(EquipType.Legs)]
    public class Riptide_Greaves : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riptide Leggings");
            Tooltip.SetDefault("10% increased magic damage\nGrants the ability to swim and provides increased movement speed in water");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.defense = 4;
            Item.value = Item.buyPrice(silver: 30);
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.GetDamage(DamageClass.Magic) += 0.1f;
            player.accFlipper = true;
            if (player.adjWater) {
                player.moveSpeed *= 1.25f;
            }
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.ShellPileBlock, 13);
            recipe.AddIngredient(ItemID.Flipper);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
