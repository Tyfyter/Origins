using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.OriginExtensions;
using static Microsoft.Xna.Framework.MathHelper;
using SysDraw = System.Drawing;

namespace Origins.Items.Weapons.Explosives {
	public class Crystal_Grenade : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Grenade");
			Tooltip.SetDefault("");
		}
		public override void SetDefaults() {
            item.CloneDefaults(ItemID.Grenade);
            //item.maxStack = 999;
            item.damage = 30;
			item.value*=14;
            item.shoot = ModContent.ProjectileType<Crystal_Grenade_P>();
			item.shootSpeed*=1.5f;
            item.knockBack = 5f;
            item.ammo = ItemID.Grenade;
			item.rare = ItemRarityID.Lime;
		}
        public override void AddRecipes() {
            Origins.AddExplosive(item);
        }
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool CanUseItem(Player player) {
            if(player.altFunctionUse==2) {
                if(player.controlUp) {
                    Crystal_Grenade_P.blendState.ColorSourceBlend++;
                    Crystal_Grenade_P.blendState.ColorSourceBlend = (Blend)((int)Crystal_Grenade_P.blendState.ColorSourceBlend%13);
                    Main.NewText(Crystal_Grenade_P.blendState.ColorSourceBlend);
                }
                if(player.controlLeft) {
                    Crystal_Grenade_P.blendState.AlphaSourceBlend++;
                    Crystal_Grenade_P.blendState.AlphaSourceBlend = (Blend)((int)Crystal_Grenade_P.blendState.AlphaSourceBlend%13);
                    Main.NewText(Crystal_Grenade_P.blendState.AlphaSourceBlend);
                }
                if(player.controlDown) {
                    Crystal_Grenade_P.blendState.ColorDestinationBlend++;
                    Crystal_Grenade_P.blendState.ColorDestinationBlend = (Blend)((int)Crystal_Grenade_P.blendState.ColorDestinationBlend%13);
                    Main.NewText(Crystal_Grenade_P.blendState.ColorDestinationBlend);
                }
                if(player.controlRight) {
                    Crystal_Grenade_P.blendState.AlphaDestinationBlend++;
                    Crystal_Grenade_P.blendState.AlphaDestinationBlend = (Blend)((int)Crystal_Grenade_P.blendState.AlphaDestinationBlend%13);
                    Main.NewText(Crystal_Grenade_P.blendState.AlphaDestinationBlend);
                }
                item.shoot = ProjectileID.None;
                item.consumable = false;
                return true;
            }
            item.shoot = ModContent.ProjectileType<Crystal_Grenade_P>();
            item.consumable = true;
            return base.CanUseItem(player);
        }
    }
    public class Crystal_Grenade_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Explosives/Crystal_Grenade_Blue";
        public static BlendState blendState = new BlendState() {
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
        };
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Grenade");
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.Grenade);
            projectile.timeLeft = 135;
            projectile.penetrate = -1;
        }
        public override bool PreKill(int timeLeft) {
            projectile.type = ProjectileID.Grenade;
            return true;
        }
        public override void Kill(int timeLeft) {
			projectile.position.X += projectile.width / 2;
			projectile.position.Y += projectile.height / 2;
			projectile.width = 128;
			projectile.height = 128;
			projectile.position.X -= projectile.width / 2;
			projectile.position.Y -= projectile.height / 2;
			projectile.Damage();
            //Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 122, 2f, 1f);
            int t = ModContent.ProjectileType<Crystal_Grenade_Shard>();
            for(int i = Main.rand.Next(3); i < 7; i++) {
                Projectile proj = Projectile.NewProjectileDirect(projectile.Center, (Main.rand.NextVector2Unit()*6)+(projectile.velocity/12), t, projectile.damage/4, 6, projectile.owner);
            }
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
            //SysDraw.Bitmap lightMap = new SysDraw.Bitmap(10, 10);
            //mod.Logger.Info("setting up variables");
            Texture2D lightMap = new Texture2D(spriteBatch.GraphicsDevice, 10, 10);
            Color[] lightData = new Color[100];
            Vector2 pos = projectile.position;
            Vector3 col;
            //mod.Logger.Info("set up variables");
            for(int x = 0; x < 10; x++) {
                pos.X+=2;
                for(int y = 0; y < 10; y++) {
                    pos.Y+=2;
                    col = Lighting.GetSubLight(pos);
                    lightData[(y*10)+x] = new Color(((col.X+col.Y+col.Z)/1.5f-0.66f)*Min(projectile.timeLeft/85f, 1),0,0);
                    //lightMap.SetPixel(x,y,SysDraw.Color.FromArgb(255,(int)((col.X+col.Y+col.Z)/3),0,0));
                }
                pos.Y-=20;
            }
            //mod.Logger.Info("setting data");
            lightMap.SetData(lightData);
            //mod.Logger.Info("set data");
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, blendState, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            DrawData data2 = new DrawData(mod.GetTexture("Items/Weapons/Explosives/Crystal_Grenade_Purple"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 14, 20), new Color(255,255,255,255), projectile.rotation, new Vector2(7, 7), Vector2.One, SpriteEffects.None, 0);
            //Origins.perlinFade0.Shader.Parameters["uOffset"].SetValue(projectile.position);
            //Origins.perlinFade0.Shader.Parameters["uRotation"].SetValue(-projectile.rotation);
            Main.graphics.GraphicsDevice.Textures[1] = lightMap;
            Origins.perlinFade0.Shader.Parameters["uThreshold0"].SetValue(0f);
            Origins.perlinFade0.Shader.Parameters["uThreshold1"].SetValue(0.25f);
            Origins.perlinFade0.Apply(data2);
            DrawData data = new DrawData(mod.GetTexture("Items/Weapons/Explosives/Crystal_Grenade_Pink"), projectile.Center - Main.screenPosition, new Rectangle(0, 0, 14, 20), new Color(255,255,255,255), projectile.rotation, new Vector2(7, 7), Vector2.One, SpriteEffects.None, 0);
            //Origins.perlinFade0.Shader.Parameters["uOffset"].SetValue(projectile.position);
            //Origins.perlinFade0.Shader.Parameters["uRotation"].SetValue(projectile.rotation);
            //Main.graphics.GraphicsDevice.Textures[1] = lightMap;
            Origins.perlinFade0.Shader.Parameters["uThreshold0"].SetValue(0.5f);
            Origins.perlinFade0.Shader.Parameters["uThreshold1"].SetValue(0.75f);
            Origins.perlinFade0.Apply(data);
			data.Draw(spriteBatch);
            spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
            blendState = new BlendState() {
                ColorSourceBlend = blendState.ColorSourceBlend,
                AlphaSourceBlend = blendState.AlphaSourceBlend,
                ColorDestinationBlend = blendState.ColorDestinationBlend,
                AlphaDestinationBlend = blendState.AlphaDestinationBlend,
            };
        }
    }
    public class Crystal_Grenade_Shard : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public static int ID { get; private set; } = 0;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Shard");
            ID = projectile.type;
            ProjectileID.Sets.TrailingMode[ID] = 0;
            if(Main.netMode!=NetmodeID.Server) {
                Main.projectileTexture[94] = Main.instance.OurLoad<Texture2D>(string.Concat(new object[]{"Images",Path.DirectorySeparatorChar,"Projectile_94"}));
		        Main.projectileLoaded[94] = true;
            }
		}
        public override void SetDefaults() {
            projectile.CloneDefaults(ProjectileID.CrystalStorm);
            projectile.aiStyle = 0;
            projectile.penetrate = -1;
            projectile.timeLeft = 30;
            projectile.extraUpdates+=1;
            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = 15;
        }
        public override void AI() {
            projectile.rotation = Main.rand.NextFloatDirection();
        }
        public override Color? GetAlpha(Color lightColor) {
            //float a = Math.Min(projectile.timeLeft/10f, 1);
			return new Color(200, 200, 200, 25);
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(projectile.timeLeft<25) {
                projectile.Kill();
                return false;
            }
			if (projectile.velocity.X != oldVelocity.X){
				projectile.velocity.X = 0f - oldVelocity.X;
			}
			if (projectile.velocity.Y != oldVelocity.Y){
				projectile.velocity.Y = 0f - oldVelocity.Y;
			}
            return false;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            projectile.type = ProjectileID.CrystalStorm;
            projectile.ai[1] = 7;
            return true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) {
            projectile.type = ID;
        }
    }
}
