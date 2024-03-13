using Origins.Questing;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		bool dummyInitialize = false;
		bool netInitialized = false;
		void NetInit() {
			if (Main.netMode == NetmodeID.Server) {
				if (!dummyInitialize) {
					Mod.Logger.Info($"FakeInit {netInitialized}, {Player.name}");
					dummyInitialize = true;
					return;
				}
				Mod.Logger.Info($"NetInit {netInitialized}, {Player.name}");
				if (!netInitialized) {
					netInitialized = true;
					ModPacket packet = Mod.GetPacket();
					packet.Write(Origins.NetMessageType.sync_peat);
					packet.Write((short)OriginSystem.Instance.peatSold);
					packet.Send(Player.whoAmI);
					foreach (var quest in Quest_Registry.NetQuests) {
						quest.Sync(Player.whoAmI);
					}
				}
			} else {
				if (!dummyInitialize) {
					Mod.Logger.Info($"Client FakeInit {netInitialized}, {Player.name}");
					dummyInitialize = true;
					ModPacket packet = Origins.instance.GetPacket();
					packet.Write(Origins.NetMessageType.sync_guid);
					packet.Write((byte)Player.whoAmI);
					packet.Write(guid.ToByteArray());
					packet.Send(-1, Main.myPlayer);
					return;
				}
				Mod.Logger.Info($"Client NetInit {netInitialized}, {Player.name}");
				if (!netInitialized) {
					netInitialized = true;
					ModPacket packet = Origins.instance.GetPacket();
					packet.Write(Origins.NetMessageType.sync_guid);
					packet.Write((byte)Player.whoAmI);
					packet.Write(guid.ToByteArray());
					packet.Send(-1, Main.myPlayer);
				}
			}
		}
		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			//return;
			if (Main.netMode == NetmodeID.SinglePlayer) return;
			NetInit();
			ModPacket packet = Mod.GetPacket();
			packet.Write(Origins.NetMessageType.sync_player);
			packet.Write((byte)Player.whoAmI);
			packet.Write((byte)quantumInjectors);
			packet.Write((byte)defiledWill);
			packet.Send(toWho, fromWho);
		}
		public void ReceivePlayerSync(BinaryReader reader) {
			quantumInjectors = reader.ReadByte();
			defiledWill = reader.ReadByte();
		}
	}
}
