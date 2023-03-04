﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Lousy_Liver : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Lousy Liver");
			Tooltip.SetDefault("Weakens up to 4 nearby enemies");
			SacrificeTotal = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(26, 22);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.lousyLiverCount = 4;
			originPlayer.lousyLiverDebuff = Buffs.Rasterized_Debuff.ID;
		}
	}
	public class Lousy_Liver_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; } = -1;
		public override void SetStaticDefaults() {
			ID = Type;
		}
		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<NPCs.OriginGlobalNPC>().rasterizedTime = 8;
		}
	}
}