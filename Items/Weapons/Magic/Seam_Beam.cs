using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.NPCs;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class Seam_Beam : ModItem, ICustomWikiStat {
		public string[] Categories => new string[] {
			"Torn",
			"TornSource",
			"MagicGun"
		};
		static short glowmask;
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}

		public override void SetDefaults() {
			Item.damage = 42;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 19;
			Item.shoot = ModContent.ProjectileType<Seam_Beam_Beam>();
			Item.shootSpeed = 0f;
			Item.useTime = Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = false;
			Item.noMelee = true;
			Item.shootSpeed = 1;
			Item.width = 12;
			Item.height = 10;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = Origins.Sounds.EnergyRipple;
			Item.glowMask = glowmask;
		}
		public override bool AltFunctionUse(Player player) {
			return false;
		}
		public override bool CanUseItem(Player player) {
			return true;
		}
	}
	public class Seam_Beam_Beam : ModProjectile {
		public override string Texture => "Origins/Projectiles/Weapons/Seam_Beam_Mid";
		public static AutoCastingAsset<Texture2D> StartTexture { get; private set; }
		public static AutoCastingAsset<Texture2D> EndTexture { get; private set; }
		Vector2 velocity;
		public override void Unload() {
			StartTexture = null;
			EndTexture = null;
		}
		private Vector2 _targetPos;         //Ending position of the laser beam
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				StartTexture = Mod.Assets.Request<Texture2D>("Projectiles/Weapons/Seam_Beam_Start");
				EndTexture = Mod.Assets.Request<Texture2D>("Projectiles/Weapons/Seam_Beam_End");
			}
		}
		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 32;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 300, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}

		public override bool PreDraw(ref Color lightColor) {
			Vector2 unit = velocity;
			unit.Normalize();
			DrawLaser(Main.spriteBatch, Projectile.Center, unit, 1, new Vector2(1f, 0.55f), maxDist: (_targetPos - Projectile.Center).Length());
			return false;
		}
		/// <summary>
		/// line check size
		/// </summary>
		const int lcs = 1;
		/// <summary>
		/// The core function of drawing a laser
		/// </summary>
		public void DrawLaser(SpriteBatch spriteBatch, Vector2 start, Vector2 unit, float step, Vector2? uScale = null, float maxDist = 200f, Color color = default) {
			Vector2 scale = uScale ?? new Vector2(0.66f, 0.66f);
			Vector2 origin = start;
			float maxl = (_targetPos - start).Length();
			float r = unit.ToRotation();// + rotation??(float)(Math.PI/2);
			float l = unit.Length();//*2.5f;
			int t = Projectile.timeLeft > 10 ? 25 - Projectile.timeLeft : Projectile.timeLeft;
			float s = Math.Min(t / 15f, 1f);
			Vector2 perpUnit = unit.RotatedBy(MathHelper.PiOver2);
			//Dust dust;
			DrawData data;
			int dustTimer = 48;
			Texture2D midTexture = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D texture = StartTexture;
			System.Collections.Generic.Queue<DrawData> drawDatas = new System.Collections.Generic.Queue<DrawData>() { };
			for (float i = 0; i <= maxDist; i += step) {
				if ((i * l) > maxl) break;
				origin = start + i * unit;
				//*
				if (maxl - (i * l) < 16 && !(Collision.CanHitLine(origin - unit, lcs, lcs, origin, lcs, lcs)
					|| Collision.CanHitLine(origin - unit + perpUnit, lcs, lcs, origin - unit - perpUnit, lcs, lcs)
					)) break;
				//||Collision.CanHitLine(origin-unit-perpUnit, lcs, lcs, origin-unit, lcs, lcs)
				if (i == 16) {
					texture = midTexture;
				} else if (maxl - (i * l) == 16) {
					texture = EndTexture;
				}
				//*/
				/*spriteBatch.Draw(texture, origin - Main.screenPosition,
                    new Rectangle((int)i%44, 0, 1, 32), Color.OrangeRed, r,
                    new Vector2(0, 16+(float)Math.Sin(i/4f)*2), scale*new Vector2(1,s), 0, 0);*/
				data = new DrawData(texture, origin - Main.screenPosition,
					new Rectangle((int)i % 16, ((Projectile.frame + (int)(i / 16)) % 4) * 24, 1, 24), Color.White, r,
					new Vector2(0, 11),
					scale, 0, 0);
				drawDatas.Enqueue(data);
				//data.Draw(spriteBatch);
				//dust = Dust.NewDustPerfect(origin, 6, null, Scale:2);
				//dust.shader = EpikV2.fireDyeShader;
				//dust.noGravity = true;
				Lighting.AddLight(origin, 0.1f * s, 0.35f * s, 1f * s);
				if (Main.rand.Next(++dustTimer) > 48) {
					Dust.NewDustPerfect(origin + (perpUnit * Main.rand.NextFloat(-2, 2)), DustID.BlueTorch, unit * 5).noGravity = true;
					dustTimer = Main.rand.NextBool() ? 16 : 0;
				}
			}
			DrawData drawData;
			int di = 0;
			while (drawDatas.Count > 0) {
				drawData = drawDatas.Dequeue();
				if (drawDatas.Count < 16) {
					drawData.texture = EndTexture;
					drawData.sourceRect = new Rectangle(++di, Projectile.frame * 24, 1, 24);
				}
				drawData.Draw(spriteBatch);
			}
			//IceTorch and Maybe CoralTorch could also be good
			Dust.NewDustPerfect(origin + (perpUnit * Main.rand.NextFloat(-4, 4)), DustID.BlueTorch, unit * 5).noGravity = true;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Player p = Main.player[Projectile.owner];
			Vector2 unit = (Main.player[Projectile.owner].MountedCenter - _targetPos);
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

			Vector2 unit = velocity;
			unit.Normalize();

			// Overriding that, if the player shoves the Prism into or through a wall, the interpolation starts at the player's center.
			// This last part prevents the player from projecting beams through walls under any circumstances.
			Player player = Main.player[Projectile.owner];
			if (Projectile.timeLeft == 32) {
				velocity = Vector2.Normalize(Main.MouseWorld - player.MountedCenter);
				Projectile.position += velocity * 16;
				if (!Collision.CanHitLine(player.Center, 0, 0, Projectile.Center, 0, 0)) {
					Projectile.Center = player.Center;
				}
			}
			float[] laserScanResults = new float[sample_points];
			Collision.LaserScan(Projectile.Center, unit, collision_size * Projectile.scale, 1200f, laserScanResults);
			float averageLengthSample = 0f;
			for (int i = 0; i < laserScanResults.Length; ++i) {
				averageLengthSample += laserScanResults[i];
			}
			averageLengthSample /= sample_points;
			_targetPos = Projectile.Center + (unit * averageLengthSample);

			if (++Projectile.frameCounter > 5) {
				Projectile.frame = (Projectile.frame + 1) % 4;
				Projectile.frameCounter = 0;
			}
		}

		public override bool ShouldUpdatePosition() {
			return false;
		}
	}
}