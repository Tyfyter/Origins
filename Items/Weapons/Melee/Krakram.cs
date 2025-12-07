using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Melee {
	public class Krakram : ModItem {
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ThornChakram);
			Item.damage = 25;
			Item.width = 34;
			Item.height = 34;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.shoot = ModContent.ProjectileType<Krakram_P>();
			Item.shootSpeed = 9.75f;
			Item.value = Item.sellPrice(silver: 40);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
			
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Defiled_Bar>(), 9)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool CanUseItem(Player player) {
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}
	}
	public class Krakram_P : ModProjectile, IOutlineDrawer {
		public override string Texture => "Origins/Items/Weapons/Melee/Krakram";
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
			ProjectileID.Sets.TrailingMode[Type] = 2;

		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ThornChakram);
			Projectile.penetrate = -1;
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.localAI[0] = 1;
		}
		public override bool PreAI() {
			Projectile.aiStyle = 3;
			//Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.localAI[0] * 0.15f);
			//Projectile.localAI[0] = (float)System.Math.Sin(Projectile.timeLeft);
			return true;
		}
		public override void AI() {
			if (Projectile.localAI[1] > 0 && --Projectile.localAI[1] > 0) return;
			float dist = 16 * 5;
			dist *= dist;
			int targetIndex = -1;
			OriginGlobalProj globalProj = Projectile.GetGlobalProjectile<OriginGlobalProj>();
			bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
				if (globalProj.alreadyUnmissed[target.whoAmI]) return false;
				float newDist = Projectile.DistanceSQ(target.Center);
				if (target.whoAmI == Projectile.ai[2]) newDist *= 0.25f;
				if (newDist < dist) {
					dist = newDist;
					targetIndex = target.whoAmI;
					return true;
				}
				return false;
			});
			if (targetIndex != Projectile.ai[2]) {
				Projectile.localAI[0] = 0;
				Projectile.ai[2] = targetIndex;
			} else if (targetIndex != -1) {
				if (Projectile.ai[0] == 0) Projectile.ai[1] = 0;

				globalProj.unmissTargetPos = Main.npc[targetIndex].Center - Projectile.velocity;
				globalProj.unmissAnimation = (int)++Projectile.localAI[0];
				if (globalProj.unmissAnimation == 8) {
					globalProj.alreadyUnmissed[targetIndex] = true;
					(globalProj.unmissTargetPos, Projectile.Center) = (Projectile.Center, globalProj.unmissTargetPos);
					Projectile.localAI[0] = 0;
					Projectile.localAI[1] = 9;
					Projectile.ai[2] = -1;
				}
			}
		}
		public override bool? CanHitNPC(NPC target) {
			Projectile.aiStyle = 0;
			return null;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 27;
			height = 27;
			Projectile.velocity.RotatedBy(Main.rand.NextFloatDirection());
			return true;
		}


		public override bool PreDraw(ref Color lightColor) {
			this.DrawOutline();
			Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2f,1f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None);
			return false;
		}
		public override bool PreDrawExtras() {
			default(KrakramTrail).Draw(Projectile);
			return base.PreDrawExtras();
		}
		public readonly struct KrakramTrail {
		private static readonly VertexStrip _vertexStrip = new();
		public readonly void Draw(Projectile proj) {
			MiscShaderData miscShaderData = GameShaders.Misc["LightDisc"];
			miscShaderData.UseSaturation(-2.8f);
			miscShaderData.UseOpacity(2f);
			miscShaderData.Apply();
			for (int i = 0; i < proj.oldPos.Length; i++) {
				Vector2 pos = proj.oldPos[i];
				if (pos != default) Lighting.AddLight(pos, StripColors(i / (float)proj.oldPos.Length).ToVector3() * 0.2f);
			}
			_vertexStrip.PrepareStripWithProceduralPadding(proj.oldPos, proj.oldRot, StripColors, StripWidth, -Main.screenPosition + proj.Size / 2f);
			_vertexStrip.DrawTrail();
			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}
		private readonly Color StripColors(float progressOnStrip) {
			float num = 1f - progressOnStrip;
			Color result = Color.Lerp(Color.White, Color.Purple, num) * num;
			result.A = 0;
			return result;
		}
		private readonly float StripWidth(float progressOnStrip) => 16f;
	}
		public Color? SetOutlineColor(float progress) {
			return Color.Lerp(Color.White,Color.Purple,0.1f);
		}

		public DrawData[] OutlineDrawDatas => [new DrawData(TextureAssets.Projectile[Type].Value, Projectile.Center, null, Color.White, Projectile.rotation, TextureAssets.Projectile[Type].Size() / 2f,1f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None)];
		public int OutlineSteps => 8;
		public float OutlineOffset => 2;
	}
}
