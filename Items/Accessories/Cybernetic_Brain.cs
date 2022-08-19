using Origins.Buffs;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    public class Cybernetic_Brain : ModItem {
        static short glowmask;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Cybernetic Brain");
            Tooltip.SetDefault("Increases mana regeneration speed\nGrants immunity to most debuffs");
            glowmask = Origins.AddGlowMask(this);
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.accessory = true;
            Item.rare = Butterscotch.ID;
            Item.glowMask = glowmask;
        }
        public override void UpdateEquip(Player player) {
            player.buffImmune[BuffID.Confused] = true;
            player.buffImmune[BuffID.Darkness] = true;
            player.buffImmune[BuffID.Blackout] = true;
            player.buffImmune[BuffID.Silenced] = true;
            player.buffImmune[BuffID.Horrified] = true;
            player.buffImmune[BuffID.Cursed] = true;
            player.buffImmune[BuffID.Slow] = true;
            player.buffImmune[BuffID.Weak] = true;
            player.buffImmune[Toxic_Shock_Debuff.ID] = true;
            player.buffImmune[Solvent_Debuff.ID] = true;
            player.manaRegen += 5;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Strange_String>(), 8); //Could use vertebra, rotten chunks, or riven item
            recipe.AddIngredient(ModContent.ItemType<Formium_Bar>(), 6);
            recipe.AddTile(TileID.Anvils); //No Omni-Printer
            recipe.Register();
        }
    }
}
