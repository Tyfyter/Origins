using Origins.Core;
using Origins.Dev;
using PegasusLib.Networking;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.HandsOn)]
	public class Resizing_Glove : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Combat",
			"MeleeBoostAcc"
		];
		public override void Load() {
			On_Player.ItemCheck_StartActualUse += On_Player_ItemCheck_StartActualUse;
		}
		static void On_Player_ItemCheck_StartActualUse(On_Player.orig_ItemCheck_StartActualUse orig, Player self, Item sItem) {
			orig(self, sItem);
			if (self.whoAmI != Main.myPlayer) return;
			OriginPlayer originPlayer = self.OriginPlayer();
			if (originPlayer.resizingGlove) {
				const float strength = 2f;
				new Resizing_Glove_Action(self, Math.Clamp(Main.rand.NextFloat(1 / strength, float.BitIncrement(strength)), 0.75f, 2)).Perform();
			}
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(22, 26);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().resizingGlove = true;
			player.autoReuseGlove = true;
		}
	}
	public record class Resizing_Glove_Action(Player Player, float Scale) : SyncedAction {
		public Resizing_Glove_Action() : this(default, default) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			Player = Main.player[reader.ReadByte()],
			Scale = reader.ReadSingle()
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)Player.whoAmI);
			writer.Write(Scale);
		}
		protected override void Perform() {
			Player.OriginPlayer().resizingGloveScale = Scale;
		}
	}
}
