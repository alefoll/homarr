using System.Collections.Generic;

namespace homarr.Serie {
    public record class Season(
        int SeasonNumber = 0,
        IEnumerable<Episode> Episodes = null
    );

}