using _Main.Scripts.Datas;
using BaseSystems.Scripts.LevelSystem;
using BaseSystems.Scripts.Utilities;
using BaseSystems.Scripts.Utilities.Singletons;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BaseSystems.Scripts.Managers
{
	[DefaultExecutionOrder(-2)]
	public class LevelManager : Singleton<LevelManager>
	{
		[Header("Single Level Prefab")]
		[SerializeField] private Level levelPrefab;

		[Header("Level Datas")]
		[SerializeField] private LevelsSO levelsSO;
		public LevelsSO LevelsSO => levelsSO;

		public Level CurrentLevel { get; private set; }
		private int currentLevelIndex;

		public static event UnityAction OnLevelLoad;
		public static event UnityAction OnLevelUnload;
		public static event UnityAction OnLevelStart;
		public static event UnityAction OnLevelRestart;
		public static event UnityAction OnLevelWin;
		public static event UnityAction<int> OnLevelWinWithMoveCount;
		public static event UnityAction OnLevelLose;

		public int LevelNo
		{
			get => PlayerPrefs.GetInt(PlayerPrefsNames.LEVEL_NO, 1);
			set => PlayerPrefs.SetInt(PlayerPrefsNames.LEVEL_NO, value);
		}

		private void Start()
		{
			LoadCurrentLevel(true);
		}

		// -----------------------------
		// LOAD CURRENT LEVEL
		// -----------------------------
		public void LoadCurrentLevel(bool isStart = false)
		{
			int maxLevelCount = levelsSO.Levels.Count;

			currentLevelIndex = LevelNo - 1;

			int loadIndex;

			if (currentLevelIndex >= maxLevelCount)
			{
				loadIndex = Random.Range(0, maxLevelCount);
			}
			else
			{
				loadIndex = currentLevelIndex;
			}

			LoadLevel(loadIndex);
		}

		// -----------------------------
		// LOAD LEVEL
		// -----------------------------
		private void LoadLevel(int index)
		{
			if (index < 0 || index >= levelsSO.Levels.Count)
			{
				Debug.LogError($"Invalid Level Index: {index}");
				return;
			}

			// Get LevelData
			LevelData levelData = levelsSO.Levels[index];

			// Instantiate single prefab
			CurrentLevel = Instantiate(levelPrefab);
			CurrentLevel.levelData = levelData;

			// Initialize level systems
			CurrentLevel.Load();

			OnLevelLoad?.Invoke();
			StartLevel();
		}

		// -----------------------------
		// PLAY LEVEL
		// -----------------------------
		public void StartLevel()
		{
			CurrentLevel.Play();
			OnLevelStart?.Invoke();
		}

		// -----------------------------
		// RETRY / RESTART
		// -----------------------------
		public void RetryLevel()
		{
			UnloadLevel();
			LoadLevel(currentLevelIndex);
		}

		public void RestartLevel()
		{
			OnLevelRestart?.Invoke();
			RetryLevel();
		}

		public void RestartFromFirstLevel()
		{
			LevelNo = 1;
			OnLevelRestart?.Invoke();
			LoadCurrentLevel();
		}

		// -----------------------------
		// NEXT / PREVIOUS LEVEL
		// -----------------------------
		public void LoadNextLevel()
		{
			UnloadLevel();
			LevelNo++;

			// if (LevelNo > levelsSO.Levels.Count)
			// 	LevelNo = 1;

			LoadCurrentLevel();
		}

		public void LoadBackLevel()
		{
			UnloadLevel();
			LevelNo--;

			if (LevelNo < 1)
				LevelNo = levelsSO.Levels.Count;

			LoadCurrentLevel();
		}

		// -----------------------------
		// UNLOAD LEVEL
		// -----------------------------
		private void UnloadLevel()
		{
			OnLevelUnload?.Invoke();

			if (CurrentLevel != null)
				Destroy(CurrentLevel.gameObject);
		}

		// -----------------------------
		// RESULT
		// -----------------------------
		[Button]
		public void Win()
		{
			OnLevelWin?.Invoke();
		}

		public void Win(int moveCount)
		{
			OnLevelWinWithMoveCount?.Invoke(moveCount);
		}

		public void Lose(string loseText)
		{
			OnLevelLose?.Invoke();
		}
	}
}