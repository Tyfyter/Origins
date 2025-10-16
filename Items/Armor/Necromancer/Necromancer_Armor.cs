using Origins.Dev;
using Origins.Items.Armor.Acrid;
using Origins.Items.Materials;
using Origins.Items.Weapons;
using PegasusLib;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Necromancer {
    [AutoloadEquip(EquipType.Head)]
	public class Necromancer_Helmet : ModItem, IWikiArmorSet, INoSeperateWikiPage {
        public string[] Categories => [
            WikiCategories.ArmorSet,
            WikiCategories.SummonBoostGear
        ];
        public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Summon) += 0.25f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Necromancer_Breastplate>() && legs.type == ModContent.ItemType<Necromancer_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Necromancer");
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			originPlayer.artifactManaCost *= 0.5f;
			if (player.ZoneGraveyard) {
				player.manaCost *= 0.5f;
			}
			originPlayer.necroSet = true;
			float killMult = originPlayer.necroSetAmount * 0.002f;
			player.GetAttackSpeed(DamageClass.Summon) += Math.Min(0.1f * killMult, 0.6f);
			player.GetDamage(DamageClass.Summon) += Math.Min(0.1f * killMult, 0.75f);
			if (killMult > 0) {
				player.GetDamage(DamageClass.Summon) += (float)Math.Max(Math.Pow(0.1f * killMult, 0.5f), 0);
			}
			player.lifeRegenCount += (int)Math.Min(4 * killMult, 5);
			player.statDefense += (int)Math.Min(6 * killMult, 13);

			/*godmode dev set:
			float killMult = originPlayer.necroSetAmount * 0.002f;
			player.GetAttackSpeed(DamageClass.Summon) += 0.1f * killMult;
			player.GetDamage(DamageClass.Generic) += 0.1f * killMult;
			player.lifeRegenCount += (int)(killMult * 4);
			player.statDefense += (int)(12 * killMult);
			 */

			player.maxMinions += 3;
		}
		public override void ArmorSetShadows(Player player) {
			//player.armorEffectDrawOutlines = true;
			//player.armorEffectDrawOutlinesForbidden = true;
			//player.armorEffectDrawShadow = true;
			player.armorEffectDrawShadowLokis = true;
			player.armorEffectDrawShadowSubtle = true;
			/*if (Main.rand.NextBool(1)) {
				Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Blood, 0, 0, 165, new Color(80, 50, 50));
				dust.noGravity = true;
				dust.scale *= 1.02f;
			}*/
		}
		public override void UpdateVanitySet(Player player) {
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.ChlorophyteBar, 12)
			.AddIngredient(ItemID.DarkShard)
			.AddRecipeGroupWithItem(OriginSystem.CursedFlameRecipeGroupID, showItem: ModContent.ItemType<Black_Bile>(), 7)
			.AddTile(TileID.DemonAltar)
			.Register();
		}
		public string ArmorSetName => "Necromancer_Armor";
		public IEnumerable<int> SharedPageItems => [
			ModContent.ItemType<Necromancer_Crown>()
		];
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Necromancer_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Necromancer_Greaves>();
	}
	[AutoloadEquip(EquipType.Body)]
	public class Necromancer_Breastplate : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 14;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.maxMinions += 1;
			player.GetAttackSpeed(DamageClass.Summon) += 0.15f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.ChlorophyteBar, 24)
			.AddIngredient(ItemID.DarkShard)
			.AddRecipeGroupWithItem(OriginSystem.CursedFlameRecipeGroupID, showItem: ModContent.ItemType<Black_Bile>(), 7)
			.AddTile(TileID.DemonAltar)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Legs)]
	public class Necromancer_Greaves : ModItem, INoSeperateWikiPage {
		
		public override void SetDefaults() {
			Item.defense = 10;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.OriginPlayer().artifactDamage += 0.25f;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.ChlorophyteBar, 18)
			.AddIngredient(ItemID.DarkShard)
			.AddRecipeGroupWithItem(OriginSystem.CursedFlameRecipeGroupID, showItem: ModContent.ItemType<Black_Bile>(), 7)
			.AddTile(TileID.DemonAltar)
			.Register();
		}
	}
	[AutoloadEquip(EquipType.Head)]
	public class Necromancer_Crown : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		public string[] Categories => [
			WikiCategories.ArmorSet,
			WikiCategories.SummonBoostGear
		];
		public override void SetDefaults() {
			Item.defense = 7;
			Item.value = Item.sellPrice(gold: 5);
			Item.rare = ItemRarityID.Yellow;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Summon) += 0.15f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Necromancer_Breastplate>() && legs.type == ModContent.ItemType<Necromancer_Greaves>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.NecromancerCrown");
			OriginPlayer originPlayer = player.GetModPlayer<OriginPlayer>();
			if (player.ZoneGraveyard) {
				player.manaCost *= 0.5f;
			}
			originPlayer.necroSet2 = true;

			player.maxMinions += 3;
		}
		public override void ArmorSetShadows(Player player) {
			player.armorEffectDrawShadowLokis = true;
			player.armorEffectDrawShadowSubtle = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ItemID.ChlorophyteBar, 12)
			.AddIngredient(ItemID.DarkShard)
			.AddRecipeGroupWithItem(OriginSystem.CursedFlameRecipeGroupID, showItem: ModContent.ItemType<Black_Bile>(), 7)
			.AddTile(TileID.DemonAltar)
			.Register();
		}
		public bool SharedPageSecondary => true;
		public string ArmorSetName => "Necromancer_Crown";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Necromancer_Breastplate>();
		public int LegsItemID => ModContent.ItemType<Necromancer_Greaves>();
	}
	public class Unsatisfied_Soul : ModProjectile {
		public static int ID { get; private set; }
		public override void SetStaticDefaults() {
			ID = Type;
			Main.projFrames[Type] = 5;
		}
		public override void SetDefaults() {
			Projectile.DamageType = DamageClass.Summon;
			Projectile.friendly = true;
			Projectile.tileCollide = false;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.penetrate = 1;
			Projectile.ArmorPenetration += 15;
		}
		public override void AI() {
			float targetWeight = 16 * 20;
			Vector2 targetPos = default;
			bool foundTarget = Main.player[Projectile.owner].DoHoming((target) => {
				Vector2 currentPos = target.Center;
				float dist = Math.Abs(Projectile.Center.X - currentPos.X) + Math.Abs(Projectile.Center.Y - currentPos.Y);
				if (target is Player) dist *= 2.5f;
				if (dist < targetWeight && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, target.position, target.width, target.height)) {
					targetWeight = dist;
					targetPos = currentPos;
					return true;
				}
				return false;
			});

			if (foundTarget) {
				float scaleFactor = 12f * Origins.HomingEffectivenessMultiplier[Projectile.type];

				Vector2 targetVelocity = (targetPos - Projectile.Center).SafeNormalize(-Vector2.UnitY);
				scaleFactor += Vector2.Dot(Projectile.velocity.SafeNormalize(-Vector2.UnitY), targetVelocity) * 4;
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVelocity * scaleFactor, 0.06f);
			}
			if (++Projectile.frameCounter >= 3) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Type]) Projectile.frame = 0;
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
		}
	}
}
