using Microsoft.Xna.Framework;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Riven {
    public class Threemoepeater : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Threemoepeater");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.ChlorophyteShotbow);
            Item.damage = 14;
            Item.DamageType = DamageClass.Ranged;
            Item.knockBack = 5;
            Item.noMelee = true;
            Item.useTime = 13;
            Item.width = 50;
            Item.height = 10;
            Item.UseSound = SoundID.Item11;
            Item.rare = ItemRarityID.Blue;
        }
	}
}
