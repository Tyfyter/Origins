using Microsoft.Xna.Framework;
using Origins.Buffs;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Projectiles;
using Terraria.DataStructures;

namespace Origins.Items.Weapons.Demolitionist {
	public class Ace_Shrapnel : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "Launcher"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Flamethrower);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 18;
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useTime = 1;
			Item.useAnimation = 20;
			Item.shoot = ModContent.ProjectileType<Impeding_Shrapnel_Shard>();
			Item.shootSpeed = 2;
			Item.useAmmo = ModContent.ItemType<Scrap>();
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
			Item.consumeAmmoOnFirstShotOnly = true;
            Item.ArmorPenetration += 2;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<NE8>(), 10);
            recipe.AddIngredient(ModContent.ItemType<Scrap>(), 15);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
            velocity = velocity.RotatedByRandom(0.27f);
			type = Item.shoot;
		}
        // Pulled from Viper code. Attempted to align projectile spawn location to the front of the barrel
        /*public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            Vector2 unit = Vector2.Normalize(velocity);
            float dist = 56 - velocity.Length();
            position -= unit * dist;
            EntitySource_ItemUse_WithAmmo barrelSource = new EntitySource_ItemUse_WithAmmo(source.Player, source.Item, source.AmmoItemIdUsed, OriginExtensions.MakeContext(source.Context, OriginGlobalProj.no_multishot_context, "barrel"));
            OriginGlobalProj.killLinkNext = Projectile.NewProjectile(barrelSource, position, unit * (dist / 20), type, damage, knockback, player.whoAmI);
            return true;
        } */
    }
}
