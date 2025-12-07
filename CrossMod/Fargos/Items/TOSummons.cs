using Origins.NPCs.Ashen;
using Origins.NPCs.Defiled;
using Origins.NPCs.Riven;
using PegasusLib.Networking;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Items {
	public abstract class TOSummons<TSummon> : ModItem where TSummon : ModNPC {
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("Fargowiltas");
		public override void SetStaticDefaults() {
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
	public class Defiled_Chest : TOSummons<Defiled_Mimic> { }
	public class Riven_Chest : TOSummons<Riven_Mimic> { }
	public class Suspicious_Trash_Compactor : TOSummons<Trash_Compactor_Mimic> {
		public override string Texture => typeof(Defiled_Chest).GetDefaultTMLName();
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
			Player player = Main.player[PlayerID];
			NPC.NewNPCDirect(NPC.GetBossSpawnSource(PlayerID), Pos, Type);
		}
	}
}
