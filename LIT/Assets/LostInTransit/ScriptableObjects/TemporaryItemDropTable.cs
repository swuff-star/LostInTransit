using System;
using System.Collections.Generic;
using UnityEngine;
using RoR2;

namespace LostInTransit.ScriptableObjects
{
	[CreateAssetMenu(menuName = "LostInTransit/TemporaryItemDropTable")]
	public class TemporaryItemDropTable : PickupDropTable
	{

		public float tier1Weight = 0.8f;
		public float tier2Weight = 0.2f;
		public float tier3Weight = 0.01f;
		public float bossWeight;
		public float lunarEquipmentWeight;
		public float lunarItemWeight;
		public float lunarCombinedWeight;
		public float equipmentWeight;
		public float voidTier1Weight;
		public float voidTier2Weight;
		public float voidTier3Weight;
		public float voidBossWeight;

		public ItemTag[] requiredItemTags = Array.Empty<ItemTag>();
		public ItemTag[] bannedItemTags = Array.Empty<ItemTag>();

		[NonSerialized]
		public WeightedSelection<PickupIndex> selector = new WeightedSelection<PickupIndex>(8);

		public override void Regenerate(Run run)
		{
			this.GenerateWeightedSelection(run);
		}

		public bool IsFilterRequired()
		{
			return LITTempItems.blacklist.Length > 0 || this.requiredItemTags.Length != 0 || this.bannedItemTags.Length != 0;
		}

		public bool PassesFilter(PickupIndex pickupIndex)
		{
			PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);

			if (pickupDef.itemIndex != ItemIndex.None)
			{
				ItemDef itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
				foreach (ItemTag value in this.requiredItemTags)
				{
					if (Array.IndexOf<ItemTag>(itemDef.tags, value) == -1)
					{
						return false;
					}
				}
				foreach (ItemTag value2 in this.bannedItemTags)
				{
					if (Array.IndexOf<ItemTag>(itemDef.tags, value2) != -1)
					{
						return false;
					}
				}
			}

			return pickupDef.itemIndex != ItemIndex.None && Array.IndexOf<ItemIndex>(LITTempItems.blacklist, pickupDef.itemIndex) != -1;
		}
		public void Add(List<PickupIndex> sourceDropList, float chance)
		{
			if (chance <= 0f || sourceDropList.Count == 0)
			{
				return;
			}
			foreach (PickupIndex pickupIndex in sourceDropList)
			{
				if (!this.IsFilterRequired() || this.PassesFilter(pickupIndex))
				{
					this.selector.AddChoice(pickupIndex, chance);
				}
			}
		}

		public void GenerateWeightedSelection(Run run)
		{
			this.selector.Clear();
			this.Add(run.availableTier1DropList, this.tier1Weight);
			this.Add(run.availableTier2DropList, this.tier2Weight);
			this.Add(run.availableTier3DropList, this.tier3Weight);
			this.Add(run.availableBossDropList, this.bossWeight);
			this.Add(run.availableLunarItemDropList, this.lunarItemWeight);
			this.Add(run.availableLunarEquipmentDropList, this.lunarEquipmentWeight);
			this.Add(run.availableLunarCombinedDropList, this.lunarCombinedWeight);
			this.Add(run.availableEquipmentDropList, this.equipmentWeight);
			this.Add(run.availableVoidTier1DropList, this.voidTier1Weight);
			this.Add(run.availableVoidTier2DropList, this.voidTier2Weight);
			this.Add(run.availableVoidTier3DropList, this.voidTier3Weight);
			this.Add(run.availableVoidBossDropList, this.voidBossWeight);
		}
		public override PickupIndex GenerateDropPreReplacement(Xoroshiro128Plus rng)
		{
			return PickupDropTable.GenerateDropFromWeightedSelection(rng, this.selector);
		}

		public override int GetPickupCount()
		{
			return this.selector.Count;
		}

		public override PickupIndex[] GenerateUniqueDropsPreReplacement(int maxDrops, Xoroshiro128Plus rng)
		{
			return PickupDropTable.GenerateUniqueDropsFromWeightedSelection(maxDrops, rng, this.selector);
		}

		
	}
}
