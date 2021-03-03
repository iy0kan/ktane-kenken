using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class KenKenScript : MonoBehaviour {
	[Header("KTaNE Boilerplate")]
	public KMBombInfo Bomb;
	public KMBombModule Module;
	public KMAudio Audio;

	[Header("Buttons")]
	public KMSelectable clearButton;
	public KMSelectable submitButton;

	[Header("Board")]
	public GameObject background;
	public const int BOARD_SIZE = 4;
	public GameObject cell;

	private Cell[,] cells;
	private byte[,] soln;

	// for logging
	private static int _next_id = 1;
	private int id;

	void Awake() {
		this.id = _next_id++;

		AddButtonBehavior(clearButton);
		AddButtonBehavior(submitButton);
		clearButton.OnInteract += delegate() {
			StartCoroutine(DoClear());
			return false;
		};
		submitButton.OnInteract += OnSubmit;

		this.soln = MakeSoln();
		DrawBoard();
		for(int i=0; i<BOARD_SIZE; i++) {
			for(int j=0; j<BOARD_SIZE; j++) {
				this.cells[i,j].SetText(this.soln[i,j].ToString());
			}
		}
	}

	void Log(string str) {
		Debug.LogFormat(@"[KenKen {0}] {1}", this.id, str);
	}

	void AddButtonBehavior(KMSelectable btn) {
		btn.OnInteract += delegate() {
			StartCoroutine(ButtonPressAnimation(btn.transform));
			Audio.PlayGameSoundAtTransform(
				KMSoundOverride.SoundEffect.BigButtonPress,
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

	IEnumerator DoClear() {
		if(this.cells == null) yield break;
		for(int i=0; i<2*BOARD_SIZE-1; i++) {
			int lo = Mathf.Max(0, i-BOARD_SIZE+1);
			int hi = Mathf.Min(BOARD_SIZE-1, i);
			for(int j=lo; j<=hi; j++) {
				this.cells[j, i-j].Clear();
			}
			yield return new WaitForSeconds(0.02f);
		}
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
