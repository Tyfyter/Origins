using MonoMod.Cil;
using Origins.Dev;
using Origins.Items.Materials;
using Origins.Items.Weapons.Ammo;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Accessories {
	public class Gun_Magazine : ModItem, ICustomWikiStat {
		public string[] Categories => [
			WikiCategories.Combat,
			WikiCategories.RangedBoostAcc
		];
		public virtual float SpeedBonus => 0.05f;
		public override LocalizedText Tooltip => Language.GetOrRegister("Mods.Origins.Items.Gun_Magazine.Tooltip").WithFormatArgs(SpeedBonus);
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().gunSpeedBonus += SpeedBonus;
		}
	}
	public class Demonite_Gun_Magazine : Gun_Magazine {
		public virtual int BarType => ItemID.DemoniteBar;
		public override float SpeedBonus => 0.1f;
		public override void SetStaticDefaults() {
			OriginSystem.EvilGunMagazineRecipeGroup.ValidItems.Add(Type);
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Gun_Magazine>()
			.AddIngredient(BarType, 3)
			.Register();
		}
	}
	public class Crimtane_Gun_Magazine : Demonite_Gun_Magazine {
		public override int BarType => ItemID.CrimtaneBar;
		public override float SpeedBonus => 0.12f;
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			OriginSystem.EvilGunMagazineRecipeGroup.IconicItemId = Type;
		}
	}
	public class Defiled_Gun_Magazine : Demonite_Gun_Magazine {
		public override int BarType => ModContent.ItemType<Defiled_Bar>();
	}
	public class Encrusted_Gun_Magazine : Demonite_Gun_Magazine {
		public override int BarType => ModContent.ItemType<Encrusted_Bar>();
	}
	public class Sanguinite_Gun_Magazine : Demonite_Gun_Magazine {
		public override int BarType => ModContent.ItemType<Sanguinite_Bar>();
	}
	public class Molten_Gun_Magazine : Gun_Magazine {
		public override float SpeedBonus => 0.15f;
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddRecipeGroup(OriginSystem.EvilGunMagazineRecipeGroup.RegisteredId)
			.AddIngredient(ItemID.HellstoneBar, 3)
			.Register();
		}
	}
	public class Eitrite_Gun_Magazine : Gun_Magazine {
		public override float SpeedBonus => 0.2f;
		public override LocalizedText Tooltip => OriginExtensions.CombineTooltips(
				base.Tooltip,
				this.GetLocalization(nameof(Tooltip))
			);
		public override void SetStaticDefaults() {
			AmmoID.Sets.IsBullet[ItemID.Sunflower] = true;
			AmmoID.Sets.IsBullet[ItemID.Fireblossom] = true;
			AmmoID.Sets.IsBullet[ModContent.ItemType<Harpoon>()] = true;
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(18, 24);
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			base.UpdateAccessory(player, hideVisual);
			player.OriginPlayer().eitriteGunMagazine = true;
		}
		public override void AddRecipes() {
			CreateRecipe()
			.AddIngredient<Molten_Gun_Magazine>()
			.AddIngredient<Eitrite_Bar>(3)
			.Register();
		}
		internal static void IL_Player_ItemCheck_OwnerOnlyCode(ILContext il) {
			ILCursor c = new(il);
			try {
				int useLimitPerAnimation = -1;
				while (c.TryGotoNext(MoveType.After,
					i => i.MatchLdloca(out useLimitPerAnimation),
					i => i.MatchCall<int?>("get_" + nameof(Nullable<int>.Value))
				)) {
					if (!c.TryFindPrev(out ILCursor[] cursors, i => i.MatchStloc(useLimitPerAnimation))) continue;
					if (!cursors[0].Prev.MatchCallOrCallvirt<Item>("get_" + nameof(Item.useLimitPerAnimation))) continue;
					c.EmitLdarg0();
					c.EmitDelegate<Func<int, Player, int>>((uses, player) => {
						if (uses >= 2 && player.HeldItem.IsAGun() && player.OriginPlayer().eitriteGunMagazine) uses++;
						return uses;
					});
				}
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(IL_Player_ItemCheck_OwnerOnlyCode), e)) throw;
			}
		}
	}
}
