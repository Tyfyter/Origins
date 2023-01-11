using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ammo {
    public class Flammable_Harpoon : ModItem {
        public override string Texture => "Origins/Items/Weapons/Ammo/Flammable_Harpoon";
        public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Flammable Harpoon");
            SacrificeTotal = 99;
            ID = Type;
        }
        public override void SetDefaults() {
            Item.damage = 10;
            Item.DamageType = DamageClass.Ranged;
            Item.consumable = true;
            Item.maxStack = 99;
            Item.shoot = Flammable_Harpoon_P.ID;
            Item.ammo = Harpoon.ID;
            Item.value = Item.sellPrice(silver: 26);
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.IronBar);
            recipe.AddIngredient(ItemID.Gel, 2);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.LeadBar);
            recipe.AddIngredient(ItemID.Gel, 2);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
            recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Harpoon>());
            recipe.AddIngredient(ItemID.Gel, 2);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    public class Flammable_Harpoon_P : Harpoon_P {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Harpoon;
		public static int ID { get; private set; } = -1;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Flammable Harpoon");
            ID = Type;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            target.AddBuff(BuffID.OnFire, Main.rand.Next(270, 360));
        }
    }
}
