using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Tools;
using Origins.Journal;
using Origins.NPCs;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Magic {
	public class Seam_Beam : ModItem, ICustomWikiStat, IJournalEntrySource, ITornSource {
		public static float TornSeverity => 0.3f;
		float ITornSource.Severity => TornSeverity;
		public string[] Categories => [
			WikiCategories.MagicGun
		];
		static short glowmask;
		public string EntryName => "Origins/" + typeof(Seam_Beam_Entry).Name;
		public class Seam_Beam_Entry : JournalEntry {
			public override string TextKey => "Seam_Beam";
			public override JournalSortIndex SortIndex => new("Riven", 10);
		}
		public override void SetStaticDefaults() {
			glowmask = Origins.AddGlowMask(this);
		}

		public override void SetDefaults() {
			Item.damage = 42;
			Item.DamageType = DamageClass.Magic;
			Item.mana = 19;
			Item.shoot = ModContent.ProjectileType<Seam_Beam_Beam>();
			Item.useTime = Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noUseGraphic = false;
			Item.noMelee = true;
			Item.shootSpeed = Seam_Beam_Beam.tick_motion;
			Item.width = 32;
			Item.height = 24;
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = Origins.Sounds.EnergyRipple;
			Item.glowMask = glowmask;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			Vector2 direction = velocity.SafeNormalize(default);
			float[] samples = new float[3];
			Collision.LaserScan(
				position,
				direction,
				5,
				32,
				samples
			);
			position += direction * samples.Average();
		}
	}
	public class Seam_Beam_Beam : ModProjectile {
		public const int tick_motion = 8;
		const int max_length = 1200;
		public override string Texture => "Origins/Projectiles/Weapons/Seam_Beam_P";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = max_length / tick_motion;
			ProjectileID.Sets.DrawScreenCheckFluff[Type] = max_length + 16;
			OriginsSets.Projectiles.DuplicationAIVariableResets[Type].second = true;
			if (GetType() == typeof(Seam_Beam_Beam)) ID = Type;
		}
		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.extraUpdates = 25;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 300, targetSeverity: Seam_Beam.TornSeverity, source: Main.player[Projectile.owner].GetModPlayer<OriginPlayer>());
		}

		public override bool PreDraw(ref Color lightColor) {
			Seam_Beam_Drawer drawer = default;
			drawer.Draw(Projectile);
			return false;
		}
		public override void ModifyDamageHitbox(ref Rectangle hitbox) {
			hitbox.Inflate(2, 2);
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			for (int i = 1; i < Projectile.ai[1]; i++) {
				Vector2 pos = Projectile.oldPos[^i];
				if (pos == default) {
					break;
				} else if (projHitbox.Recentered(pos).Intersects(targetHitbox)) {
					return true;
				}
			}
			return null;
		}
		public override void AI() {
			if (Projectile.numUpdates == -1 && ++Projectile.ai[2] >= 38) {
				Projectile.Kill();
				return;
			}
			if (Projectile.ai[0] != 1) {
				if (++Projectile.ai[1] > ProjectileID.Sets.TrailCacheLength[Type]) {
					StopMovement();
				} else {
					int index = (int)Projectile.ai[1];
					Projectile.oldPos[^index] = Projectile.Center;
					Projectile.oldRot[^index] = Projectile.velocity.ToRotation();
				}
			}
			if (++Projectile.frameCounter > 5) {
				Projectile.frame = (Projectile.frame + 1) % 4;
				Projectile.frameCounter = 0;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			Vector2 direction = oldVelocity.SafeNormalize(default);
			if (direction != default) {
				float[] samples = new float[3];
				Collision.LaserScan(
					Projectile.Center,
					direction,
					5,
					32,
					samples
				);
				if (samples.Average() > tick_motion * 0.5f) {
					Projectile.Center += direction * tick_motion;
					int index = Math.Min((int)++Projectile.ai[1], Projectile.oldPos.Length);
					Projectile.oldRot[^index] = oldVelocity.ToRotation();
				}
			}
			StopMovement();
			return false;
		}
		void StopMovement() {
			Projectile.velocity = Vector2.Zero;
			Projectile.ai[0] = 1;
			Projectile.extraUpdates = 0;
		}
	}
	public struct Seam_Beam_Drawer {
		public const int TotalIllusions = 1;

		public const int FramesPerImportantTrail = 60;

		private static VertexStrip _vertexStrip = new();

		public float Length;
		public void Draw(Projectile proj) {
			MiscShaderData miscShaderData = GameShaders.Misc["Origins:Beam"];
			float uTime = (float)Main.timeForVisualEffects / 44;
			int length = proj.oldPos.Length;
			if (length == 0) return;
			float[] rot = new float[length];
			Vector2[] pos = new Vector2[length];
			Vector2 unit = default;
			int dustTimer = 0;
			for (int i = 0; i < length; i++) {
				Index reverseIndex = ^(i + 1);
				if (proj.oldPos[reverseIndex] == default) {
					length = i;
					Array.Resize(ref rot, length);
					Array.Resize(ref pos, length);
					break;
				}
				rot[i] = proj.oldRot[reverseIndex];
				pos[i] = proj.oldPos[reverseIndex];
				Lighting.AddLight(pos[i], 0.1f, 0.35f, 1f);
				unit = new Vector2(1, 0).RotatedBy(rot[i]);
				if (Main.rand.Next(++dustTimer) > 6) {
					Dust.NewDustPerfect(pos[i] + (new Vector2(unit.Y, -unit.X) * Main.rand.NextFloat(-4, 4)), DustID.BlueTorch, unit * 5).noGravity = true;
					dustTimer = Main.rand.NextBool() ? 2 : 0;
				}
			}
			if (length == 0) return;
			Dust.NewDustPerfect(pos[length - 1] + (new Vector2(unit.Y, -unit.X) * Main.rand.NextFloat(-4, 4)), DustID.BlueTorch, unit * 5).noGravity = true;
			Asset<Texture2D> texture = TextureAssets.Projectile[Seam_Beam_Beam.ID];
			miscShaderData.UseImage0(texture);
			miscShaderData.UseShaderSpecificData(texture.UVFrame(verticalFrames: 4, frameY: proj.frame));
			float endLength = (16f / Seam_Beam_Beam.tick_motion) / length;
			miscShaderData.Shader.Parameters["uLoopData"].SetValue(new Vector2(
				16f / 48f,
				endLength
			));
			miscShaderData.Apply();
			_vertexStrip.PrepareStrip(pos, rot, StripColors, StripWidth, -Main.screenPosition, length, includeBacksides: true);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		private Color StripColors(float progressOnStrip) {
			return Color.White;
		}

		private float StripWidth(float progressOnStrip) {
			return 10;
		}
	}
}