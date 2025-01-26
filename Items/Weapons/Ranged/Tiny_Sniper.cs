using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
	public class Tiny_Sniper : ModItem, ICustomWikiStat {
        public string[] Categories => [
            "Gun"
        ];
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SniperRifle);
			Item.damage = 96;
			Item.crit = 36;
			Item.useAnimation = 33;
			Item.useTime = 33;
			Item.width = 34;
			Item.height = 12;
			Item.scale = 0.5f;
			Item.holdStyle = ItemHoldStyleID.HoldHeavy;
			Item.value = Item.sellPrice(gold: 5);
		}
		public override void HoldItemFrame(Player player) {
			if (player.scope && (PlayerInput.UsingGamepad ? (PlayerInput.GamepadThumbstickRight.Length() != 0f || !Main.SmartCursorIsUsed) : Main.mouseRight)) {
				Vector2 diff = (Main.MouseWorld - (player.MountedCenter + player.headPosition)).SafeNormalize(Vector2.Zero);
				float rot = diff.ToRotation();
				player.direction = System.Math.Sign(diff.X);
				player.itemRotation = rot - MathHelper.PiOver2 + MathHelper.PiOver2 * player.direction;
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters, rot - MathHelper.PiOver2);
				player.itemLocation -= new Vector2(0, 4 * player.direction).RotatedBy(rot);
			}
		}
		static Player.CompositeArmStretchAmount getStretchAmountForAnimationTime(float time) => time switch {
			< 1f and > 0.4f => Player.CompositeArmStretchAmount.None,
			< 0.95f and > 0.2f => Player.CompositeArmStretchAmount.Quarter,
			_ => Player.CompositeArmStretchAmount.ThreeQuarters
		};
		public override void UseItemFrame(Player player) {
			HoldItemFrame(player);
			if (player.compositeFrontArm.enabled) {
				Player.CompositeArmStretchAmount stretchAmount = getStretchAmountForAnimationTime(player.itemAnimation / (float)player.itemAnimationMax);
				player.SetCompositeArmFront(true, stretchAmount, player.compositeFrontArm.rotation);
				switch (stretchAmount) {
					case Player.CompositeArmStretchAmount.None:
					player.itemLocation -= new Vector2(0, 4).RotatedBy(player.compositeFrontArm.rotation);
					break;
					case Player.CompositeArmStretchAmount.Quarter:
					player.itemLocation -= new Vector2(0, 2).RotatedBy(player.compositeFrontArm.rotation);
					break;
				}
			}
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Item36.WithVolume(0.75f), player.itemLocation);
			return null;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == ProjectileID.Bullet) type = ProjectileID.BulletHighVelocity;
			float i0 = CombinedHooks.TotalUseAnimationMultiplier(player, Item);
			if (i0 != 1 && i0 == CombinedHooks.TotalUseTimeMultiplier(player, Item)) {
				Debugging.ChatOverhead("they fixed it", 30);
			}
			player.velocity -= velocity * 0.2f;
		}
	}
}
