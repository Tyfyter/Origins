using ModLiquidLib.ID;
using ModLiquidLib.ModLoader;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Tools.Liquids {
	#region Base Classes
	//This is an example of a modded bucket
	//Here we do some extra logic to make our bucket dispense liquid
	//While also using a new ID Set added by ModLiquid Library to allow buckets to create this item
	public abstract class BucketBase<TLiquid> : BucketBase where TLiquid : ModLiquid {
		public override int LiquidType => LiquidLoader.LiquidType<TLiquid>();
	}
	[ReinitializeDuringResizeArrays]
	public abstract class BucketBase : ModItem {
		public string GetTexture() {
			if (Debug) return LiquidLoader.GetLiquid(LiquidType).Texture;
			return base.Texture;
		}
		public override string Texture => GetTexture();
		public static int[] EndlessBucketByLiquid { get; } = LiquidID_TLmod.Sets.Factory.CreateIntSet();
		public abstract int LiquidType { get; }
		public virtual bool Endless { get; }
		public virtual bool Debug { get; }
		public virtual bool LavaImmune => true;
		public virtual bool LargePour => (Endless || Debug) && OriginsModIntegrations.Avalon is not null;
		//The SetStaticDefaults of a bucket
		public override void SetStaticDefaults() {
			ItemID.Sets.IsLavaImmuneRegardlessOfRarity[Type] = LavaImmune;
			ItemID.Sets.AlsoABuildingItem[Type] = true; //Unused ID Set, but may be useful for other modders or when the game updates
			ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;

			//Here is how we make buckets create this item
			//Using the CreateLiquidBucketItem ID Set, we are able to set which liquid creates an item as well as which item its creates
			//NOTE: a liquid can only create 1 bucket item at a time
			if (!Debug) LiquidID_TLmod.Sets.CreateLiquidBucketItem[LiquidType] = Type;

			if (Endless) {
				ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
				EndlessBucketByLiquid[LiquidType] = Type;
			} else if (Debug) {
				Item.ResearchUnlockCount = 0;
			} else {
				Item.ResearchUnlockCount = 5;
				OriginExtensions.InsertIntoShimmerCycle(Type, ItemID.HoneyBucket);
			}
			SafeSetStaticDefaults();
		}
		public virtual void SafeSetStaticDefaults() { }

		//The normal SetDefaults for a bucket
		public override void SetDefaults() {
			/*Item.width = 20;
			Item.height = 20;
			Item.useTurn = true;
			Item.autoReuse = true;*/
			if (Endless || Debug) {
				Item.CloneDefaults(ItemID.BottomlessBucket);
				if (Debug) {
					Item.value = 0;
					Item.tileBoost += 10;
				}
				/*Item.maxStack = 1;
				Item.useAnimation = 12;
				Item.useTime = 5;
				Item.rare = ItemRarityID.Lime;
				Item.value = Item.sellPrice(0, 10);
				Item.tileBoost += 2;*/
			} else {
				Item.CloneDefaults(ItemID.WaterBucket);
				/*Item.maxStack = Item.CommonMaxStack;
				Item.useAnimation = 15;
				Item.useTime = 10;*/
			}
			//Item.useStyle = ItemUseStyleID.Swing;
			SafeSetDefaults();
		}
		public virtual void SafeSetDefaults() { }
		public virtual int GetLiquid(int x, int y) => LiquidType;
		public override bool AltFunctionUse(Player player) {
			return LargePour;
		}
		//Here is how we do the bucket logic. Calculating when the player is able to place a liquid tile or not.
		//This is taken from the Lava bucket logic
		//We use HoldItem, as its the hook/method ran just before the bucket logic is ran.
		//This is so no other extra item logic is run inbetween this hook/method and when we would normally have bucket logic called
		public override void HoldItem(Player player) {
			if (!player.JustDroppedAnItem)//make sure the player hasn't just dropped an item recently
			{
				if (player.whoAmI != Main.myPlayer) //make sure to only run on clients (we sync multiplayer data later down the line)
					return;
				//We make sure the player is in range to be able to place the liquid tile
				if (player.noBuilding || !player.IsTargetTileInItemRange(Item))
					return;
				//We set the cursor's item to be of the bucket item. To show the player that they are able to place a liquid tile at that coordinate normally
				if (!Main.GamepadDisableCursorItemIcon) {
					player.cursorItemIconEnabled = true;
					Main.ItemIconCacheUpdate(Item.type);
				}
				//Make sure that the item is being used to place liquid
				if (!player.ItemTimeIsZero || player.itemAnimation <= 0 || !player.controlUseItem)
					return;

				ContentExtensions.PourBucket(Item, player, GetLiquid, LargePour && player.altFunctionUse == 2, bottomless: Endless || Debug);
			}
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (LargePour && OriginsModIntegrations.Avalon is not null) {
				tooltips.Insert("Tooltip0", Language.GetTextValue("Mods.Avalon.TooltipEdits.BottomlessBuckets"));
			}
		}
	}
	#endregion
	#region Debug
	// TODO: uncomment me when Ameebic Gel liquid is created
	/*public class Amebic_Bucket : BucketBase<Amebic_Gel> {
		public override bool Debug => true;
	}*/
	#endregion
}
