using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.NPCs.MiscB.Shimmer_Construct;
using PegasusLib;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Armor.Aetherite {
	[AutoloadEquip(EquipType.Head)]
	public class Aetherite_Wreath : ModItem, IWikiArmorSet, INoSeperateWikiPage {
		//private AutoLoadingAsset<Texture2D> cords = typeof(Chambersite_Helmet).GetDefaultTMLName() + "_Cords";
		public override void SetStaticDefaults() {
			Origins.AddHelmetGlowmask(this);
		}
		public override void SetDefaults() {
			Item.defense = 8;
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			OriginPlayer originPlayer = player.OriginPlayer();
			originPlayer.projectileSpeedBoost += 0.15f;
			originPlayer.meleeScaleMultiplier *= 1.15f;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs) {
			return body.type == ModContent.ItemType<Aetherite_Robe>();
		}
		public override void UpdateArmorSet(Player player) {
			player.setBonus = Language.GetTextValue("Mods.Origins.SetBonuses.Aetherite").SubstituteKeybind(Keybindings.TriggerSetBonus);
			player.OriginPlayer().setActiveAbility = SetActiveAbility.aetherite_armor;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Aetherite_Bar>(12)
			.AddTile(TileID.Anvils)
			.Register();
		}
		public string ArmorSetName => "Aetherite_Armor";
		public int HeadItemID => Type;
		public int BodyItemID => ModContent.ItemType<Aetherite_Robe>();
		public int LegsItemID => 0;
	}
	[AutoloadEquip(EquipType.Body)]
	public class Aetherite_Robe : ModItem, INoSeperateWikiPage {
		int legsID;
		public override void Load() {
			legsID = EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, name: $"{Name}_{EquipType.Legs}");
		}
		public override void SetDefaults() {
			Item.defense = 8;
			Item.value = Item.sellPrice(silver: 80);
			Item.rare = ItemRarityID.Orange;
		}
		public override void UpdateEquip(Player player) {
			player.GetAttackSpeed(DamageClass.Generic) += 0.24f;
			player.maxMinions += 1;
			if (!player.controlDown) player.gravity *= 0.8f;
			player.jumpSpeedBoost += 2f;
		}
		public override void SetMatch(bool male, ref int equipSlot, ref bool robes) {
			robes = true;
			equipSlot = legsID;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient<Aetherite_Bar>(24)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
	public class Aetherite_Aura_P : ModProjectile {
		public override string Texture => "Terraria/Images/Misc/StarDustSky/Planet";
		public override void SetDefaults() {
			Projectile.width = 0;
			Projectile.height = 0;
			Projectile.scale = 0;
		}
		public override void AI() {
			Projectile.timeLeft = 2;
			if (Projectile.ai[0] == 0) {
				if (MathUtils.LinearSmoothing(ref Projectile.scale, 1, 1 / 60f)) Projectile.ai[0] = 1;
			} else if (++Projectile.ai[0] > 600) {
				if (MathUtils.LinearSmoothing(ref Projectile.scale, 0, 1 / 60f)) Projectile.Kill();
			}
			float radius = 312f * Projectile.scale;
			foreach (Player player in Main.ActivePlayers) {
				if (player.Hitbox.IsWithin(Projectile.position, radius)) {
					player.AddBuff(Weak_Shimmer_Debuff.ID, 5, true);
				}
			}
			if (!Main.gamePaused) {
				Texture2D circle = TextureAssets.Projectile[Type].Value;
				SC_Phase_Three_Overlay.drawDatas.Add(new(
					circle,
					Projectile.position - Main.screenPosition,
					null,
					Color.White
				) {
					origin = circle.Size() * 0.5f,
					scale = Vector2.One * Projectile.scale
				});
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			return false;
		}
	}
}
