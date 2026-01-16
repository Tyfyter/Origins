using Origins.NPCs.Critters;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

//what is this? bees according to California?
namespace Origins.Items.Other.Critters {
	public abstract class Critter_Item<TNPC> : ModItem where TNPC : ModNPC {
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
			SafeSetStaticDefaults();
		}
		public override void SetDefaults() {
			Item.DefaultToCapturedCritter(ModContent.NPCType<TNPC>());
			SafeSetDefaults();
		}
		public virtual void SafeSetStaticDefaults() { }
		public virtual void SafeSetDefaults() { }
	}
	public class Amoeba_Buggy_Item : Critter_Item<Amoeba_Buggy> {
		public override void SafeSetDefaults() {
			Item.bait = 40;
			Item.value = Item.sellPrice(silver: 5);
		}
	}
	public class Bug_Item : Critter_Item<Bug> {
		public override void SafeSetDefaults() {
			Item.rare = ItemRarityID.Lime;
			Item.bait = 60;
			Item.value = Item.sellPrice(gold: 1);
		}
	}
	public class Cicada_3301_Item : Critter_Item<Cicada_3301> {
		public override void SafeSetDefaults() {
			Item.bait = 40;
			Item.value = Item.sellPrice(silver: 5);
		}
	}
	public class Chambersite_Bunny_Item : Critter_Item<Chambersite_Bunny> {
		public override void SafeSetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Chambersite_Item>();
		}
		public override void SafeSetDefaults() {
			Item.value = Item.sellPrice(silver: 10);
		}
	}
	public class Chambersite_Squirrel_Item : Critter_Item<Chambersite_Squirrel> {
		public override void SafeSetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Chambersite_Item>();
		}
		public override void SafeSetDefaults() {
			Item.value = Item.sellPrice(silver: 10);
		}
	}
	public class Hyrax_Item : Critter_Item<Hyrax> {
		public override void SafeSetDefaults() {
			Item.value = Item.sellPrice(silver: 5);
		}
	}
	public class Peppered_Moth_Item : Critter_Item<Peppered_Moth> {
		public override void SafeSetDefaults() {
			Item.rare = ItemRarityID.Blue;
			Item.bait = 40;
			Item.value = Item.sellPrice(silver: 5);
		}
	}
}
