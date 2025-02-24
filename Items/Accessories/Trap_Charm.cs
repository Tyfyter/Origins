using MonoMod.Cil;
using Origins.Dev;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Origins.World.BiomeData;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace Origins.Items.Accessories {
	public class Trap_Charm : ModItem, ICustomWikiStat {
		public string[] Categories => [
			"Vitality"
		];
		public override void SetDefaults() {
			Item.DefaultToAccessory(30, 24);
			Item.value = Item.sellPrice(gold: 2);
			Item.rare = ItemRarityID.Green;
		}
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().trapCharm = true;
		}

		internal static void IL_Player_ApplyTouchDamage(ILContext il) {
			ILCursor c = new(il);
			if (c.TryGotoNext(MoveType.After, 
				i => i.MatchLdsfld(typeof(TileID.Sets), nameof(TileID.Sets.TouchDamageImmediate)),
				i => i.MatchLdarg1(),
				i => i.MatchLdelemI4()
			)) {
				c.EmitLdarg0();
				c.EmitDelegate((int damage, Player player) => {
					if (player.OriginPlayer().trapCharm) damage /= 2;
					return damage;
				});
			} else {
				Origins.LogLoadingWarning(Language.GetOrRegister("Mods.Origins.Warnings.TrapCharmILEditFail"));
			}
		}
	}
}
