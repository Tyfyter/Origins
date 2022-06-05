using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
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
            Origins.ExplosiveItems[Item.type] = true;
		}
		public override void SetDefaults() {
            Item.CloneDefaults(ItemID.Grenade);
            //item.maxStack = 999;
            Item.damage = 45;
			Item.value*=14;
            Item.shoot = ModContent.ProjectileType<Crystal_Grenade_P>();
			Item.shootSpeed*=1.5f;
            Item.knockBack = 5f;
            Item.ammo = ItemID.Grenade;
			Item.rare = ItemRarityID.Lime;
		}
        public override void AddRecipes() {
        }
        #region blend mode testing
        /*public override bool AltFunctionUse(Player player) {
            return true;
        }*/
        /*public override bool CanUseItem(Player player) {
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
        }*/
        #endregion
    }
    public class Crystal_Grenade_P : ModProjectile {
        public override string Texture => "Origins/Items/Weapons/Explosives/Crystal_Grenade_Blue";
        public static BlendState blendState => new BlendState() {
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
        };
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Grenade");
            Origins.ExplosiveProjectiles[Projectile.type] = true;
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.Grenade);
            Projectile.timeLeft = 135;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override bool PreKill(int timeLeft) {
            Projectile.type = ProjectileID.Grenade;
            return true;
        }
        public override void Kill(int timeLeft) {
			Projectile.position.X += Projectile.width / 2;
			Projectile.position.Y += Projectile.height / 2;
			Projectile.width = 128;
			Projectile.height = 128;
			Projectile.position.X -= Projectile.width / 2;
			Projectile.position.Y -= Projectile.height / 2;
			Projectile.Damage();
            //Main.PlaySound(2, (int)projectile.Center.X, (int)projectile.Center.Y, 122, 2f, 1f);
            int t = ModContent.ProjectileType<Crystal_Grenade_Shard>();
            int count = 14 - Main.rand.Next(3);
            float rot = TwoPi/count;
            for(int i = count; i > 0; i--) {
                Projectile.NewProjectile(Projectile.Center, (Vec2FromPolar(rot*i, 6) + Main.rand.NextVector2Unit())+(Projectile.velocity/12), t, Projectile.damage/4, 6, Projectile.owner);
            }
        }
        public override void PostDraw(Color lightColor) {
            //SysDraw.Bitmap lightMap = new SysDraw.Bitmap(10, 10);
            //mod.Logger.Info("setting up variables");
            Texture2D lightMap = new Texture2D(spriteBatch.GraphicsDevice, 10, 10);
            Color[] lightData = new Color[100];
            Vector2 pos = Projectile.position;
            Vector3 col;
            //mod.Logger.Info("set up variables");
            for(int x = 0; x < 10; x++) {
                pos.X+=2;
                for(int y = 0; y < 10; y++) {
                    pos.Y+=2;
                    col = Lighting.GetSubLight(pos);
                    lightData[(y*10)+x] = new Color(((col.X+col.Y+col.Z)/1.5f-0.66f)*Min(Projectile.timeLeft/85f, 1),0,0);
                    //lightMap.SetPixel(x,y,SysDraw.Color.FromArgb(255,(int)((col.X+col.Y+col.Z)/3),0,0));
                }
                pos.Y-=20;
            }
            //mod.Logger.Info("setting data");
            lightMap.SetData(lightData);
            //mod.Logger.Info("set data");
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, blendState, SamplerState.PointWrap, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            DrawData data2 = new DrawData(Mod.GetTexture("Items/Weapons/Explosives/Crystal_Grenade_Purple"), Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 14, 20), new Color(255,255,255,255), Projectile.rotation, new Vector2(7, 7), Vector2.One, SpriteEffects.None, 0);
            //Origins.perlinFade0.Shader.Parameters["uOffset"].SetValue(projectile.position);
            //Origins.perlinFade0.Shader.Parameters["uRotation"].SetValue(-projectile.rotation);
            Main.graphics.GraphicsDevice.Textures[1] = lightMap;
            Origins.perlinFade0.Shader.Parameters["uThreshold0"].SetValue(0f);
            Origins.perlinFade0.Shader.Parameters["uThreshold1"].SetValue(0.25f);
            Origins.perlinFade0.Apply(data2);
            DrawData data = new DrawData(Mod.GetTexture("Items/Weapons/Explosives/Crystal_Grenade_Pink"), Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 14, 20), new Color(255,255,255,255), Projectile.rotation, new Vector2(7, 7), Vector2.One, SpriteEffects.None, 0);
            //Origins.perlinFade0.Shader.Parameters["uOffset"].SetValue(projectile.position);
            //Origins.perlinFade0.Shader.Parameters["uRotation"].SetValue(projectile.rotation);
            //Main.graphics.GraphicsDevice.Textures[1] = lightMap;
            Origins.perlinFade0.Shader.Parameters["uThreshold0"].SetValue(0.5f);
            Origins.perlinFade0.Shader.Parameters["uThreshold1"].SetValue(0.75f);
            Origins.perlinFade0.Apply(data);
			data.Draw(spriteBatch);
            spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.instance.Rasterizer, null, Main.Transform);
        }
    }
    public class Crystal_Grenade_Shard : ModProjectile {
        public override string Texture => "Origins/Projectiles/Pixel";
        public static int ID { get; private set; } = 0;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystal Shard");
            ID = Projectile.type;
			try{
				Origins.ExplosiveProjectiles[Projectile.type] = true;
				ProjectileID.Sets.TrailingMode[ID] = 0;
                Mod.Logger.Info("loading crystal shard");
				if(!Main.dedServ) {
                    Mod.Logger.Info("not dedicated server");
					TextureAssets.Projectile[94].Value = Main.instance.OurLoad<Texture2D>(string.Concat(new object[]{"Images",Path.DirectorySeparatorChar,"Projectile_94"}));
					Main.projectileLoaded[94] = true;
				}
			}catch(Exception){
                Origins.instance.Logger.Warn(Main.netMode+" "+Main.dedServ);
			}
		}
        public override void SetDefaults() {
            Projectile.CloneDefaults(ProjectileID.CrystalStorm);
            Projectile.aiStyle = 0;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30+Main.rand.Next(-5,16);
            Projectile.extraUpdates+=1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void AI() {
            Projectile.rotation = Main.rand.NextFloatDirection();
        }
        public override Color? GetAlpha(Color lightColor) {
            //float a = Math.Min(projectile.timeLeft/10f, 1);
			return new Color(200, 200, 200, 25);
        }
        public override bool OnTileCollide(Vector2 oldVelocity) {
            if(Projectile.timeLeft<25) {
                Projectile.Kill();
                return false;
            }
			if (Projectile.velocity.X != oldVelocity.X){
				Projectile.velocity.X = 0f - oldVelocity.X;
			}
			if (Projectile.velocity.Y != oldVelocity.Y){
				Projectile.velocity.Y = 0f - oldVelocity.Y;
			}
            return false;
        }
        public override bool PreDraw(ref Color lightColor) {
            Projectile.type = ProjectileID.CrystalStorm;
            Projectile.ai[1] = 7;
            return true;
        }
        public override void PostDraw(Color lightColor) {
            Projectile.type = ID;
        }
    }
}
