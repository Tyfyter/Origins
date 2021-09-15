using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Riven {
	public class Seam_Beam : ModItem {

        public override void SetDefaults(){
            item.damage = 42;
            item.magic = true;
            item.mana = 8;
            item.shoot = ModContent.ProjectileType<Seam_Beam_Beam>();
            item.shootSpeed = 0f;
            item.useTime = item.useAnimation = 20;
            item.useStyle = 5;
            item.noUseGraphic = false;
            item.noMelee = true;
            item.shootSpeed = 1;
            item.width = 12;
            item.height = 10;
            item.value = 10000;
            item.rare = ItemRarityID.Pink;
            item.UseSound = new LegacySoundStyle(SoundID.Item, Origins.Sounds.EnergyRipple);
        }
        public override void AddRecipes() {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddTile(TileID.DemonAltar);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Seam Beam");
			Tooltip.SetDefault("");
		}
        public override bool AltFunctionUse(Player player) {
            return true;
        }
        public override bool CanUseItem(Player player) {
            return true;
        }
    }
    public class Seam_Beam_Beam : ModProjectile {
        public override string Texture => "Origins/Projectiles/Weapons/Seam_Beam_Mid";
        public static Texture2D startTexture { get; private set; }
        public static Texture2D endTexture { get; private set; }
        Vector2 velocity;
        internal void Unload() {
            startTexture = null;
            endTexture = null;
        }
        private Vector2 _targetPos;         //Ending position of the laser beam
        public override void SetDefaults() {
            //projectile.name = "Wind";  //this is the projectile name
            projectile.width = 10;
            projectile.height = 10;
            projectile.friendly = true;     //this defines if the projectile is frendly
            projectile.penetrate = -1;  //this defines the projectile penetration, -1 = infinity
            projectile.tileCollide = true;   //this defines if the tile can colide with walls
            projectile.magic = true;
            projectile.timeLeft = 32;
            //projectile.hide = true;
        }
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Seam Beam");
            startTexture = mod.GetTexture("Projectiles/Weapons/Seam_Beam_Start");
            endTexture = mod.GetTexture("Projectiles/Weapons/Seam_Beam_End");
		}

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough) {
            width = 1;
            height = 1;
			return false;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
            Vector2 unit = velocity;//_targetPos - projectile.Center;
            unit.Normalize();
            DrawLaser(spriteBatch, projectile.Center, unit, 1, new Vector2(1f,0.55f), maxDist:(_targetPos-projectile.Center).Length());
            return false;

        }
        /// <summary>
        /// line check size
        /// </summary>
        const int lcs = 1;
        /// <summary>
        /// The core function of drawing a laser
        /// </summary>
        public void DrawLaser(SpriteBatch spriteBatch, Vector2 start, Vector2 unit, float step, Vector2? uScale = null, float maxDist = 200f, Color color = default){
            Vector2 scale = uScale??new Vector2(0.66f, 0.66f);
            Vector2 origin = start;
            float maxl = (_targetPos-start).Length();
            float r = unit.ToRotation();// + rotation??(float)(Math.PI/2);
            float l = unit.Length();//*2.5f;
            int t = projectile.timeLeft>10?25-projectile.timeLeft:projectile.timeLeft;
            float s = Math.Min(t/15f,1f);
            Vector2 perpUnit = unit.RotatedBy(MathHelper.PiOver2);
            //Dust dust;
            DrawData data;
            int dustTimer = 48;
            Texture2D midTexture = Main.projectileTexture[projectile.type];
            Texture2D texture = startTexture;
            System.Collections.Generic.Queue<DrawData> drawDatas = new System.Collections.Generic.Queue<DrawData>() { };
            for (float i = 0; i <= maxDist; i += step){
                if((i*l)>maxl)break;
                origin = start + i * unit;
                //*
                if(maxl-(i*l)<16&&!(Collision.CanHitLine(origin-unit, lcs, lcs, origin, lcs, lcs)
                    ||Collision.CanHitLine(origin-unit+perpUnit, lcs, lcs, origin-unit-perpUnit, lcs, lcs)
                    ))break;
                    //||Collision.CanHitLine(origin-unit-perpUnit, lcs, lcs, origin-unit, lcs, lcs)
                if(i==16) {
                    texture = midTexture;
                } else if(maxl-(i*l)==16) {
                    texture = endTexture;
                }
                //*/
                /*spriteBatch.Draw(texture, origin - Main.screenPosition,
                    new Rectangle((int)i%44, 0, 1, 32), Color.OrangeRed, r,
                    new Vector2(0, 16+(float)Math.Sin(i/4f)*2), scale*new Vector2(1,s), 0, 0);*/
                data = new DrawData(texture, origin - Main.screenPosition,
                    new Rectangle((int)i%16, ((projectile.frame+(int)(i/16))%4)*24, 1, 24), Color.White, r,
                    new Vector2(0, 11),
                    scale, 0, 0);
                drawDatas.Enqueue(data);
                //data.Draw(spriteBatch);
                //dust = Dust.NewDustPerfect(origin, 6, null, Scale:2);
                //dust.shader = EpikV2.fireDyeShader;
                //dust.noGravity = true;
                Lighting.AddLight(origin, 1*s, 0.45f*s, 0.1f*s);
                if(Main.rand.Next(++dustTimer)>48) {
                    Dust.NewDustPerfect(origin+(perpUnit*Main.rand.NextFloat(-2, 2)), 6, unit*5).noGravity = true;
                    dustTimer = Main.rand.NextBool()?16:0;
                }
            }
            DrawData drawData;
            int di = 0;
            while(drawDatas.Count>0){
                drawData = drawDatas.Dequeue();
                if(drawDatas.Count<16) {
                    drawData.texture = endTexture;
                    drawData.sourceRect = new Rectangle(++di, projectile.frame * 24, 1, 24);
                }
                drawData.Draw(spriteBatch);
            }
            Dust.NewDustPerfect(origin+(perpUnit*Main.rand.NextFloat(-4,4)), 6, unit*5).noGravity = true;
        }

        /// <summary>
        /// Change the way of collision check of the projectile
        /// </summary>
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
            Player p = Main.player[projectile.owner];
            Vector2 unit = (Main.player[projectile.owner].MountedCenter - _targetPos);
            unit.Normalize();
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), p.Center - 45f * unit, _targetPos, 24, ref point);
        }

        const int sample_points = 3;
        const float collision_size = 0.5f;
        /// <summary>
        /// The AI of the projectile
        /// </summary>
        public override void AI() {

            Vector2 start = projectile.Center;
            Vector2 unit = velocity;
            unit.Normalize();
            Vector2 samplingPoint = projectile.Center;

            // Overriding that, if the player shoves the Prism into or through a wall, the interpolation starts at the player's center.
            // This last part prevents the player from projecting beams through walls under any circumstances.
            Player player = Main.player[projectile.owner];
			if (projectile.timeLeft == 32) {
                velocity = Vector2.Normalize(Main.MouseWorld - player.MountedCenter);
                projectile.position += velocity * 16;
                if(!Collision.CanHitLine(player.Center, 0, 0, projectile.Center, 0, 0)) {
                    samplingPoint = projectile.Center = player.Center;
                }
			}
            float[] laserScanResults = new float[sample_points];
			Collision.LaserScan(samplingPoint, unit, collision_size * projectile.scale, 1200f, laserScanResults);
			float averageLengthSample = 0f;
			for (int i = 0; i < laserScanResults.Length; ++i) {
				averageLengthSample += laserScanResults[i];
			}
			averageLengthSample /= sample_points;
            _targetPos = projectile.Center + (unit*averageLengthSample);

            if(++projectile.frameCounter>5) {
                projectile.frame = (projectile.frame + 1) % 4;
                projectile.frameCounter = 0;
            }
        }

        public override bool ShouldUpdatePosition(){
            return false;
        }
    }
}