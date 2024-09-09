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

using Origins.Dev;
using Origins.Tiles.Other;
namespace Origins.Items.Weapons.Magic {
    public class Pike_of_Deepneus : ModItem, ICustomWikiStat {
		public const int baseDamage = 64;
        public string[] Categories => [
            "OtherMagic"
        ];
        public override void SetDefaults() {
			Item.damage = 160;
			Item.DamageType = DamageClass.Magic;
			Item.shoot = ModContent.ProjectileType<Pike_of_Deepneus_P>();
			Item.knockBack = 8;
			Item.shootSpeed = 24f;
			Item.mana = 34;
			Item.useStyle = -1;
			Item.useTime = 17;
			Item.useAnimation = 17;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.autoReuse = true;
			Item.reuseDelay = 29;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 3);
			Item.UseSound = SoundID.Item69;
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddRecipeGroup("HellBars", 16);
			recipe.AddRecipeGroup("AdamantiteBars", 16);
			recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 14);
			recipe.AddIngredient(ModContent.ItemType<Power_Core>());
			recipe.AddTile(ModContent.TileType<Fabricator>());
			recipe.Register();
		}
		public override void UseItemFrame(Player player) {
			float rotation = player.itemRotation - MathHelper.PiOver2 - GetArmDrawAngle(player);
			player.SetCompositeArmFront(
				true,
				Player.CompositeArmStretchAmount.Full,
				rotation
			);
			float num23 = rotation * (float)player.direction;
			player.bodyFrame.Y = player.bodyFrame.Height * 3;
			if (num23 < -0.75) {
				player.bodyFrame.Y = player.bodyFrame.Height * 2;
				if (player.gravDir == -1f) {
					player.bodyFrame.Y = player.bodyFrame.Height * 4;
				}
			}
			if (num23 > 0.6) {
				player.bodyFrame.Y = player.bodyFrame.Height * 4;
				if (player.gravDir == -1f) {
					player.bodyFrame.Y = player.bodyFrame.Height * 2;
				}
			}
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(
				source,
				position,
				velocity,
				type,
				damage,
				knockback,
				player.whoAmI,
				ai0: player.itemAnimationMax
			);
			return false;
		}

		internal static float GetArmDrawAngle(Player player) {
			return Math.Max((player.itemAnimation / (float)player.itemAnimationMax) * 6 - 5, 0) * (MathHelper.PiOver2 * 0.85f) * player.direction;
		}
	}
	public class Pike_of_Deepneus_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Magic/Pike_of_Deepneus";
		public new AutoCastingAsset<Texture2D> GlowTexture { get; private set; }
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Pike of Deepneus");
			if (!Main.dedServ) {
				GlowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
			}
		}
		public override void Unload() {
			GlowTexture = default;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Daybreak);
			Projectile.DamageType = DamageClass.Magic;
			Projectile.width = Projectile.height = 32;
			Projectile.extraUpdates = 3;
			Projectile.aiStyle = 0;
			Projectile.alpha = 0;
			Projectile.tileCollide = false;
			Projectile.penetrate = 1;
			Projectile.ArmorPenetration = 40;
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item167, Projectile.position);

			Dust dust = Dust.NewDustDirect(Projectile.Center, -11, 0, DustID.GoldFlame, 0, 0, 255, new Color(255, 150, 30), 1f);
			dust.noGravity = false;
			dust.velocity *= 8f;

			dust = Dust.NewDustDirect(Projectile.Center, -11, 0, DustID.Firework_Yellow, 0, 0, 255, default, 4f);
			dust.noGravity = false;
			dust.velocity *= 8f;
		}
		public override void AI() {
			Dust dust = Dust.NewDustDirect(Projectile.Center, -11, 0, DustID.GoldFlame, 0, 0, 255, new Color(255, 150, 30), 0.6f);
			dust.noGravity = false;
			dust.velocity *= 8f;

			/*Dust dust = Dust.NewDustDirect(Projectile.Center, -11, 0, DustID.Firework_Yellow, 0, 0, 50, default, 0.6f);
			dust.noGravity = false;
			dust.velocity *= 4f;*/

			if (Projectile.ai[0] != 0) {
				Player player = Main.player[Projectile.owner];
				if (Projectile.owner == Main.myPlayer) {
					if (player.channel) {
						Vector2 newVel = (Main.MouseWorld - player.RotatedRelativePoint(player.MountedCenter)).SafeNormalize(Vector2.UnitX * player.direction);
						if (player.HeldItem.shoot == Type) {
							newVel *= player.HeldItem.shootSpeed;
						}
						if (newVel != Projectile.velocity) {
							Projectile.netUpdate = true;
						}
						Projectile.velocity = newVel;
						if (Projectile.ai[1] < 1) {
							Projectile.ai[1] += 1 / (Projectile.ai[0] * 2);
						}
						player.reuseDelay -= (int)(player.reuseDelay * Projectile.ai[1] * 0.5f);
					} else {
						Projectile.ai[0] = 0;
						Projectile.velocity *= 1 + Projectile.ai[1] * 0.5f;
						Projectile.netUpdate = true;
					}
				}
				player.itemRotation = Projectile.velocity.ToRotation();
				player.itemAnimation = (int)(player.itemAnimationMax * (1 + Projectile.ai[1] * 0.15f));
				player.itemTime = player.itemTimeMax = player.itemAnimation;
				player.heldProj = Projectile.whoAmI;
				player.ChangeDir(Projectile.direction = Math.Sign(Projectile.velocity.X));
				Projectile.rotation = player.itemRotation;
				Projectile.Center = player.MountedCenter
					+ new Vector2(player.direction * -4, -6)
					+ OriginExtensions.Vec2FromPolar(player.itemRotation - Pike_of_Deepneus.GetArmDrawAngle(player), 16)
					+ Projectile.velocity.SafeNormalize(default) * 36;
			} else {
				Projectile.hide = false;
				Projectile.tileCollide = true;
				Projectile.velocity.Y += 0.04f;
				Projectile.rotation = Projectile.velocity.ToRotation();
			}
		}
		public override bool? CanHitNPC(NPC target) {
			if (Projectile.ai[0] != 0) return false;
			return null;
		}
		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			modifiers.SourceDamage *= 0.34f + Projectile.ai[1] * Projectile.ai[1] * 0.66f;
			modifiers.Knockback *= 1 + Projectile.ai[1] * 0.65f;
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			width = 14;
			height = 14;
			fallThrough = true;
			return true;
		}
		public override bool PreDraw(ref Color lightColor) {
			Vector2 position = Projectile.Center - Main.screenPosition;
			float rotation = Projectile.rotation + (MathHelper.Pi * 0.8f * Projectile.direction - MathHelper.PiOver2);
			Vector2 origin = new Vector2(30 + 25 * Projectile.direction, 9);
			float scale = Projectile.scale;
			SpriteEffects spriteEffects = Projectile.direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				position,
				null,
				lightColor,
				rotation,
				origin,
				scale,
				spriteEffects,
			0);
			if (GlowTexture.IsLoaded) {
				Main.EntitySpriteDraw(
					GlowTexture,
					position,
					null,
					Color.White,
					rotation,
					origin,
					scale,
					spriteEffects,
					0
				);
			}
			return false;
		}
	}
}
