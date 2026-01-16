using Origins.Core;
using Origins.CrossMod.Fargos.Items;
using Origins.Items.Accessories;
using Origins.Items.Armor.Chambersite;
using Origins.Items.Tools.Wiring;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Melee;
using Origins.Items.Weapons.Ranged;
using Origins.Questing;
using Origins.Reflection;
using Origins.Tiles;
using Origins.Tiles.Defiled;
using Origins.Tiles.Other;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;
using static Origins.Origins.NetMessageType;

namespace Origins {
	public partial class Origins : Mod {
		/*int[] packetTypesReceived = new int[NetMessageType.clone_npc + 1];
		uint lastLoggedGameFrameNumber = 0;*/
		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			byte type = reader.ReadByte();
			bool altHandle = false;
			lastPacketType = type;
			/*packetTypesReceived[type]++;
			if (Origins.gameFrameCount - lastLoggedGameFrameNumber > 10 * 60) {
				lastLoggedGameFrameNumber = Origins.gameFrameCount;
				if (NetMessageType.names is null) {
					NetMessageType.names = new string[NetMessageType.clone_npc + 1];
					foreach (FieldInfo item in typeof(NetMessageType).GetFields(BindingFlags.NonPublic | BindingFlags.Static)) {
						if (item.FieldType == typeof(byte)) {
							NetMessageType.names[(byte)item.GetValue(null)] = item.Name;
						}
					}
				}
				StringBuilder builder = new();
				for (int i = 0; i < packetTypesReceived.Length; i++) {
					if (packetTypesReceived[i] > 0) {
						builder.Append($"{NetMessageType.names[i]}:{packetTypesReceived[i]}");
					}
				}
				if (builder.Length > 0) Logger.Info(builder.ToString());
			}*/
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
					case start_laser_tag or laser_tag_hit or end_laser_tag or laser_tag_respawn or laser_tag_score:
					case custom_knockback:
					case entity_interaction:
					case soul_snatcher_activate:
					case shinedown_spawn_shadows:
					case sync_ashen_wires:
					altHandle = true;
					break;

					case place_tile_entity:
					TESystem.Get(reader.ReadUInt16()).tileEntityLocations.Add(new(reader.ReadInt16(), reader.ReadInt16()));
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
					ModContent.GetInstance<OriginSystem>().TryAddVoidLock(new(reader.ReadInt32(), reader.ReadInt32()), new Guid(reader.ReadBytes(16), false), fromNet: true);
					break;

					case sync_void_locks: {
						OriginSystem originSystem = ModContent.GetInstance<OriginSystem>();
						for (int i = reader.ReadUInt16(); i-- > 0;) {
							originSystem.TryAddVoidLock(new(reader.ReadInt32(), reader.ReadInt32()), new Guid(reader.ReadBytes(16), false), fromNet: true);
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

					case sync_neural_network: 
					Neural_Network_Buff.SetTime(Main.player[reader.ReadByte()], reader.ReadByte());
					break;

					case defiled_relay_message:
					Defiled_Relay.DisplayMessage(reader.ReadString(), true);
					break;

					case chest_sync or chest_sync_projectile: {
						ushort chestIndex = reader.ReadUInt16();
						if (Main.chest[chestIndex] == null) {
							Main.chest[chestIndex] = new Chest();
						}
						Chest chest = Main.chest[chestIndex];
						for (int i = 0; i < 40; i++) {
							chest.item[i] ??= new();
							ItemIO.Receive(chest.item[i], reader, readStack: true);
						}
						switch (type) {
							case chest_sync_projectile:
							Projectile lotus = Main.projectile[reader.ReadUInt16()];
							if (lotus.ModProjectile is IChestSyncRecipient recipient) recipient.ReceiveChestSync(chestIndex);
							break;
						}
					}
					break;

					case tyrfing_zap: {
						Tyrfing_P.DoArcVisual(reader.ReadVector2(), reader.ReadVector2());
						break;
					}

					case mass_teleport: {
						for (int i = reader.ReadUInt16(); i > 0; i--) {
							Player player = Main.player[reader.ReadUInt16()];
							Vector2 position = reader.ReadPackedVector2();
							ParticleOrchestrator.BroadcastOrRequestParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings {
								PositionInWorld = player.Bottom
							});
							player.Teleport(position, 12);
							ParticleOrchestrator.BroadcastOrRequestParticleSpawn(ParticleOrchestraType.ShimmerTownNPC, new ParticleOrchestraSettings {
								PositionInWorld = player.Bottom
							});
							player.velocity = Vector2.Zero;
						}
						break;
					}

					case sync_npc_interactions: {
						NPC npc = Main.npc[reader.ReadUInt16()];
						Utils.ReceiveBitArray(Main.maxPlayers + 1, reader).CopyTo(npc.playerInteraction, 0);
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
					case inflict_assimilation:
					case start_laser_tag or laser_tag_hit or end_laser_tag or laser_tag_respawn or laser_tag_score:
					case custom_knockback:
					case entity_interaction:
					case soul_snatcher_activate:
					case shinedown_spawn_shadows:
					case sync_ashen_wires:
					altHandle = true;
					break;

					case place_tile_entity:
					TESystem.Get(reader.ReadUInt16()).AddTileEntity(new(reader.ReadInt16(), reader.ReadInt16()));
					break;

					case spawn_boss_on_player:
					Main.player[reader.ReadUInt16()].SpawnBossOn(reader.ReadInt32());
					break;

					case add_void_lock:
					ModContent.GetInstance<OriginSystem>().TryAddVoidLock(new(reader.ReadInt32(), reader.ReadInt32()), new Guid(reader.ReadBytes(16), false), netOwner: whoAmI);
					break;

					case remove_void_lock:
					ModContent.GetInstance<OriginSystem>().RemoveVoidLock(new(reader.ReadInt32(), reader.ReadInt32()), netOwner: whoAmI);
					break;

					case custom_combat_text: {
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
					}
					break;

					case sync_neural_network: {
						ModPacket packet = GetPacket();
						packet.Write(sync_neural_network);
						packet.Write(reader.ReadByte());
						packet.Write(reader.ReadByte());
						packet.Send(-1, whoAmI);
					}
					break;

					case defiled_relay_message: {
						ModPacket packet = GetPacket();
						packet.Write(defiled_relay_message);
						packet.Write(reader.ReadString());
						packet.Send(-1, whoAmI);
					}
					break;

					case request_chest_sync or request_chest_sync_projectile: {
						ushort chestIndex = reader.ReadUInt16();
						Chest chest = Main.chest[chestIndex];
						ModPacket packet = GetPacket();
						packet.Write(type);
						packet.Write(chestIndex);
						for (int i = 0; i < 40; i++) {
							ItemIO.Send(chest.item[i], packet, writeStack: true);
						}
						switch (type) {
							case request_chest_sync_projectile:
							packet.Write(reader.ReadUInt16());
							break;
						}
						packet.Send(whoAmI);
					}
					break;

					case tyrfing_zap: {
						ModPacket packet = GetPacket();
						packet.Write(type);
						packet.WriteVector2(reader.ReadVector2());
						packet.WriteVector2(reader.ReadVector2());
						packet.Send(ignoreClient: whoAmI);
						break;
					}

					case set_gem_lock: {
						short i = reader.ReadInt16();
						short j = reader.ReadInt16();
						bool on = reader.ReadBoolean();
						if (TileLoader.GetTile(Framing.GetTileSafely(i, j).TileType) is ModGemLock gemLock) gemLock.ToggleGemLock(i, j, on);
						break;
					}

					case clone_npc: {
						Vector2 position = reader.ReadPackedVector2();
						NPC npc = NPC.NewNPCDirect(Entity.GetSource_None(), position, reader.ReadInt32());
						npc.velocity = reader.ReadPackedVector2() * MathF.Pow(1.2f, npc.knockBackResist);
						npc.value = 0;
						npc.SpawnedFromStatue = true;
						SoundEngine.PlaySound(SoundID.Item2, position);
						break;
					}

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
						NPCs.Riven.World_Cracker.World_Cracker_Head.DamageArmor(npc, hit, armorPenetration, fromNet: true);

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
						originPlayer.guid = new(reader.ReadBytes(16), false);
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
					Main.player[reader.ReadByte()].OriginPlayer().InflictAssimilation(reader.ReadUInt16(), reader.ReadSingle());
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
						originPlayer.laserTagPoints = 0;
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
						Player target = Main.player[reader.ReadByte()];
						if (Laser_Tag_Console.LaserTagRules.Teams) {
							Laser_Tag_Console.LaserTagTeamHits[attacker.team]++;
						}
						attacker.OriginPlayer().laserTagHits++;
						if (Laser_Tag_Console.LaserTagRules.HitsArePoints) Laser_Tag_Console.ScorePoint(attacker, true);
						OriginPlayer originTarget = target.OriginPlayer();
						if (--originTarget.laserTagHP <= 0) {
							originTarget.laserTagVestActive = false;
							originTarget.laserTagRespawnDelay = Laser_Tag_Console.LaserTagRules.RespawnTime;
							if (target.whoAmI == Main.myPlayer) {
								for (int i = 0; i < target.inventory.Length; i++) {
									if (target.inventory[i].type is >= ItemID.LargeAmethyst and <= ItemID.LargeDiamond) target.TryDroppingSingleItem(target.GetSource_Death(), target.inventory[i]);
								}
							}
							if (Main.netMode != NetmodeID.Server) {
								SoundEngine.PlaySound(Sounds.LaserTag.Death, target.Center);
								ChatMessageContainerMethods.CreateCustomMessage(
									ChatManager.ParseMessage(attacker.name, Main.teamColor[attacker.team]),
									ChatManager.ParseMessage(" [i:Origins/Laser_Tag_Gun] ", Color.White),
									ChatManager.ParseMessage(target.name, Main.teamColor[target.team])
								);
							}
						} else {
							if (Main.netMode != NetmodeID.Server) {
								SoundEngine.PlaySound(Sounds.LaserTag.Hurt, target.Center);
							}
						}
						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the clients
							ModPacket packet = GetPacket();
							packet.Write(laser_tag_hit);
							packet.Write((byte)attacker.whoAmI);
							packet.Write((byte)target.whoAmI);
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
					case entity_interaction: {
						byte npcIndex = reader.ReadByte();
						if (Main.npc[npcIndex].ModNPC is IInteractableNPC npc) {
							npc.Interact();
							if (npc.NeedsSync && Main.netMode == NetmodeID.Server) {
								// Forward the changes to the other clients
								ModPacket packet = GetPacket();
								packet.Write(entity_interaction);
								packet.Write(npcIndex);
								packet.Send(-1, whoAmI);
							}
						}
						break;
					}
					case soul_snatcher_activate: {
						byte player = reader.ReadByte();
						OriginPlayer originPlayer = Main.player[player].OriginPlayer();
						originPlayer.soulSnatcherActive = reader.ReadBoolean();
						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the other clients
							ModPacket packet = instance.GetPacket();
							packet.Write(soul_snatcher_activate);
							packet.Write(player);
							packet.Write(originPlayer.soulSnatcherActive);
							packet.Send(ignoreClient: player);
						}
						break;
					}

					case shinedown_spawn_shadows: {
						byte owner = reader.ReadByte();
						ushort identity = reader.ReadUInt16();
						byte[] indices = reader.ReadBytes(reader.ReadByte());
						Shinedown_Staff_P shinedown = null;
						Chambersite_Commander_Sentinel sentinel = null;
						foreach (Projectile proj in Main.ActiveProjectiles) {
							if (proj.owner == owner && proj.identity == identity) {
								shinedown = proj.ModProjectile as Shinedown_Staff_P;
								sentinel = proj.ModProjectile as Chambersite_Commander_Sentinel;
								break;
							}
						}
						shinedown?.RecieveSync(indices);
						sentinel?.RecieveSync(indices);
						if (Main.netMode == NetmodeID.Server) {
							// Forward the changes to the other clients
							ModPacket packet = instance.GetPacket();
							packet.Write(shinedown_spawn_shadows);
							packet.Write(owner);
							packet.Write(identity);
							packet.Write((byte)indices.Length);
							packet.Write(indices);
							packet.Send(ignoreClient: owner);
						}
						break;
					}

					case sync_ashen_wires: {
						ushort i = reader.ReadUInt16();
						ushort j = reader.ReadUInt16();
						Main.tile[i, j].Get<Ashen_Wire_Data>().data = reader.ReadByte();

						if (Main.netMode == NetmodeID.Server) {
							Ashen_Wire_System.SendWireData(i, j, whoAmI);
						}
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
			internal const byte sync_neural_network = 21;
			internal const byte defiled_relay_message = 22;
			internal const byte entity_interaction = 23;
			internal const byte request_chest_sync = chest_sync;
			internal const byte chest_sync = 24;
			internal const byte request_chest_sync_projectile = chest_sync_projectile;
			internal const byte chest_sync_projectile = 25;
			internal const byte soul_snatcher_activate = 26;
			internal const byte tyrfing_zap = 27;
			internal const byte set_gem_lock = 28;
			internal const byte mass_teleport = 29;
			internal const byte shinedown_spawn_shadows = 30;
			internal const byte sync_npc_interactions = 31;
			internal const byte clone_npc = 32;
			internal const byte sync_ashen_wires = 33;
			internal const byte sync_special_chest = 34;

			//public static string[] names;
		}
	}
	public interface IChestSyncRecipient {
		public void ReceiveChestSync(int i);
	}
	public static class NetworkingExtensions {
		public static void RequestChestSync(this IChestSyncRecipient recipient, int chestIndex) {
			byte messageType;
			ushort recipientIndex;
			if (recipient is ModProjectile proj) {
				messageType = request_chest_sync_projectile;
				recipientIndex = (ushort)proj.Projectile.whoAmI;
			} else {
				throw new NotImplementedException();
			}
			ModPacket packet = Origins.instance.GetPacket();
			packet.Write(messageType);
			packet.Write((ushort)chestIndex);
			packet.Write(recipientIndex);
			packet.Send();
		}
	}
}
