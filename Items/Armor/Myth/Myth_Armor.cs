using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Myth {
    [AutoloadEquip(EquipType.Head)]
    public class Mythic_Skull : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Mythic Skull");
            Tooltip.SetDefault("Increased explosive velocity\n'Skull ON'");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.defense = 4;
            Item.rare = ItemRarityID.Green;
        }
        public override void UpdateEquip(Player player) {
            player.GetModPlayer<OriginPlayer>().explosiveThrowSpeed+=0.15f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Mythic_Shell>() && legs.type == ModContent.ItemType<Mythic_Leggings>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "Explosive damage and velocity increased by movement speed\nBoosted knockback from explosions\nChance to dash toward explosions";
            //player.GetModPlayer<OriginPlayer>().mythSet = true;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Bone, 10);
            recipe.AddIngredient(ItemID.Cloud, 5);
            recipe.AddIngredient(ItemID.Feather, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    [AutoloadEquip(EquipType.Body)]
    public class Mythic_Shell : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Mythic Shell");
            Tooltip.SetDefault("Greatly increased jump speed");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.defense = 4;
            Item.wornArmor = true;
            Item.rare = ItemRarityID.Green;
        }
        public override void UpdateEquip(Player player) {
        player.jumpSpeedBoost += 0.45f;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Bone, 25);
            recipe.AddIngredient(ItemID.Cloud, 15);
            recipe.AddIngredient(ItemID.Feather, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    [AutoloadEquip(EquipType.Legs)]
    public class Mythic_Leggings : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Mythic Leggings");
            Tooltip.SetDefault("Greatly increased movement speed");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.defense = 4;
            Item.rare = ItemRarityID.Green;
        }
        public override void UpdateEquip(Player player) {
            player.moveSpeed+=0.2f;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Bone, 15);
            recipe.AddIngredient(ItemID.Cloud, 10);
            recipe.AddIngredient(ItemID.Feather, 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
