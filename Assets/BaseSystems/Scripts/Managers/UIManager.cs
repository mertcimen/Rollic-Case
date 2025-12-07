using BaseSystems.Scripts.UI;
using BaseSystems.Scripts.Utilities.Singletons;
using TMPro;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace BaseSystems.Scripts.Managers
{
	public class UIManager : SingletonInit<UIManager>
	{

		[Title("Panels")]
		[SerializeField] private StartPanel startPanel;
		[SerializeField] private WinPanel winPanel;
		[SerializeField] private LosePanel losePanel;
		[SerializeField] private SettingsUI settingsPanel;
		public InGameUI InGameUI { get; private set; }
		[Title("Timer")]
		public TextMeshProUGUI TimerText;
		public Image TimerBar;

		protected override void Awake()
		{
			base.Awake();

			InGameUI = GetComponentInChildren<InGameUI>();
			//InGameUI.Hide();
		}

		private void OnEnable()
		{
			LevelManager.OnLevelUnload += OnLevelUnloaded;
			LevelManager.OnLevelLoad += OnLevelLoad;
			LevelManager.OnLevelStart += OnLevelStart;
			LevelManager.OnLevelWin += OnLevelWin;
			LevelManager.OnLevelLose += OnLevelLose;
			LevelManager.OnLevelWinWithMoveCount += OnLevelWinWithMoveCount;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelUnload -= OnLevelUnloaded;
			LevelManager.OnLevelLoad -= OnLevelLoad;
			LevelManager.OnLevelStart -= OnLevelStart;
			LevelManager.OnLevelWin -= OnLevelWin;
			LevelManager.OnLevelLose -= OnLevelLose;
			LevelManager.OnLevelWinWithMoveCount -= OnLevelWinWithMoveCount;
		}
		// [Button]
		private void ShowWinPanel()
		{
			winPanel.Open();
		}

		private void ShowLosePanel()
		{
			losePanel.Open();
		}

		private void HideWinPanel()
		{
			winPanel.Close();
		}

		private void HideLosePanel()
		{
			losePanel.Close();
		}

		private void HideStartPanel()
		{
			startPanel.Close();
		}

		public void ShowSettingsPanel()
		{
			settingsPanel.Open();
		}

		public void SetLosePanelText(string text)
		{
			losePanel.SetLosePanelText(text);
			
		}
		public void HideSettingsPanel()
		{
			settingsPanel.Close();
		}

		private void ShowInGameUI()
		{
			InGameUI.Show();
		}

		private void HideInGameUI()
		{
			InGameUI.Hide();
		}

		

		private void OnLevelUnloaded()
		{
			HideWinPanel();
			HideLosePanel();
		}

		private void OnLevelLoad()
		{
			startPanel.Open();
		}

		private void OnLevelStart()
		{
			ShowInGameUI();
			HideStartPanel();
		}

		private void OnLevelWin()
		{
			ShowWinPanel();
			HideInGameUI();
		}

		private void OnLevelWinWithMoveCount(int moveCount)
		{
			OnLevelWin();
		}

		private void OnLevelLose()
		{
			ShowLosePanel();
			HideInGameUI();
		}
	}
}