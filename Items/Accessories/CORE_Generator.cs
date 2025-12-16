using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Dev;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using PegasusLib;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Waist)]
	public class CORE_Generator : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.ExplosiveBoostAcc
		];
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[Type] = AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher];
			IL_Player.Hurt_HurtInfo_bool += IL_Player_Hurt_HurtInfo_bool;
			ID = Type;
		}

		private static void IL_Player_Hurt_HurtInfo_bool(ILContext il) {
			ILCursor c = new(il);
			int item = -1;
			int type = -1;
			c.GotoNext(MoveType.Before,
				i => i.MatchLdarg0(),
				i => i.MatchLdfld<Player>(nameof(Player.starCloakItem_starVeilOverrideItem)),
				i => i.MatchStloc(out item),
				i => i.MatchLdcI4(ProjectileID.StarVeilStar),
				i => i.MatchStloc(out type)
			);
			c.GotoPrev(MoveType.After,
				i => i.MatchLdarg0(),
				i => i.MatchLdfld<Player>(nameof(Player.starCloakItem)),
				i => i.MatchStloc(out item)
			);
			c.EmitLdloc(item);
			c.EmitLdloca(type);
			c.EmitDelegate<StarCloakOverride>(static (Item item, ref int type) => {
				if (item.type == ID) {
					type = CORE_Generator_Star_P.ID;
				}
			});
		}
		delegate void StarCloakOverride(Item item, ref int type);
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 26);
			Item.value = Item.sellPrice(gold: 7);
			Item.rare = ItemRarityID.LightPurple;
			Item.accessory = true;
			Item.damage = 10;
			Item.DamageType = DamageClasses.Explosive;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.shootSpeed = 5;
			Item.useAmmo = AmmoID.Rocket;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Last_Descendant>())
			.AddIngredient(ModContent.ItemType<Missile_Armcannon>())
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			player.GetAttackSpeed(DamageClasses.Explosive) += 0.1f;
			originPlayer.explosiveBlastRadius += 0.15f;
			player.GetKnockback(DamageClasses.Explosive) += 0.2f;
			originPlayer.explosiveProjectileSpeed += 0.25f;

			originPlayer.destructiveClaws = true;
			originPlayer.gunGlove = true;
			originPlayer.gunGloveItem = Item;
			originPlayer.guardedHeart = true;
			originPlayer.coreGenerator = true;
			originPlayer.coreGeneratorItem = Item;
			player.longInvince = true;
			player.starCloakItem = Item;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (CanisterGlobalItem.GetCanisterType(source.AmmoItemIdUsed) != -1) {
				type = ModContent.ProjectileType<CORE_Generator_P>();
				float speed = velocity.Length();
				for (int i = 0; i < 5; i++) {
					Projectile.NewProjectile(source, position, GeometryUtils.Vec2FromPolar(speed, MathHelper.TwoPi * (i / 5f) - MathHelper.PiOver2), type, damage, knockback, player.whoAmI);
				}
				SoundEngine.PlaySound(Item.UseSound, position);
				return false;
			}
			return true;
		}
	}
	public class CORE_Generator_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.aiStyle = 0;
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.timeLeft = 120;
			Projectile.scale = 0.85f;
			Projectile.penetrate = 1;
		}
		public override void AI() {
			Projectile.velocity.Y += 0.2f * Projectile.GetGlobalProjectile<CanisterGlobalProjectile>().gravityMultiplier;
			Projectile.rotation += Projectile.velocity.X * 0.1f;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			if (Projectile.timeLeft > 105) {
				if (Projectile.velocity.X == 0f) {
					Projectile.velocity.X = -oldVelocity.X;
				}
				if (Projectile.velocity.Y == 0f) {
					Projectile.velocity.Y = -oldVelocity.Y;
				}
				return false;
			}
			return true;
		}
	}
	public class CORE_Generator_Star_P : ModProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StarVeilStar;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.StarVeilStar);
			Projectile.DamageType = DamageClasses.Explosive;
		}
		public override void AI() {
			base.AI();
			if (Main.rand.NextBool(3)) {
				if (Main.rand.NextBool(9)) {
					Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.position, Projectile.velocity.RotatedByRandom(0.1f) * -0.2f, Main.rand.Next(61, 64));
				} else {
					Dust.NewDustDirect(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						DustID.Smoke,
						Projectile.velocity.X * -0.2f,
						Projectile.velocity.Y * -0.2f,
						100,
						default,
						1f
					);
				}
			}
		}
		public override void OnKill(int timeLeft) {
			ExplosiveGlobalProjectile.DoExplosion(Projectile, 96, false, sound: SoundID.Item62);
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D trailTexture = TextureAssets.Extra[ExtrasID.FallingStar].Value;
			Vector2 trailOrigin = new(trailTexture.Width / 2f, 10f);
			Vector2 gfxOffset = new(0f, Projectile.gfxOffY);
			float visualTime = (float)Main.timeForVisualEffects / 45f;
			Vector2 spinOffset = new Vector2(0f, -5f).RotatedBy(MathHelper.TwoPi * visualTime);
			Vector2 trailPosition = Projectile.Center + Projectile.velocity - Main.screenPosition;
			Color mainColor = new(77, 35, 61, 70);
			float rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2f;
			Main.EntitySpriteDraw(trailTexture, trailPosition + gfxOffset + spinOffset, null, mainColor, rotation, trailOrigin, 1f, SpriteEffects.None);
			Main.EntitySpriteDraw(trailTexture, trailPosition + gfxOffset + spinOffset.RotatedBy(MathHelper.TwoPi / 3f), null, mainColor, rotation, trailOrigin, 0.6f, SpriteEffects.None);
			Main.EntitySpriteDraw(trailTexture, trailPosition + gfxOffset + spinOffset.RotatedBy(MathHelper.TwoPi * 1.5f), null, mainColor, rotation, trailOrigin, 0.8f, SpriteEffects.None);
			Vector2 pulsePos = Projectile.Center - Projectile.velocity * 0.5f;
			Color pulseColor = new(255, 44, 0, 80);
			for (float i = 0f; i < 1f; i += 0.5f) {
				float pulseFactorFactor = visualTime % 0.5f / 0.5f;
				pulseFactorFactor = (pulseFactorFactor + i) % 1f;
				float pulseFactor = pulseFactorFactor * 2f;
				if (pulseFactor > 1f)
					pulseFactor = 2f - pulseFactor;

				Main.EntitySpriteDraw(trailTexture, pulsePos - Main.screenPosition + gfxOffset, null, pulseColor * pulseFactor, rotation, trailOrigin, 0.5f + pulseFactorFactor * 0.5f, SpriteEffects.None);
			}
			Texture2D starTexture = TextureAssets.Projectile[Projectile.type].Value;
			Main.EntitySpriteDraw(
				starTexture,
				Projectile.Center - Main.screenPosition + gfxOffset,
				starTexture.Bounds,
				Color.Coral * Projectile.Opacity,
				Projectile.rotation,
				starTexture.Size() * 0.5f,
				Projectile.scale + 0.1f,
				Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally
			);
			return false;
		}
	}
}
