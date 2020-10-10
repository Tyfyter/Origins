using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using static Microsoft.Xna.Framework.MathHelper;

namespace Origins.Items.Weapons.Explosives {
	public class Black_Hole_Bomb : ModItem {
        public override string Texture => "Origins/Items/Weapons/Explosives/Impact_Bomb";
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Black Hole Bomb");
			Tooltip.SetDefault("Be very careful.");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.Bomb);
            item.damage = 250;
			item.value*=2;
			item.useTime = (int)(item.useTime*0.75);
			item.useAnimation = (int)(item.useAnimation*0.75);
            item.shoot = ModContent.ProjectileType<Black_Hole_Bomb_P>();
			item.shootSpeed*=2;
            item.knockBack = 13f;
			item.rare = ItemRarityID.Green;
            item.color = Color.Black;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
    }
    public class Black_Hole_Bomb_P : ModProjectile {
        const int initDur = 5;
        const int maxDur = 1800;
        const int growDur = 180;
        const int collapseDur = 10;
        const int totalDur = growDur+collapseDur;
        public override string Texture => "Origins/Items/Weapons/Explosives/Impact_Bomb";
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Bomb);
            projectile.aiStyle = 14;
            projectile.penetrate = -1;
            projectile.timeLeft = maxDur;
            projectile.scale = 0;
        }
        public override void AI() {
            if(Main.netMode != NetmodeID.Server) {
                if(projectile.timeLeft > maxDur-initDur) {
                    projectile.scale+=1f/initDur;
                }
                if(projectile.timeLeft <= 190) {
                    projectile.alpha = 255;
                    projectile.scale+=1f/totalDur;
                }
            }
            float percent = Clamp((totalDur-projectile.timeLeft) / (float)growDur,0,1)*2;
            float scale = 0;
            if(projectile.timeLeft<collapseDur) {
                scale = (collapseDur-projectile.timeLeft)/(collapseDur/2f);
            }
            //float range = 80*(projectile.scale+scale);
            float strength = 32*(1+percent)*(projectile.scale+scale);
            NPC target;
            for(int i = 0; i < Main.npc.Length; i++) {
                target = Main.npc[i];
                if(target.CanBeChasedBy()) {
                    float dist = (target.Center-projectile.Center).Length()/16f;
                    float distSQ = (float)Math.Pow(dist,1.5f);
                    float force = strength*(target.knockBackResist*0.95f+0.05f)/distSQ;
                    if(force>1)target.velocity-=((target.Center-projectile.Center).SafeNormalize(Vector2.Zero)*Min(force, dist));
                    if(force>=(projectile.Center.Clamp(target.Hitbox)-projectile.Center).Length()&&target.immune[projectile.owner]<=0) {
                        target.StrikeNPC(projectile.damage/5, 0, 0);
                        target.immune[projectile.owner] = 10;
                    }
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(projectile.timeLeft<=totalDur)return false;
            projectile.aiStyle = 0;
            projectile.velocity = Vector2.Zero;
            projectile.timeLeft = totalDur;
            projectile.tileCollide = false;
            projectile.friendly = false;
            return false;
        }
        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
            if(projectile.timeLeft<=totalDur)return;
            projectile.aiStyle = 0;
            projectile.velocity = Vector2.Zero;
            projectile.timeLeft = totalDur;
            projectile.tileCollide = false;
            projectile.friendly = false;
        }
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.RocketI;
            return true;
        }
        public override void Kill(int timeLeft) {
            projectile.friendly = true;
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 128;
			projectile.height = 128;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor){
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
			float percent = Clamp((totalDur-projectile.timeLeft) / (float)growDur,0,1);
            float scale = 0;
            if(projectile.timeLeft<collapseDur) {
                scale = (collapseDur-projectile.timeLeft)/(collapseDur/2f);
            }
			DrawData data = new DrawData(Origins.instance.GetTexture("Projectiles/Pixel"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), new Color(0,0,0,255), 0, new Vector2(0.5f, 0.5f), new Vector2(160,160)*(projectile.scale-scale), SpriteEffects.None, 0);
            Origins.blackHoleShade.UseOpacity(0.985f);
            Origins.blackHoleShade.UseSaturation(3f+percent);
            Origins.blackHoleShade.UseColor(0,0,0);
            Origins.blackHoleShade.Shader.Parameters["uScale"].SetValue(0.5f);
            Origins.blackHoleShade.Apply(data);
			data.Draw(spriteBatch);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
        }
    }
}
