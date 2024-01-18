using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Projectiles;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	public class Viper_Rifle : ModItem {
		public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				global.viperEffect = true;
				proj.extraUpdates += 2;
				if (contextArgs.Contains("barrel")) {
					proj.extraUpdates = 19;
					proj.timeLeft = 20;
				}
			});
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Gatligator);
			Item.damage = 60;
			Item.crit = 9;
			Item.knockBack = 7.75f;
			Item.useAnimation = Item.useTime = 23;
			Item.width = 100;
			Item.height = 28;
			Item.autoReuse = false;
			Item.value = Item.sellPrice(silver: 50);
			Item.rare = ItemRarityID.LightRed;
			Item.UseSound = Origins.Sounds.HeavyCannon;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 26);
			recipe.AddTile(TileID.MythrilAnvil);
			recipe.Register();
		}
		public override Vector2? HoldoutOffset() => new Vector2(-6, 0);
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 unit = Vector2.Normalize(velocity);
			position += unit * 16;
			float dist = 80 - velocity.Length();
			position += unit * dist;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Vector2 unit = Vector2.Normalize(velocity);
			float dist = 80 - velocity.Length();
			position -= unit * dist;
			EntitySource_ItemUse_WithAmmo barrelSource = new EntitySource_ItemUse_WithAmmo(source.Player, source.Item, source.AmmoItemIdUsed, OriginExtensions.MakeContext(source.Context, OriginGlobalProj.no_multishot_context, "barrel"));
			OriginGlobalProj.killLinkNext = Projectile.NewProjectile(barrelSource, position, unit * (dist / 20), type, damage, knockback, player.whoAmI);
			return true;
		}
	}
}
