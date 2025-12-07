using UnityEngine;

namespace _Main.Scripts.Datas
{
    [CreateAssetMenu(fileName = "GameReferencesSO", menuName = "Data/Game References")]
    public class GameReferencesSO : ScriptableObject
    {
        
        private static GameReferencesSO _instance;
        public static GameReferencesSO Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<GameReferencesSO>("GameReferencesSO");
                return _instance;
            }
        }
        
    }
}