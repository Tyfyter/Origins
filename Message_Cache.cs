using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Origins {
	public interface ITicker {
		void Tick();
	}
	public abstract class Message_Cache<E> : ILoadable, ITicker where E : struct, Enum {
		FieldInfo instanceField;
		protected readonly Dictionary<E, int> cooldowns = [];
		readonly Dictionary<E, Refresh_Cache> messagesByType = [];
		void ILoadable.Load(Mod mod) {
			Load();
			instanceField = GetType().GetField("Instance", BindingFlags.Public | BindingFlags.Static);
			if(instanceField is not null) {
				if (instanceField.FieldType != instanceField.DeclaringType) instanceField = null;
				instanceField?.SetValue(null, this);
			}
			foreach (E item in Enum.GetValues<E>()) cooldowns.TryAdd(item, 0);
			Origins.tickers.Add(this);
		}
		void ILoadable.Unload() {
			Unload();
			instanceField?.SetValue(null, null);
		}
		void ITicker.Tick() {
			foreach (E key in cooldowns.Keys) {
				int value = cooldowns[key];
				if (value > 0) cooldowns[key] = value - 1;
			}
		}
		public abstract string KeyBase { get; }
		public virtual void Load() { }
		public virtual void Unload() { }
		public abstract void StartCooldown(E type);
		public virtual void PlayMessage(string text, Vector2 position, Vector2? velocity = null) {
			PopupText.NewText(new AdvancedPopupRequest() {
				Text = text,
				DurationInFrames = 120,
				Velocity = velocity ?? new Vector2(Main.rand.NextFloatDirection() * 7f, -2f + Main.rand.NextFloat() * -2f),
				Color = new Color(242, 250, 255)
			}, position);
			SoundEngine.PlaySound(SoundID.LucyTheAxeTalk, position);
		}
		public string GetRandomVariation(E type) {
			if (messagesByType.TryGetValue(type, out Refresh_Cache refreshCache) && refreshCache.LastCulture?.IsActive == true && refreshCache.LastGameMode == Main.GameModeInfo.Id) {
				return Main.rand.Next(messagesByType[type].Cache);
			}
			List<string> cache = [];
			static bool TryGetText(string key, out string text) {
				if (Language.Exists(key)) {
					text = Language.GetTextValue(key);
					return true;
				}
				text = key;
				return false;
			}
			if (TryGetText($"{KeyBase}.{type}", out string quote)) {
				cache.Add(quote);
			} else {
				int count = 0;
				while (TryGetText($"{KeyBase}.{type}_{count}", out quote)) {
					cache.Add(quote);
					count++;
				}
			}
			if (Main.expertMode) {
				if (TryGetText($"{KeyBase}.{type}_Expert", out quote)) {
					cache.Add(quote);
				} else {
					int count = 0;
					while (TryGetText($"{KeyBase}.{type}_Expert_{count}", out quote)) {
						cache.Add(quote);
						count++;
					}
				}
			}
			if (Main.masterMode) {
				if (TryGetText($"{KeyBase}.{type}_Master", out quote)) {
					cache.Add(quote);
				} else {
					int count = 0;
					while (TryGetText($"{KeyBase}.{type}_Master_{count}", out quote)) {
						cache.Add(quote);
						count++;
					}
				}
			}
			messagesByType[type] = new([..cache], LanguageManager.Instance.ActiveCulture, Main.GameModeInfo.Id);
			if (cache.Count == 0) {
				messagesByType[type] = new([$"missingno ({KeyBase}.{type})"], LanguageManager.Instance.ActiveCulture, Main.GameModeInfo.Id);
				return messagesByType[type].Cache[0];
			}
			return Main.rand.Next(cache);
		}
		public void PlayRandomMessage(E type, Vector2 position, Vector2? velocity = null) {
			if (cooldowns[type] <= 0) {
				PlayMessage(
					GetRandomVariation(type),
					position,
					velocity
				);
				StartCooldown(type);
			}
		}
		record struct Refresh_Cache(string[] Cache, GameCulture LastCulture, int LastGameMode);
	}
}
