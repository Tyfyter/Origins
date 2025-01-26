using Origins.Dev;
using Origins.Items.Accessories;
using Origins.Items.Materials;
using Origins.Tiles.Other;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Blast {
    [AutoloadEquip(EquipType.Head)]
	public class Blast_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            "ExplosiveBoostGear",
			"SelfDamageProtek"
		];
        public override void SetStaticDefaults() {
			Origins.AddHelmetGlowmask(this);
			Origins.AddHelmetGlowmask(BerserkerModeHead, "Origins/Items/Armor/Blast/Blast_Helmet_Head_Berserker_Glow");
        }
        public static int BerserkerModeHead { get; private set; }
		public override void Load() {
			BerserkerModeHead = EquipLoader.AddEquipTexture(Mod, $"{Texture}_Head_Berserker", EquipType.Head, name: $"{Name}_Head_Berserker");
		}
		public override void SetDefaults() {
			Item.defense = 8;
			Item.value = Item.sellPrice(gold: 7, silver: 50);
			Item.rare = ItemRarityID.Yellow;
		}
        public override void UpdateEquip(Player player) {
			player.GetCritChance(DamageClass.Generic) += 16f;
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.1f;
        }
        public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Blast_Breastplate>() && legs.type == ModContent.ItemType<Blast_Greaves>();
		}
		public override void EquipFrameEffects(Player player, EquipType type) {
			if (player.GetModPlayer<OriginPlayer>().blastSetActive) {
				player.head = BerserkerModeHead;
			}
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Blast");
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.explosiveSelfDamage -= 0.25f;
			originPlayer.blastSet = true;
			originPlayer.setActiveAbility = SetActiveAbility.blast_armor;
			if (originPlayer.blastSetActive) {
				player.GetModPlayer<OriginPlayer>().explosiveProjectileSpeed += 0.5f;
				player.GetModPlayer<OriginPlayer>().explosiveBlastRadius += 0.5f;
				player.GetAttackSpeed(DamageClasses.Explosive) += 0.5f;
				player.GetModPlayer<OriginPlayer>().explosiveFuseTime -= 0.5f;
				player.GetKnockback(DamageClasses.Explosive) += 0.5f;
				// crit chance boost in Combat.cs
			}
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.ChlorophyteBar, 12)
            .AddIngredient(ModContent.ItemType<Blast_Resistant_Plate>())
            .AddIngredient(ModContent.ItemType<Busted_Servo>(), 4)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
		public string ArmorSetName => "Blast_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Blast_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Blast_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Blast_Breastplate : ModItem, INoSeperateWikiPage {
        public override void SetStaticDefaults() {
			Origins.AddBreastplateGlowmask(this);
		}
        public override void SetDefaults() {
			Item.defense = 18;
			Item.value = Item.sellPrice(gold: 6);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
            player.noKnockback = true;
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.1f;
        }
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ItemID.ChlorophyteBar, 24)
            .AddIngredient(ModContent.ItemType<Blast_Resistant_Plate>())
            .AddIngredient(ModContent.ItemType<Busted_Servo>(), 6)
            .AddIngredient(ModContent.ItemType<Power_Core>(), 2)
            .AddIngredient(ModContent.ItemType<Rotor>(), 2)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Blast_Greaves : ModItem, INoSeperateWikiPage {
        public override void SetStaticDefaults() {
			Origins.AddLeggingGlowMask(this);
		}
        public override void SetDefaults() {
			Item.defense = 13;
			Item.value = Item.sellPrice(gold: 4, silver: 50);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.moveSpeed += 0.25f;
			player.runAcceleration += 0.02f;
			player.jumpSpeedBoost += 2;
            player.GetModPlayer<OriginPlayer>().explosiveSelfDamage -= 0.1f;
        }
		public override void AddRecipes() {
			Recipe.Create(Type)
            .AddIngredient(ItemID.ChlorophyteBar, 18)
            .AddIngredient(ModContent.ItemType<Blast_Resistant_Plate>())
            .AddIngredient(ModContent.ItemType<Busted_Servo>(), 4)
            .AddIngredient(ModContent.ItemType<Rotor>(), 2)
			.AddTile(ModContent.TileType<Fabricator>())
			.Register();
		}
	}
}
