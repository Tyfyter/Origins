using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace Origins.Items.Weapons.Other {
	public class Bled_Out_Staff : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Bled Out Staff");
			Tooltip.SetDefault("");
			Item.staff[Item.type] = true;
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 47;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.width = 42;
			Item.height = 42;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.mana = 16;
			Item.value = 5000;
            Item.shoot = ModContent.ProjectileType<Bled_Out_Staff_P>();
			Item.rare = ItemRarityID.LightRed;
		}
		public override void AddRecipes() {
			Recipe recipe = Mod.CreateRecipe(Type);
			recipe.AddRecipeGroup("Origins:Gem Staves", 1);
			recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Shard>(), 45);
			recipe.Register();
		}
	}
    public class Bled_Out_Staff_P : ModProjectile {
        public override string Texture => "Terraria/Images/Projectile_125";
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.DiamondBolt);//sets the projectile stat values to those of Ruby Bolts
            Projectile.penetrate = 1;//when projectile.penetrate reaches 0 the projectile is destroyed
            Projectile.extraUpdates = 1;
        }
        public override void AI() {
	        Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GemDiamond, 0f, 0f, 0, new Color(255,255,0), 1f);
	        dust.noGravity = true;
	        dust.velocity/=2;
        }
    }
}
