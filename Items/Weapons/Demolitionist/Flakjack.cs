using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Flakjack : ModItem, ICustomDrawItem {
		public static AutoCastingAsset<Texture2D> UseTexture { get; private set; }
		public override void Unload() {
			UseTexture = null;
		}
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Flakjack");
			if (!Main.dedServ) {
				UseTexture = Mod.Assets.Request<Texture2D>("Items/Weapons/Demolitionist/Flakjack_Use");
			}
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SniperRifle);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 96;
			Item.crit = 14;
			Item.useAnimation = 32;
			Item.useTime = 16;
			Item.shoot = ModContent.ProjectileType<Dreikan_Shot>();
			Item.reuseDelay = 6;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = CrimsonRarity.ID;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == ProjectileID.Bullet) type = Item.shoot;
			SoundEngine.PlaySound(SoundID.Item40, position);
			SoundEngine.PlaySound(SoundID.Item36.WithVolume(0.75f), position);
			OriginGlobalProj.extraUpdatesNext = 2;
			Vector2 perp = velocity.RotatedBy(MathHelper.PiOver2).SafeNormalize(default);
			if (player.ItemUsesThisAnimation == 1) {
				position += perp * player.direction * 2;
			} else {
				position -= perp * player.direction * 6;
			}
		}

		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			float itemRotation = drawPlayer.itemRotation;

			Vector2 pos = new Vector2((int)(drawInfo.ItemLocation.X - Main.screenPosition.X + itemCenter.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

			int frame = 0;
			int useFrame = drawPlayer.itemTimeMax - drawPlayer.itemTime;
			switch (drawPlayer.ItemUsesThisAnimation) {
				case 1:
				if (useFrame < 3) {
					frame += 1;
				} else if (useFrame < 6) {
					frame += 2;
				} else {
					frame += 3;
				}
				break;

				case 2:
				frame = 3;
				goto case 1;
			}
			frame %= 6;

			drawInfo.DrawDataCache.Add(new DrawData(
				UseTexture,
				pos,
				new Rectangle(0, 30 * frame, 72, 28),
				Item.GetAlpha(lightColor),
				itemRotation,
				drawOrigin,
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect,
			0));
		}
	}
}
