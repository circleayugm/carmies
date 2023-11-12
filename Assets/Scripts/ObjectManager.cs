/*******************************************************************************************************
 * 
 *	ObjectManager.cs 
 *		オブジェクト動作マネージャ
 *		生成・消去・リスト管理など
 *		当たり判定の独自実装部分もここ
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 *		20221211	3日前くらいにWSc101用に再構成
 *		20221213	止むに止まれず当たり判定を独自実装
 *		20221215	新しいキャラ実装にあたって再構成
 * 
 * 
********************************************************************************************************/
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public sealed class ObjectManager : MonoBehaviour {

	[SerializeField]
	Transform OBJ_ROOT;

	[SerializeField]
	public Sprite[] SPR_SHIP; // 暫定；キャラクタ画像置き場・将来的にはキャラクタ設定時にリストに登録するようにする.
	[SerializeField]
	public Sprite[] SPR_MYSHOT;
	[SerializeField]
	public Sprite[] SPR_ENEMY;
	[SerializeField]
	public Sprite[] SPR_WALL;
	[SerializeField]
	public Sprite[] SPR_CRUSH;

	[SerializeField]
	public ObjectCtrl PREFAB_OBJECT;    // キャラクタオブジェクトのprefab.

	public int LIMIT = 700;             // キャラクタ表示総数.

	public enum MODE    // 処理モード.
	{
		NOUSE = 0,          // 現在未使用.
		INIT,               // 初期化(出現直後).
		NOHIT,              // 当たり無視.
		HIT,                // 当たり有効・通常時はこちら.
		DEAD,               // 死亡アニメ.
		FINISH,             // 最終処理.
	}
	public enum TYPE    // 処理タイプ(当たり判定有無を大雑把に仕分け).
	{
		NOUSE = 0,          // 未使用.
		MYSHIP,				// 自機.
		MYSHOT,		        // 自機ショット.
		DEBRIS,				// 隕石
		FIRE,				// 炎
		WALL,				// 壁
		NOHIT_EFFECT,		// 爆風
	}

	public readonly Vector3 OBJECT_DISPLAY_LIMIT = new Vector3((272 * 3) / 2, (480 * 3) / 2, 0);
	public readonly Vector3 OBJECT_BULLET_LIMIT = new Vector3(272 / 2, 480 / 2, 0);

	public List<ObjectCtrl> objectStock = new List<ObjectCtrl>();
	public List<ObjectCtrl> objectUsed = new List<ObjectCtrl>();

	public int objectStockMax = 700;
	public int objectUsedMax = 0;

	public bool SW_BOOT = false;		// 動作準備が整った時true
#if false
	public readonly int WATER_MAX = 1000;	// 水最大値
	public int CNT_WATER = 500;				// 水ゲージ

	public readonly int CNT_FIRE_MAX = 30;	// 炎(固定)最大出現数
	public int CNT_FIRE = 30;				// 炎(固定)残り出現数
	public int CNT_FIRE_REMAIN = 0;			// 炎(固定)倒した数
	public int CNT_MOVEFIRE_REMAIN = 0;     // 動く炎の残り数
#endif


	// carmiesここから
	public readonly int CNT_TIMER_START = 60 * 60 * 3;  // 制限時間初期値

	public readonly int SCORE_PLAYER_MULTIPLIER = 100;	// 他プレイヤーを倒した時の倍率
	public readonly float[] SCORE_MULTIPLIER_DISP = new float[8] { 1, 100, 30, 10, 1, 0.5f, 0.25f, 0.25f };  // 自分サイドのプレイヤー数に応じた得点倍率
																											 // 少なければ少ないほど高い

	public readonly int[] SCORE_MULTIPLIER = new int[8] { 10, 1000, 300, 100, 10, 5, 3, 3 };				// 自分サイドのスコア・実加算用																										public readonly int CNT_TIMER_SHUFFLE = 60;			// 相手サイドのプレイヤーが0になってから強制組分けが発生するまでの時間
	public readonly int CNT_CHANGE_SIDE = 4;			// 自力でサイドを変更できる回数

	public readonly int CNT_STEPS_NORMAL = 4;			// プレイヤーの1intの移動距離(通常時)
	public readonly int CNT_STEPS_TURBO = 16;			//							(ターボ時)
	public readonly int CNT_TIMER_TURBO = 60;			// プレイヤーがターボで動ける時間



	public int[] CNT_PLAYER_SIDE = new int[2] { 0, 0 }; // 左・右のプレイヤー数
	public int[] SCORE_PLAYER = new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 };	// 各プレイヤーのスコア
	public int CNT_TIMER = 60 * 60 * 3;                 // 制限時間





	float[] ang256 = new float[256];

	void Awake()
	{
		objectStock.Clear();
		objectStockMax = 0;
		objectUsed.Clear();
		objectUsedMax = 0;
		for (int i = 0; i < ang256.Length; i++)
		{
			ang256[i] = (90-(360.0f / 256.0f) * i);
		}
		for (int i = 0; i < LIMIT; i++)
		{
			ObjectCtrl obj;
			obj = Generate(TYPE.NOUSE, new Vector3(-1000, 0, 0), 0, 0);
			obj.OBJcnt = i;
			objectStock.Add(obj);
			objectStockMax++;
		}
		SW_BOOT = true;
	}




	

	/// <summary>
	///		初期設定；オブジェクトの前準備
	/// </summary>
	/// <param name="type">オブジェクトタイプ</param>
	/// <param name="pos">座標</param>
	/// <param name="angle">角度</param>
	/// <param name="speed">速度</param>
	/// <returns>前準備と設置が終わったオブジェクト</returns>
	public ObjectCtrl Generate(TYPE type, Vector3 pos, int angle, int speed)
	{
		ObjectCtrl obj = ObjectCtrl.Instantiate(PREFAB_OBJECT, pos, Quaternion.identity) as ObjectCtrl;
		obj.transform.SetParent(OBJ_ROOT);
		obj.obj_type = TYPE.NOUSE;
		obj.obj_mode = MODE.NOUSE;
		obj.MainHit.enabled = false;
		obj.DisplayOff();
		return obj;
	}



	/// <summary>
	///		オブジェクト設定
	/// </summary>
	/// <param name="type">オブジェクトタイプ</param>
	/// <param name="mode">動作モード</param>
	/// <param name="pos">座標</param>
	/// <param name="angle">角度</param>
	/// <param name="speed">速度</param>
	/// <returns>キャラクタ設定を終えたオブジェクト</returns>
	public ObjectCtrl Set(TYPE type, int mode,Vector3 pos, int angle, int speed)
	{
		if (objectStock.Count == 0)
		{
			return null;
		}
		ObjectCtrl obj;
		obj = objectStock[0];
		objectStockMax--;
		objectStock.RemoveAt(0);	// ここまで「ストックから取り出し」処理.

		obj.mode = mode;            // オブジェクト設定.
		obj.angle = angle;
		obj.speed = speed;
		obj.LIFE = 1;
		obj.obj_mode = MODE.INIT;
		obj.obj_type = type;
		obj.MainPic.sortingOrder = 0;
		obj.target = new Vector3(0, 0, 0);

		obj.transform.localPosition = pos;
		obj.transform.localScale = new Vector3(1, 1, 1);
		obj.transform.localRotation = Quaternion.identity;
		obj.transform.localEulerAngles = new Vector3(0, 0, 0);
		obj.interval = 0;
		obj.enabled = true;
		for (int i=0;i<obj.param.Length;i++)
		{
			obj.param[i] = 0;
		}
		objectUsed.Add(obj);        // 使用中リストに入れる処理.
		objectUsedMax++;
		return obj;
	}


	/// <summary>
	///		オブジェクト返却
	/// </summary>
	/// <param name="obj">返却するオブジェクト</param>
	/// <returns>使っていないオブジェクトの個数</returns>
	public int Return(ObjectCtrl obj)
	{
		obj.DisplayOff();
		objectUsed.Remove(obj);
		objectUsedMax--;
		obj.MainPic.sprite = null;
		obj.MainHit.enabled = false;
		obj.GraphPic.enabled = false;
		obj.MainPos.localPosition = new Vector3(-1000, 0, 0);
		obj.obj_mode = MODE.NOUSE;
		obj.obj_type = TYPE.NOUSE;
		obj.enabled = false;
		objectStock.Add(obj);
		objectStockMax++;
		return objectStock.Count;
	}



	/// <summary>
	///		オブジェクト全部返却
	/// </summary>
	public void ResetAll()
	{
		int objcnt = objectUsed.Count;
		while (objectUsed.Count > 0)
		{
			ObjectCtrl obj;
			obj = objectUsed[0];
			objectUsed.RemoveAt(0);
			obj.obj_mode = MODE.NOUSE;
			obj.obj_type = TYPE.NOUSE;
			obj.DisplayOff();
			obj.enabled = false;
			objectStock.Add(obj);
			objectStockMax = objectStock.Count;
			objectUsedMax = 0;
		}
	}



	/// <summary>
	///		使えるオブジェクトの個数を得る
	/// </summary>
	/// <returns>使えるオブジェクトの個数</returns>
	public int GetRestObject()
	{
		return objectStockMax;
	}








	//-------------------------------------------------------------------
	/// <summary>
	///		当たり判定部・Unityの衝突判定を使わない版
	///		Unityの当たり判定を使うと処理順序が狂うので独自実装
	/// </summary>
	/// <param name="obj">対象のオブジェクト</param>
	//-------------------------------------------------------------------
	public void CheckHit(ObjectCtrl obj)
	{
		if (objectUsed.Count == 0)
		{
			return;
		}
		for (int i = 0; i < objectUsed.Count; i++)
		{
			{
				if (obj.obj_type == TYPE.FIRE)
				{
					if (objectUsed[i].obj_type == TYPE.FIRE)
					{
						continue;
					}
#if false

					else if (objectUsed[i].obj_type == TYPE.WATERPOOL)
					{
						float xx = objectUsed[i].transform.localPosition.x - obj.transform.localPosition.x;
						float yy = objectUsed[i].transform.localPosition.y - obj.transform.localPosition.y;
						float check1 = (xx * xx) + (yy * yy);
						float check2 = (objectUsed[i].MainHit.radius * objectUsed[i].transform.localScale.x) + (obj.MainHit.radius * obj.transform.localScale.x);
						if (check1 <= (check2 * check2))
						{
							switch (obj.mode)
							{
								case 0:
									if (obj.count < 10)
									{
										CNT_FIRE++;
										Return(obj);
									}
									else
									{
										obj.param[1] = 0;
									}
									break;
								case 1:
								case 2:
									obj.Damage(10);
									break;
							}
						}
					}
#endif
					else if (objectUsed[i].obj_type == TYPE.MYSHOT)
					{
						float xx = objectUsed[i].transform.localPosition.x - obj.transform.localPosition.x;
						float yy = objectUsed[i].transform.localPosition.y - obj.transform.localPosition.y;
						float check1 = (xx * xx) + (yy * yy);
						float check2 = (objectUsed[i].MainHit.radius * objectUsed[i].transform.localScale.x) + (obj.MainHit.radius * obj.transform.localScale.x);
						if (check1 <= (check2 * check2))
						{
							obj.param[0]--;
							if (obj.param[0] < 10)
							{
								obj.param[0] = 10;
							}
							if (obj.param[0] < 20)
							{
								obj.Damage(10);
							}
						}
					}
				}
				else if (obj.obj_type == TYPE.MYSHIP)
				{
					if (objectUsed[i].obj_type == TYPE.WALL)
					{
						float xx = objectUsed[i].transform.localPosition.x - obj.transform.localPosition.x;
						float yy = objectUsed[i].transform.localPosition.y - obj.transform.localPosition.y;
						float check1 = (xx * xx) + (yy * yy);
						float check2 = objectUsed[i].MainHit.radius + obj.MainHit.radius;
						if (check1 <= (check2 * check2))
						{
							Vector3 v = obj.transform.localPosition;
							if (obj.vect.x < 0.0f)
							{
								if (objectUsed[i].transform.localPosition.x - check2 < v.x)
								{
									v.x -= obj.vect.x;
								}
							}
							else if (obj.vect.x > 0.0f)
							{
								if (objectUsed[i].transform.localPosition.x + check2 > v.x)
								{
									v.x -= obj.vect.x;
								}
							}
							if (obj.vect.y < 0.0f)
							{
								if (objectUsed[i].transform.localPosition.y - check2 < v.y)
								{
									v.y -= obj.vect.y;
								}
							}
							else if (obj.vect.y > 0.0f)
							{
								if (objectUsed[i].transform.localPosition.y + check2 > v.y)
								{
									v.y -= obj.vect.y;
								}
							}
							obj.transform.localPosition = v;


						}
					}
#if false
					else if (objectUsed[i].obj_type == TYPE.WATERPOOL)
					{
						float xx = objectUsed[i].transform.localPosition.x - obj.transform.localPosition.x;
						float yy = objectUsed[i].transform.localPosition.y - obj.transform.localPosition.y;
						float check1 = (xx * xx) + (yy * yy);
						float check2 = (objectUsed[i].MainHit.radius * objectUsed[i].transform.localScale.x) + (obj.MainHit.radius * obj.transform.localScale.x);
						if (check1 <= (check2 * check2))
						{
							CNT_WATER += 6;
							if (CNT_WATER > WATER_MAX)
							{
								CNT_WATER = WATER_MAX;
							}
							objectUsed[i].param[0]--;
							if (objectUsed[i].param[0] < 1)
							{
								objectUsed[i].param[0] = 1;
							}
						}
					}
#endif
					else if (objectUsed[i].obj_type == TYPE.FIRE)
					{
						float xx = objectUsed[i].transform.localPosition.x - obj.transform.localPosition.x;
						float yy = objectUsed[i].transform.localPosition.y - obj.transform.localPosition.y;
						float check1 = (xx * xx) + (yy * yy);
						float check2 = (objectUsed[i].MainHit.radius * objectUsed[i].transform.localScale.x) + (obj.MainHit.radius * obj.transform.localScale.x);
						if (check1 <= (check2 * check2))
						{
							if (objectUsed[i].count > 30)   // 火の出現直後は当たり判定を取らない
							{
								// ゲームオーバー判定
								obj.mode = 10;
							}
						}
					}
				}
			}
		}
	}


	//-------------------------------------------------------------------
	/// <summary>
	///		敵弾に位置を基準とした移動量を設定
	/// </summary>
	/// <param name="pos">基準座標(相手側)</param>
	/// <param name="spd">移動速度</param>
	/// <param name="rad_offset">追加角度(扇状弾などに使う)</param>
	/// <param name="rotation">自分の絵を回転させる？(true=回転させる・false=回転させない)</param>
	/// <returns>移動量(Vector3・Z軸は常に0)</returns>
	//-------------------------------------------------------------------
	public Vector3 SetVector(Vector3 pos, float spd, float rad_offset, bool rotation)
	{
		Vector3 vec = Vector3.zero;
		if (spd == 0.0f)    //必ず少しは動くようにする.
		{
			spd = 1f;
		}
		//spd = spd / 100.0f;
		float rad = Mathf.Atan2(
								pos.y - this.transform.position.y,
								pos.x - this.transform.position.x
								);              //座標から角度を求める.
		rad += rad_offset;                      //角度にオフセット値追加.
		vec.x = spd * Mathf.Cos(rad);           //角度からベクトル求める.
		vec.y = spd * Mathf.Sin(rad);
		if (rotation == true)
		{
			this.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90.0f + RadToRotation(rad)));
		}
		return vec;
	}
	//-------------------------------------------------------------------
	/// <summary>
	/// 角度と速度からxy移動量を算出する
	/// 戻り値はVector3
	/// </summary>
	/// <param name="ang">角度</param>
	/// <param name="spd">速度</param>
	/// <returns>実際の移動量(z軸は使わないので常に0)</returns>
	//-------------------------------------------------------------------
	public Vector3 AngleToVector3(float ang, float spd)
	{
		Vector3 vec = new Vector3(0, 0, 0);
		float rot = AngleToRotation(ang);
		float rad = ((rot) * Mathf.PI) / 180.0f;
		vec.x = spd * Mathf.Cos(rad);           //角度からベクトル求める.
		vec.y = spd * Mathf.Sin(rad);
		//vec = vec / 3;  // 一律で速度1/3に調整
		return vec;
	}


	//-------------------------------------------------------------------
	/// <summary>
	/// 自分の位置と相手の位置からラジアン角を取る
	/// </summary>
	/// <param name="mine">自分の位置</param>
	/// <param name="target">相手の位置</param>
	/// <returns>ラジアン角</returns>
	//-------------------------------------------------------------------
	public float GetRad(Vector3 mine,Vector3 target)
	{
		return Mathf.Atan2(target.y - mine.y,
							target.x - mine.x
						);
	}
	//-------------------------------------------------------------------
	/// <summary>
	/// ラジアン角からUnity回転角を取得(下が0、時計回りに359まで)
	/// </summary>
	/// <param name="rad">ラジアン角</param>
	/// <returns>360度方向系</returns>
	//-------------------------------------------------------------------
	public float RadToRotation(float rad)
	{
		return (float)(((rad * 180.0f) / Mathf.PI) % 360.0f);
	}
	//-------------------------------------------------------------------
	/// <summary>
	/// 256度方向系からUnity回転角を得る
	/// </summary>
	/// <param name="ang">256度方向系</param>
	/// <returns>Unity回転角</returns>
	//-------------------------------------------------------------------
	public float AngleToRotation(float ang)
	{
		return (float)(360.0f - (360.0f * ((ang % 256) / 256.0f)));
	}
	//-------------------------------------------------------------------
	/// <summary>
	/// Unity回転角から256度方向系を得る
	/// </summary>
	/// <param name="rot">Unity回転角</param>
	/// <returns>256度方向系</returns>
	//-------------------------------------------------------------------
	public int RotationToAngle(float rot)
	{
		float r = rot;
		if (r<=180.0f)
		{
			r = 360.0f - r;
		}
		else
		{
			r = 180.0f - r;
		}
		return (int)(((256.0f * (360.0f - ((r % 360.0f) / 360.0f)))) % 256);
	}
}
