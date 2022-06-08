using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;

namespace Origins.Items.Weapons.Felnum.Tier2 {
    public class Tolruk : ModItem {
        int charge = 0;
        public static short[] glowmasks;
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Tolruk");
            Tooltip.SetDefault("Receives 50% higher damage bonuses\n[tɵlɹɘk]\nSprite needs reshaping to be less like dart rifle");
            glowmasks = new short[]{
                -1,//Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_0"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_1"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_2"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_3"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_4"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_5"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_6"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_7"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_8"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_9"),
                Origins.AddGlowMask("Weapons/Felnum/Tier2/Tolruk_Glow_10")
            };
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults() {
            Item.damage = 37;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.width = 18;
            Item.height = 36;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 1;
            Item.value = 500000;
            Item.shootSpeed = 14;
            Item.autoReuse = true;
            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ProjectileID.Bullet;
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item11;
        }
        public override void AddRecipes() {
            Recipe recipe = Mod.CreateRecipe(Type);
            recipe.AddIngredient(ModContent.ItemType<Valkyrum_Bar>(), 15);
            recipe.AddIngredient(ItemID.Uzi, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
        public override void HoldStyle(Player player, Rectangle heldItemFrame) {
            if (charge > 0) {
                charge--;
            }
        }
        public override void HoldItem(Player player) {
            Item.glowMask = glowmasks[Math.Min(charge / 4, 10)];
        }
        public override Vector2? HoldoutOffset() {
            return new Vector2(-12, 0);
        }
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
            damage = damage.MultiplyBonuses(1.5f);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
		    Vector2 offset = velocity.SafeNormalize(Vector2.Zero);
            position -= offset.RotatedBy(MathHelper.PiOver2) * player.direction * 3;
            Projectile p = Projectile.NewProjectileDirect(source, position, velocity, type, damage+charge, knockback, player.whoAmI);
            if(p.penetrate>0&&Main.rand.NextBool(5-charge/10)) {
                p.penetrate++;
                p.localNPCHitCooldown = 10;
                p.usesLocalNPCImmunity = true;
            }
            SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithPitch(1f).WithVolume(0.15f), position);
            if(charge >= 40) {
                Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<Tolruk_Bolt>(), damage*3, knockback, player.whoAmI);
                charge = 0;
                SoundEngine.PlaySound(Origins.Sounds.DeepBoom.WithVolume(2), position);
                SoundEngine.PlaySound(SoundID.Item122.WithPitch(1).WithVolume(3), position);
            } else charge+=4;
            return false;
        }
        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
            if(Item.glowMask>-1)spriteBatch.Draw(TextureAssets.GlowMask[Item.glowMask].Value, position, frame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
    public class Tolruk_Bolt : ModProjectile {
        (Vector2?, Vector2)[] oldPos = new (Vector2?,Vector2)[14];
        public override string Texture => "Terraria/Images/Projectile_3";
        public override void SetStaticDefaults() {
            DisplayName.SetDefault("Tolruk Bolt");
        }
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.CultistBossLightningOrbArc);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.width = 15;
            Projectile.height = 15;
            Projectile.aiStyle = 0;
            Projectile.extraUpdates = 5;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.timeLeft*=3;
            Projectile.penetrate = -1;
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.position+=oldVelocity;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 14;
            return false;
        }
        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
            width = 1;
            height = 1;
            return true;
        }
        public override bool PreDraw(ref Color lightColor) {
            int l = Math.Min(Projectile.timeLeft, 14);
            for(int i = l; --i>0;) {
                oldPos[i] = oldPos[i-1];
                oldPos[i].Item1+=oldPos[i].Item2;
            }
            Vector2 dir = Main.rand.NextVector2Unit();
            oldPos[0] = (Projectile.Center+dir*3, dir);
            Texture2D texture = Mod.Assets.Request<Texture2D>("Projectiles/Pixel").Value;
            Vector2 displ = Vector2.Zero;
            for(int i = l; --i>0;) {
                if(oldPos[i].Item1.HasValue) {
                    displ = oldPos[i-1].Item1.Value-oldPos[i].Item1.Value;
                    DrawLaser(Main.spriteBatch, texture, oldPos[i].Item1.Value, Vector2.Normalize(displ), 1, new Vector2(2, 2), displ.Length(), Color.Aqua);
                }
            }
            if(oldPos[0].Item1.HasValue) {
                displ = Projectile.Center-oldPos[0].Item1.Value+Main.rand.NextVector2Unit()*2;
                DrawLaser(Main.spriteBatch, texture, oldPos[0].Item1.Value, Vector2.Normalize(displ), 1, new Vector2(2, 2), displ.Length(), Color.Aqua);
            }
            return false;
        }
        public void DrawLaser(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 unit, float step, Vector2? uScale = null, float maxDist = 200f, Color color = default){
            Vector2 scale = uScale??new Vector2(0.66f, 0.66f);
            Vector2 origin = start;
            float maxl = (float)Math.Sqrt(Main.screenWidth*Main.screenWidth+Main.screenHeight*Main.screenHeight);//(_targetPos-start).Length();
            float r = unit.ToRotation();// + rotation??(float)(Math.PI/2);
            float l = unit.Length()*2.5f;
            int t = Projectile.timeLeft>10?25-Projectile.timeLeft:Projectile.timeLeft;
            float s = Math.Min(t/15f,1f);
            Vector2 perpUnit = unit.RotatedBy(MathHelper.PiOver2);
            for (float i = 0; i <= maxDist; i += step){
                if(i*unit.Length()>maxl)break;
                origin = start + i * unit;
                spriteBatch.Draw(texture, origin - Main.screenPosition,
                    new Rectangle(0, 0, 1, 1), color, r,
                    new Vector2(0.5f, 0.5f), scale, 0, 0);
                spriteBatch.Draw(texture, origin - Main.screenPosition + perpUnit*Main.rand.NextFloat(-1f,1f),
                    new Rectangle(0, 0, 1, 1), color, r,
                    new Vector2(0.5f, 0.5f), scale, 0, 0);
                Lighting.AddLight(origin, color.R/255f, color.G/255f, color.B/255f);
            }
        }
        /*public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            int l = Math.Min(projectile.timeLeft, 14);
            for(int i = l; --i>0;) {
                oldPos[i] = oldPos[i-1];
                oldPos[i].Item1+=oldPos[i].Item2;
            }
            Vector2 dir = Main.rand.NextVector2Unit();
            oldPos[0] = (projectile.Center+dir*3, dir);
            for(int i = l; --i>0;) {
                if(oldPos[i].Item1.HasValue)spriteBatch.DrawLine(new Color(100,255,255), oldPos[i].Item1.Value, oldPos[i-1].Item1.Value, 2);
            }
            if(oldPos[0].Item1.HasValue)spriteBatch.DrawLine(new Color(100,255,255), oldPos[0].Item1.Value, projectile.Center+Main.rand.NextVector2Unit()*2, 2);
            return false;
        }*/
    }
}
