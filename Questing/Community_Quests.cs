using Origins.Core.Structures;
using Origins.Items.Accessories;
using Origins.UI;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace Origins.Questing;
public abstract class CommunitySuggestionsQuest : Quest {
	public abstract string BaseKey { get; }
	public abstract int HandlerNPC { get; }
	public virtual bool IsFinished => false;
	public QuestChatButton Button { get; protected set; }
	public override bool SaveToWorld => false;
	public override int Stage {
		get => field;
		set => field = value;
	}
	public List<string> Suggested {
		get => field ??= [];
		set => field = value ?? [];
	}
	public override bool Started => Stage > 0;
	public override bool Completed => Stage > 1;
	public override void Load() {
		Mod.AddContent(Button = new Send_Suggestion_Chat_Button(this));
	}
	public sealed override bool CanStart(NPC npc) => npc.type == HandlerNPC && Stage == 0 && CanStart();
	public abstract bool CanStart();
	public override bool HasQuestButton(NPC npc, Player player) => !Completed;
	public override string GetInquireText(NPC npc) => Language.GetTextValue(BaseKey + "Inquire");
	public override void OnAccept(NPC npc) {
		Stage = 1;
		Main.npcChatText = Language.GetTextValue(BaseKey + "Start");
	}
	public override string ReadyToCompleteText(NPC npc) => Language.GetOrRegister(BaseKey + "ReadyToComplete").Value;
	public override bool CanComplete(NPC npc) => false;
	public override void OnComplete(NPC npc) {
		Main.npcChatText = Language.GetTextValue(BaseKey + "Complete");
		Stage = 2;
		ShouldSync = true;
	}
	public override string GetJournalPage() {
		return Language.GetTextValue(
			BaseKey + "Journal",
			string.Join('\n', Suggested)
		);
	}
	public override void SetStaticDefaults() {
		NameKey = BaseKey + "Name";
	}
	public override void SaveData(TagCompound tag) {
		//save stage and kills
		if (!IsFinished) tag.Add("Stage", Stage);
		tag.Add("Suggested", Suggested);
	}
	public override void LoadData(TagCompound tag) {
		//load stage and kills, note that it uses the Stage property so that it sets the event handlers
		//SafeGet returns the default value (0 for ints) if the tag doesn't have the data
		if (IsFinished) Stage = 2;
		else Stage = tag.SafeGet<int>("Stage");
		Suggested = tag.SafeGet<List<string>>("Suggested");
	}
	protected readonly HttpClient httpClient = new() {
		Timeout = TimeSpan.FromSeconds(10)
	};
	public async void Suggest(string idea, Color color, NPC npc) {
		string identifier = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(BitConverter.GetBytes(SteamUser.GetSteamID().m_SteamID)));
		HttpResponseMessage response = await httpClient.GetAsync("https://raw.githubusercontent.com/Tyfyter/Origins/refs/heads/master/blockedSuggestors.txt");
		if (!response.IsSuccessStatusCode) {
			Main.QueueMainThreadAction(() => Main.NewText($"Error {(int)response.StatusCode}: {response.ReasonPhrase}"));
			return;
		}
		if ((await response.Content.ReadAsStringAsync()).Split().Contains(identifier)) {
			Suggested.Add($"[c/{color.Hex3()}:{idea}]");
			return;
		}

		JsonContent content = JsonContent.Create(new {
			embeds = new object[] {
				new {
					title = $"from {Main.LocalPlayer.name} ({SteamFriends.GetPersonaName()})",
					description = $"({color.R}, {color.G}, {color.B}): {idea}",
					color = (int)(color.R << 16 | color.G << 8 | color.B),
					footer = new {
						text = identifier
					}
				}
			},
			username = npc.GivenOrTypeName
		});
		const string anyone_can_already_use_this = "MTU2MzI2NDM1MDg1OTMwNTA0Mi9hb0JCSjhfaVJfQjloMTJiMW9yU05QVF8tOVk5RDAzY1pWVnNPYjhiLU5hR0VNWGV3RTI5NUFSbUh1aUpaLUZvOFlBUA==";
		response = await httpClient.PostAsync($"https://discord.com/api/webhooks/{System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(anyone_can_already_use_this))}", content);
		if (response.IsSuccessStatusCode) {
			Suggested.Add($"[c/{color.Hex3()}:{idea}]");
		} else {
			Main.QueueMainThreadAction(() => Main.NewText($"Error {(int)response.StatusCode}: {response.ReasonPhrase}"));
		}
		string responseContent = await response.Content.ReadAsStringAsync();
		Main.QueueMainThreadAction(() => Origins.instance.Logger.Info($"{(int)response.StatusCode} {response.ReasonPhrase}: {responseContent}"));
	}
	public class SuggestionsUISystem : ModSystem {
		public static readonly UserInterface ui = new();
		public override void UpdateUI(GameTime gameTime) {
			ui.Update(gameTime);
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
			if (inventoryIndex != -1) {//error prevention & null check
				layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer(
					"Origins: Suggestion UI",
					delegate {
						ui?.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
		public class SuggestionState(CommunitySuggestionsQuest quest, NPC npc) : UIState {
			const string the_best_idea_ever = $"I.I.I.I.I.I.I.I.I.I.I.I.I.I.I.I .I.I.I.I.\nI.I.I.I.I.I.I.I.I.I.I.I.I.I.I.I.I.I.I.I.I.\nI.I.I.I.I.I .I.I.I.I.I.I.I.I.I.I.I.I.I.I.\nI.I.I.I.I.I.I.I.I.I.I.I.I.I.I.I.I I.I.I.I\n.I.I.I.I.I.I.I.I.I.I.I.";
			const string the_worst_idea_ever = $"iii.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.\ni.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.\ni.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.\ni.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.i.ii";
			Color color = Color.White;
			static new float Width => 432;
			static new float Height => 130;
			static new StyleDimension Left = new((Main.screenWidth - Width) * 0.5f, 0);
			static new StyleDimension Top = new(Main.screenHeight - Height, 0);
			UIPanel panel;
			UIInputPanel ideaInput;
			UIColorPicker colorPicker;
			int lastLinesCount = 0;
			public override void OnInitialize() {
				panel = new() {
					Left = Left,
					Top = Top,
					Width = new(Width, 0),
					Height = new(Height, 0),
					PaddingTop = 8,
					PaddingRight = 8
				};
				UIDragController.Attach(panel, new(Drop: UpdatePositions));
				panel.StopClickThrough();
				ideaInput = new() {
					PaddingTop = 6,
					PaddingBottom = 6,
					TextHAlign = 0,
					Width = new(0, 1),
				};
				ideaInput.OnUpdate += IdeaPanel_OnUpdate;
				ideaInput.OnSubmit += IdeaPanel_OnSubmit;
				panel.Append(ideaInput);
				colorPicker = new(() => ref color);
				panel.Append(colorPicker);
				Append(panel);
			}
			public override void Update(GameTime gameTime) {
				if (lastLinesCount.TrySet(ideaInput.Text.Count('\n') + 1)) {
					panel.Height.Set(Height + 14 + lastLinesCount * FontAssets.MouseText.Value.LineSpacing, 0);
					ideaInput.Height.Set(lastLinesCount * FontAssets.MouseText.Value.LineSpacing, 0);
					colorPicker.Top.Pixels = ideaInput.Height.Pixels + 14;
					Recalculate();
				}
				base.Update(gameTime);
				mouseLeftRelease = !Main.mouseLeft;
				if (Main.LocalPlayer.talkNPC != npc.whoAmI) ui.SetState(null);
			}
			bool mouseLeftRelease;
			void SubmitIdea() {
				Main.npcChatText = Language.GetTextValue(quest.BaseKey + "MadeSuggestion");
				quest.Suggest(ideaInput.Text, color, npc);
				ui.SetState(null);
			}
			void UpdatePositions() {
				Left = panel.Left;
				Top = panel.Top;
			}
			void IdeaPanel_OnUpdate(UIElement affectedElement) {
				if (Main.mouseLeft && mouseLeftRelease) ideaInput.IsWritingText = ideaInput.IsMouseHovering;
			}
			void IdeaPanel_OnSubmit(string hex) {
				if (Main.inputText.PressingShift()) {
					ITextInputContainer container = ideaInput;
					container.Text.Insert(container.CursorIndex, '\n');
					container.CursorIndex++;
				} else ideaInput.IsWritingText = false;
			}
		}
	}
	public static void OpenSuggestionBox(CommunitySuggestionsQuest quest, NPC npc) {
		SuggestionsUISystem.ui.SetState(new SuggestionsUISystem.SuggestionState(quest, npc));
	}
}
public class SPE_Suggestions : CommunitySuggestionsQuest {
	struct Unfinished : IBroken {
		static string IBroken.BrokenReason => "It just ain't finished";
	}
	public override string BaseKey => "Mods.Origins.Quests.Community.SPE_Suggestions.";
	public override int HandlerNPC => NPCID.Steampunker;
	public override bool CanStart() => Main.LocalPlayer.OriginPlayer().itemCounts[ModContent.ItemType<Space_Pirates_Eye>()] > 0;
}
[Autoload(false)]
file class Send_Suggestion_Chat_Button(CommunitySuggestionsQuest quest) : QuestChatButton(quest) {
	public override string Name => $"{base.Name}_{Quest.Name}";
	public override double Priority => 101;
	public override void OnClick(NPC npc, Player player) {
		CommunitySuggestionsQuest.OpenSuggestionBox(quest, npc);
	}
	public override string Text(NPC npc, Player player) => Language.GetTextValue(quest.BaseKey + "MakeSuggestion");
	public override bool IsActive(NPC npc, Player player) {
		if (!Questing.QuestListSelected || npc.type != quest.HandlerNPC) return false;
		return Quest.HasQuestButton(npc, player);
	}
}
