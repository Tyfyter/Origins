using Microsoft.Xna.Framework;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

using Origins.Dev;
namespace Origins.Items.Weapons.Demolitionist {
	public class Mine_Flayer : ModItem, ICustomWikiStat {
        public string[] Categories => new string[] {
            "Launcher",
			"CanistahUser"
        };
        public override void SetDefaults() {
			Item.CloneDefaults(ItemID.TerraBlade);
			Item.shootsEveryUse = false;
			Item.damage = 60;
			Item.DamageType = DamageClasses.ExplosiveVersion[DamageClass.Melee];
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 4;
			Item.useAnimation = 36;
			Item.knockBack = 4f;
			Item.useAmmo = ModContent.ItemType<Resizable_Mine_One>();
			Item.shoot = ModContent.ProjectileType<Resizable_Mine_P_1>();
			Item.shootSpeed = 9;
			Item.rare = ItemRarityID.Pink;
			Item.value = Item.sellPrice(gold: 5);
			Item.reuseDelay = 60;
			Item.autoReuse = false;
			Item.UseSound = null;
		}
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return Main.rand.NextBool(6);
		}
		public override void AddRecipes() {
			Recipe recipe = Recipe.Create(Type);
			recipe.AddIngredient(ModContent.ItemType<Busted_Servo>(), 30);
			recipe.AddIngredient(ModContent.ItemType<Power_Core>());
			recipe.AddIngredient(ModContent.ItemType<Rotor>(), 8);
			recipe.AddTile(TileID.MythrilAnvil); //Fabricator
			recipe.Register();
		}
		public override void MeleeEffects(Player player, Rectangle hitbox) {
			base.MeleeEffects(player, hitbox);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = OriginExtensions.Vec2FromPolar(player.direction == 1 ? player.itemRotation : player.itemRotation + MathHelper.Pi, velocity.Length());
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item61.WithPitch(0.25f), position);
			type += Item.shoot - 1;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
		public override void HoldItem(Player player) {
		}
	}
}
