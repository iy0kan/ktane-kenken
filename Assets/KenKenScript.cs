using System.Collections;
using System.Collections.Generic;
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
	private const int BOARD_SIZE = 4;
	public GameObject cell;

	private Cell[,] cells;

	// for logging
	private static int NEXT_ID = 1;
	private int id;

	void Awake() {
		this.id = NEXT_ID++;

		AddButtonBehavior(clearButton);
		AddButtonBehavior(submitButton);
		clearButton.OnInteract += delegate() {
			StartCoroutine(DoClear());
			return false;
		};
		submitButton.OnInteract += OnSubmit;

		DrawBoard();
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

	float lerp(float lo, float hi, int i, int max) {
		float n = i / (float)(max-1);
		return (1-n)*lo + n*hi;
	}

	void DrawBoard() {
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
					lerp(min.x, max.x, i, BOARD_SIZE),
					max.y + 0.001f,
					lerp(min.z, max.z, j, BOARD_SIZE)
				);
				cells[i,j] = cell.GetComponent<Cell>();
				cells[i,j].SetText("256×");
				cells[i,j].textRenderer.enabled = false;
				cells[i,j].Audio = this.Audio;
			}
		}
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
		Module.HandlePass();
		return false;
	}
}
