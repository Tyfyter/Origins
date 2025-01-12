using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Ranged {
	public class Neural_Network : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Gun"
		];
		public static int ID { get; set; }
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.Musket);
			Item.damage = 9;
			Item.crit = -4;
			Item.useAnimation = 21;
			Item.useTime = 21;
			Item.width = 86;
			Item.height = 22;
			Item.autoReuse = true;
		}
		public override Vector2? HoldoutOffset() => Vector2.Zero;
	}
	public class Neural_Network_Buff : ModBuff {
		public override string Texture => typeof(Neural_Network).GetDefaultTMLName();
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
			BuffID.Sets.TimeLeftDoesNotDecrease[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			if (player.HeldItem.type != Neural_Network.ID) {
				player.DelBuff(buffIndex--);
			} else {
				player.GetAttackSpeed(DamageClass.Ranged) += 0.05f * player.buffTime[buffIndex];
				player.GetDamage(DamageClass.Ranged).Base -= 0.13f * player.buffTime[buffIndex];
			}
		}
		public override bool ReApply(Player player, int time, int buffIndex) {
			ref int buffTime = ref player.buffTime[buffIndex];
			buffTime += time;
			if (buffTime > 50) buffTime = 50;
			Sync(player, time);
			return false;
		}
		public override void PostDraw(SpriteBatch spriteBatch, int buffIndex, BuffDrawParams drawParams) {
			Color color = new Color(new Vector4(Main.buffAlpha[buffIndex]));
			spriteBatch.DrawString(
				FontAssets.ItemStack.Value,
				Main.LocalPlayer.buffTime[buffIndex].ToString(),
				drawParams.TextPosition,
				color,
				0f,
				Vector2.Zero,
				0.8f,
				SpriteEffects.None,
			0f);
		}
		public static void SetTime(Player player, int time) {
			int type = ModContent.BuffType<Neural_Network_Buff>();
			int index = player.FindBuffIndex(type);
			if (time == 0) {
				if (index >= 0) player.ClearBuff(index);
			} else {
				if (index >= 0) {
					player.buffTime[index] = time;
				} else {
					player.AddBuff(type, time);
					return;
				}
			}
			Sync(player, time);
		}
		static void Sync(Player player, int time) {
			if (player.whoAmI == Main.myPlayer) {
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.custom_combat_text);
				packet.Write((byte)Main.myPlayer);
				packet.Write((byte)time);
				packet.Send(-1, Main.myPlayer);
			}
		}
	}
}
