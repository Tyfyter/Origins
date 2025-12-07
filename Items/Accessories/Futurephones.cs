using Origins.Buffs;
using Origins.Dev;
using Origins.Items.Vanity.Dev.PlagueTexan;
using Origins.Layers;
using PegasusLib;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
    [AutoloadEquip(EquipType.Face)]
	public class Futurephones : ModItem, ICustomWikiStat {
		public static int FaceID { get; private set; }
		public string[] Categories => [
			"Combat"
		];
        public override void SetStaticDefaults() {
            Origins.AddGlowMask(this);
			Accessory_Glow_Layer.AddGlowMask<Face_Glow_Layer>(Item.faceSlot, Texture + "_Face_Glow", ShaderFunc: (player, dye) => player.GetModPlayer<OriginsDyeSlots>().cFuturephonesGlow ?? dye);
			FaceID = Item.faceSlot;
		}
        public override void SetDefaults() {
			Item.DefaultToAccessory(32, 20);
			Item.value = Item.sellPrice(gold: 8);
			Item.rare = ItemRarityID.Pink;
        }
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<OriginPlayer>().futurephones = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Headphones>()
			.AddIngredient<Snipers_Mark>()
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
	public class Futurephones_Glow_Dye_Slot : ExtraDyeSlot {
		static int GetFaceSlot(Item equipped, Item vanity) {
			int slot = vanity?.faceSlot ?? -1;
			if (slot != -1) return slot;
			return equipped.faceSlot;
		}
		public override bool UseForSlot(Item equipped, Item vanity, bool equipHidden) => GetFaceSlot(equipped, vanity) == Futurephones.FaceID;
		public override void ApplyDye(Player player, [NotNull] Item dye) {
			player.GetModPlayer<OriginsDyeSlots>().cFuturephonesGlow = dye.dye;
		}
	}
	public class Futurephones_Buff : ModBuff, ICustomWikiStat {
		public bool CanExportStats => false;
		public override string Texture => "Terraria/Images/Buff_160";
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			BuffID.Sets.IsATagBuff[Type] = true;
			ID = Type;
		}
	}
}
