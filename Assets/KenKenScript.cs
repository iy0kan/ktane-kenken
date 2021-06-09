using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Random = UnityEngine.Random;

public class KenKenScript : MonoBehaviour {
	[Header("KTaNE Boilerplate")]
	public KMBombInfo Bomb;
	public KMBombModule Module;
	public KMAudio Audio;
	public KMRuleSeedable RuleSeed;

	[Header("Buttons")]
	public KMSelectable clearButton;
	public KMSelectable submitButton;

	[Header("Board")]
	public GameObject background;
	public const int BOARD_SIZE = 4;
	public GameObject cell;

	private Cell[,] cells;
	private byte[,] soln;

	private static GridChunks[,] rules;
	private GridChunks rule;

	// for logging
	private static int _next_id = 1;
	private int id;

	void Awake() {
		this.id = _next_id++;

		if(rules == null) {
			var rng = RuleSeed.GetRNG();
			rules = GridChunks.MakeRules(rng);
		}

		AddButtonBehavior(clearButton);
		AddButtonBehavior(submitButton);
		clearButton.OnInteract += delegate() { DoClear(); return false; };
		submitButton.OnInteract += OnSubmit;

		DrawBoard();
	}

	void Start() {
		var sn = Bomb.GetSerialNumber();
		int i = (char.IsLetter(sn[0]) ? 2 : 0) + (char.IsLetter(sn[1]) ? 1 : 0);
		int j = Bomb.GetBatteryCount() % 3;
		Log("Rule: {0},{1}", i, j);
		this.rule = rules[i, j];
		Reset();
	}

	void Reset() {
		this.soln = MakeSoln();
		var labels = PlaceLabels(MakeLabels());
		for(int i=0; i<BOARD_SIZE; i++) {
			for(int j=0; j<BOARD_SIZE; j++) {
				var v = new Vector2Int(i, j);
				this.cells[i, j].SetText(
					labels.ContainsKey(v) ? labels[v] : ""
				);
			}
		}
	}

	void Log(string str, params object[] args) {
		Debug.LogFormat(
			"[KenKen {0}] {1}",
			this.id,
			String.Format(str, args)
		);
	}

	void AddButtonBehavior(KMSelectable btn) {
		btn.OnInteract += delegate() {
			StartCoroutine(ButtonPressAnimation(btn.transform));
			Audio.PlayGameSoundAtTransform(
				KMSoundOverride.SoundEffect.ButtonPress,
				this.transform
			);
			return false;
		};
	}

	IEnumerator ButtonPressAnimation(Transform loc) {
		yield return null;
	}

	float Lerp(float lo, float hi, int i, int max) {
		float n = i / (float)(max-1);
		return (1-n)*lo + n*hi;
	}

	void DrawBoard() {
		var me = this.GetComponent<KMSelectable>();
		List<KMSelectable> children = me.Children.ToList();
		Vector3 padding =
			(Vector3.one - this.cell.transform.localScale*BOARD_SIZE) / (BOARD_SIZE + 1);
		padding.y = 0f;
		Vector3 max = (Vector3.one - this.cell.transform.localScale) / 2 - padding;
		Vector3 min = -max;
		this.cells = new Cell[BOARD_SIZE,BOARD_SIZE];
		for(int i=0; i<BOARD_SIZE; i++) {
			for(int j=0; j<BOARD_SIZE; j++) {
				GameObject cell = Instantiate(this.cell, this.background.transform);
				cell.transform.localPosition = new Vector3(
					Lerp(min.x, max.x, i, BOARD_SIZE),
					max.y + 0.01f,
					Lerp(min.z, max.z, j, BOARD_SIZE)
				);
				var sel = cell.GetComponent<KMSelectable>();
				children.Add(sel);
				sel.Parent = me;
				cells[i,j] = cell.GetComponent<Cell>();
				cells[i,j].Audio = this.Audio;
			}
		}
		me.Children = children.ToArray();
		me.UpdateChildren();
	}

	byte[,] MakeSoln() {
		var range = Enumerable.Range(0, BOARD_SIZE);
		HashSet<byte>[] left = range.Select(_ => {
			var h = new HashSet<byte>();
			for(byte i=0; i<BOARD_SIZE; i++) h.Add(i);
			return h;
		}).ToArray();
		var soln = new byte[BOARD_SIZE,BOARD_SIZE];
		for(int i=0; i<BOARD_SIZE-1; i++) {
			byte[] vs = range.Select(x => (byte)x).ToArray();
			do vs.Shuffle();
			while(vs.Select((v, j) => left[j].Contains(v)).Any(x => !x));
			for(int j=0; j<BOARD_SIZE; j++) {
				left[j].Remove(vs[j]);
				soln[i,j] = (byte)(vs[j] + 1);
			}
		}
		for(int i=BOARD_SIZE-1, j=0; j<BOARD_SIZE; j++)
			soln[i,j] = (byte)(left[j].First() + 1);
		return soln;
	}

	Dictionary<byte, string> MakeLabels() {
		var ret = new Dictionary<byte, string>();
		foreach(var kvp in rule.groups) {
			List<int> vs = kvp.Value
				.Select(v2 => (int)this.soln[v2.x, v2.y])
				.ToList();
			vs.Sort();
			var opts = new List<string> {
				String.Format("{0}+", vs.Sum()),
				String.Format("{0}×", vs.Product())
			};
			if(kvp.Value.Count == 1)
				opts.Add(vs.First().ToString());
			int max = vs[vs.Count - 1];
			vs.RemoveAt(vs.Count - 1);
			int rest;
			if((rest = vs.Sum()) <= max)
				opts.Add(String.Format("{0}-", max - rest));
			if(max % (rest = vs.Product()) == 0)
				opts.Add(String.Format("{0}÷", max / rest));
			ret.Add(kvp.Key, opts[Random.Range(0, opts.Count-1)]);
		}
		return ret;
	}

	Dictionary<Vector2Int, string> PlaceLabels(
		Dictionary<byte, string> labels
	) {
		var ret = new Dictionary<Vector2Int, String>();
		foreach(var kvp in rule.groups) {
			var opts = kvp.Value;
			ret.Add(opts[Random.Range(0, opts.Count-1)], labels[kvp.Key]);
		}
		return ret;
	}

	void DoClear() {
		if(this.cells == null) return;
		foreach(var cell in this.cells) cell.Clear();
	}

	bool OnSubmit() {
		if(CheckAnswer())
			Module.HandlePass();
		else
			Module.HandleStrike();
		return false;
	}

	bool CheckAnswer() {
		for(int i=0; i<BOARD_SIZE; i++)
			for(int j=0; j<BOARD_SIZE; j++)
				if(this.cells[i,j].Value != this.soln[i,j])
					return false;
		return true;
	}
}
