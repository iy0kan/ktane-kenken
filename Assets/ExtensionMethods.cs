using System.Collections.Generic;
using System.Linq;

public static class ListSum {
    public static int Product(this IEnumerable<int> xs) {
        return xs.Aggregate(1, (a, b) => a * b);
    }
}
