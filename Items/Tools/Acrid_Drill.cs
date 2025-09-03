using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using PegasusLib;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Tools {
	public class Acrid_Drill : ModItem {
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ItemID.Sets.IsDrill[Type] = true;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TitaniumDrill);
			Item.damage = 28;
			Item.pick = 195;
			Item.width = 28;
			Item.height = 26;
			Item.knockBack = 1f;
			Item.shootSpeed = 56f;
			Item.shoot = ModContent.ProjectileType<Acrid_Drill_P>();
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.LightRed;
		}
		public override float UseTimeMultiplier(Player player) {
			return player.wet ? 1.5f : 1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Eitrite_Bar>(), 15)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
	public class Acrid_Drill_P : ModProjectile {
		static AutoLoadingAsset<Texture2D> glowTexture;
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 2;
			glowTexture = GlowTexture;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Melee;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.aiStyle = 20;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
			Projectile.ownerHitCheck = true;
		}
		public override void AI() {
			Projectile.position -= Projectile.velocity.SafeNormalize(default) * 4;
			if (Main.player[Projectile.owner].wet) ++Projectile.frameCounter;
			if (++Projectile.frameCounter > 4) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame > 1) {
					Projectile.frame = 0;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			int offsetY = 0;
			int offsetX = 0;
			float originX = (TextureAssets.Projectile[Projectile.type].Width() - Projectile.width) * 0.5f + Projectile.width * 0.5f;
			ProjectileLoader.DrawOffset(Projectile, ref offsetX, ref offsetY, ref originX);
			SpriteEffects dir = SpriteEffects.None;
			if (Projectile.spriteDirection == -1) {
				dir = SpriteEffects.FlipHorizontally;
			}
			if (Main.player[Projectile.owner].gravDir == -1f) {
				dir ^= SpriteEffects.FlipHorizontally;
			}
			int frameSize = TextureAssets.Projectile[Projectile.type].Height() / Main.projFrames[Projectile.type];
			int frameY = frameSize * Projectile.frame;
			Color alpha = Projectile.GetAlpha(lightColor);
			Main.EntitySpriteDraw(TextureAssets.Projectile[Projectile.type].Value, new Vector2(Projectile.position.X - Main.screenPosition.X + originX + offsetX, Projectile.position.Y - Main.screenPosition.Y + Projectile.height / 2 + Projectile.gfxOffY), new Rectangle(0, frameY, TextureAssets.Projectile[Projectile.type].Width(), frameSize - 1), alpha, Projectile.rotation, new Vector2(originX, Projectile.height / 2 + offsetY), Projectile.scale, dir);
			if (glowTexture.Exists) {
				Main.EntitySpriteDraw(glowTexture.Value, Projectile.position - Main.screenPosition + new Vector2(originX + offsetX, Projectile.height / 2 + Projectile.gfxOffY), new Rectangle(0, frameY, TextureAssets.Projectile[Projectile.type].Width(), frameSize - 1), new Color(250, 250, 250, Projectile.alpha), Projectile.rotation, new Vector2(originX, Projectile.height / 2 + offsetY), Projectile.scale, dir);
			}
			return false;
		}
	}
}
