using BetterDialogue.UI.VanillaChatButtons;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace Origins.Items.Accessories {
    public class Priority_Mail : ModItem {
        public string[] Categories => [
            "Combat",
			"SummonBoostAcc"
        ];
		public override void Load() {
			try {
				IL_ItemSlot.RightClick_ItemArray_int_int += (il) => {
					ILCursor c = new(il);
					c.GotoNext(MoveType.After,
						i => i.MatchLdarg0(),//IL_00fe: ldarg.0
						i => i.MatchLdarg2(),//IL_00ff: ldarg.2
						i => i.MatchLdelemRef(),//IL_0100: ldelem.ref
						i => i.MatchLdfld<Item>(nameof(Item.maxStack)),//IL_0101: ldfld int32 Terraria.Item::maxStack
						i => i.MatchLdcI4(1),//IL_0106: ldc.i4.1
						i => i.MatchBneUn(out _)//IL_0107: bne.un.s IL_011c
					);
					c.Index -= 2;
					c.EmitLdarg0();
					c.EmitLdarg2();
					c.EmitLdelemRef();
					c.EmitDelegate((int maxStack, Item item) => item.stack == 1 && item.accessory ? 1 : maxStack);
				};
			} catch (Exception e) {
				if (Origins.LogLoadingILError("EnableRightClickSwappingOnStackableAccessories", e)) throw;
			}
		}

		public override void SetDefaults() {
			Item.DefaultToAccessory(32, 28);
			Item.maxStack = 2;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 1);
			Item.AllowReforgeForStackableItem = true;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.priorityMail = true;
			originPlayer.asylumWhistle = true;
			player.GetDamage(DamageClass.Summon) += 0.1f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type, 2)
			.AddIngredient(ItemID.FallenStar)
			.AddIngredient(ItemID.PaperAirplaneA)
			.AddIngredient(ModContent.ItemType<Asylum_Whistle>())
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
	public class Priority_Mail_Buff : ModBuff {
		public override string Texture => "Origins/Items/Accessories/Priority_Mail";
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
		}
	}
}
