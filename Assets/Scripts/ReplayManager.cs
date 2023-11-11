/*
 *  ReplayManager.cs
 *		リプレイ周り・入力周りの処理一切合切
 * 
 * 
 * 
 * 
 * 
 * 
 *	20221211	WSc101用に作成
 *	20221215	ランダムシードを保持しておくよう変更
 * 
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReplayManager : MonoBehaviour
{
	private const int V = 1;
	[SerializeField]
	MyRandom RND = new MyRandom(0);
	[SerializeField]
	public bool SW_REPLAY = false;
	[SerializeField]
	public bool SW_REPLAY_END = false;


	readonly int[] timer_msec = new int[60]
	{
		00,01,03,05,07,09,
		10,12,14,16,17,19,
		20,21,23,25,27,29,
		30,32,34,35,37,39,
		40,41,43,45,47,49,
		50,52,54,56,58,59,
		60,61,63,65,67,69,
		70,72,74,76,78,79,
		80,81,83,85,87,89,
		90,92,94,96,98,99
	};



	public bool SW_READ = false;
	public uint SEED = 0;

	private List<ReplayInput> REPLAY = new List<ReplayInput>();
	private MainSceneCtrl MAIN = null;

	// Start is called before the first frame update
	void Awake()
	{
		MAIN = GameObject.Find("root_game").GetComponent<MainSceneCtrl>();
	}

	// Update is called once per frame
	void Update()
	{
		if (SW_REPLAY == false)
		{
			for (uint i = 0; i < 8; i++)
			{
				ReplayInput inp = new ReplayInput();
				inp.count = MAIN.count;
				inp.player = i + 1;
				string msg = "Fire" + (i + 1).ToString();
				inp.button = Input.GetButton(msg);
				msg = "Horizontal" + (i + 1).ToString();
				string msg2 = "Vertical" + (i + 1).ToString();
				inp.vector = new Vector2(Input.GetAxis(msg), Input.GetAxis(msg2));
				//Debug.Log("ReplayManager:Update count:" + inp.count + " / controller:" + inp.player + " / button:" + inp.button + " / vector:" + inp.vector);
				REPLAY.Add(inp);
				//Debug.Log("ReplayManager:Update REPLAY.FindLast{}.player={" + REPLAY.FindLast(index => index.player == (i + 1)).player + "}");
			}

#if false
			for(int i=0;i<REPLAY.Count;i++)
			{
				Debug.Log("REPLAY[" + i + "].count=" + REPLAY[i].count + " / player=" + REPLAY[i].player + " / button=" + REPLAY[i].button + " / vector=" + REPLAY[i].vector);
			}
#endif
		}
	}

	public void Reset(uint seed)
	{
		SEED = seed;
		RND.setSeed(seed);
		Debug.Log("REPLAY.Reset:seed=" + seed);
		if (SW_REPLAY == false)
		{
			SW_REPLAY_END = false;
			REPLAY.Clear();
			ReplayInput inp = new ReplayInput();
			inp.count = -1;
			inp.random_seed = seed;
			REPLAY.Add(inp);
		}
		else
		{
			SW_REPLAY_END = false;
			RND.setSeed(REPLAY[0].random_seed);
		}
	}
	public int RandomRange(int min, int max)
	{
		return RND.Range(min, max);
	}
	public ReplayInput GetControl(int p)
	{
		if (MAIN.count <= -1)
		{
			//Debug.Log("MAIN.count==-1");
			return new ReplayInput();
		}
		else if (MAIN.count + 1 > REPLAY.Count)
		{
			if (SW_REPLAY == true)
			{
				SW_REPLAY_END = true;
				//Debug.Log("SW_REPLAY_END=true setting.");
			}
			//Debug.Log("MAIN.count+1>REPLAY.Count");
			return new ReplayInput();
		}
		ReplayInput ret = new ReplayInput();
		int cnt = MAIN.count;
		//Debug.Log("cnt=" + cnt);
		if (SW_REPLAY == true)
		{
			cnt--;
		}
		//Debug.Log("MAIN.count=" + MAIN.count+" / cnt="+cnt);
		if (cnt > 0)
		{
			//Debug.Log("ReplayManager:GetControl:cnt=" + cnt + " / controller=" + p);
#if false
			for (int i=0;i<REPLAY.Count;i++)
			{
				Debug.Log("REPLAY[" + i + "] ={count=" + REPLAY[i].count + " / controller=" + REPLAY[i].player + "}");
			}
#endif
			ret = REPLAY[REPLAY.FindLastIndex(counter => (counter.player == p) && (counter.count == (cnt - 1)))];
		}
		//Debug.Log("ReplayManager:GetControl:cnt=" + cnt + " / controller="+p+" / ret.vector=" + ret.vector);
		if (SW_REPLAY == true)
		{
			if (cnt == 0)
			{
				ret = new ReplayInput();
			}
		}
		return ret;
	}
	public int GetMsec(int count)
	{
		if (count < 0)
		{
			return 0;
		}
		return timer_msec[count % 60];
	}
	public bool Load(string fname)
	{
		SW_READ = true;
		bool ret = false;
		ReplayInput inp = new ReplayInput();
		var reader = new BinaryReader(new FileStream(fname, FileMode.Open));
		try
		{
			REPLAY.Clear();
			inp.random_seed = reader.ReadUInt32();
			inp.count = -1;
			REPLAY.Add(inp);
			RND.setSeed(inp.random_seed);
			SEED = inp.random_seed;
			int c = 0;
			uint pl = 1;
			while (true)
			{
				inp = new ReplayInput();
				float x = reader.ReadSingle();
				float y = reader.ReadSingle();
				inp.vector = new Vector2(x, y);
				inp.button = reader.ReadBoolean();
				inp.count = c;
				inp.player = reader.ReadUInt32();
				REPLAY.Add(inp);
				c++;
			}
		}
		catch
		{
			reader.Close();
		}
		SW_READ = false;
		return ret;
	}
	public bool Save(string fname,bool sw)
	{
		bool ret = false;
		FileMode fm = FileMode.OpenOrCreate | FileMode.Truncate;
		if (sw == true)
		{
			fm = FileMode.CreateNew;
		}
		var writer = new BinaryWriter(new FileStream(fname, fm));
		try
		{
			writer.Write(REPLAY[0].random_seed);
			for (int i = 1; i < REPLAY.Count; i++)
			{
				Vector2 v = new Vector2(0, 0);
				writer.Write(REPLAY[i].vector.x);
				writer.Write(REPLAY[i].vector.y);
				writer.Write(REPLAY[i].button);
				writer.Write(REPLAY[i].player);
			}
		}
		finally
		{
			writer.Close();
			ret = true;
		}
		return ret;
	}
}
