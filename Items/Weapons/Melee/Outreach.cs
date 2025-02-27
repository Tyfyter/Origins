using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Origins.Dev;
using Origins.Projectiles;
using PegasusLib;

namespace Origins.Items.Weapons.Melee {
	[LegacyName("Outreach")]
	public class Soldering_Iron : ModItem, ICustomWikiStat {
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ItemID.Sets.Spears[Type] = true;
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 26;
			Item.DamageType = DamageClass.Melee;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.width = 46;
			Item.height = 46;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.shoot = ModContent.ProjectileType<Soldering_Iron_P>();
			Item.shootSpeed = 1f;
			Item.useTurn = false;
			Item.channel = true;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Item1;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<NE8>(), 5)
			.AddIngredient(ModContent.ItemType<Sanguinite_Bar>(), 9)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool MeleePrefix() => true;
	}
	[LegacyName("Outreach_P")]
	public class Soldering_Iron_P : ModProjectile {
		static new AutoCastingAsset<Texture2D> GlowTexture;
		public override void SetStaticDefaults() {
			MeleeGlobalProjectile.ApplyScaleToProjectile[Type] = true;
			Main.projFrames[Type] = 7;
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(base.GlowTexture);
			}
		}
		public override void Unload() {
			GlowTexture = null;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.timeLeft = 3600;
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.aiStyle = 0;
			Projectile.scale = 1f;
			Projectile.noEnchantmentVisuals = true;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.direction = player.direction;
			player.heldProj = Projectile.whoAmI;
			player.itemTime = player.itemAnimation;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
			float armRot = (Projectile.velocity * new Vector2(1, player.gravDir)).ToRotation() - MathHelper.PiOver2;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, armRot + 0.35f * Projectile.direction);
			Projectile.Center = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.ThreeQuarters, Projectile.rotation - MathHelper.PiOver4 * 3);
			//Projectile.position.X = ownerMountedCenter.X - (Projectile.width / 2);
			//Projectile.position.Y = ownerMountedCenter.Y - (Projectile.height / 2);
			Vector2 shoulderPos = player.RotatedRelativePoint(player.MountedCenter);//position + new Vector2(20, 23);
			shoulderPos += new Vector2(player.direction * 6, player.gravDir * -2);
			player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, ((Projectile.Center + Projectile.velocity * 8) - shoulderPos).ToRotation() - MathHelper.PiOver2);
			Projectile.position += Projectile.velocity * 90 * Projectile.scale;
			if (player.ItemAnimationEndingOrEnded) {
				Projectile.Kill();
			}
			Projectile.frame = (int)(Main.projFrames[Type] * (1 - player.itemAnimation / (float)player.itemAnimationMax));
			Projectile.friendly = Projectile.frame == 4;
			float flaskOffsetAmount = 0;
			switch (Projectile.frame) {
				case 0:
				case 5:
				flaskOffsetAmount = 40;
				break;
				case 1:
				flaskOffsetAmount = 42;
				break;
				case 2:
				flaskOffsetAmount = 48;
				break;
				case 3:
				flaskOffsetAmount = 50;
				break;
				case 6:
				flaskOffsetAmount = 37;
				break;
			}
			Projectile.EmitEnchantmentVisualsAt(Projectile.position - Projectile.velocity * flaskOffsetAmount * Projectile.scale, Projectile.width, Projectile.height);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Rectangle frame = texture.Frame(verticalFrames: Main.projFrames[Type], frameY: Projectile.frame);
			SpriteEffects spriteEffects = Projectile.direction == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
			float rotation = Projectile.rotation - (Projectile.direction == -1 ? MathHelper.PiOver2 : 0);
			Vector2 origin = new Vector2(99, 11).Apply(spriteEffects, frame.Size());
			Main.EntitySpriteDraw(
				texture,
				(Projectile.Center) - Main.screenPosition,
				frame,
				lightColor,
				rotation,
				origin,
				Projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				GlowTexture,
				(Projectile.Center) - Main.screenPosition,
				frame,
				Color.White,
				rotation,
				origin,
				Projectile.scale,
				spriteEffects
			);
			return false;
		}
	}
}