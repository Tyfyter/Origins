using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Dev;
using Origins.Items.Materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Weapons.Demolitionist {
	public class Nova_Swarm : ModItem, ICustomDrawItem {
		public const float rocket_scale = 0.85f;
		public static int ID { get; private set; }
		public static AutoCastingAsset<Texture2D> UseTexture { get; private set; }
		public override void Unload() {
			UseTexture = null;
		}
		public override void SetStaticDefaults() {
			if (!Main.dedServ) {
				UseTexture = Mod.Assets.Request<Texture2D>("Items/Weapons/Demolitionist/Nova_Swarm_Use");
			}
			AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[Type] = AmmoID.Sets.SpecificLauncherAmmoProjectileMatches[ItemID.RocketLauncher];
			ID = Type;
		}
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ProximityMineLauncher);
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Ranged];
			Item.damage = 110;
			Item.noMelee = true;
			Item.width = 60;
			Item.height = 30;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.value = Item.sellPrice(gold: 20);
			Item.rare = ItemRarityID.Red;
			Item.autoReuse = true;
		}
		public override void AddRecipes() {
			Recipe.Create(Type)
			.AddIngredient(ModContent.ItemType<Nova_Fragment>(), 18)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (velocity != default) position += Vector2.Normalize(velocity) * 54;
			velocity = velocity.RotatedByRandom(0.15f);
		}
		public override Vector2? HoldoutOffset() {
			return new Vector2(-7, -4);
		}
		public void DrawInHand(Texture2D itemTexture, ref PlayerDrawSet drawInfo, Vector2 itemCenter, Color lightColor, Vector2 drawOrigin) {
			Player drawPlayer = drawInfo.drawPlayer;
			float itemRotation = drawPlayer.itemRotation;

			Vector2 pos = new((int)(drawInfo.ItemLocation.X - Main.screenPosition.X), (int)(drawInfo.ItemLocation.Y - Main.screenPosition.Y + itemCenter.Y));

			int frame;
			switch (drawPlayer.itemAnimationMax - drawPlayer.itemAnimation) {
				case 1:
				case 2:
				case 3:
				case 4:
				frame = 1;
				break;

				case 5:
				case 6:
				case 7:
				case 8:
				frame = 2;
				break;

				case 9:
				case 10:
				case 11:
				case 12:
				frame = 3;
				break;

				case 13:
				case 14:
				case 15:
				case 16:
				frame = 4;
				break;
				default:
				frame = 0;
				break;
			}
			Rectangle frameRect = new(0, 32 * frame, 60, 30);
			drawInfo.DrawDataCache.Add(new DrawData(
				UseTexture,
				pos,
				frameRect,
				Item.GetAlpha(lightColor),
				itemRotation,
				new Vector2(10, 14).Apply(drawInfo.itemEffect, frameRect.Size()),
				drawPlayer.GetAdjustedItemScale(Item),
				drawInfo.itemEffect
			));
		}
	}
}
