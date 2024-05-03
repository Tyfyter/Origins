using Origins.Items.Accessories;
using Origins.Questing;
using Origins.Tiles;
using Origins.Tiles.Other;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
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
					case world_cracker_hit:
					case sync_guid:
					altHandle = true;
					break;

					case win_lottery: {
						Item lotteryTicketItem = Main.LocalPlayer.GetModPlayer<OriginPlayer>().lotteryTicketItem;
						if (--lotteryTicketItem.stack <= 0) lotteryTicketItem.TurnToAir();
						break;
					}

					case pickle_lottery: {
						Item brineCloverItem = Main.LocalPlayer.GetModPlayer<OriginPlayer>().brineCloverItem;
						int prefix = brineCloverItem.prefix;
						brineCloverItem.SetDefaults((brineCloverItem.ModItem as Brine_Leafed_Clover)?.NextLowerTier ?? ItemID.None);
						brineCloverItem.Prefix(prefix);
						break;
					}

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
					case world_cracker_hit:
					case sync_guid:
					altHandle = true;
					break;

					case place_tile_entity:
					TESystem.Get(reader.ReadUInt16()).AddTileEntity(new(reader.ReadInt16(), reader.ReadInt16()));
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
					case world_cracker_hit: {
						NPC npc = Main.npc[reader.ReadUInt16()];
						NPC.HitInfo hit = new NPC.HitInfo() {
							SourceDamage = reader.ReadInt32(),
							Crit = reader.ReadBoolean(),
							HitDirection = reader.ReadInt32(),
							Knockback = reader.ReadSingle(),
						};
						int armorPenetration = reader.ReadInt32();
						NPCs.Riven.World_Cracker.World_Cracker_Head.DamageArmor(npc, hit, armorPenetration, fromNet:true);

						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the other clients
							ModPacket packet = Origins.instance.GetPacket();
							packet.Write(Origins.NetMessageType.world_cracker_hit);
							packet.Write((ushort)npc.whoAmI);
							packet.Write((int)hit.SourceDamage);
							packet.Write((bool)hit.Crit);
							packet.Write((int)hit.HitDirection);
							packet.Write((float)hit.Knockback);
							packet.Write((int)armorPenetration);
							packet.Send(-1, Main.myPlayer);
						}
						break;
					}
					case sync_guid: {
						OriginPlayer originPlayer = Main.player[Main.netMode == NetmodeID.Server ? whoAmI : reader.ReadByte()].GetModPlayer<OriginPlayer>();
						originPlayer.guid = new(reader.ReadBytes(16));
						// Forward the changes to the other clients
						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the other clients
							ModPacket packet = Origins.instance.GetPacket();
							packet.Write(Origins.NetMessageType.sync_guid);
							packet.Write((byte)whoAmI);
							packet.Write(originPlayer.guid.ToByteArray());
							packet.Send(-1, Main.myPlayer);
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
			internal const byte world_cracker_hit = 4;
			internal const byte sync_guid = 5;
			internal const byte win_lottery = 6;
			internal const byte pickle_lottery = 7;
			internal const byte place_tile_entity = 8;
		}
	}
}
