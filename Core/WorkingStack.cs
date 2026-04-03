using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Terraria;

namespace Origins.Core {
	public class WorkingStack<T> {
		Stack<T> working = [];
		Stack<T> active = [];
		public void Finish() {
			Utils.Swap(ref working, ref active);
			working.Clear();
		}
		public void Push(T item) => working.Push(item);
		public T Pop() => active.Pop();
		public bool TryPop([MaybeNullWhen(false)] out T result) => active.TryPop(out result);
		public bool Contains(T item) => active.Contains(item);
	}
	public class WorkingHashSet<T> : IEnumerable<T> {
		HashSet<T> working = [];
		HashSet<T> active = [];
		public void Finish() {
			Utils.Swap(ref working, ref active);
			working.Clear();
		}
		public void Add(T item) => working.Add(item);
		public bool Contains(T item) => active.Contains(item);
		public IEnumerator<T> GetEnumerator() => active.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)active).GetEnumerator();
	}
}
