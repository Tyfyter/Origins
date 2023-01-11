using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Felnum.Tier2 {
    public class Felnum_Greatbow : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Astoxo");
            Tooltip.SetDefault("Receives 50% higher damage bonuses");
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Tsunami);
            Item.damage = 78;
            Item.width = 18;
            Item.height = 58;
            Item.useTime = Item.useAnimation = 29;
            Item.shootSpeed*=1.5f;
            Item.autoReuse = true;
            Item.value = Item.sellPrice(gold: 2);
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 14);
            recipe.AddIngredient(ItemID.DaedalusStormbow, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(-8f,0);
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            damage = damage.MultiplyBonuses(1.5f);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
            Vector2 offset = velocity.SafeNormalize(Vector2.Zero)*18;

            OriginGlobalProj.godHunterEffectNext = 0.25f;
            OriginGlobalProj.extraUpdatesNext = 1;
            Projectile.NewProjectile(source, position+offset.RotatedBy(2.75), velocity.RotatedBy(0.01), type, damage, knockback, player.whoAmI);

            OriginGlobalProj.godHunterEffectNext = 0.25f;
            OriginGlobalProj.extraUpdatesNext = 1;
            Projectile.NewProjectile(source, position + offset.RotatedBy(-2.75), velocity.RotatedBy(-0.01), type, damage, knockback, player.whoAmI);

            velocity *= 1.3f;
            if(type == ProjectileID.WoodenArrowFriendly) type = ProjectileID.MoonlordArrowTrail;
            OriginGlobalProj.godHunterEffectNext = 0.5f;
            OriginGlobalProj.extraUpdatesNext = -1;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
    }
}
