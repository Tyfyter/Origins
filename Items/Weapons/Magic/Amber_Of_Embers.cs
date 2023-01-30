using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Tiles.Dusk;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Tyfyter.Utils;

namespace Origins.Items.Weapons.Magic {
    public class Amber_Of_Embers : ModItem {
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Amber of Embers");
            Item.staff[Item.type] = true;
            SacrificeTotal = 1;
        }
        public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Flamelash);
            Item.damage = 31;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 11;
            Item.useTime = 11;
            Item.width = 48;
            Item.height = 54;
            Item.mana = 7;
            Item.shoot = ModContent.ProjectileType<Amber_Of_Embers_P>();
            Item.shootSpeed = 8f;
            Item.autoReuse = true;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Pink;
        }
        public override void AddRecipes() {
            Recipe recipe = Recipe.Create(Type);
            recipe.AddIngredient(ItemID.Amber, 8);
            recipe.AddIngredient(ModContent.ItemType<Bleeding_Obsidian_Item>(), 4);
            recipe.AddIngredient(ModContent.ItemType<Felnum_Bar>(), 10);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
    public class Amber_Of_Embers_P : ModProjectile {
        public override string Texture => "Origins/Projectiles/Weapons/Fire_Wave_P";

        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Burning Ember");
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 19;
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.DD2SquireSonicBoom);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.extraUpdates = 1;
            Projectile.aiStyle = 0;
            Projectile.timeLeft = 200;
        }
        public override void AI() {
            if(Projectile.timeLeft >= 80)Projectile.rotation = Projectile.velocity.ToRotation();
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(Main.rand.NextBool())target.AddBuff(BuffID.OnFire, 300);
            if(Projectile.penetrate == 1) {
                Projectile.penetrate = 2;
                Projectile.timeLeft = 80;
                Projectile.velocity = Vector2.Zero;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            if(Projectile.timeLeft < 80) {
                return false;
            }
            Rectangle hitbox;
            Vector2 offset = Vector2.Zero;
            Vector2 velocity = Projectile.velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.UnitY)*8;
            for(int n = 0; n < 4; n++) {
                offset += velocity;
                hitbox = projHitbox;
                hitbox.Offset(offset.ToPoint());
                if(hitbox.Intersects(targetHitbox))return true;
                hitbox = projHitbox;
                hitbox.Offset((-offset).ToPoint());
                if(hitbox.Intersects(targetHitbox))return true;
            }
            return null;
        }
        public override bool PreDraw(ref Color lightColor) {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2[] positions = new Vector2[Projectile.oldPos.Length+1];
            Projectile.oldPos.CopyTo(positions, 1);
            positions[0] = Projectile.position;
            Vector2 halfSize = new Vector2(Projectile.width, Projectile.height) * 0.5f;
            int alpha = 0;
            int min = Math.Max(80-Projectile.timeLeft, 0)/2;
            for(int i = positions.Length - 1; i >= min; i--) {
                if((i&3)!=0) {
                    continue;
                }
                alpha = (20 - i) * 10;
                Main.EntitySpriteDraw(
                    texture,
                    (positions[i] + halfSize) - Main.screenPosition,
                    null,
                    new Color(alpha, alpha, alpha, alpha),
                    Projectile.rotation,
                    texture.Size()*0.5f,
                    Projectile.scale,
                    Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                    0);
            }
            Vector2 offset = Vector2.Zero;
            Vector2 velocity = (Vector2)new PolarVec2(8, Projectile.rotation+MathHelper.PiOver2);
            const int backset = 2;
            if(min+backset<positions.Length)for(int n = 0; n < 4; n++) {
                offset += velocity;
                if(Main.rand.NextBool(3))Dust.NewDust(positions[min+backset] + offset, Projectile.width, Projectile.height, DustID.Torch);
                if(Main.rand.NextBool(3))Dust.NewDust(positions[min+backset] - offset, Projectile.width, Projectile.height, DustID.Torch);
            }
            return false;
        }
    }
}
