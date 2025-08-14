using Origins.NPCs.Defiled;
using Origins.NPCs.Riven;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.CrossMod.Fargos.Items {
	public abstract class TOSummons<TSummon> : ModItem where TSummon : ModNPC {
		public override bool IsLoadingEnabled(Mod mod) => ModLoader.HasMod("Fargowiltas");
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 3;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.WormFood);
			Item.value = Item.sellPrice(silver: 2);
			Item.rare = ItemRarityID.Blue;
		}
		public override bool? UseItem(Player player) {
			SoundEngine.PlaySound(SoundID.Roar, player.Center);
			player.SpawnCloseOn(ModContent.NPCType<TSummon>(), true);
			return true;
		}
	}
	public class Defiled_Chest : TOSummons<Defiled_Mimic> { }
	public class Riven_Chest : TOSummons<Riven_Mimic> { }
	/*
	public class Ashen_Chest : TOSummons<Ashen_Mimic> { }
	*/
}
