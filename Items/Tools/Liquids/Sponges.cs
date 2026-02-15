using ModLiquidLib.ID;
using ModLiquidLib.ModLoader;
using Origins.Liquids;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins.Items.Tools.Liquids {
	#region Base Classes
	//This is an example of a modded sponge
	//Here we make this item only absorb Example Liquid using very similar logic from that of ExampleLiquidBucket
	public abstract class SpongeBase<TLiquid> : SpongeBase where TLiquid : ModLiquid {
		public override int LiquidType => LiquidLoader.LiquidType<TLiquid>();
	}
	public abstract class SpongeBase : ModItem {
		public abstract int LiquidType { get; }
		//The SetStaticDefaults of a sponge
		public override void SetStaticDefaults() {
			ItemID.Sets.AlsoABuildingItem[Type] = true; //Unused, but useful to have here for both other mods and future game updates
			ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;
			ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
			if (BucketBase.EndlessBucketByLiquid[LiquidType] > -1) {
				ItemID.Sets.ShimmerTransformToItem[Type] = BucketBase.EndlessBucketByLiquid[LiquidType];
				ItemID.Sets.ShimmerTransformToItem[BucketBase.EndlessBucketByLiquid[LiquidType]] = Type;
			}

			//Unlike buckets, sponges have extra functionality to allow the removing and adding of sponge items to liquids
			LiquidID_TLmod.Sets.CanBeAbsorbedBy[LiquidType].Add(Type);
			SafeSetStaticDefaults();
		}
		public virtual void SafeSetStaticDefaults() { }
		//The SetDefaults of a sponge
		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.SuperAbsorbantSponge);
			SafeSetDefaults();
		}
		public virtual void SafeSetDefaults() { }
		public override bool AltFunctionUse(Player player) {
			return OriginsModIntegrations.Avalon is not null;
		}

		//Here in HoldItem we do our sponge logic
		//We use HoldItem as it is the hook/method run right before sponge logic
		//This is so no extra item logic is run inbetween this hook/method and when the normal sponge logic would occur
		public override void HoldItem(Player player) {
			if (!player.JustDroppedAnItem)//make sure the player hasn't dropped an item
			{
				if (player.whoAmI != Main.myPlayer)//only run this logic on clients, we sync this in multiplayer later
				{
					return;
				}
				//we check that the player's cursor is in range to suck up liquid
				if (player.noBuilding || !player.IsTargetTileInItemRange(Item)) {
					return;
				}
				//We then set the player's cursor to be that of this item
				if (!Main.GamepadDisableCursorItemIcon) {
					player.cursorItemIconEnabled = true;
					Main.ItemIconCacheUpdate(Item.type);
				}
				//Make sure that the player isn't using the item
				if (!player.ItemTimeIsZero || player.itemAnimation <= 0 || !player.controlUseItem) {
					return;
				}
				Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
				if (LiquidID_TLmod.Sets.CanBeAbsorbedBy[tile.LiquidType].Contains(Type))//tile.LiquidType == LiquidType) //if the cursor's liquid selected is example liquid, then run the following logic
				{
					int origType = tile.LiquidType;
					if (tile.LiquidAmount <= 0) {
						return;
					}
					ContentExtensions.SpongeAbsorb(Item, player, tile, origType, player.altFunctionUse == 2);
				}
			}
		}
		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (OriginsModIntegrations.Avalon is not null) {
				tooltips.Insert("Tooltip0", "Tooltip0", Language.GetTextValue("Mods.Avalon.TooltipEdits.Sponges"));
			}
		}
	}
	#endregion
	public class Oil_Sponge : SpongeBase<Oil> {
		public override void SafeSetStaticDefaults() {
			LiquidID_TLmod.Sets.CanBeAbsorbedBy[LiquidLoader.LiquidType<Burning_Oil>()].Add(Type);
		}
	}
}