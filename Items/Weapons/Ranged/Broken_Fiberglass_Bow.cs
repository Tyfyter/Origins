using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

using Origins.Dev;
using System;
namespace Origins.Items.Weapons.Ranged {
	public class Broken_Fiberglass_Bow : AnimatedModItem, IElementalItem {
		public ushort Element => Elements.Fiberglass;
		protected override bool CloneNewInstances => true;
		int strung = 0;
		const int strungMax = 50;
		static DrawAnimationManual animation;
		public override DrawAnimation Animation {
			get {
				animation.Frame = strung > 0 ? 1 : 0;
				return animation;
			}
		}
		public override void SetStaticDefaults() {
			animation = new DrawAnimationManual(2);
			Main.RegisterItemAnimation(Item.type, animation);
			ItemID.Sets.SkipsInitialUseSound[Type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
		}
		public override void SetDefaults() {
			Item.damage = 17;
			Item.DamageType = DamageClass.Ranged;
			Item.noMelee = true;
			Item.noUseGraphic = false;
			Item.width = 18;
			Item.height = 36;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 1;
			Item.shootSpeed = 9;
			Item.autoReuse = false;
			Item.useAmmo = AmmoID.Arrow;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.value = Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item5;
		}
		public override void LoadData(TagCompound tag) {
			strung = tag.GetInt("strung");
		}
		public override void SaveData(TagCompound tag) {
			tag.Add("strung", strung);
		}
		public override void HoldItem(Player player) {
			if (player.itemAnimation != 0) player.GetModPlayer<OriginPlayer>().itemLayerWrench = true;
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-0.5f, 0);
		}
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool CanUseItem(Player player) {
			if (player.altFunctionUse != 0) {
				if (strung >= strungMax) return false;
				if (player.ConsumeItem(ItemID.VineRope)) {
					strung += strung > 0 ? 2 : 1;
				} else if (strung < strungMax - 10 && player.ConsumeItem(ItemID.Vine)) {
					strung += 25;
				}
				if (strung > strungMax) strung = strungMax;
			}
			return strung > 0;
		}
		public override float UseSpeedMultiplier(Player player) => player.altFunctionUse == 0 ? 1 : 1.75f;
		public override void UseItemFrame(Player player) {
			if (player.ItemAnimationActive && player.altFunctionUse == 0) return;
			player.bodyFrame.Y = player.bodyFrame.Height * 3;
			player.itemRotation = 1f * player.direction;
			player.itemLocation.X = player.MountedCenter.X;
			player.itemLocation.Y = player.position.Y + player.HeightOffsetHitboxCenter;
			player.itemLocation += Main.OffsetsPlayerHeadgear[player.bodyFrame.Y / 56];
			float animationMax = player.itemAnimationMax;
			if (animationMax == 0f) animationMax = Item.useAnimation;

			float progress = player.itemAnimation / animationMax;
			if (!restringAnimation2) progress = 1 - progress;
			progress *= 2f;
			float wave = MathF.Cos(progress * MathHelper.PiOver2) * 0.2f;
			Player.CompositeArmStretchAmount stretchAmount = Player.CompositeArmStretchAmount.None;
			if (progress > 1.85f) {
				stretchAmount = Player.CompositeArmStretchAmount.Full;
			} else if (progress > 1.75f) {
				stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
			} else if (progress > 1.35f) {
				stretchAmount = Player.CompositeArmStretchAmount.ThreeQuarters;
			} else if (progress > 0.6f) {
				stretchAmount = Player.CompositeArmStretchAmount.Quarter;
			}


			player.SetCompositeArmFront(enabled: true, stretchAmount, (wave * 3 - MathHelper.PiOver4) * player.direction);
			player.SetCompositeArmBack(enabled: true, Player.CompositeArmStretchAmount.Quarter, -1 * player.direction);
			player.FlipItemLocationAndRotationForGravity();
			if (player.gravDir == -1) player.itemLocation.Y -= 40;
			if (!restringAnimation2) player.OriginPlayer().itemComboAnimationTime = 4;
		}
		bool restringAnimation2 = false;
		public override bool? UseItem(Player player) {
			if (player.altFunctionUse != 2) {
				SoundEngine.PlaySound(Item.UseSound, player.Center);
				if (strung > 0 && --strung <= 0) {
					SoundEngine.PlaySound(SoundID.Item102.WithPitch(1).WithVolume(0.75f), player.Center);
					Vector2 pos = player.Center + (Main.MouseWorld - player.Center).SafeNormalize(Vector2.Zero) * (10 - player.direction * 2);
					Gore.NewGoreDirect(player.GetSource_ItemUse(Item), pos, player.velocity, ModContent.GoreType<Gores.NPCs.FG2_Gore>()).position = pos;
				}
			} else if (player.ItemAnimationJustStarted) {
				restringAnimation2 = player.OriginPlayer().itemComboAnimationTime > 0;
			}
			return null;
		}
		public override bool CanShoot(Player player) => player.altFunctionUse != 2;
		public override bool CanConsumeAmmo(Item ammo, Player player) => player.altFunctionUse != 2;
		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			Texture2D texture = TextureAssets.Item[Item.type].Value;
			spriteBatch.Draw(texture, position, Animation.GetFrame(texture), drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
			return false;
		}
		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			if (Main.playerInventory) return;
			float inventoryScale = Main.inventoryScale * 0.9f;
			string strang = strung.ToString();
			ChatManager.DrawColorCodedStringWithShadow(
				spriteBatch,
				FontAssets.ItemStack.Value,
				strang,
				position + origin * scale * new Vector2(2f, -1.4f) - FontAssets.ItemStack.Value.MeasureString(strang) * Vector2.UnitX * inventoryScale,
				drawColor,
				0f,
				Vector2.Zero,
				new Vector2(inventoryScale),
				-1f,
				inventoryScale
			);
		}
	}
}
