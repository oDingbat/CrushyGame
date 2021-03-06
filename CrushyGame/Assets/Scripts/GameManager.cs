﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GameManager : MonoBehaviour {

	[Space (10)][Header ("References")]
	public PlayerController player;
	public MonsterController monster;

	[Space (10)][Header ("Text")]
	public DynamicText UI_score;
	public DynamicText UI_coins;

	[Space (10)][Header ("Item Collection Info")]
	public List<Item> itemsCollected = new List<Item>();
	public List<Transform> itemCollectedImages = new List<Transform>();
	public Transform itemCollectionContainer;
	
	[Space (10)][Header ("Heart Info")]
	public List<HeartImage> heartImages = new List<HeartImage>();
	public Transform heartCollectionContainer;

	float sizePoints;
	float sizeCoins;
	int points = 0;
	int coins;
	
	[Space (10)][Header ("Prefabs")]
	public GameObject prefab_textFloater;
	public GameObject prefab_ItemImage;
	public GameObject prefab_HeartImage;

	private void Start () {
		monster.EventPlayerLived += OnPlayerLived;

		// Setup coins
		coins = PlayerPrefs.GetInt("coins", 0);
		UI_coins.SetText(coins.ToString());

		// Create hearts
		CreateHeartImages(player.attributesBase.hearts);
	}

	private void Update() {
		if (UI_score != null) {
			UI_score.transform.localScale = Vector3.one * sizePoints;
			sizePoints = Mathf.Clamp(sizePoints - Time.deltaTime * 10f, 2f, Mathf.Infinity);
		} else {
			if (GameObject.FindGameObjectWithTag("ScoreCounter") != null) {
				UI_score = GameObject.FindGameObjectWithTag("ScoreCounter").GetComponent<DynamicText>();
			}
		}

		PositionUIContainers();

		UI_coins.transform.localScale = Vector3.one * sizeCoins;
		sizeCoins = Mathf.Clamp(sizeCoins - Time.deltaTime * 10f, 1f, Mathf.Infinity);
	}

	public void OnPlayerLived () {
		sizePoints = 4f;
		points++;
		if (UI_score != null) {
			UI_score.SetText(points.ToString());
		}
	}

	private void PositionUIContainers () {
		float verticalOffset = Mathf.Clamp((monster.mouthWidth - 6f) / 2, 0, 10);
		heartCollectionContainer.localPosition = new Vector3(heartCollectionContainer.localPosition.x, 1.96f + verticalOffset);
		itemCollectionContainer.localPosition = new Vector3(itemCollectionContainer.localPosition.x, 1.96f + verticalOffset);
	}

	public void OnCoinGrabbed (int value) {
		sizeCoins = 2f;
		coins += value;
		PlayerPrefs.SetInt("coins", coins);
		UI_coins.SetText(coins.ToString());
	}

	public void CollectItem (ItemDrop itemDrop) {
		Item item = itemDrop.item;

		// Dont collect Item if we already have one of it
		if (itemsCollected.Exists(i => i.name == item.name)) {
			return;
		}

		// Create hearts
		int heartsAdded = Mathf.Clamp(item.attributes.hearts, 0, 5 - player.attributesCombined.hearts);
		CreateHeartImages(heartsAdded);
		
		// Add Attributes
		player.attributesCombined.Add(item.attributes);
		player.attributesCombined.ClampValues();

		if (item.isCollectable == true) {
			// Create textFloater
			TextFloater newTextFloater = Instantiate(prefab_textFloater, player.transform.position, Quaternion.identity).GetComponent<TextFloater>();
			newTextFloater.SetText(item.name.ToLower());

			// Create new item collected image
			Vector3 offset = new Vector3(3, -0.75f * itemsCollected.Count);
			GameObject newItemCollectedImage = Instantiate(prefab_ItemImage, itemCollectionContainer.transform.position + offset, Quaternion.identity, itemCollectionContainer);
			newItemCollectedImage.GetComponent<Animator>().runtimeAnimatorController = item.animController;

			// Add item to items collected
			itemsCollected.Add(item);
			itemCollectedImages.Add(newItemCollectedImage.transform);
		}
	}

	public void CreateHeartImages (int amount) {
		// Create heart images
		for (int i = 0; i < amount; i++) {
			// Create single heart socket
			HeartImage newHeartSocketImage = Instantiate(prefab_HeartImage, Vector3.zero, Quaternion.identity, heartCollectionContainer).GetComponent<HeartImage>();
			newHeartSocketImage.transform.localPosition = new Vector3(-3, -0.75f * heartImages.Count);
			newHeartSocketImage.heartIndex = heartImages.Count;
			newHeartSocketImage.player = player;
			heartImages.Add(newHeartSocketImage);
		}
	}
}
