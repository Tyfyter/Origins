using Origins.Buffs;
using Origins.Core;
using Origins.Questing;
using Origins.Tiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.PlayerSyncDatas;
using static Origins.PlayerVisualSyncDatas;

namespace Origins {
	public partial class OriginPlayer : ModPlayer {
		bool dummyInitialize = false;
		bool netInitialized = false;
		void NetInit() {
			if (guid == Guid.Empty) guid = Guid.NewGuid();
			if (Main.netMode == NetmodeID.Server) {
				if (!dummyInitialize) {
					Mod.Logger.Info($"FakeInit {netInitialized}, {Player.name}");
					dummyInitialize = true;
					return;
				}
				if (!netInitialized) {
					Mod.Logger.Info($"NetInit {netInitialized}, {Player.name}");
					netInitialized = true;
					ModPacket packet = Mod.GetPacket();
					packet.Write(Origins.NetMessageType.sync_peat);
					packet.Write((short)OriginSystem.Instance.peatSold);
					packet.Send(Player.whoAmI);
					TESystem.SyncAllToPlayer(Player.whoAmI);
					
					packet = Mod.GetPacket();
					packet.Write(Origins.NetMessageType.sync_void_locks);
					packet.Write((ushort)OriginSystem.Instance.VoidLocks.Count);
					foreach (KeyValuePair<Point, Guid> @lock in OriginSystem.Instance.VoidLocks) {
						packet.Write(@lock.Key.X);
						packet.Write(@lock.Key.Y);
						packet.Write(@lock.Value.ToByteArray());
					}
					packet.Send(Player.whoAmI);
					foreach (Quest quest in Quest_Registry.NetQuests) {
						quest.Sync(Player.whoAmI);
					}
					SpecialChest.SyncToPlayer(Player.whoAmI);
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
				if (!netInitialized) {
					Mod.Logger.Info($"Client NetInit {netInitialized}, {Player.name}");
					netInitialized = true;
					ModPacket packet = Origins.instance.GetPacket();
					packet.Write(Origins.NetMessageType.sync_guid);
					packet.Write((byte)Player.whoAmI);
					packet.Write(guid.ToByteArray());
					packet.Send(-1, Main.myPlayer);
				}
			}
		}
		public bool CheckAssimilationDesync(OriginPlayer clone) {
			const float significant_threshold = 0.01f;
			foreach (AssimilationInfo info in IterateAssimilation()) {
				if (Math.Abs(clone.GetAssimilation(info.Type.AssimilationType).Percent - info.Percent) > significant_threshold) return true;
			}
			return false;
		}
		public override void CopyClientState(ModPlayer targetCopy) {
			OriginPlayer clone = (OriginPlayer)targetCopy;// shoot this one
			clone.mojoInjection = mojoInjection;
			clone.MojoInjectionEnabled = MojoInjectionEnabled;
			clone.crownJewel = crownJewel;
			clone.CrownJewelEnabled = CrownJewelEnabled;
			clone.quantumInjectors = quantumInjectors;
			clone.defiledWill = defiledWill;
			clone.tornTarget = tornTarget;
			clone.tornSeverityRate = tornSeverityRate;
			clone.ownedLargeGems = ownedLargeGems.ToList();
			if (CheckAssimilationDesync(clone) && !Player.HasBuff(Purifying_Buff.ID)) {
				foreach (AssimilationInfo info in IterateAssimilation()) {
					clone.GetAssimilation(info.Type.AssimilationType).Percent = info.Percent;
				}
			}
			clone.blastSetActive = blastSetActive;
		}
		public override void SendClientChanges(ModPlayer clientPlayer) {
			OriginPlayer clone = (OriginPlayer)clientPlayer;// shoot this one
			PlayerSyncDatas syncDatas = 0;
			PlayerVisualSyncDatas visualSyncDatas = 0;
			if (clone.mojoInjection != mojoInjection) syncDatas |= MojoInjection;
			if (clone.MojoInjectionEnabled != MojoInjectionEnabled) syncDatas |= MojoInjectionToggled;

			if (clone.crownJewel != crownJewel) syncDatas |= CrownJewel;
			if (clone.CrownJewelEnabled != CrownJewelEnabled) syncDatas |= CrownJewelToggled;

			if (clone.quantumInjectors != quantumInjectors) syncDatas |= QuantumInjectors;

			if (clone.defiledWill != defiledWill) syncDatas |= DefiledWills;

			if (clone.tornTarget != tornTarget || clone.tornSeverityRate != tornSeverityRate) syncDatas |= Torn;

			if (CheckAssimilationDesync(clone) && !Player.HasBuff(Purifying_Buff.ID)) syncDatas |= Assimilation;

			if (clone.blastSetActive != blastSetActive) visualSyncDatas |= BlastSetActive;

			SyncPlayer(-1, Main.myPlayer, false, syncDatas, visualSyncDatas);
		}
		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			//return;
			if (Main.netMode == NetmodeID.SinglePlayer) return;
			NetInit();
			SyncPlayer(toWho, fromWho, newPlayer, (PlayerSyncDatas)ushort.MaxValue, (PlayerVisualSyncDatas)ushort.MaxValue);
			if (newPlayer) {
				ModPacket packet = Origins.instance.GetPacket();
				packet.Write(Origins.NetMessageType.sync_guid);
				packet.Write((byte)Player.whoAmI);
				packet.Write(guid.ToByteArray());
				packet.Send(toWho, fromWho);
			}
		}
		public void SyncPlayer(int toWho, int fromWho, bool newPlayer, PlayerSyncDatas syncDatas, PlayerVisualSyncDatas visualSyncDatas) {
			//return;
			if (Main.netMode == NetmodeID.SinglePlayer || (syncDatas == 0 && visualSyncDatas == 0)) return;
			ModPacket packet = Mod.GetPacket();
			packet.Write(Origins.NetMessageType.sync_player);
			packet.Write((byte)Player.whoAmI);
			packet.Write((ushort)syncDatas);
			if (syncDatas.HasFlag(MojoInjection)) packet.Write(mojoInjection);
			if (syncDatas.HasFlag(MojoInjectionToggled)) packet.Write(MojoInjectionEnabled);

			if (syncDatas.HasFlag(CrownJewel)) packet.Write(crownJewel);
			if (syncDatas.HasFlag(CrownJewelToggled)) packet.Write(CrownJewelEnabled);

			if (syncDatas.HasFlag(QuantumInjectors)) packet.Write((byte)quantumInjectors);

			if (syncDatas.HasFlag(DefiledWills)) packet.Write((byte)defiledWill);
			if (syncDatas.HasFlag(Torn)) {
				packet.Write(tornTarget);
				packet.Write(tornSeverityRate);
			}
			if (syncDatas.HasFlag(Assimilation)) { // by sending it with a precision of 1% we can put all of the assimilations in the 4 bytes one of them would take with full precision with very little inaccuracy
				foreach (AssimilationInfo info in IterateAssimilation()) {
					packet.Write((byte)(info.Percent * 100));
				}
			}

			packet.Write((ushort)visualSyncDatas);

			if (visualSyncDatas.HasFlag(BlastSetActive)) packet.Write(blastSetActive);

			packet.Send(toWho, fromWho);
		}
		public void ReceivePlayerSync(BinaryReader reader) {
			PlayerSyncDatas syncDatas = (PlayerSyncDatas)reader.ReadUInt16();
			if (syncDatas.HasFlag(MojoInjection)) mojoInjection = reader.ReadBoolean();
			if (syncDatas.HasFlag(MojoInjectionToggled)) MojoInjectionEnabled = reader.ReadBoolean();

			if (syncDatas.HasFlag(CrownJewel)) crownJewel = reader.ReadBoolean();
			if (syncDatas.HasFlag(CrownJewelToggled)) CrownJewelEnabled = reader.ReadBoolean();

			if (syncDatas.HasFlag(QuantumInjectors)) quantumInjectors = reader.ReadByte();

			if (syncDatas.HasFlag(DefiledWills)) defiledWill = reader.ReadByte();

			if (syncDatas.HasFlag(Torn)) {
				tornTarget = reader.ReadSingle();
				tornSeverityRate = reader.ReadSingle();
			}
			if (syncDatas.HasFlag(Assimilation)) {
				foreach (AssimilationInfo info in IterateAssimilation()) {
					info.Percent = reader.ReadByte() / 100f;
				}
			}

			PlayerVisualSyncDatas visualSyncDatas = (PlayerVisualSyncDatas)reader.ReadUInt16();
			if (visualSyncDatas.HasFlag(BlastSetActive)) blastSetActive = reader.ReadBoolean();
		}
	}
	[Flags]
	public enum PlayerSyncDatas : ushort {
		Assimilation		 = 0b0000000000000001,
		Torn				 = 0b0000000000000010,
		MojoInjectionToggled = 0b0000000000000100,
		CrownJewelToggled	 = 0b0000000000001000,
		CrownJewel			 = 0b0000000000010000,
		MojoInjection		 = 0b0000000000100000,
		DefiledWills		 = 0b0000000001000000,
		QuantumInjectors	 = 0b0000000010000000,
	}
	[Flags]
	public enum PlayerVisualSyncDatas : ushort {
		BlastSetActive   = 0b0000000000000001,
	}
}
