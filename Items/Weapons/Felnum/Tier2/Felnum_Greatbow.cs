using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Felnum.Tier2 {
    public class Felnum_Greatbow : ModItem {
        public static int baseDamage = 78;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Astoxo");
            Tooltip.SetDefault("Receives 50% higher damage bonuses");
        }
        public override void SetDefaults() {
            item.CloneDefaults(ItemID.Tsunami);
            item.damage = baseDamage;
            item.width = 18;
            item.height = 58;
            item.useTime = item.useAnimation = 29;
            item.shootSpeed*=1.5f;
            item.autoReuse = true;
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 14);
            recipe.AddIngredient(ItemID.DaedalusStormbow, 1);
            recipe.SetResult(this);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.AddRecipe();
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(-8f,0);
        }
        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
            Vector2 velocity = new Vector2(speedX, speedY);
            Vector2 offset = velocity.SafeNormalize(Vector2.Zero)*18;

            OriginGlobalProj.godHunterEffectNext = 0.25f;
            OriginGlobalProj.extraUpdatesNext = 1;
            Projectile.NewProjectile(position+offset.RotatedBy(2.75), velocity.RotatedBy(0.01), type, damage, knockBack, player.whoAmI);

            OriginGlobalProj.godHunterEffectNext = 0.25f;
            OriginGlobalProj.extraUpdatesNext = 1;
            Projectile.NewProjectile(position+offset.RotatedBy(-2.75), velocity.RotatedBy(-0.01), type, damage, knockBack, player.whoAmI);

            speedX *= 1.3f;
            speedY *= 1.3f;
            if(type == ProjectileID.WoodenArrowFriendly) type = ProjectileID.MoonlordArrowTrail;
            OriginGlobalProj.godHunterEffectNext = 0.5f;
            OriginGlobalProj.extraUpdatesNext = -1;
            return true;
        }
        public override void GetWeaponDamage(Player player, ref int damage) {
            damage+=(damage-baseDamage)/2;//if(!OriginPlayer.ItemChecking)
        }
    }
}
