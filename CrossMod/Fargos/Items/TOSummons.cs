using Origins.NPCs.Defiled;
using Origins.NPCs.MiscE;
using Origins.NPCs.Riven;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Items {
	public abstract class TOSummons<Summon> : ModItem where Summon : ModNPC {
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("Fargowiltas");
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.Blue;
		}
		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				Vector2 pos = new(player.Center.X + Main.rand.NextFloat(-800, 800), player.Center.Y + Main.rand.NextFloat(-800, -250));
				NPC.NewNPCDirect(NPC.GetBossSpawnSource(player.whoAmI), pos, ModContent.NPCType<Summon>());
				SoundEngine.PlaySound(SoundID.Roar);
			}
			return true;
		}
	}
	public class Defiled_Chest : TOSummons<Defiled_Mimic> {
	}
	public class Riven_Chest : TOSummons<Riven_Mimic> {
	}/*
	public class Ashen_Chest : TOSummons<Ashen_Mimic> {
	}*/
}
