using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Origins.Journal {
	public interface IJournalEntryItem {
		public string IndicatorKey { get; }
		public string EntryName { get; }
	}
}
