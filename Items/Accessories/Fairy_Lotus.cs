using MonoMod.Cil;
using Origins.Dev;
using PegasusLib.Reflection;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	[AutoloadEquip(EquipType.Face)]
	public class Fairy_Lotus : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Misc
		];
		public override void SetStaticDefaults() {
			ArmorIDs.Face.Sets.DrawInFaceFlowerLayer[Item.faceSlot] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(20, 34);
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(gold: 1);
		}
		public override void UpdateAccessory(Player player, bool isHidden) {
			player.OriginPlayer().fairyLotus = true;
		}
		internal static void IL_NPC_SpawnNPC_CheckToSpawnUndergroundFairy(ILContext il) {
			ILCursor c = new(il);
			try {
				c.GotoNext(MoveType.Before, i => i.MatchCallOrCallvirt<Player>(nameof(Player.RollLuck)));
				ILCursor dup = new(c);
				MonoModMethods.SkipPrevArgument(dup);
				c.EmitDelegate<Func<Player, int, int>>((player, odds) => {
					if (player.OriginPlayer().fairyLotus) {
						odds /= 4;
					}
					return odds;
				});
				dup.EmitDup();
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(IL_NPC_SpawnNPC_CheckToSpawnUndergroundFairy), e)) throw;
			}
		}
	}
}
