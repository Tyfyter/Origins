using Microsoft.Xna.Framework.Graphics;
using Origins.Buffs;
using Origins.Dev;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace Origins.Items.Other.Consumables {
	public class Mojo_Injection : ModItem, ICustomWikiStat {
		public const float healing = 0.0000444f;
		public string[] Categories => [
			WikiCategories.PermaBoost
		];
		public override void SetStaticDefaults() {
			ItemID.Sets.ShimmerTransformToItem[ItemID.NaturesGift] = Type;
			glowmask = Origins.AddGlowMask(this);
		}
		static short glowmask;
		public override void SetDefaults() {
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.buyPrice(silver: 40);
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.width = 16;
			Item.height = 26;
			Item.accessory = false;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = SoundID.Item3;
			Item.glowMask = glowmask;
			Item.consumable = true;
		}
		public override bool? UseItem(Player player) {
			ref bool mojoInjection = ref player.GetModPlayer<OriginPlayer>().mojoInjection;
			if (mojoInjection) return false;
			mojoInjection = true;
			return true;
		}
		public static void UpdateEffect(OriginPlayer originPlayer) {
			foreach (AssimilationInfo info in originPlayer.IterateAssimilation()) {
				info.Percent -= Math.Min(healing, info.Percent);
			}
			/*originPlayer.CorruptionAssimilation -= Math.Min(healing, originPlayer.CorruptionAssimilation);
			originPlayer.CrimsonAssimilation -= Math.Min(healing, originPlayer.CrimsonAssimilation);
			originPlayer.DefiledAssimilation -= Math.Min(healing, originPlayer.DefiledAssimilation);
			originPlayer.RivenAssimilation -= Math.Min(healing, originPlayer.RivenAssimilation);*/
		}
	}
	public class Mojo_Injection_Toggle : BuilderToggle {
		public override string HoverTexture => Texture;
		public override Position OrderPosition => new Before(HideAllWires);
		public override bool Active() => OriginPlayer.LocalOriginPlayer?.mojoInjection ?? false;
		public override string DisplayValue() => Language.GetOrRegister($"Mods.Origins.Items.{nameof(Mojo_Injection)}.Toggle_" + (CurrentState == 0 ? "On" : "Off")).Value;
		public override bool Draw(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			drawParams.Frame.Y = 0;
			drawParams.Frame.Height = 18;
			switch (CurrentState) {
				case 1://disabled
				drawParams.Color = drawParams.Color.MultiplyRGB(Color.Gray);
				break;
			}
			return true;
		}
		public override bool DrawHover(SpriteBatch spriteBatch, ref BuilderToggleDrawParams drawParams) {
			drawParams.Frame.Y = 20;
			drawParams.Frame.Height = 18;
			return true;
		}
	}
}
