using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Origins.Items.Accessories;
using Origins.Questing;
using Origins.Tiles;
using Origins.Tiles.Other;
using System;
using System.IO;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Origins.Origins.NetMessageType;

namespace Origins {
	public partial class Origins : Mod {
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			byte type = reader.ReadByte();
			bool altHandle = false;
			lastPacketType = type;
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
					case inflict_assimilation:
					case start_laser_tag or laser_tag_hit or end_laser_tag or laser_tag_respawn:
					case custom_knockback:
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

					case add_void_lock:
					ModContent.GetInstance<OriginSystem>().TryAddVoidLock(new(reader.ReadInt32(), reader.ReadInt32()), new Guid(reader.ReadBytes(16)), fromNet: true);
					break;

					case sync_void_locks: {
						OriginSystem originSystem = ModContent.GetInstance<OriginSystem>();
						for (int i = reader.ReadUInt16(); i-- > 0;) {
							originSystem.TryAddVoidLock(new(reader.ReadInt32(), reader.ReadInt32()), new Guid(reader.ReadBytes(16)), fromNet: true);
						}
						break;
					}

					case remove_void_lock:
					ModContent.GetInstance<OriginSystem>().RemoveVoidLock(new(reader.ReadInt32(), reader.ReadInt32()), fromNet: true);
					break;

					case custom_combat_text:
					CombatText.NewText(
						new(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()),
						new Color() {
							PackedValue = reader.ReadUInt32()
						},
						reader.ReadInt32(),
						reader.ReadBoolean(),
						reader.ReadBoolean()
					);
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
					case world_cracker_hit:
					case sync_guid:
					case inflict_assimilation:
					case start_laser_tag or laser_tag_hit or end_laser_tag or laser_tag_respawn or laser_tag_score:
					case custom_knockback:
					altHandle = true;
					break;

					case place_tile_entity:
					TESystem.Get(reader.ReadUInt16()).AddTileEntity(new(reader.ReadInt16(), reader.ReadInt16()));
					break;

					case spawn_boss_on_player:
					Main.player[reader.ReadUInt16()].SpawnBossOn(reader.ReadInt32());
					break;

					case add_void_lock:
					ModContent.GetInstance<OriginSystem>().TryAddVoidLock(new(reader.ReadInt32(), reader.ReadInt32()), new Guid(reader.ReadBytes(16)), netOwner: whoAmI);
					break;

					case remove_void_lock:
					ModContent.GetInstance<OriginSystem>().RemoveVoidLock(new(reader.ReadInt32(), reader.ReadInt32()), netOwner: whoAmI);
					break;

					case custom_combat_text:
					ModPacket packet = GetPacket();
					packet.Write(custom_combat_text);

					packet.Write(reader.ReadInt32());
					packet.Write(reader.ReadInt32());
					packet.Write(reader.ReadInt32());
					packet.Write(reader.ReadInt32());

					packet.Write(reader.ReadUInt32());

					packet.Write(reader.ReadInt32());

					packet.Write(reader.ReadBoolean());

					packet.Write(reader.ReadBoolean());
					packet.Send(-1, whoAmI);
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
							packet.Write(sync_peat);
							packet.Write((short)OriginSystem.Instance.peatSold);
							packet.Send(-1, whoAmI);
						}
						break;
					}
					case world_cracker_hit: {
						NPC npc = Main.npc[reader.ReadUInt16()];
						NPC.HitInfo hit = new() {
							SourceDamage = reader.ReadInt32(),
							Crit = reader.ReadBoolean(),
							HitDirection = reader.ReadInt32(),
							Knockback = reader.ReadSingle(),
						};
						int armorPenetration = reader.ReadInt32();
						NPCs.Riven.World_Cracker.World_Cracker_Head.DamageArmor(npc, hit, armorPenetration, fromNet:true);

						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the other clients
							ModPacket packet = GetPacket();
							packet.Write(world_cracker_hit);
							packet.Write((ushort)npc.whoAmI);
							packet.Write((int)hit.SourceDamage);
							packet.Write((bool)hit.Crit);
							packet.Write((int)hit.HitDirection);
							packet.Write((float)hit.Knockback);
							packet.Write((int)armorPenetration);
							packet.Send(-1, whoAmI);
						}
						break;
					}
					case sync_guid: {
						OriginPlayer originPlayer = Main.player[reader.ReadByte()].GetModPlayer<OriginPlayer>();
						originPlayer.guid = new(reader.ReadBytes(16));
						// Forward the changes to the other clients
						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the other clients
							ModPacket packet = GetPacket();
							packet.Write(sync_guid);
							packet.Write((byte)originPlayer.Player.whoAmI);
							packet.Write(originPlayer.guid.ToByteArray());
							packet.Send(-1, whoAmI);
						}
						break;
					}
					case inflict_assimilation:
					Main.player[reader.ReadByte()].OriginPlayer().InflictAssimilation(reader.ReadByte(), reader.ReadSingle());
					break;

					case start_laser_tag or end_laser_tag:
					bool startLaserTag = type == start_laser_tag;
					if (startLaserTag) {
						Laser_Tag_Console.LaserTagRules = Laser_Tag_Rules.Read(reader);
						Laser_Tag_Console.LaserTagTimeLeft = Laser_Tag_Console.LaserTagRules.Time;
					} else {
						Laser_Tag_Console.LaserTagTimeLeft = -1;
					}
					foreach (Player player in Main.ActivePlayers) {
						OriginPlayer originPlayer = player.OriginPlayer();
						if (originPlayer.laserTagVest) {
							if (startLaserTag) {
								originPlayer.laserTagVestActive = true;
								originPlayer.laserTagHP = Laser_Tag_Console.LaserTagRules.HP;
							} else {
								originPlayer.ResetLaserTag();
							}
							if (!Laser_Tag_Console.LaserTagRules.Teams) player.team = 0;
						}
					}
					if (Main.netMode == NetmodeID.Server) {
						// Forward the changes to the clients
						ModPacket packet = GetPacket();
						packet.Write(type);
						if (startLaserTag) Laser_Tag_Console.LaserTagRules.Write(packet);
						packet.Send();
					}
					for (int i = 0; i < Laser_Tag_Console.LaserTagTeamPoints.Length; i++) {
						Laser_Tag_Console.LaserTagTeamPoints[i] = 0;
					}
					break;
					case laser_tag_hit: {
						Player attacker = Main.player[reader.ReadByte()];
						byte target = reader.ReadByte();
						if (Laser_Tag_Console.LaserTagRules.Teams) {
							Laser_Tag_Console.LaserTagTeamHits[attacker.team]++;
						}
						attacker.OriginPlayer().laserTagHits++;
						if (Laser_Tag_Console.LaserTagRules.HitsArePoints) Laser_Tag_Console.ScorePoint(attacker, true);
						OriginPlayer originTarget = Main.player[target].OriginPlayer();
						if (--originTarget.laserTagHP <= 0) {
							originTarget.laserTagVestActive = false;
							originTarget.laserTagRespawnDelay = Laser_Tag_Console.LaserTagRules.RespawnTime;
							if (target == Main.myPlayer) {
								Player targetPlayer = Main.player[target];
								for (int i = 0; i < targetPlayer.inventory.Length; i++) {
									if (targetPlayer.inventory[i].type is >= ItemID.LargeAmethyst and <= ItemID.LargeDiamond) targetPlayer.DropItem(targetPlayer.GetSource_Death(), targetPlayer.MountedCenter, ref targetPlayer.inventory[i]);
								}
							}
						}
						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the clients
							ModPacket packet = GetPacket();
							packet.Write(laser_tag_hit);
							packet.Write((byte)attacker.whoAmI);
							packet.Write(target);
							packet.Send();
						}
						break;
					}
					case laser_tag_respawn: {
						byte player = reader.ReadByte();
						OriginPlayer originPlayer = Main.player[player].OriginPlayer();
						originPlayer.laserTagHP = Laser_Tag_Console.LaserTagRules.HP;
						originPlayer.laserTagVestActive = true;
						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the other clients
							ModPacket packet = GetPacket();
							packet.Write(laser_tag_respawn);
							packet.Write(player);
							packet.Send(-1, whoAmI);
						}
						break;
					}
					case laser_tag_score: {
						byte sender = reader.ReadByte();
						Laser_Tag_Console.ScorePoint(Main.player[sender], true, sender);
						break;
					}
					case custom_knockback: {
						Main.npc[reader.ReadInt32()].DoCustomKnockback(new(reader.ReadSingle(), reader.ReadSingle()), Main.netMode == NetmodeID.MultiplayerClient);
						break;
					}
				}
			}
			//if (reader.BaseStream.Position != reader.BaseStream.Length) Logger.Warn($"Bad read flow (+{reader.BaseStream.Position - reader.BaseStream.Length}) in packet type {type}");
		}
		internal static byte lastPacketType;
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
			internal const byte spawn_boss_on_player = 9;
			internal const byte add_void_lock = 10;
			internal const byte remove_void_lock = 11;
			internal const byte inflict_assimilation = 12;
			internal const byte start_laser_tag = 13;
			internal const byte laser_tag_hit = 14;
			internal const byte end_laser_tag = 15;
			internal const byte laser_tag_respawn = 16;
			internal const byte laser_tag_score = 17;
			internal const byte sync_void_locks = 18;
			internal const byte custom_knockback = 19;
			internal const byte custom_combat_text = 20;
		}
	}
}
