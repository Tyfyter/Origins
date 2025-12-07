using Origins.Dev;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Origins.Tiles.Defiled;
using Origins.Tiles.Riven;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Origins.Tiles.Ashen;
using Origins.Items.Materials;
using Terraria.Localization;
using Origins.Items.Weapons.Demolitionist;
using Origins.Items.Weapons.Magic;
using Origins.Items.Weapons.Ranged;
using Origins.Items.Weapons.Summoner;
using Origins.Items.Other.Fish;

namespace Origins.Core; 
class ShimmerCycles : ModSystem {
	RecipeGroup evilMushrooms;
	RecipeGroup evilFish;
	RecipeGroup evilFishWeapons;
	public override void AddRecipeGroups() {
		static RecipeGroup CreateGroup(string name, int corrupt, int crimson, int defiled, int riven, int ashen) {
			_ = Language.GetOrRegister("Mods.Origins.RecipeGroups." + name);
			RecipeGroup group = new(() => Language.GetOrRegister("Mods.Origins.RecipeGroups." + name).Value, corrupt, crimson, defiled, riven, ashen);
			RecipeGroup.RegisterGroup(name, group);
			return group;
		}
		evilMushrooms = CreateGroup("EvilMushrooms", ItemID.VileMushroom, ItemID.ViciousMushroom, ItemType<Soulspore_Item>(), ItemType<Acetabularia_Item>(), ItemType<Fungarust_Item>());
		evilFish = CreateGroup("EvilFish", ItemID.Ebonkoi, ItemID.Hemopiranha, ItemType<Bilemouth>(), ItemType<Tearracuda>(), ItemType<Polyeel>());
		evilFishWeapons = CreateGroup("EvilFishWeapons", ItemID.PurpleClubberfish, ItemType<Blotopus>(), ItemType<Manasynk>(), ItemType<Ocotoral_Bud>(), ItemType<Internal_Combustionfish>());
	}
	public override void PostSetupRecipes() {
		ALRecipeGroups.EvilOres.CreateEvilShimmerCycle(ItemID.DemoniteOre, ItemID.CrimtaneOre, ItemType<Lost_Ore_Item>(), ItemType<Encrusted_Ore_Item>(), ItemType<Sanguinite_Ore_Item>());

		ALRecipeGroups.RottenChunks.CreateEvilShimmerCycle(ItemID.RottenChunk, ItemID.Vertebrae, ItemType<Strange_String>(), ItemType<Bud_Barnacle>(), ItemType<Biocomponent10>());

		ALRecipeGroups.CursedFlames.CreateEvilShimmerCycle(ItemID.CursedFlame, ItemID.Ichor, ItemType<Black_Bile>(), ItemType<Alkahest>(), ItemType<Respyrite>());

		ALRecipeGroups.ShadowScales.CreateEvilShimmerCycle(ItemID.ShadowScale, ItemID.TissueSample, ItemType<Undead_Chunk>(), ItemType<Riven_Carapace>(), ItemType<NE8>());

		ALRecipeGroups.Deathweed.CreateEvilShimmerCycle(ItemID.Deathweed, ItemID.Deathweed, ItemType<Wilting_Rose_Item>(), ItemType<Wrycoral_Item>(), ItemType<Surveysprout_Item>());

		evilMushrooms.CreateEvilShimmerCycle(ItemID.VileMushroom, ItemID.ViciousMushroom, ItemType<Soulspore_Item>(), ItemType<Acetabularia_Item>(), ItemType<Fungarust_Item>());

		evilFish.CreateEvilShimmerCycle(ItemID.Ebonkoi, ItemID.Hemopiranha, ItemType<Bilemouth>(), ItemType<Tearracuda>(), ItemType<Polyeel>());

		evilFishWeapons.CreateEvilShimmerCycle(ItemID.PurpleClubberfish, ItemType<Blotopus>(), ItemType<Manasynk>(), ItemType<Ocotoral_Bud>(), ItemType<Internal_Combustionfish>());
	}
}
