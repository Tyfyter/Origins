using Origins.Buffs;
using Origins.Dev;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Mithrafin : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Misc"
		];
		public static bool[] buffTypes = BuffID.Sets.Factory.CreateBoolSet(false);
		public override void AddRecipes() {
			bool added;
			buffTypes = BuffID.Sets.Factory.CreateBoolSet(false, BuffID.Poisoned, BuffID.Venom, Toxic_Shock_Debuff.ID);
			do {
				added = false;
				for (int i = 0; i < BuffLoader.BuffCount; i++) {
					if (buffTypes[i]) continue;
					if (BuffID.Sets.GrantImmunityWith[i] is List<int> buffs && buffs.Any(v => buffTypes[v])) {
						buffTypes[i] = true;
						added = true;
					}
				}
			} while (added);
		}
		public override void Unload() {
			buffTypes = null;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().mithrafin = true;
		}
	}
	public class Mithrafin_Poison_Extend_Debuff : ModBuff {
		public override string Texture => "Terraria/Images/Buff_" + BuffID.Poisoned;
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
			Main.buffNoTimeDisplay[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			bool extendedSelf = false;
			for (int i = 0; i < player.buffType.Length; i++) {
				if (Mithrafin.buffTypes[player.buffType[i]]) {
					if (!extendedSelf) {
						player.buffTime[buffIndex]++;
						extendedSelf = true;
					}
					if (Main.rand.NextBool(5)) player.buffTime[i]++;
				}
			}
		}
		public override void Update(NPC npc, ref int buffIndex) {
			bool extendedSelf = false;
			for (int i = 0; i < npc.buffType.Length; i++) {
				if (Mithrafin.buffTypes[npc.buffType[i]]) {
					if (!extendedSelf) {
						npc.buffTime[buffIndex]++;
						extendedSelf = true;
					}
					if (Main.rand.NextBool(5)) npc.buffTime[i]++;
				}
			}
		}
	}
}
