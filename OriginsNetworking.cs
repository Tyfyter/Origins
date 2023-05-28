using Origins.Questing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Origins.Origins.NetMessageType;

namespace Origins {
	public partial class Origins : Mod {
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			byte type = reader.ReadByte();
			bool altHandle = false;
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				switch (type) {
					case tile_counts:
					OriginSystem.tDefiled = reader.ReadByte();
					break;

					case sync_player:
					case sync_quest:
					altHandle = true;
					break;

					default:
					Logger.Warn($"Invalid packet type ({type}) received on client");
					break;
				}
			} else if (Main.netMode == NetmodeID.Server) {
				switch (type) {
					case tile_counts:
					OriginSystem.tDefiled = reader.ReadByte();
					break;

					case sync_player:
					case sync_quest:
					altHandle = true;
					break;

					default:
					Logger.Warn($"Invalid packet type ({type}) received on server");
					break;
				}
			}

			if (altHandle) {
				switch (type) {
					case sync_player: {
						byte playerIndex = reader.ReadByte();
						OriginPlayer originPlayer = Main.player[playerIndex].GetModPlayer<OriginPlayer>();
						originPlayer.ReceivePlayerSync(reader);

						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the other clients
							originPlayer.SyncPlayer(-1, whoAmI, false);
						}
						break;
					}
					case sync_quest: {
						Quest quest = Quest_Registry.GetQuestByType(reader.ReadInt32());
						quest.ReceiveSync(reader);

						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the other clients
							quest.Sync(-1, whoAmI);
						}
						break;
					}
				}
			}
		}
		internal static class NetMessageType {
			internal const byte tile_counts = 0;
			internal const byte sync_player = 1;
			internal const byte sync_quest = 2;
		}
	}
	public partial class OriginPlayer : ModPlayer {
		bool netInitialized = false;
		void NetInit() {
			if (!netInitialized) {
				netInitialized = true;
				foreach (var quest in Quest_Registry.NetQuests) {
					quest.Sync(Player.whoAmI);
				}
			}
		}
		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer) {
			//return;
			if (Main.netMode == NetmodeID.SinglePlayer) return;
			if (Main.netMode == NetmodeID.Server) {
				NetInit();
			}
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
