using System.Collections;
using System.Collections.Generic;
using _Main.Scripts.BlockSystem;
using _Main.Scripts.GridSystem;
using _Main.Scripts.ShredderSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "ReferenceManagerSO", menuName = "Data/Reference ManagerSO")]
public class ReferenceManagerSO : ScriptableObject
{
	private static ReferenceManagerSO _instance;

	[SerializeField] private Transform gridEgdeMiddlePoint;
	[SerializeField] private Transform gridEgdeCornerPoint;
	[SerializeField] private GridPointController gridPointController;
	[SerializeField] private float blockMoveSpeed;
	
	
	public List<Shredder> shredders = new List<Shredder>();
	
	public List<Block> blocks = new List<Block>();
	
	public Transform GridEgdeMiddlePoint => gridEgdeMiddlePoint;

	public Transform GridEgdeCornerPoint => gridEgdeCornerPoint;
	public GridPointController GridPointController => gridPointController;

	public float BlockMoveSpeed => blockMoveSpeed;
	public static ReferenceManagerSO Instance
	{
		get
		{
			if (_instance == null)
				_instance = Resources.Load<ReferenceManagerSO>("ReferenceManagerSO");
			return _instance;
		}
	}
}