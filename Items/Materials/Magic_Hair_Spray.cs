using PegasusLib.Networking;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Materials {
	public class Magic_Hair_Spray : MaterialItem {
		public override int ResearchUnlockCount => 1;
		public override int Value => Item.sellPrice(copper: 40);
		public override int Rare => ItemRarityID.Quest;
		public override bool Hardmode => false;
		public override void SetDefaults() {
			base.SetDefaults();
			Item.damage = -5;
			Item.ammo = AmmoID.Gel;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 5)
			.AddIngredient(ItemID.BottledWater, 5)
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ModContent.ItemType<Silicon_Bar>())
			.AddTile(TileID.Bottles)
			.Register();
		}
	}
	public record class Extend_Deuffs_Action(NPC NPC, int Amount) : SyncedAction {
		public Extend_Deuffs_Action() : this(default, default) { }
		public override SyncedAction NetReceive(BinaryReader reader) => this with {
			NPC = Main.npc[reader.ReadByte()],
			Amount = reader.ReadInt32()
		};
		public override void NetSend(BinaryWriter writer) {
			writer.Write((byte)NPC.whoAmI);
			writer.Write(Amount);
		}
		protected override void Perform() {
			for (int i = 0; i < NPC.buffTime.Length; i++) {
				if (NPC.buffTime[i] > 0 && NPC.buffTime[i] < 60 * 10) NPC.buffTime[i] += Amount;
			}
		}
	}
}