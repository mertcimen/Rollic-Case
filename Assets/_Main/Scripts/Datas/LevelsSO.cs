using System.Collections.Generic;
using UnityEngine;

namespace _Main.Scripts.Datas
{
	[CreateAssetMenu(fileName = "Levels", menuName = "Data/Levels")]
	public class LevelsSO : ScriptableObject
	{
		[SerializeField] private List<LevelData> levelDatas = new();
		public List<LevelData> Levels => levelDatas;

		public void AddLevel(LevelData levelData)
		{
			if (!levelDatas.Contains(levelData))
				levelDatas.Add(levelData);
		}
	}

	// [System.Serializable]
	// public class LevelData
	// {
	// 	[SerializeField] private Level level;
	// 	[SerializeField] private bool isLoopingLevel = true;
	//
	// 	public Level Level
	// 	{
	// 		get => level;
	// 		set => level = value;
	// 	}
	//
	// 	public bool IsLoopingLevel
	// 	{
	// 		get => isLoopingLevel;
	// 		set => isLoopingLevel = value;
	// 	}
	// }
}