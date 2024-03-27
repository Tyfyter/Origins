using Origins.Buffs;
using Origins.Questing;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.PlayerSyncDatas;

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
		public override void CopyClientState(ModPlayer targetCopy) {
			OriginPlayer clone = (OriginPlayer)targetCopy;// shoot this one
			clone.quantumInjectors = quantumInjectors;
			clone.defiledWill = defiledWill;
			clone.corruptionAssimilation = corruptionAssimilation;
			clone.crimsonAssimilation = crimsonAssimilation;
			clone.defiledAssimilation = defiledAssimilation;
			clone.rivenAssimilation = rivenAssimilation;
		}
		public override void SendClientChanges(ModPlayer clientPlayer) {
			OriginPlayer clone = (OriginPlayer)clientPlayer;// shoot this one
			PlayerSyncDatas syncDatas = None;
			if (clone.quantumInjectors != quantumInjectors) syncDatas |= QuantumInjectors;
			if (clone.defiledWill != defiledWill) syncDatas |= DefiledWills;

			if ((clone.corruptionAssimilation != corruptionAssimilation ||
				clone.crimsonAssimilation != crimsonAssimilation ||
				clone.defiledAssimilation != defiledAssimilation ||
				clone.rivenAssimilation != rivenAssimilation)
				&& !Player.HasBuff(Purifying_Buff.ID)
				) syncDatas |= Assimilation;

			SyncPlayer(-1, Main.myPlayer, false, syncDatas);
		}
		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			//return;
			if (Main.netMode == NetmodeID.SinglePlayer) return;
			NetInit();
			SyncPlayer(toWho, fromWho, newPlayer, (PlayerSyncDatas)ushort.MaxValue);
		}
		public void SyncPlayer(int toWho, int fromWho, bool newPlayer, PlayerSyncDatas syncDatas) {
			//return;
			if (Main.netMode == NetmodeID.SinglePlayer || syncDatas == None) return;
			ModPacket packet = Mod.GetPacket();
			packet.Write(Origins.NetMessageType.sync_player);
			packet.Write((byte)Player.whoAmI);
			packet.Write((ushort)syncDatas);
			if (syncDatas.HasFlag(QuantumInjectors)) packet.Write((byte)quantumInjectors);
			if (syncDatas.HasFlag(DefiledWills)) packet.Write((byte)defiledWill);
			if (syncDatas.HasFlag(Assimilation)) { // by sending it with a precision of 1% we can put all of the assimilations in the 4 bytes one of them would take with full precision with very little inaccuracy
				packet.Write((byte)(corruptionAssimilation * 100));
				packet.Write((byte)(crimsonAssimilation * 100));
				packet.Write((byte)(defiledAssimilation * 100));
				packet.Write((byte)(rivenAssimilation * 100));
			}
			packet.Send(toWho, fromWho);
		}
		public void ReceivePlayerSync(BinaryReader reader) {
			PlayerSyncDatas syncDatas = (PlayerSyncDatas)reader.ReadUInt16();
			if (syncDatas.HasFlag(QuantumInjectors)) quantumInjectors = reader.ReadByte();
			if (syncDatas.HasFlag(DefiledWills)) defiledWill = reader.ReadByte();
			if (syncDatas.HasFlag(Assimilation)) {
				corruptionAssimilation = reader.ReadByte() / 100f;
				crimsonAssimilation = reader.ReadByte() / 100f;
				defiledAssimilation = reader.ReadByte() / 100f;
				rivenAssimilation = reader.ReadByte() / 100f;
			}
		}
	}
	[Flags]
	public enum PlayerSyncDatas : ushort {
		None = 0b00000000,
		Assimilation = 0b00000001,
		QuantumInjectors = 0b10000000,
		DefiledWills = 0b01000000,
	}
}
