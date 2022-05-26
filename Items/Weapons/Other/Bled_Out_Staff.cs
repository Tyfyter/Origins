using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Other {
	public class Bled_Out_Staff : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bled_Out_Staff");
			Tooltip.SetDefault("");
			Item.staff[item.type] = true;
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.RubyStaff);
			item.damage = 47;
			item.magic = true;
			item.noMelee = true;
			item.width = 42;
			item.height = 42;
			item.useTime = 20;
			item.useAnimation = 20;
			item.mana = 16;
			item.value = 5000;
            item.shoot = ModContent.ProjectileType<Bled_Out_Staff_P>();
			item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddRecipeGroup("Origins:Gem Staves", 1);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 45);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
    public class Bled_Out_Staff_P : ModProjectile {
        public override string Texture => "Terraria/Projectile_125";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.DiamondBolt);//sets the projectile stat values to those of Ruby Bolts
            projectile.penetrate = 1;//when projectile.penetrate reaches 0 the projectile is destroyed
            projectile.extraUpdates = 1;
        }
        public override void AI() {
	        Dust dust = Dust.NewDustDirect(projectile.Center, 0, 0, DustID.DiamondBolt, 0f, 0f, 0, new Color(255,255,0), 1f);
	        dust.noGravity = true;
	        dust.velocity/=2;
        }
    }
}
