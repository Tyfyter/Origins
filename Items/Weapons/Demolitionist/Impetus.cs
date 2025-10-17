using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo.Canisters;
using Origins.NPCs;
using Origins.World.BiomeData;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Misc.Physics;

namespace Origins.Items.Weapons.Demolitionist {
	public class Impetus : ModItem, ICustomDrawItem, ITornSource {
		public static float TornSeverity => 0.3f;
		float ITornSource.Severity => TornSeverity;
		AutoLoadingAsset<Texture2D> UseTexture = typeof(Impetus).GetDefaultTMLName() + "_Use";
		AutoLoadingAsset<Texture2D> UseGlowTexture = typeof(Impetus).GetDefaultTMLName() + "_Use_Glow";
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Impetus_P>(10, 24, 14, 60, 24);
			Item.reuseDelay = 8;
			Item.knockBack = 5.75f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Zombie24.WithPitchRange(0.6f, 1f);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Encrusted_Bar>(), 10)
			.AddIngredient(ModContent.ItemType<Riven_Carapace>(), 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			if (drawPlayer.HandPosition is not Vector2 pos) return;
			pos = (pos - Main.screenPosition).Floor();
			float itemRotation = drawPlayer.itemRotation;
			int frameNum = 8 - (drawPlayer.itemAnimation * 8) / drawPlayer.itemAnimationMax;
			if (drawPlayer.reuseDelay == 0) frameNum = 0;
			Rectangle frame = UseTexture.Frame(verticalFrames: 8, frameY: frameNum);
			drawInfo.DrawDataCache.Add(new DrawData(
				UseTexture,
				pos,
				frame,
				Item.GetAlpha(lightColor),
				itemRotation,
				new Vector2(14, 34).Apply(drawInfo.itemEffect, frame.Size()),//drawInfo.itemEffect.ApplyToOrigin(new(17, 14), frame),
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect
			));
			drawInfo.DrawDataCache.Add(new DrawData(
				UseGlowTexture,
				pos,
				frame,
				Riven_Hive.GetGlowAlpha(lightColor),
				itemRotation,
				new Vector2(14, 34).Apply(drawInfo.itemEffect, frame.Size()),//drawInfo.itemEffect.ApplyToOrigin(new(17, 14), frame),
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect
			));
		}
	}
	public class Impetus_P : ModProjectile, ICanisterProjectile {
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Terraria/Images/NPC_0";
		public static AutoLoadingAsset<Texture2D> innerTexture = typeof(Impetus_P).GetDefaultTMLName() + "_Inner";
		public AutoLoadingAsset<Texture2D> OuterTexture => outerTexture;
		public AutoLoadingAsset<Texture2D> InnerTexture => innerTexture;
		public override void SetStaticDefaults() {
			Origins.MagicTripwireRange[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Projectile.timeLeft = 420;
			Projectile.penetrate = 1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		public override bool ShouldUpdatePosition() => Projectile.ai[0] == 2;
		public override void AI() {
			if (Main.rand.NextBool(3)) Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Riven_Hive.DefaultTileDust);
			if (Projectile.ai[0] != 2 && Projectile.TryGetOwner(out Player player) && player.HandPosition is Vector2 handPosition) {
				Vector2 offset = new();
				Projectile.hide = false;
				switch (8 - (player.itemAnimation * 8) / player.itemAnimationMax) {
					case 0:
					Projectile.hide = true;
					break;
					case 1:
					offset = new(48, -16);
					Projectile.ai[0] = 1;
					break;
					case 2:
					offset = new(58, -16);
					Projectile.ai[0] = 1;
					break;
					case 3:
					offset = new(72, -16);
					Projectile.ai[0] = 1;
					break;
					case 4:
					offset = new(92, -16);
					Projectile.ai[0] = 1;
					break;
					default:
					Projectile.ai[0] = 2;
					return;
				}
				offset.X *= player.direction;
				offset.Y *= player.gravDir;
				Projectile.Center = handPosition + offset.RotatedBy(player.itemRotation);
			} else {
				Projectile.ai[0] = 2;
				this.DoGravity(0.08f);
				Projectile.rotation += Projectile.velocity.X * 0.03f;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 180, targetSeverity: Impetus.TornSeverity, source: Main.player[Projectile.owner].OriginPlayer());
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			OriginPlayer.InflictTorn(target, 180, targetSeverity: Impetus.TornSeverity);
			Projectile.penetrate--;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.ai[0] == 0 ? false : null;
		public void CustomDraw(Projectile projectile, CanisterData canisterData, Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 origin = texture.Size() * 0.5f;
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
				texture,
				projectile.Center - Main.screenPosition,
				null,
				lightColor,
				projectile.rotation,
				origin,
				projectile.scale,
				spriteEffects
			);
		}
		public override void OnKill(int timeLeft) {
			if (Projectile.owner != Main.myPlayer) return;
			int type = ModContent.ProjectileType<Impetus_Splut_P>();
			for (int i = 0; i < 2; i++) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					Projectile.oldVelocity.RotatedByRandom(MathHelper.PiOver4 * 2) * -0.5f,
					type,
					Projectile.damage / 2,
					Projectile.knockBack / 2
				);
			}
		}
	}
	public class Impetus_Splut_P : Impetus_P, ICanisterProjectile {
		public override string Texture => typeof(Impetus_P).GetDefaultTMLName();
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.scale = 0.85f;
		}
		public override bool ShouldUpdatePosition() => true;
		public override void AI() {
			if (Main.rand.NextBool(5)) Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Riven_Hive.DefaultTileDust);
			this.DoGravity(0.12f);
			Projectile.rotation += Projectile.velocity.X * 0.05f;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 180, targetSeverity: Impetus.TornSeverity * 2 / 3, source: Main.player[Projectile.owner].OriginPlayer());
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			OriginPlayer.InflictTorn(target, 180, targetSeverity: Impetus.TornSeverity * 2 / 3);
			Projectile.penetrate--;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => null;
		public override void OnKill(int timeLeft) {
			if (Projectile.owner != Main.myPlayer) return;
			int type = ModContent.ProjectileType<Impetus_Shrapnel>();
			for (int i = 0; i < 2; i++) {
				Projectile.NewProjectile(
					Projectile.GetSource_Death(),
					Projectile.Center,
					Projectile.oldVelocity.RotatedByRandom(MathHelper.PiOver4 * 2) * -0.5f,
					type,
					Projectile.damage / 2,
					Projectile.knockBack / 2
				);
			}
		}
	}
	public class Impetus_Shrapnel : Ameballoon_Shrapnel {
		public override void SetDefaults() {
			base.SetDefaults();
			Projectile.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			OriginGlobalNPC.InflictTorn(target, 180, targetSeverity: Impetus.TornSeverity / 3, source: Main.player[Projectile.owner].OriginPlayer());
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			OriginPlayer.InflictTorn(target, 180, targetSeverity: Impetus.TornSeverity / 3);
			Projectile.penetrate--;
		}
	}
}
