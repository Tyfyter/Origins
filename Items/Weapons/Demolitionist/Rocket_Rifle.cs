using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Dusts;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Rocket_Rifle : ModItem, ICustomWikiStat {
        public string[] Categories => [
            WikiCategories.Launcher
        ];
        public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
		}
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Rocket_Rifle_P>(50, 53, 8, 60, 24, true);
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.Pink;
			Item.UseSound = SoundID.Item11;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofNight, 15)
			.AddIngredient(ModContent.ItemType<Phoenum>(), 25)
			.AddIngredient(ModContent.ItemType<Scrap>(), 50)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-6f, -6);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position += velocity.SafeNormalize(default).RotatedBy(player.direction * MathHelper.PiOver2) * -6 * player.gravDir;
		}
	}
	public class Rocket_Rifle_P : ModProjectile, ICanisterProjectile {
		public override string Texture => ICanisterProjectile.base_texture_path + "Rocket_Thrust";
		public static AutoLoadingAsset<Texture2D> thrustTexture = ICanisterProjectile.base_texture_path + "Rocket_Thrust_Tintable";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Rocket_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Rocket_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public static int FreeFuel => 50;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
			ProjectileID.Sets.IsARocketThatDealsDoubleDamageToPrimaryEnemy[Type] = true;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.RocketI);
			Projectile.extraUpdates = 1;
			Projectile.timeLeft = 420;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
			Projectile.alpha = 255;
			Projectile.appliesImmunityTimeOnSingleHits = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.TryGetGlobalProjectile(out ExplosiveGlobalProjectile global)) {
				global.modifierBlastRadius = global.modifierBlastRadius.CombineWith(new(1, 1.5f));
			}
		}
		public override void AI() {
			if (++Projectile.localAI[0] < 7) return;
			Color dustColor = default;
			int dustType = DustID.Torch;
			if (Projectile.TryGetGlobalProjectile(out CanisterGlobalProjectile global)) {
				if (global.CanisterData?.Ammo is not Rocket_Dummy_Canister) Projectile.ai[0]++;
				if (global.CanisterData?.HasSpecialEffect ?? false) {
					dustType = Tintable_Torch_Dust.ID;
					dustColor = global.CanisterData.InnerColor with { A = 100 };
				}
			}
			if (Projectile.ai[0] < FreeFuel) {
				Vector2 dustBasePos = new Vector2(Projectile.position.X + 3f, Projectile.position.Y + 3f);
				for (int i = 0; i < 2; i++) {
					Vector2 offset = Projectile.velocity * i * 0.5f;

					if (Main.rand.NextBool(2)) {
						Dust dust = Dust.NewDustDirect(
							dustBasePos + offset - Projectile.velocity * 0.5f,
							Projectile.width - 8,
							Projectile.height - 8,
							dustType,
							0f,
							0f,
							100,
							dustColor
						);
						dust.scale *= 1.4f + Main.rand.Next(10) * 0.1f;
						dust.velocity *= 0.2f;
						dust.noGravity = true;
					}

					if (Main.rand.NextBool(2)) {
						Dust dust = Dust.NewDustDirect(
							dustBasePos + offset - Projectile.velocity * 0.5f,
							Projectile.width - 8,
							Projectile.height - 8,
							DustID.Smoke,
							0f,
							0f,
							100,
							default,
							0.5f
						);
						dust.fadeIn = 0.5f + Main.rand.Next(5) * 0.1f;
						dust.velocity *= 0.05f;
					}
				}
			} else {
				Projectile.velocity *= 0.997f;
				this.DoGravity(0.04f);
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;
		}
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			Vector2 origin = OuterTexture.Value.Size() * 0.5f;
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (projectile.spriteDirection == -1) spriteEffects |= SpriteEffects.FlipHorizontally;
			Main.EntitySpriteDraw(
				InnerTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.InnerColor,
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			Main.EntitySpriteDraw(
				OuterTexture,
				projectile.Center - Main.screenPosition,
				null,
				canisterData.OuterColor.MultiplyRGBA(lightColor),
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
			if (Projectile.ai[0] > FreeFuel) return;
			if (Projectile.TryGetGlobalProjectile(out CanisterGlobalProjectile global) && (global.CanisterData?.HasSpecialEffect ?? false)) {
				Main.EntitySpriteDraw(
					thrustTexture,
					projectile.Center - Main.screenPosition,
					null,
					canisterData.InnerColor,
					projectile.rotation,
					origin,
					projectile.scale,
					spriteEffects
				);
			} else {
				Main.EntitySpriteDraw(
					TextureAssets.Projectile[Type].Value,
					projectile.Center - Main.screenPosition,
					null,
					Color.White,
					projectile.rotation,
					origin,
					projectile.scale,
					spriteEffects
				);
			}
		}
	}
}
