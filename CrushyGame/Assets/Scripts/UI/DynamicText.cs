using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DynamicText : MonoBehaviour {
	
	[Space (10)][Header ("References")]
	public TextDictionary textDictionary;
	public Transform textContainer;

	[Space (10)][Header ("Text Settings")]
	public Color textColor;
	public Color borderColor;
	public int textCount;
	public string text;
	public TextAlignment textAlignment;
	public int textSortingOrder = 499;

	float pixelUnit = (1f / 12f);
	int pixelWidth;
	GameObject prefab_Letter;
	string textCurrent;
	
	private void Start () {
		prefab_Letter = Resources.Load("Prefabs/UI/Letter", typeof(GameObject)) as GameObject;
		textContainer = transform.Find("[TextContainer]");
	}

	private void Update () {
		if (textCurrent != text) {
			SetText(text);
		}
	}

	public void SetText (string newText, Color newTextColor) {
		textColor = newTextColor;
		SetText(newText);
	}

	public void SetText (string newText) {
		text = newText;
		if (textCurrent != text) {
			// Get letter prefab
			if (prefab_Letter == null) {
				prefab_Letter = Resources.Load("Prefabs/UI/Letter", typeof(GameObject)) as GameObject;
			}

			textContainer = transform.Find("[TextContainer]");

			// Get text Dictionary
			textDictionary = GameObject.FindGameObjectWithTag("TextDictionary").GetComponent<TextDictionary>();

			// Destroy the old text
			foreach (Transform textChild in textContainer) {
				Destroy(textChild.gameObject);
			}

			List<Sprite> textSprites = textDictionary.GetSprites(newText);

			float initialOffset = 0;

			if (textAlignment == TextAlignment.Center) {
				initialOffset = -pixelUnit / 2;
				foreach (Sprite textSprite in textSprites) {
					initialOffset -= (textSprite.textureRect.width / 24) - pixelUnit;
					if (textSprite != textSprites[0]) {
						initialOffset -= pixelUnit;
					}
				}
			} else if (textAlignment == TextAlignment.Right) {
				initialOffset = -pixelUnit;
				foreach (Sprite textSprite in textSprites) {
					initialOffset -= ((textSprite.textureRect.width / 12) - pixelUnit);
					if (textSprite != textSprites[0]) {
						initialOffset -= (1 / 12);
					}
				}
			}
			
			for (int i = 0; i < textCount; i++) {
				float widthCurrent = 0;
				foreach (Sprite textSprite in textSprites) {
					GameObject newLetter = (GameObject)Instantiate(prefab_Letter, transform.position, Quaternion.identity, textContainer);
					newLetter.transform.localPosition = new Vector3((textSprite.textureRect.width / 24) + widthCurrent + initialOffset, 0);
					SpriteRenderer newLetterRenderer = newLetter.GetComponent<SpriteRenderer>();
					newLetterRenderer.sprite = textSprite;
					widthCurrent += (textSprite.textureRect.width / 12) - pixelUnit;
					pixelWidth += (int)(textSprite.textureRect.width) + (i != 0 ? 1 : 0);
					newLetterRenderer.color = (i == 0 ? textColor : borderColor);

					if (i > 0) {    // Border
						Vector2 borderOffset = Vector2.zero;
						switch (i) {
							case (1):
								borderOffset = Vector2.up * -pixelUnit;
								break;
							case (2):
								borderOffset = Vector2.up * pixelUnit;
								break;
							case (3):
								borderOffset = Vector2.right * pixelUnit;
								break;
							case (4):
								borderOffset = Vector2.right * -pixelUnit;
								break;
						}

						newLetterRenderer.transform.position += (Vector3)borderOffset;
						newLetterRenderer.sortingOrder = textSortingOrder - 1;
					} else {
						newLetterRenderer.sortingOrder = textSortingOrder;
					}
				}
			}

			// Set textCurrent
			textCurrent = text;
		}
	}

	public void FixTextContainerPosition () {
		float pixelUnit = 12f;
		float oddPixelOffset = (pixelWidth % 2 != 1) ? (1f / 24f) : 0;
		textContainer.transform.position = new Vector3((Mathf.Round(transform.position.x * pixelUnit) / pixelUnit) + oddPixelOffset, Mathf.Round(transform.position.y * pixelUnit) / pixelUnit);
	}

}
