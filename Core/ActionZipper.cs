using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origins.Core {
	public class ActionZipper<T>(IEnumerable<T> values, IEnumerable<Action<T>> actions) {
		readonly List<T> values = new(values);
		readonly List<Action<T>> actions = new(actions);

		public ActionZipper(params T[] values) : this(values, []) { }
		public ActionZipper(params Action<T>[] actions) : this([], actions) { }
		public ActionZipper() : this([], []) { }
		public void Add(T item) {
			values.Add(item);
			for (int i = 0; i < actions.Count; i++) actions[i](item);
		}

		public void Add(Action<T> action) {
			actions.Add(action);
			for (int i = 0; i < values.Count; i++) action(values[i]);
		}
	}
}
