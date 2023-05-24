using Origins.Buffs;
using Origins.Journal;
using System;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Other.Consumables {
	public class Captured_Mojo : ModItem, IJournalEntryItem {
		public string IndicatorKey => "Mods.Origins.Journal.Indicator.Other";
		public string EntryName => "Origins/" + typeof(Captured_Mojo_Entry).Name;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Captured Mojo");
			Tooltip.SetDefault("Cleanses a small amount of assimilation and provides temporary immunity to assimilation's effects\n'Extremely refreshing for the body and soul'");
			SacrificeTotal = 20;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WrathPotion);
			Item.buffType = Captured_Mojo_Buff.ID;
			Item.buffTime = 60 * 30;
			Item.value = Item.sellPrice(copper: 60);
			Item.maxStack = 99;
		}
	}
	public class Captured_Mojo_Buff : ModBuff {
		public override string Texture => "Origins/Buffs/Purification_Buff";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Captured Mojo");
			Description.SetDefault("Refreshing!");
			ID = Type;
		}
		public override void Update(Player player, ref int buffIndex) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.corruptionAssimilationDebuffMult = 0;
			originPlayer.crimsonAssimilationDebuffMult = 0;
			originPlayer.defiledAssimilationDebuffMult = 0;
			originPlayer.rivenAssimilationDebuffMult = 0;

			originPlayer.CorruptionAssimilation -= Math.Min(0.00333f, originPlayer.CorruptionAssimilation);
			originPlayer.CrimsonAssimilation -= Math.Min(0.00333f, originPlayer.CrimsonAssimilation);
			originPlayer.DefiledAssimilation -= Math.Min(0.00333f, originPlayer.DefiledAssimilation);
			originPlayer.RivenAssimilation -= Math.Min(0.00333f, originPlayer.RivenAssimilation);
		}
	}
}
public class Captured_Mojo_Entry : JournalEntry {
	public override string TextKey => "Captured_Mojo";
	public override ArmorShaderData TextShader => null;
}
