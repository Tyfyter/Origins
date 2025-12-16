using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Origins.Journal;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.GameContent;
using Terraria.UI.Chat;

namespace Origins.UI {
	public class Journal_Control_Handler : ITagHandler {
		public class Journal_Control_Snippet : TextSnippet {
			public readonly string options;

			public Journal_Control_Snippet(string text, string options, Color color = default) : base(text, color) {
				this.options = options;
				int skipLength = 1;
				Process = _ => { };
				switch (options.ToLower()) {
					case "telemetry":
					if (!DebugConfig.Instance.DebugMode) return;
					switch (text.ToLower()) {
						case "begin_track_length":
						Process = parameters => parameters.telemetryLength = 0;
						break;
						case "state_track_length":
						Process = parameters => Main.NewText($"length: " + parameters.telemetryLength);
						break;
					}
					return;

					case "skip_next_if":
					int cursor = 0;
					string[] symbols = mathExpressionSymbolizer.Matches(text).Select(m => m.Value).ToArray();
					int openCount = 0;
					int closeCount = 0;
					for (int i = 0; i < symbols.Length; i++) {
						if (symbols[i] == "(") openCount++;
						else if (symbols[i] == ")") closeCount++;
					}
					if (closeCount != openCount) {
						throw new Exception($"Error evaluating expression \"{text}\": opening and closing brackets must match");
					}
					Func<JournalExpressionParams, bool> condition = PegasusLib.PegasusLib.Compile<Func<JournalExpressionParams, bool>>(text, 
						ParseMathExpression(symbols, ref cursor)
						.Concat([(OpCodes.Ret, null)]
					).ToArray());
					Process = parameters => {
						if (condition(parameters.ExpressionParams)) parameters.snippetIndex += skipLength;
					};
					return;

					default:
					if (skipNextN.Match(options) is Match { Success: true } skipCount && int.TryParse(skipCount.Groups[1].Value, out skipLength)) goto case "skip_next_if";
					break;
				}
				switch (text.ToLower()) {
					case "//":
					Process = parameters => parameters.snippetIndex++;
					break;
					case "end_page":
					Process = parameters => parameters.finishPage();
					break;
				}
			}
			static readonly Regex skipNextN = new("skip_next_(\\d+)_if");
			static readonly Regex mathExpressionSymbolizer = new("(\\d+)|([a-zA-Z]+)|-|\\*|\\/|%|\\^|>=?|<=?|=|\\(|\\)");
			public static List<(OpCode, object)> ParseMathExpression(string[] symbols, ref int cursor) {
				List<(OpCode, object)> instructions = [];
				while (cursor < symbols.Length) {
					string symbol = symbols[cursor++];
					switch (symbol) {
						case "+":
						instructions.Add((OpCodes.Add, null));
						break;
						case "-":
						instructions.Add((OpCodes.Sub, null));
						break;
						case "*":
						instructions.Add((OpCodes.Mul, null));
						break;
						case "/":
						instructions.Add((OpCodes.Div, null));
						break;
						case "%":
						instructions.Add((OpCodes.Rem, null));
						break;
						case "^":
						instructions.Add((OpCodes.Call, typeof(Journal_Control_Snippet).GetMethod(nameof(Pow))));
						break;

						case ">":
						instructions.Add((OpCodes.Cgt, null));
						break;
						case ">=":
						instructions.Add((OpCodes.Clt, null));
						instructions.Add((OpCodes.Ldc_I4_0, null));
						instructions.Add((OpCodes.Ceq, null));
						break;
						case "<":
						instructions.Add((OpCodes.Clt, null));
						break;
						case "<=":
						instructions.Add((OpCodes.Cgt, null));
						instructions.Add((OpCodes.Ldc_I4_0, null));
						instructions.Add((OpCodes.Ceq, null));
						break;
						case "=":
						instructions.Add((OpCodes.Ceq, null));
						break;

						case "(":
						instructions.AddRange(ParseMathExpression(symbols, ref cursor));
						break;

						case ")":
						return instructions;

						default:
						if (int.TryParse(symbol, out int constant)) {
							instructions.Add((OpCodes.Ldc_I4, constant));
						} else {
							instructions.Add((OpCodes.Ldarga, 0));
							instructions.Add((OpCodes.Call, typeof(JournalExpressionParams).GetProperty(symbol).GetMethod));
						}
						break;
					}
				}
				return instructions;
			}
			public delegate void JournalControlProcess(JournalControlParams parameters);
			public JournalControlProcess Process { get; private set; }
			public static int Pow(int @base, int exponent) {
				int ret = 1;
				while (exponent != 0) {
					if ((exponent & 1) == 1)
						ret *= @base;
					@base *= @base;
					exponent >>= 1;
				}
				return ret;
			}
		}
		public readonly ref struct JournalControlParams(Journal_UI_Open self, Action finishPage, ref int snippetIndex, ref float telemetryLength, int maxLines, int maxWidth) {
			public readonly Journal_UI_Open self = self;
			public readonly Action finishPage = finishPage;
			public readonly ref int snippetIndex = ref snippetIndex;
			public readonly ref float telemetryLength = ref telemetryLength;
			public readonly JournalExpressionParams ExpressionParams => new(maxLines, maxLines * maxWidth);
		}
		public record struct JournalExpressionParams(int MaxLines, int MaxLength);
		public TextSnippet Parse(string text, Color baseColor = default, string options = null) {
			return new Journal_Control_Snippet(text, options, baseColor);
		}
	}
}
