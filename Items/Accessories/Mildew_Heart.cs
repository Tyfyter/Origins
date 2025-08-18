using BetterDialogue.UI.VanillaChatButtons;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using Origins.Dev;
using Origins.Journal;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ID;
using Terraria.ModLoader;

namespace Origins.Items.Accessories {
	public class Mildew_Heart : ModItem, ICustomWikiStat, IJournalEntrySource {
		public string[] Categories => [
			"Combat"
		];
		public string EntryName => "Origins/" + typeof(Mildew_Heart_Entry).Name;
		public class Mildew_Heart_Entry : JournalEntry {
			public override string TextKey => "Mildew_Heart";
			public override JournalSortIndex SortIndex => new("Brine_Pool_And_Lost_Diver", 8);
		}
		public override void Load() {
			try {
				IL_NetMessage.SendData += DontDieWrongRemotely;
			} catch (Exception e) {
				if (Origins.LogLoadingILError(nameof(DontDieWrongRemotely), e)) throw;
			}
		}
		static void DontDieWrongRemotely(ILContext il) {
			ILCursor c = new(il);
			ILLabel[] cases = default;
			c.GotoNext(
				i => i.MatchLdarg0(),
				i => i.MatchLdcI4(1),
				i => i.MatchSub(),
				i => i.MatchSwitch(out cases)
			);
			c.GotoLabel(cases[MessageID.PlayerLifeMana - 1]);
			c.GotoNext(MoveType.After, i => i.MatchLdfld<Player>(nameof(Player.statLife)));
			c.EmitLdarg(4); // number
			c.EmitDelegate((int statLife, int number) => {
				if (statLife <= 0 && !Main.player[number].dead && Main.player[number].OriginPlayer().mildewHeart) statLife = 1;
				return statLife;
			});
		}
		public override void SetDefaults() {
			Item.DefaultToAccessory(28, 22);
			Item.value = Item.sellPrice(gold: 4);
			Item.rare = ItemRarityID.LightRed;
		}
		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.OriginPlayer().mildewHeart = true;
		}
	}
	public class Mildew_Heart_Display : ModResourceOverlay {
		public delegate void MoldDrawer(ResourceOverlayDrawContext context);
		public delegate void MoldBarDrawer(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet);
		public static Dictionary<IPlayerResourcesDisplaySet, MoldDrawer> MoldDrawers { get; private set; } = [];
		public static Dictionary<IPlayerResourcesDisplaySet, MoldBarDrawer> MoldBarDrawers { get; private set; } = [];
		Asset<Texture2D> normalHeart;
		Asset<Texture2D> barTexture;
		public override void Load() {
			normalHeart = ModContent.Request<Texture2D>("Origins/UI/Heart_Mildew");
			barTexture = ModContent.Request<Texture2D>("Origins/UI/HP_Mildew");
			MoldDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("Default"), DoMoldHeartDraw);

			MoldDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("New"), DoCoolMoldHeartDraw);
			MoldDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("NewWithText"), DoCoolMoldHeartDraw);

			MoldDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("HorizontalBarsWithFullText"), DoMoldBarDraw);
			MoldBarDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("HorizontalBarsWithFullText"), BarDrawer(2));

			MoldDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("HorizontalBarsWithText"), DoMoldBarDraw);
			MoldBarDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("HorizontalBarsWithText"), BarDrawer(4));

			MoldDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("HorizontalBars"), DoMoldBarDraw);
			MoldBarDrawers.Add(Main.ResourceSetsManager.GetDisplaySet("HorizontalBars"), BarDrawer(0));
		}
		void DoCoolMoldHeartDraw(ResourceOverlayDrawContext context) {
			if (!context.texture.Name.Contains("Heart_Fill")) return;
			float displayHealth = Main.LocalPlayer.OriginPlayer().mildewHealth;
			for (int i = 0; i < context.resourceNumber; i++) {
				displayHealth -= context.snapshot.LifePerSegment;
				if (displayHealth <= 0) return;
			}
			float mult = 255 / context.color.A;
			if (displayHealth < context.snapshot.LifePerSegment) {
				mult *= displayHealth / context.snapshot.LifePerSegment;
			}
			context.color = Color.White * ((context.scale.X + 1) * 0.5f);
			context.scale = Vector2.One * mult;
			context.texture = normalHeart;
			context.Draw();
		}
		void DoMoldHeartDraw(ResourceOverlayDrawContext context) {
			if (context.texture != TextureAssets.Heart && context.texture != TextureAssets.Heart2) return;
			float displayHealth = Main.LocalPlayer.OriginPlayer().mildewHealth;
			for (int i = 0; i < context.resourceNumber; i++) {
				displayHealth -= context.snapshot.LifePerSegment;
				if (displayHealth <= 0) return;
			}
			float mult = 255 / context.color.A;
			if (displayHealth < context.snapshot.LifePerSegment) {
				mult *= displayHealth / context.snapshot.LifePerSegment;
			}
			context.color *= mult;
			context.texture = normalHeart;
			context.Draw();
		}
		MoldBarDrawer BarDrawer(int offsetFromText) => (PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet) => {
			void LifeFillingDrawer(int elementIndex, int firstElementIndex, int lastElementIndex, out Asset<Texture2D> sprite, out Vector2 offset, out float drawScale, out Rectangle? sourceRect) {
				sprite = barTexture;
				HorizontalBarsPlayerResourcesDisplaySet.FillBarByValues(elementIndex, sprite, snapshot.AmountOfLifeHearts, Main.LocalPlayer.OriginPlayer().mildewHealth / (float)snapshot.LifeMax, out offset, out drawScale, out sourceRect);
				int drawIndexOffset = lastElementIndex - (elementIndex - firstElementIndex) - elementIndex;
				offset.X += drawIndexOffset * sprite.Width();
			}
			Vector2 vector = new Vector2(Main.screenWidth - 300 - 22 + 16, 18 + offsetFromText);
			vector.X += (20 - snapshot.AmountOfLifeHearts) * barTexture.Width();
			bool isHovered = false;
			ResourceDrawSettings resourceDrawSettings = default;
			resourceDrawSettings.ElementCount = snapshot.AmountOfLifeHearts;
			resourceDrawSettings.ElementIndexOffset = 0;
			resourceDrawSettings.TopLeftAnchor = vector + new Vector2(6f, 6f);
			resourceDrawSettings.GetTextureMethod = LifeFillingDrawer;
			resourceDrawSettings.OffsetPerDraw = new Vector2(barTexture.Width(), 0f);
			resourceDrawSettings.OffsetPerDrawByTexturePercentile = Vector2.Zero;
			resourceDrawSettings.OffsetSpriteAnchor = Vector2.Zero;
			resourceDrawSettings.OffsetSpriteAnchorByTexturePercentile = Vector2.Zero;
			resourceDrawSettings.StatsSnapshot = snapshot;
			resourceDrawSettings.DisplaySet = displaySet;
			resourceDrawSettings.Draw(Main.spriteBatch, ref isHovered);
		};
		void DoMoldBarDraw(ResourceOverlayDrawContext context) {
			if (context.texture != barTexture) return;
			float displayHealth = Main.LocalPlayer.statLife;
			for (int i = 0; i < context.resourceNumber; i++) {
				displayHealth -= context.snapshot.LifePerSegment;
			}
			if (displayHealth >= context.snapshot.LifePerSegment) return;

			context.color = Color.Gray;
			if (context.source is Rectangle frame && frame.Width > 0) {
				frame.Width -= (int)Math.Max(context.texture.Width() * displayHealth / context.snapshot.LifePerSegment, 0);
				context.source = frame;
				context.Draw();
			}
		}
		public override void Unload() => MoldDrawers = null;
		public override void PostDrawResource(ResourceOverlayDrawContext context) {
			if (Main.LocalPlayer.OriginPlayer().mildewHeart && MoldDrawers.TryGetValue(Main.ResourceSetsManager.ActiveSet, out MoldDrawer moldDrawer)) {
				moldDrawer(context);
			}
		}
		public override void PostDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, Color textColor, bool drawText) {
			if (drawingLife && Main.LocalPlayer.OriginPlayer().mildewHeart && MoldBarDrawers.TryGetValue(Main.ResourceSetsManager.ActiveSet, out MoldBarDrawer moldBarDrawer)) {
				moldBarDrawer(snapshot, displaySet);
			}
		}
	}
}
