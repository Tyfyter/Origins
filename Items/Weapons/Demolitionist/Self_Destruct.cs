using Microsoft.Xna.Framework.Graphics;
using Origins.Achievements;
using Origins.CrossMod;
using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Items.Tools;
using Origins.Projectiles;
using Origins.Tiles.Other;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Self_Destruct : ModItem {
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			Origins.AddGlowMask(this);
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
		}
		public override void SetDefaults() {
			Item.DamageType = DamageClasses.Explosive;
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.damage = 261;
			Item.crit = 24;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.UseSound = Origins.Sounds.IMustScream.WithVolumeScale(0.4f);
			Item.shoot = ModContent.ProjectileType<Self_Destruct_P>();
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(gold: 3);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.SoulofMight, 15)
			.AddIngredient(ModContent.ItemType<Busted_Servo>(), 28)
			.AddIngredient(ModContent.ItemType<Power_Core>(), 2)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(Item.UseSound, player.Center, (sound) => {
				sound.Position = player.Center;
				return true;
			});
			return null;
		}
		public override bool WeaponPrefix() => true;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			position = player.MountedCenter;
			velocity = player.velocity;
		}
	}
	public class Self_Destruct_P : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Self_Destruct_Body";
		public override void SetDefaults() {
			Projectile.timeLeft = 110;
			Projectile.tileCollide = false;
			Projectile.width = 32;
			Projectile.height = 48;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			//player.velocity *= 0.95f;
			Projectile.Center = player.MountedCenter;
			Projectile.velocity = player.velocity;
			player.heldProj = Projectile.whoAmI;
			Main.instance.CameraModifiers.Add(new CameraShakeModifier(
				Projectile.Center, 10f, 18f, 30, 1000f, 2f, nameof(Self_Destruct)
			));
			/*if (Main.rand.NextBool(5)) {
				Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Electric, 0, 0, 255, [255, 0, 0]);
				dust.noGravity = true;
				dust.velocity *= 0.1f;
			}*/
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false;
		}
		public override void OnKill(int timeLeft) {
			if (Projectile.owner == Main.myPlayer) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					default,
					ModContent.ProjectileType<Self_Destruct_Explosion>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner
				);
			}
			Main.instance.CameraModifiers.Add(new CameraShakeModifier(
				Projectile.Center, 10f, 18f, 30, 1000f, 2f, nameof(Self_Destruct)
			));
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			Rectangle frame = new((player.bodyFrame.Y / player.bodyFrame.Height == 5 ? 1 : 0) * 40, (player.Male ? 0 : 2) * 56, 40, 56);
			Vector2 position = new Vector2(
					(int)(player.position.X - (player.bodyFrame.Width / 2) + (player.width / 2)),
					(int)(player.position.Y + player.height - player.bodyFrame.Height)
				)
				+ player.bodyPosition
				+ new Vector2(player.bodyFrame.Width / 2, player.bodyFrame.Height / 2)
				+ Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / player.bodyFrame.Height];
			Main.EntitySpriteDraw(
				TextureAssets.Projectile[Type].Value,
				position - Main.screenPosition,
				frame,
				Color.White,
				player.bodyRotation,
				new Vector2(player.legFrame.Width * 0.5f, player.legFrame.Height * 0.5f),
				1f,
				player.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None
			);
			return false;
		}
	}
	public class Self_Destruct_Explosion : ModProjectile, IIsExplodingProjectile {
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.GeyserTrap;
		public override void SetDefaults() {
			Projectile.DamageType = DamageClasses.Explosive;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 5;
			Projectile.penetrate = -1;
			Projectile.aiStyle = 0;
			Projectile.width = 352;
			Projectile.height = 352;
			Projectile.hide = false;
			Projectile.localNPCHitCooldown = -1;
			Projectile.usesLocalNPCImmunity = true;
		}
		public override void AI() {
			if (Projectile.ai[0] == 0) {
				ExplosiveGlobalProjectile.ExplosionVisual(
					Projectile,
					true,
					sound: SoundID.Item62.WithVolumeScale(0.6f)
				);
				ExplosiveGlobalProjectile.DealSelfDamage(Projectile);
				Projectile.ai[0] = 1;
			}
			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Self_Destruct_Flash>(), 0, 6, Projectile.owner, ai1: -0.5f).scale = 1f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (target.boss || NPCID.Sets.ShouldBeCountedAsBoss[target.type]) {
				if ((hit.InstantKill || damageDone >= target.life) && Main.player[Projectile.owner].dead)
					ModContent.GetInstance<Martyrdom>().Condition.Complete();
			}
		}
		public void Explode(int delay = 0) { }
		public bool IsExploding => true;
	}
	public class Self_Destruct_Flash : ModProjectile {
		public override string Texture => "Origins/Items/Weapons/Demolitionist/Self_Destruct_Visual";
		public override void SetDefaults() {
			Projectile.timeLeft = 15;
			Projectile.tileCollide = false;
			Projectile.alpha = 100;
		}
		public override void AI() {
			Lighting.AddLight(Projectile.Center, new Vector3(2, 0, 0));
		}
		public override bool PreDraw(ref Color lightColor) {
			const float scale = 3f;
			Main.spriteBatch.Restart(SpriteSortMode.Immediate);
			DrawData data = new(
				Mod.Assets.Request<Texture2D>("Projectiles/Pixel").Value,
				Projectile.Center - Main.screenPosition,
				new Rectangle(0, 0, 1, 1),
				new Color(0, 0, 0, 255),
				0, new Vector2(0.5f, 0.5f),
				new Vector2(160, 160) * scale,
				SpriteEffects.None,
			0);
			float percent = Projectile.timeLeft / 10f;
			Origins.blackHoleShade.UseOpacity(0.985f);
			Origins.blackHoleShade.UseSaturation(0f + percent);
			Origins.blackHoleShade.UseColor(3, 0, 0);
			Origins.blackHoleShade.Shader.Parameters["uScale"].SetValue(0.5f);
			Origins.blackHoleShade.Apply(data);
			Main.EntitySpriteDraw(data);
			Main.spriteBatch.Restart();
			return false;
		}
	}
	public class Self_Destruct_Crit_Type : CritType<Self_Destruct> {
		static float CritThreshold => 0.3f; // hit is dealt after self-damage, so being brought below this by SD will make it crit on that use
		public override LocalizedText Description => base.Description.WithFormatArgs(CritThreshold);
		public override bool CritCondition(Player player, Item item, Projectile projectile, NPC target, NPC.HitModifiers modifiers) => (player.statLife / (float)player.statLifeMax2) <= CritThreshold;
		public override float CritMultiplier(Player player, Item item) => player.dead ? 3f : 1.5f;
	}
}
