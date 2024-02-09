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
					OriginSystem.tRiven = reader.ReadByte();
					break;

					case sync_player:
					case sync_quest:
					case sync_peat:
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
					OriginSystem.tRiven = reader.ReadByte();
					break;

					case sync_player:
					case sync_quest:
					case sync_peat:
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
					case sync_peat: {
						OriginSystem.Instance.peatSold = reader.ReadInt16();

						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the other clients
							ModPacket packet = GetPacket();
							packet.Write(Origins.NetMessageType.sync_peat);
							packet.Write((short)OriginSystem.Instance.peatSold);
							packet.Send(-1, whoAmI);
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
			internal const byte sync_peat = 3;
		}
	}
}
