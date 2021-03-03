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
	public Renderer bounds;
	private const int BOARD_SIZE = 4;
	[Range(0f, 0.01f)] public float margin = 0.008f;
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
				KMSoundOverride.SoundEffect.ButtonPress,
				this.transform
			);
			return false;
		};
	}

	IEnumerator ButtonPressAnimation(Transform loc) {
		yield return null;
	}

	void DrawBoard() {
	}

	IEnumerator DoClear() {
		if(this.cells == null) yield break;
		for(int i=0; i<2*BOARD_SIZE-1; i++) {
			int lo = Mathf.Max(0, BOARD_SIZE-i);
			int hi = Mathf.Min(BOARD_SIZE, i);
			for(int j=lo; j<hi; j++) {
				this.cells[j, i-j].Clear();
			}
			yield return new WaitForSeconds(0.05f);
		}
	}

	bool OnSubmit() {
		Module.HandlePass();
		return false;
	}
}
