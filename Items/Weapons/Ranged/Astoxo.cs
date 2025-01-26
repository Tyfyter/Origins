using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Projectiles;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
	public class Astoxo : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Bow"
        ];
        public override void SetStaticDefaults() {
			OriginGlobalProj.itemSourceEffects.Add(Type, (global, proj, contextArgs) => {
				if (contextArgs.Contains("main")) {
					global.extraBossDamage += 0.5f;
					//proj.extraUpdates -= 1;
				} else {
					global.extraBossDamage += 0.25f;
					global.SetUpdateCountBoost(proj, global.UpdateCountBoost + 1);
				}
			});
			Origins.DamageBonusScale[Type] = 1.5f;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Tsunami);
			Item.damage = 78;
			Item.width = 18;
			Item.height = 58;
			Item.useTime = Item.useAnimation = 29;
			Item.shootSpeed *= 1.5f;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(gold: 2);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.DaedalusStormbow)
			.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 14)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-8f, 0);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Vector2 offset = velocity.SafeNormalize(Vector2.Zero) * 18;

			Projectile.NewProjectile(source, position + offset.RotatedBy(2.75), velocity.RotatedBy(0.01), type, damage, knockback, player.whoAmI);

			Projectile.NewProjectile(source, position + offset.RotatedBy(-2.75), velocity.RotatedBy(-0.01), type, damage, knockback, player.whoAmI);

			velocity *= 1.3f;
			if (type == ProjectileID.WoodenArrowFriendly) type = ProjectileID.MoonlordArrowTrail;
			EntitySource_ItemUse_WithAmmo middleSource = new(source.Player, source.Item, source.AmmoItemIdUsed, OriginExtensions.MakeContext(source.Context, "main"));
			Projectile.NewProjectile(middleSource, position, velocity, type, damage, knockback, player.whoAmI);
			return false;
		}
	}
}
