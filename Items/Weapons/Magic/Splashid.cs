using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Projectiles.Weapons;
using Origins.Tiles.Brine;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Magic {
	public class Splashid : ModItem, IElementalItem {
		static short glowmask;
        public string[] Categories => new string[] {
            "MagicStaff"
        };
        public ushort Element => Elements.Acid;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.RubyStaff);
			Item.damage = 52;
			Item.DamageType = DamageClass.Magic;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.width = 28;
			Item.height = 30;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.reuseDelay = 8;
			Item.mana = 18;
			Item.shoot = ModContent.ProjectileType<Acid_Shot>();
			Item.value = Item.sellPrice(silver: 20, copper: 80);
			Item.rare = ItemRarityID.LightRed;
			Item.glowMask = glowmask;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Brineglow_Item>(), 8);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 15);
			recipe.AddIngredient(ModContent.ItemType<Sulphur_Stone_Item>(), 10);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int a = Main.rand.Next(5, 7);
			//++i < a: increments i and returns true if i < a/2
			//a = Main.rand.Next(5,7): randomize a every loop so ((i-a/2f)/a) returns a random value but maintains a mostly constant spread
			for (int i = 0; ++i < a; a = Main.rand.Next(5, 7)) {
				//((i-a/2f)/a): returns a value based on i between -0.5 and 0.5
				Projectile.NewProjectileDirect(source, position, velocity.RotatedBy(((i - a / 2f) / a) * 0.75), type, damage, knockback, player.whoAmI, 0, 12f).timeLeft += i;
			}
			return false;
		}
	}
}
