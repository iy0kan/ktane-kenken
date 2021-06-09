using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// purity tracked wrt the RNG + internal state
// for checking implementation against JS version
public class GridChunks {
	public Dictionary<Vector2Int, byte> map;
	public Dictionary<byte, List<Vector2Int>> groups;

	private int ones;
	private int curOnes;
	private int threes;
	private int fours;

	private GridChunks() {
		this.map = new Dictionary<Vector2Int, byte>();
		this.groups = new Dictionary<byte, List<Vector2Int>>();

		byte c = 0;
		for(int i=0; i<KenKenScript.BOARD_SIZE; i++) {
			for(int j=0; j<KenKenScript.BOARD_SIZE; j++) {
				var v = new Vector2Int(i, j);
				this.map[v] = c;
				if(this.groups.ContainsKey(c))
					this.groups[c].Add(v);
				else
					this.groups[c] = new List<Vector2Int>() { v };
				c++;
			}
		}
		curOnes = KenKenScript.BOARD_SIZE * KenKenScript.BOARD_SIZE;
	}

	private static Vector2Int[] adjacents = new[] {
		new Vector2Int(-1, 0),
		new Vector2Int(1, 0),
		new Vector2Int(0, -1),
		new Vector2Int(0, 1)
	};

	// impure: rng
	public static GridChunks[,] MakeRules(MonoRandom rng) {
		var ret = new GridChunks[4, 4];
		for(int i=0; i<4; i++) {
			for(int j=0; j<4; j++) {
				ret[i,j] = (new GridChunks()).MakeBoard(rng);
			}
		}
		return ret;
	}

	// impure: rng, state
	private GridChunks MakeBoard(MonoRandom rng) {
		ones = rng.Next(3);
		threes = rng.Next(2) + 1;
		fours = (int)(rng.NextDouble() * 2.05); // 2x4 case should be rare
		var oks = new HashSet<Vector2Int>(this.map.Keys);
		while(oks.Count > 0) {
			var k = oks.ElementAt(rng.Next(oks.Count));
			var v = this.map[k];
			if(this.groups[v].Count >= 4) { oks.Remove(k); continue; }
			var neighbors = FindNeighbors(k, v);
			if(neighbors.Count == 0) { oks.Remove(k); continue; }
			bool found = false;
			foreach(var grp in neighbors
				.GroupBy(g => this.groups[g].Count)
				.SelectMany(gs => rng.ShuffleFisherYates(gs.ToList()))
			) {
				if(Merge(grp, v)) {
					found = true;
					break;
				}
			}
			if(!found) oks.Remove(k);
		}
		return this;
	}

	// pure
	private HashSet<byte> FindNeighbors(Vector2Int loc, byte thisGrp) {
		var oks = new HashSet<byte>();
		foreach(var adj in adjacents) {
			var loc_ = loc + adj;
			if(!this.map.ContainsKey(loc_)) continue;
			var grp = this.map[loc_];
			if(grp == thisGrp) continue;
			var size = this.groups[grp].Count;
			oks.Add(grp);
		}
		return oks;
	}

	// impure: state
	private bool Merge(byte src, byte dest) {
		if(curOnes <= ones) return false;
		var dc = this.groups[dest].Count;
		var sc = this.groups[src].Count;
		if(sc + dc > 4) return false;
		if(sc + dc == 4 && fours <= 0) return false;
		if(sc + dc == 3 && threes <= 0) return false;

		if(dc == 1) curOnes--;
		if(sc == 1) curOnes--;

		this.groups[dest].AddRange(this.groups[src]);
		foreach(var v in this.groups[src]) this.map[v] = dest;
		if(this.groups[dest].Count == 4) fours--;
		if(this.groups[dest].Count == 3) threes--;
		this.groups.Remove(src);
		return true;
	}
}
