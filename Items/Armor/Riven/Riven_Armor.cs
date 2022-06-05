using Origins.Items.Materials;
using Terraria;
using Origins.Buffs;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Origins.Items.Armor.Riven {
    [AutoloadEquip(EquipType.Head)]
    public class Riven_Mask : ModItem {
        public const float lightMagnitude = 0.3f;
        public static short GlowMask = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Mask");
            Tooltip.SetDefault("Increases minion damage by 10%");
            GlowMask = Origins.AddGlowMask("Armor/Riven/Riven_Mask_Head_Glow");
        }
        public override void SetDefaults() {
            Item.defense = 6;
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.GetDamage(DamageClass.Summon) += 0.1f;
            Lighting.AddLight(player.Top+new Vector2(0,8), 0.666f*lightMagnitude, 0.414f*lightMagnitude, 0.132f*lightMagnitude);
        }
        public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
            glowMask = GlowMask;
            glowMaskColor = Color.White;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
            return body.type == ModContent.ItemType<Riven_Coat>() && legs.type == ModContent.ItemType<Riven_Pants>();
        }
        public override void UpdateArmorSet(Player player) {
            player.setBonus = "Increases minion damage by up to 30% when over half health";
            player.GetModPlayer<OriginPlayer>().rivenSet = true;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Infested_Bar>(), 15);
            //recipe.AddIngredient(ModContent.ItemType<>(), 10);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    [AutoloadEquip(EquipType.Body)]
    public class Riven_Coat : ModItem {
        public static short GlowMask = -1;
        public static short femaleGlowMask = -1;
        public static short armGlowMask = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Coat");
            Tooltip.SetDefault("Increases your max number of minions by 1");
            GlowMask = Origins.AddGlowMask("Armor/Riven/Riven_Coat_Body_Glow");
            femaleGlowMask = Origins.AddGlowMask("Armor/Riven/Riven_Coat_Female_Glow");
            armGlowMask = Origins.AddGlowMask("Armor/Riven/Riven_Coat_Arms_Glow");
        }
        public override void SetDefaults() {
            Item.defense = 7;
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.maxMinions++;
            Lighting.AddLight(player.Center, 0.666f*Riven_Mask.lightMagnitude, 0.414f*Riven_Mask.lightMagnitude, 0.132f*Riven_Mask.lightMagnitude);
        }
        public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
            glowMask = drawPlayer.Male?GlowMask:femaleGlowMask;
            glowMaskColor = Color.White;
        }
        public override void ArmorArmGlowMask(Player drawPlayer, float shadow, ref int glowMask, ref Color color) {
            glowMask = armGlowMask;
            color = Color.White;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Infested_Bar>(), 25);
            //recipe.AddIngredient(ModContent.ItemType<>(), 20);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    [AutoloadEquip(EquipType.Legs)]
    public class Riven_Pants : ModItem {
        public static short GlowMask = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Riven Pants");
            Tooltip.SetDefault("Increases jump height");
            GlowMask = Origins.AddGlowMask("Armor/Riven/Riven_Pants_Legs_Glow");
        }
        public override void SetDefaults() {
            Item.defense = 6;
            Item.rare = ItemRarityID.Blue;
        }
        public override void UpdateEquip(Player player) {
            player.jumpSpeedBoost+=1f;
            Lighting.AddLight(player.Center+new Vector2(0,16), 0.666f*Riven_Mask.lightMagnitude, 0.414f*Riven_Mask.lightMagnitude, 0.132f*Riven_Mask.lightMagnitude);
        }
        public override void DrawArmorColor(Player drawPlayer, float shadow, ref Color color, ref int glowMask, ref Color glowMaskColor) {
            glowMask = GlowMask;
            glowMaskColor = Color.White;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Infested_Bar>(), 20);
            //recipe.AddIngredient(ModContent.ItemType<>(), 15);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
