using BetterDialogue.UI;
using Origins.Buffs;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;

namespace Origins.Questing {
	public class DryadPurifyButton : ChatButton {
		public override double Priority => 100.0;
		public static long GetPrice(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			long price = (int)((originPlayer.CorruptionAssimilation + originPlayer.CrimsonAssimilation + originPlayer.DefiledAssimilation + originPlayer.RivenAssimilation) * 50000);
			if (NPC.downedGolemBoss) {
				price *= 8;
			} else if (NPC.downedPlantBoss) {
				price *= 7;
			} else if (NPC.downedMechBossAny) {
				price *= 6;
			} else if (Main.hardMode) {
				price *= 5;
			} else if (NPC.downedBoss3 || NPC.downedQueenBee) {
				price *= 4;
			} else if (NPC.downedBoss2) {
				price *= 3;
			}
			if (Main.expertMode) {
				price *= 2;
			}
			price = (int)(price * player.currentShoppingSettings.PriceAdjustment);
			return price + (100 - price % 100);
		}
		public override void OnClick(NPC npc, Player player) {
			if (player.BuyItem(GetPrice(player))) {
				player.AddBuff(Purifying_Buff.ID, 120);
				//TODO: add dialog for this too
			} else {
				//TODO: add "you're too poor" dialog
			}
		}
		public override Color? OverrideColor(NPC npc, Player player) {
			long price = GetPrice(player);
			float num17 = Main.mouseTextColor / 255f;
			if (price > 1000000) return new Color((byte)(220f * num17), (byte)(220f * num17), (byte)(198f * num17), Main.mouseTextColor);
			if (price > 10000) return new Color((byte)(224f * num17), (byte)(201f * num17), (byte)(92f * num17), Main.mouseTextColor);
			if (price > 100) return new Color((byte)(181f * num17), (byte)(192f * num17), (byte)(193f * num17), Main.mouseTextColor);
			return null;
		}
		public override string Text(NPC npc, Player player) {
			string text = "";
			long price = GetPrice(player);
			if ((price / 1000000) % 100 > 0) {
				text = text + ((price / 1000000) % 100) + " " + Lang.inter[15].Value + " ";
			}
			if ((price / 10000) % 100 > 0) {
				text = text + ((price / 10000) % 100) + " " + Lang.inter[16].Value + " ";
			}
			if ((price / 100) % 100 > 0) {
				text = text + ((price / 100) % 100) + " " + Lang.inter[17].Value;
			}
			return Language.GetOrRegister("Mods.Origins.Interface.DryadPurify").Format(text.Trim());
		}
		public override bool IsActive(NPC npc, Player player) {
			//return false;
			if (npc.type != NPCID.Dryad) return false;
			OriginPlayer originPlayer = player.OriginPlayer();
			return originPlayer.CorruptionAssimilation > 0 || originPlayer.CrimsonAssimilation > 0 || originPlayer.DefiledAssimilation > 0 || originPlayer.RivenAssimilation > 0;
		}
	}
}
