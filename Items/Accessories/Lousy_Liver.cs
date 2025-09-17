using Origins.Buffs;
using Origins.Dev;
using Origins.Journal;
using PegasusLib.UI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Lousy_Liver : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string[] Categories => [
			"Combat",
			"RasterSource"
		];
		public string EntryName => "Origins/" + typeof(Lousy_Liver_Entry).Name;
		public class Lousy_Liver_Entry : JournalEntry {
			public override string TextKey => "Lousy_Liver";
			public override JournalSortIndex SortIndex => new("The_Defiled", 15);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 22);
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.lousyLiverCount = 3;
			originPlayer.lousyLiverDebuffs.Add((Lousy_Liver_Debuff.ID, 10));
		}
	}
	public class Lousy_Liver_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.GrantImmunityWith[Type] = [
				ModContent.BuffType<Rasterized_Debuff>()
			];
			Buff_Hint_Handler.ModifyTip(Type, 0, this.GetLocalization("EffectDescription").Key);
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			ref int rasterizedTime = ref npc.GetGlobalNPC<NPCs.OriginGlobalNPC>().rasterizedTime;
			if (rasterizedTime < 8) rasterizedTime = 8;
		}
	}
}
