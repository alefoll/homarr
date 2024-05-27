using System;
using System.Collections.Generic;

namespace homarr.History {
    public class HistoryGroup {
        public DateTime Date { get; set; }
        public string DateStringify { get; set; }
        public List<HistoryRecord> Records { get; set; }
    }
}