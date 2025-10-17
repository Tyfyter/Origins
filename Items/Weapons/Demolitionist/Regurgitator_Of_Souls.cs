using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Weapons.Ammo.Canisters;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Weapons.Demolitionist {
	public class Regurgitator_Of_Souls : ModItem, ICustomDrawItem {
		public static Asset<Texture2D> UseTexture { get; private set; }
		public override void Unload() {
			UseTexture = null;
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				UseTexture = ModContent.Request<Texture2D>(Texture + "_Use");
			}
		}
		public override void SetDefaults() {
			Item.DefaultToCanisterLauncher<Regurgitator_Of_Souls_P>(26, 42, 14, 60, 24);
			Item.knockBack = 5.75f;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Blue;
			Item.UseSound = SoundID.Zombie24.WithPitchRange(0.6f, 1f);
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.RottenChunk, 10)
			.AddIngredient(ItemID.DemoniteBar, 6)
			.AddIngredient(ItemID.ShadowScale, 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			if (drawPlayer.HandPosition is not Vector2 pos) return;
			pos = (pos - Main.screenPosition).Floor();
			float itemRotation = drawPlayer.itemRotation;

			Rectangle frame = UseTexture.Frame(verticalFrames: 6, frameY: 6 - (drawPlayer.itemAnimation * 6) / drawPlayer.itemAnimationMax);
			drawInfo.DrawDataCache.Add(new DrawData(
				UseTexture.Value,
				pos,
				frame,
				Item.GetAlpha(lightColor),
				itemRotation,
				new Vector2(14, 26).Apply(drawInfo.itemEffect, frame.Size()),//drawInfo.itemEffect.ApplyToOrigin(new(17, 14), frame),
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect
			));
		}
	}
	public class Regurgitator_Of_Souls_P : ModProjectile, ICanisterProjectile {
		public override string Texture => "Terraria/Images/Item_1";
		public static AutoLoadingAsset<Texture2D> outerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Outer";
		public static AutoLoadingAsset<Texture2D> innerTexture = ICanisterProjectile.base_texture_path + "Resizable_Mine_Inner";
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
		public override void AI() {
			if (Main.rand.NextBool(3)) Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Corruption);
			if (Projectile.ai[0] == 0 && Projectile.TryGetOwner(out Player player) && (player.itemAnimation * 6) / player.itemAnimationMax > 2) {
				Projectile.Center = (player.HandPosition ?? player.MountedCenter) + (Projectile.velocity.SafeNormalize(default) * 26) - Projectile.velocity;
			} else {
				Projectile.ai[0] = 1;
				this.DoGravity(0.2f);
				Projectile.rotation += Projectile.velocity.X * 0.05f;
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(Shadefire_Debuff.ID, Main.rand.Next(180, 211));
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(Shadefire_Debuff.ID, Main.rand.Next(180, 211));
			Projectile.penetrate--;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.ai[0] == 0 ? false : null;
	}
}
