using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
	public byte? Value = null;

	[Header("KTaNE Boilerplate")]
	public KMAudio Audio;

	[Header("Label in corner")]
	public TextMesh text;
	public MeshRenderer textRenderer;

	[Header("Main number")]
	public TextMesh number;

	public void Awake() {
		this.GetComponent<KMSelectable>().OnInteract += OnClick;
	}

	public void SetText(string text) {
		this.text.text = text;
	}

	public void Clear() {
		this.Value = null;
		RedrawButton();
	}

	bool OnClick() {
		Audio.PlayGameSoundAtTransform(
			KMSoundOverride.SoundEffect.ButtonPress,
			this.transform
		);
		this.Value = this.Value == null ? 1 :
			(byte?)((this.Value % KenKenScript.BOARD_SIZE) + 1);
		RedrawButton();
		return false;
	}

	void RedrawButton() {
		this.textRenderer.enabled = this.Value == null;
		this.number.text = this.Value == null ? "" : this.Value.ToString();
	}
}
