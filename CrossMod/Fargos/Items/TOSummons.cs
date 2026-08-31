using Fargowiltas.Items.Summons.Deviantt;
using Fargowiltas.Projectiles;
using Origins.NPCs.Ashen;
using Origins.NPCs.Ashen.Boss;
using Origins.NPCs.Defiled;
using Origins.NPCs.Dungeon;
using Origins.NPCs.MiscE;
using Origins.NPCs.Riven;
using PegasusLib.Networking;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Items {
	#region Base Classes
	public abstract class TOSummons<TSummon> : ModItem where TSummon : ModNPC {
		public abstract int SortingPriority { get; }
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("Fargowiltas");
		public override void SetStaticDefaults() {
			ItemID.Sets.SortingPriorityBossSpawns[Type] = SortingPriority;
			Item.ResearchUnlockCount = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.Blue;
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Roar, player.Center);
			Vector2 pos = new(player.Center.X + Main.rand.NextFloat(-800, 800), player.Center.Y + Main.rand.NextFloat(-800, -250));
			new TOSummons_Action(player.whoAmI, ModContent.NPCType<TSummon>(), pos).Perform();
			return true;
		}
	}

	public record class TOSummons_Action(int PlayerID, int Type, Vector2 Pos) : SyncedAction {
		public override bool ServerOnly => true;
		public TOSummons_Action() : this(default, default, default) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			PlayerID = reader.ReadInt16(),
			Type = reader.ReadInt32(),
			Pos = reader.ReadPackedVector2()
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write(PlayerID);
			writer.Write(Type);
			writer.WritePackedVector2(Pos);
		}
		protected override void Perform() {
			NPC.NewNPCDirect(NPC.GetBossSpawnSource(PlayerID), Pos, Type);
			ChatHelper.BroadcastChatMessage(Language.GetText("Announcement.HasAwoken").ToNetworkText(ModContent.GetModNPC(Type).DisplayName.Value), new Color(175, 75, 255));
		}
	}
	#endregion

	#region Rare Creatures
	public class Defiled_Chest : TOSummons<Defiled_Mimic> {
		public override int SortingPriority => 6;
	}
	public class Riven_Chest : TOSummons<Riven_Mimic> {
		public override int SortingPriority => 6;
	}
	public class Suspicious_Trash_Compactor : TOSummons<Trash_Compactor_Mimic> {
		public override int SortingPriority => 6;
	}
	public class High_Powered_Green_Laser : TOSummons<Fearmaker>, IBroken {
		public static string BrokenReason => "change from fearmaker to HA-24";
		public override int SortingPriority => 5;
	}
	public class Mech_Figurine : TOSummons<Fearmaker>, IBroken {
		public static string BrokenReason => "change from fearmaker to D2L2";
		public override int SortingPriority => 5;
	}
	#endregion
	#region Bosses
	#endregion

	public class ShimmerFargoItemToNPC : GlobalItem {
		public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (item.type == ModContent.ItemType<AmalgamatedSpirit>()) {
				Vector2 pos = new((int)player.position.X + Main.rand.Next(-800, 800), (int)player.position.Y + Main.rand.Next(-1000, -250));
				Projectile.NewProjectile(player.GetSource_ItemUse(source.Item), pos, Vector2.Zero, ModContent.ProjectileType<SpawnProj>(), 0, 0, Main.myPlayer, ModContent.NPCType<Etherealizer>());
			}
			return true;
		}
		public override bool CanShoot(Item item, Player player) {
			if (item.type == ModContent.ItemType<HeartChocolate>()) {
				return !player.ZoneShimmer;
			}
			return true;
		}
		public override bool CanUseItem(Item item, Player player) {
			if (item.type == ModContent.ItemType<HeartChocolate>()) return ModContent.GetInstance<HeartChocolate>().CanUseItem(player) || player.ZoneShimmer;
			return true;
		}
		public override bool? UseItem(Item item, Player player) {
			if (item.type == ModContent.ItemType<HeartChocolate>() && player.ZoneShimmer) {
				SoundEngine.PlaySound(SoundID.Roar, player.Center);
				Vector2 pos = new(player.Center.X + Main.rand.NextFloat(-800, 800), player.Center.Y + Main.rand.NextFloat(-800, -250));
				new TOSummons_Action(player.whoAmI, ModContent.NPCType<Fae_Nymph>(), pos).Perform();
				return true;
			}
			return null;
		}
		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
			if (item.type == ModContent.ItemType<HeartChocolate>()) {
				tooltips.Insert("Tooltip0", "Mods.Origins.CrossMod.Fargos.Items.HeartChocolate.Tooltip", "SummonExtraTooltip");
			}
		}
		public override void Update(Item item, ref float gravity, ref float maxFallSpeed) {
			if (!NetmodeActive.MultiplayerClient && item.shimmerWet && !item.shimmered) {
				int npcToSummon = 0;
				Func<bool> conditionToSummon = () => true;
				bool useWholeStack = true;
				bool hasShimmered = false;
				float dist = float.PositiveInfinity;
				int index = Main.maxPlayers;
				foreach (Player player in Main.ActivePlayers) {
					if (!player.dead && Minimize(ref dist, player.DistanceSQ(item.Center))) index = player.whoAmI;
				}
				IEntitySource source = NPC.GetBossSpawnSource(index);

				if (item.type == ModContent.ItemType<HeartChocolate>()) {
					npcToSummon = ModContent.NPCType<Fae_Nymph>();
				}

				bool SpawnNPC() {
					if (!conditionToSummon() || NPC.NewNPCDirect(source, item.Center, npcToSummon).whoAmI == Main.maxNPCs) return true;
					SoundEngine.PlaySound(SoundID.Roar, item.Center);
					ChatHelper.BroadcastChatMessage(Language.GetText("Announcement.HasAwoken").ToNetworkText(ModContent.GetModNPC(npcToSummon).DisplayName.Value), new Color(175, 75, 255));
					item.stack--;
					if (item.stack <= 0) item.active = false;
					return false;
				}
				if (conditionToSummon()) {
					item.shimmerTime += 0.02f;
					if (item.shimmerTime > 0.9f) {
						item.shimmerTime = 0.9f;
						if (useWholeStack) {
							for (int i = Math.Min(item.stack, Main.maxNPCs); i > 0; i--) {
								if (SpawnNPC()) {
									hasShimmered = true;
									break;
								}
							}
						} else hasShimmered = SpawnNPC();
					}
				}
				if (item.active && hasShimmered) {
					item.shimmered = true;
				}
			}
		}
	}
}
