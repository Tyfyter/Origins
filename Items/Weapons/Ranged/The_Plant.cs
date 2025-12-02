using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.NPCs;
using Origins.UI;
using PegasusLib;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Ranged {
	[ReinitializeDuringResizeArrays]
	public class The_Plant : ModItem {
		public static PlantAmmoType[] ModesByAmmo { get; } = ItemID.Sets.Factory.CreateCustomSet<PlantAmmoType>(null);
		public int ammoCount = 0;
		public int ammoTimer = 0;
		public PlantAmmoType mode = null;
		public override void SetStaticDefaults() {
			Origins.AddGlowMask(this);
			ModesByAmmo[ItemID.MusketBall] = new(ProjectileID.Ale, Color.Red, TextureAssets.Item[ItemID.MusketBall]);
			ModesByAmmo[ItemID.ChlorophyteBullet] = new(ProjectileID.ChlorophyteArrow, Color.Green, TextureAssets.Item[ItemID.ChlorophyteBullet]);
		}
		public override void SetDefaults() {
			Item.damage = 10;
			Item.DamageType = DamageClass.Ranged;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 5;
			Item.noMelee = true;
			Item.useTime = 17;
			Item.useAnimation = 17;
			Item.width = 50;
			Item.height = 10;
			Item.shoot = ProjectileID.Bullet;
			Item.shootSpeed = 8;
			Item.UseSound = SoundID.Item11;
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateInventory(Player player) {
			if (++ammoTimer > Item.useAnimation * 9) {
				ammoTimer = 0;
				ammoCount++;
				Min(ref ammoCount, 999);
				List<int> unlockedPlantModes = player.OriginPlayer().unlockedPlantModes;
				for (int i = 0; i < player.inventory.Length; i++) {
					Item ammo = player.inventory[i];
					if (ammo is null) continue;
					if (!ammo.IsAir && ModesByAmmo[ammo.type] is PlantAmmoType learnAmmoType && !unlockedPlantModes.Contains(ammo.type)) {
						unlockedPlantModes.InsertOrdered(ammo.type);
						mode ??= learnAmmoType;
					}
				}
			}
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Add(new(Mod, "Ammo", mode?.ToString() ?? ""));
		}
		public override bool CanUseItem(Player player) => mode is not null && ammoCount > 0;
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = mode.ProjectileType;
		}
	}
	public class The_Plant_Flower : ItemModeFlowerMenu<PlantAmmoType, bool> {
		public override bool IsActive() => Main.LocalPlayer.HeldItem?.ModItem is The_Plant;
		AutoLoadingAsset<Texture2D> wireMiniIcons = "Origins/Items/Tools/Wiring/Mini_Wire_Icons";
		public override float DrawCenter() => 16;
		public override bool GetData(PlantAmmoType mode) => Main.LocalPlayer.HeldItem?.ModItem is The_Plant plant && plant.mode == mode;
		public override bool GetCursorAreaTexture(PlantAmmoType mode, out Texture2D texture, out Rectangle? frame, out Color color) {
			texture = wireMiniIcons;
			frame = new Rectangle(12, 0, 10, 10);
			color = mode.Color;
			return true;
		}
		public override void Click(PlantAmmoType mode) {
			if (Main.LocalPlayer.HeldItem?.ModItem is The_Plant plant) {
				plant.mode = mode;
			}
		}
		public override IEnumerable<PlantAmmoType> GetModes() {
			List<int> unlockedPlantModes = Main.LocalPlayer.OriginPlayer().unlockedPlantModes;
			for (int i = 0; i < unlockedPlantModes.Count; i++) {
				yield return The_Plant.ModesByAmmo[unlockedPlantModes[i]];
			}
		}
	}
	public record class PlantAmmoType(int ProjectileType, Color Color, Asset<Texture2D> Texture) : IFlowerMenuItem<bool> {
		public void Draw(Vector2 position, bool hovered, bool extraData) {
			Main.spriteBatch.Draw(
				Texture.Value,
				position - Texture.Size() * 0.5f,
				Color.White * ((hovered || extraData) ? 1 : 0.5f)
			);
		}
		public bool IsHovered(Vector2 position) => Main.MouseScreen.WithinRange(position, (Texture.Width() + Texture.Height()) * 0.25f);
	}

}
