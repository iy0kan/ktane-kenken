using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;

public class KenKenScript : MonoBehaviour {
	public KMBombInfo Bomb;
	public KMBombModule Module;
	public KMAudio Audio;

	public KMSelectable clearButton;
	public KMSelectable submitButton;
	public Renderer bounds;

	private static int NEXT_ID = 1;
	private int id;

	void Awake() {
		this.id = ++NEXT_ID;

		AddButtonBehavior(clearButton);
		AddButtonBehavior(submitButton);
		clearButton.OnInteract += OnClear;
		submitButton.OnInteract += OnSubmit;

		Debug.Log(bounds.bounds);
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

	bool OnClear() {
		Module.HandleStrike();
		return false;
	}

	bool OnSubmit() {
		Module.HandlePass();
		return false;
	}
}
