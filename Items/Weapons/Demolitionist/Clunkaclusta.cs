using Microsoft.Xna.Framework.Graphics;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.Projectiles;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Clunkaclusta : ModItem, ICustomDrawItem {
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 11));
		}
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Clunkaclusta_P>(30, 30, 9f, 62, 40);
			Item.knockBack = 2f;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 45);
			Item.UseSound = null;
			Item.channel = true;
		}
		public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;
		public override Vector2? HoldoutOffset() {
			return new Vector2(-18, 0);
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			float itemRotation = drawPlayer.itemRotation;

			Vector2 pos = new((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y + drawInfo.mountOffSet));

			int frame = (int)((1 - (drawPlayer.itemAnimation + 1) / (float)drawPlayer.itemAnimationMax) * 12) % 12;

			Rectangle sourceRect = itemTexture.Frame(verticalFrames: 11, frameY: frame);
			DrawData data = new(
				itemTexture,
				pos,
				sourceRect,
				Item.GetAlpha(lightColor),
				itemRotation,
				new Vector2(31).Apply(drawInfo.itemEffect, sourceRect.Size()) + (HoldoutOffset().Value * drawPlayer.direction),
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect
			);
			drawInfo.DrawDataCache.Add(data);
			data.texture = TextureAssets.GlowMask[Item.glowMask].Value;
			data.color = Color.White;
			drawInfo.DrawDataCache.Add(data);
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item61.WithPitch(0.4f), player.itemLocation);
			return null;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = Item.shoot;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<NE8>(15)
			.AddIngredient<Sanguinite_Bar>(10)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return !Main.rand.NextBool(2);
		}
	}
	public class Clunkaclusta_P : ModProjectile, ICanisterProjectile, IIsExplodingProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingTexture outerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Outer";
		public static AutoLoadingTexture innerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Inner";
		public AutoLoadingTexture OuterTexture => outerTexture;
		public AutoLoadingTexture InnerTexture => innerTexture;
		public virtual bool SkipKillEffects => true;
		public ref float StoppedChanneling => ref Projectile.ai[0];
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 30;
			Origins.MagicTripwireDetonationStyle[Type] = 2;
		}
		public override void SetDefaults() {
			Origins.HomingEffectivenessMultiplier[Type] = 0.025f;
			Projectile.CloneDefaults(ProjectileID.ProximityMineI);
			Projectile.timeLeft = 30;
			Projectile.scale = 1.3f;
			Projectile.penetrate = 1;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.friendly = false;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			this.DoGravity(0.08f);
			Projectile.rotation += Projectile.velocity.X * 0.05f;
			if (player.channel && StoppedChanneling == 0) {
				Max(ref Projectile.timeLeft, 2);
				Max(ref player.itemAnimation, (int)((1 - 10f / 12f) * player.itemAnimationMax));
				Max(ref player.itemTime, (int)((1 - 10f / 12f) * player.itemTimeMax));

				player.itemRotation = (Projectile.Center - player.MountedCenter).ToRotation();
				if (Projectile.Center.X < player.MountedCenter.X)
					player.itemRotation += MathHelper.Pi;
				player.itemRotation = MathHelper.WrapAngle(player.itemRotation);
				player.direction = Math.Sign((Projectile.Center - player.MountedCenter).X);
				if (player.direction == 0) player.direction = 1;
			} else {
				StoppedChanneling = 1;
			}
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			float gravMult = Projectile.GetGlobalProjectile<CanisterGlobalProjectile>().gravityMultiplier;
			float bounce = 1f - gravMult * 0.4f;
			Vector2 friction = Vector2.One;
			if (Projectile.velocity.X != oldVelocity.X) {
				Projectile.velocity.X = oldVelocity.X * -bounce;
				friction.Y *= 0.9f;
			}
			if (Projectile.velocity.Y != oldVelocity.Y) {
				Projectile.velocity.Y = oldVelocity.Y * -bounce;
				friction.X *= 0.9f;
			}
			Projectile.velocity *= friction;
			return false;
		}
		public override void OnKill(int timeLeft) {
			CanisterGlobalProjectile.DefaultExplosion(Projectile, false, DustID.Torch, 96);
			int t = ModContent.ProjectileType<Clunkaclusta_Mini_P>();
			int count = 5 - Main.rand.Next(3);
			float rot = MathHelper.TwoPi / count;
			for (int i = count; i > 0; i--) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, OriginExtensions.Vec2FromPolar(5, rot * i) + Main.rand.NextVector2Unit() + Projectile.velocity, t, (int)(Projectile.damage / 3), Projectile.knockBack, Projectile.owner);
			}
		}
		public bool IsExploding => Projectile.timeLeft <= 0;
	}
	public class Clunkaclusta_Mini_P : Clunkaclusta_P {
		public override bool SkipKillEffects => false;
		public override void SetStaticDefaults() {
			//Origins.MagicTripwireRange[Type] = 32;
			Origins.MagicTripwireDetonationStyle[Type] = 2;
		}
		public override void SetDefaults() {
			base.SetDefaults();
			OriginsSets.Projectiles.HomingEffectivenessMultiplier[Type] = 0.25f;
			Projectile.timeLeft = 90;
			Projectile.scale = 0.65f;
			Projectile.friendly = true;
		}
		public override void AI() {
			this.DoGravity(0.2f);
			Projectile.rotation += Projectile.velocity.X * 0.03f;
		}
		public override void OnSpawn(IEntitySource source) {
			if (Projectile.TryGetGlobalProjectile(out ExplosiveGlobalProjectile global)) global.projectileBlastRadius *= 0.3f;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			float gravMult = Projectile.GetGlobalProjectile<CanisterGlobalProjectile>().gravityMultiplier;
			float bounce = 1f - gravMult * 0.3f;
			Vector2 friction = Vector2.One;
			if (Projectile.velocity.X != oldVelocity.X) {
				Projectile.velocity.X = oldVelocity.X * -bounce;
				friction.Y *= 0.9f;
			}
			if (Projectile.velocity.Y != oldVelocity.Y) {
				Projectile.velocity.Y = oldVelocity.Y * -bounce;
				friction.X *= 0.9f;
			}
			Projectile.velocity *= friction;
			Projectile.timeLeft--;
			return false;
		}
		public override void OnKill(int timeLeft) { }

	}
}
