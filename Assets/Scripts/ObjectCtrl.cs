/*
 * 
 *	ObjectCtrl.cs
 *		オブジェクトの固有動作の管理
 * 
 * 
 * 
 * 
 * 
 *		20221211	WSc101用に再構成
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectCtrl : MonoBehaviour
{
	[Space]
	[SerializeField]
	public int OBJcnt = 0;
	[Space]
	[SerializeField]
	public SpriteRenderer MainPic;		// メイン画像.(プライオリティ前面)
	[SerializeField]
	public SpriteRenderer GraphPic;		// グラフ用
	[SerializeField]
	public Transform MainPos;			// 座標・回転関連.
	[SerializeField]
	public CircleCollider2D MainHit;    // 当たり判定.
	[SerializeField]
	public TextMesh PlayerName;			// プレイヤーネーム(14文字まで)

	[Space]

	public int LIFE = 0;		// 耐久力.
	public bool NOHIT = false;	// 当たり判定の有無.
	[Space]
	public Vector3 target = new Vector3(0, 0, 0);
	public int speed = 0;						// 移動速度.
	public int angle = 0;						// 移動角度(360度を256段階で指定).
	public int oldangle = 0;					// 1int前の角度
	public int type = 0;						// プレイヤー番号
	public int mode = 0;						// 動作モード(キャラクタによって意味が違う).
	public int power = 0;						// 相手に与えるダメージ量.
	public int count = 0;						// 動作カウンタ.
	public int[] param = new int[4];			// パラメータ4個
	public Vector3[] parampos = new Vector3[4];	// テンポラリ座標4個
	public Vector3 vect = Vector3.zero;			// 移動量
	public int interval = 0;					// 0で自爆しない・任意の数値を入れるとカウント到達時に自爆


	public int ship_energy_charge = 0;



	[Space]

	readonly Color COLOR_NORMAL = new Color(1.0f, 1.0f, 1.0f, 1.0f);
	readonly Color COLOR_DAMAGE = new Color(1.0f, 0.0f, 0.0f, 1.0f);
	readonly Color COLOR_ERASE = new Color(0.0f, 0.0f, 0.0f, 0.0f);

	const float SHIP_MOVE_SPEED = 0.10f;
	const float WATER_MOVE_SPEED = 0.54f;

	const float WATER_PERCENTAGE = 0.001f;

	const float OFFSCREEN_MIN_X = -6.00f;
	const float OFFSCREEN_MAX_X = 6.00f;
	const float OFFSCREEN_MIN_Y = -4.00f;
	const float OFFSCREEN_MAX_Y = 4.00f;

	const float HITSIZE_MYSHIP = 0.16f;
	const float HITSIZE_MYSHOT = 0.64f;
	const float HITSIZE_ENEMY = 0.32f;

	public ObjectManager.MODE obj_mode = ObjectManager.MODE.NOUSE;  // キャラクタの管理状態.
	public ObjectManager.TYPE obj_type = ObjectManager.TYPE.NOUSE;  // キャラクタの分類(当たり判定時に必要).

	MainSceneCtrl MAIN;
	ObjectManager MANAGE;
	ReplayManager REPLAY;

	void Awake()
	{
		MAIN = GameObject.Find("root_game").GetComponent<MainSceneCtrl>();
		MANAGE = GameObject.Find("root_game").GetComponent<ObjectManager>();
		REPLAY = GameObject.Find("root_game").GetComponent<ReplayManager>();

		MainHit.enabled = false;
	}


	// Use this for initialization
	void Start()
	{
		for (int i = 0; i < param.Length; i++)
		{
			param[i] = 0;
		}
	}
	// Update is called once per frame
	void Update()
	{
		Vector3 pos = Vector3.zero;

		if (obj_mode == ObjectManager.MODE.NOUSE)
		{
			return;
		}
#if false
		if (ModeManager.mode == ModeManager.MODE.GAME_PAUSE)
		{
			return;
		}
#endif

		switch (obj_mode)
		{
			case ObjectManager.MODE.NOUSE:
				return;
			case ObjectManager.MODE.INIT:
				MainPic.enabled = true;
				count = 0;
				break;
			case ObjectManager.MODE.HIT:
				MainHit.enabled = true;
				break;
			case ObjectManager.MODE.NOHIT:
				MainHit.enabled = false;
				break;
			case ObjectManager.MODE.FINISH:
				MANAGE.Return(this);
				break;
		}
		switch (obj_type)
		{
			// 自機
			case ObjectManager.TYPE.MYSHIP:
				if (MAIN.count < 0) // メインカウンタに同期して動作・マイナスの場合動かない
				{
					return;
				}
				ReplayInput inp = REPLAY.GetControl(type);  // 入力を受け取る・1～8
				//if (type == 2)
				//	Debug.Log("ObjectCtrl:Update:MAIN.count=" + MAIN.count + " / controller=" + type + " / inp=" + inp.vector + ":" + inp.button);
				switch (mode)   // 自機の状態に応じて移動・攻撃を行う
				{
					case 0: // 初期化
						{
							this.transform.localEulerAngles = new Vector3(0, 0, 0);
							this.transform.localScale = new Vector3(1, 1, 1);
							obj_mode = ObjectManager.MODE.HIT;
							MainHit.enabled = false;                // 出現直後無敵
							MainHit.radius = HITSIZE_MYSHIP;
							NOHIT = false;
							MainPic.color = COLOR_NORMAL;
							MainPic.sortingOrder = 0;
							MainPic.sprite = MANAGE.SPR_SHIP[0];    // 自機画像
							GraphPic.enabled = false;
							PlayerName.text = "Player " + type.ToString();
							int y = REPLAY.RandomRange(0, 400) - 200;
							this.transform.localPosition = new Vector3(0, y, 0);
							angle = 0;
							param[0] = 0;   // 無敵時間
							if (REPLAY.RandomRange(0, 100) > 50)
							{
								param[1] = -1;  // 左サイド
							}
							else
							{
								param[1] = +1;  // 右サイド
								this.transform.localScale = new Vector3(-1, 1, 1);
								PlayerName.transform.localScale = new Vector3(-1, 1, 1);
							}
							param[2] = 0;   // サイド変更回数制限
							param[3] = -1;  // 上下移動スピードアップカウンタ
							mode = 1;
							power = 1;
							LIFE = 1;
						}
						break;
					case 1: // 通常(無敵時間)
						{
							if ((count % 8) >> 2 == 0)
							{
								MainPic.enabled = false;
							}
							else
							{
								MainPic.enabled = true;
							}

							bool side_change = false;
							if (inp.vector.x <= -0.4f)
							{
								if (this.transform.localPosition.x > 0)
								{
									side_change = true;
								}
							}
							else if (inp.vector.x >= 0.4f)
							{
								if (this.transform.localPosition.x < 0)
								{
									side_change = true;
								}
							}
							if (side_change == true)
							{
								if (++param[2] < MANAGE.CNT_CHANGE_SIDE)    // 回数制限に引っ掛かるまでサイド変更可能
								{
									param[1] = 0 - param[1];
									count = 0;
									param[0] = 0;
									this.transform.localScale = new Vector3(0 - this.transform.localScale.x, 1, 1);
									PlayerName.transform.localScale = new Vector3(0 - PlayerName.transform.localScale.x, 1, 1);
								}
							}
							if (param[0] <= 60)
							{
								Vector3 v = this.transform.localPosition;
								v.x = param[1] * ((float)param[0] / 60.0f) * 250f;
								vect.x = v.x;
								//Debug.Log("Vector3.x=" + v.x + " / param[0]/30.0f=" + ((float)param[0] / 30.0f));
								this.transform.localPosition = v;
								Vector3 s = this.transform.localScale;
								s.x = (1 + (((float)param[0] / 60.0f) - 1.0f) * 5.0f) * -param[1];
								s.y = (1 + (((float)param[0] / 60.0f) - 1.0f) * 5.0f);
								this.transform.localScale = s;
							}

							if (++param[0] > 120)    // 無敵時間終了
							{
								if (this.transform.localPosition.x < 0)
								{
									param[2] = 0;   // ここから左右サイドの表示用
								}
								else
								{
									param[2] = 1;
								}
								MainHit.enabled = true;
								MainPic.enabled = true;
								MANAGE.CNT_PLAYER_SIDE[param[2]]++;
								param[3] = -1;
								mode = 2;
							}
						}
						break;
					case 2: // 通常時処理
						{
							if (LIFE <= 0)	// プレイヤーやられ
							{
								mode = 10;
								param[0] = 0;
								MainHit.enabled = false;
							}
						}
						break;
					case 10:    // 死亡処理
						{
							if (param[0] < 120)
							{
								// 死亡演出
								this.transform.localEulerAngles += new Vector3(0, 0, 35);   // ぶっとび回転
								this.transform.localScale += new Vector3(0.05f, 0.05f, 0);
								param[0]++;
							}
							else if (param[0] < 180)
							{
								this.transform.localEulerAngles += new Vector3(0, 0, 2);   // ぶっとび回転・溜めてゆっくり
								this.transform.localScale += new Vector3(0.05f, 0.05f, 0);
								param[0]++;
							}
							else
							{
								MANAGE.CNT_PLAYER_SIDE[param[2]]--; // 自分の存在したサイドから消滅
								count = -1;     // 初期化に戻って復活
								param[0] = 0;
								param[1] = 0;
								param[2] = 0;
								param[3] = 0;
								mode = 0;
							}
						}
						break;
				}

				vect = new Vector3(0, 0, 0);
				if (inp.button == true)
				{
					if (mode == 2)
					{
						if (param[3] == -1)
						{
							MANAGE.Set(ObjectManager.TYPE.MYSHOT, param[2], this.transform.localPosition, param[2], 0); // 射撃
							param[3]++;
						}
					}
				}
				if (param[3] >= 0)
				{
					param[3]++;
				}
				if (inp.button==true)
				{
					if (param[3]<=MANAGE.CNT_TIMER_TURBO)
					{
						if (inp.vector.y > 0.4f)
						{
							vect.y = MANAGE.CNT_STEPS_TURBO;    // 押しっぱなしで期限切れたら通常移動
						}
						else if (inp.vector.y < -0.4f)
						{
							vect.y = -1 * MANAGE.CNT_STEPS_TURBO;    // 押しっぱなしで期限切れたら通常移動
						}
					}
					else
					{
						if (inp.vector.y > 0.4f)
						{
							vect.y = MANAGE.CNT_STEPS_NORMAL;    // 押しっぱなしで期限切れたら通常移動
						}
						else if (inp.vector.y < -0.4f)
						{
							vect.y = -1 * MANAGE.CNT_STEPS_NORMAL;    // 押しっぱなしで期限切れたら通常移動
						}
					}
				}
				else
				{
					if (inp.vector.y > 0.4f)
					{
						vect.y = MANAGE.CNT_STEPS_NORMAL;    // 押しっぱなしで期限切れたら通常移動
					}
					else if (inp.vector.y < -0.4f)
					{
						vect.y = -1 * MANAGE.CNT_STEPS_NORMAL;    // 押しっぱなしで期限切れたら通常移動
					}
				}
				if (param[3] > MANAGE.CNT_TIMER_TURBO)
				{
					if (inp.button == false)
					{
						param[3] = -1;
					}
				}
#if false
				if (param[3] <= MANAGE.CNT_TIMER_TURBO)
				{
					PlayerName.text = "case 1";
					if (inp.vector.y > 0.4f)
					{
						vect.y = MANAGE.CNT_STEPS_NORMAL;    // 押しっぱなしで期限切れたら通常移動
					}
					else if (inp.vector.y < -0.4f)
					{
						vect.y = -1 * MANAGE.CNT_STEPS_NORMAL;    // 押しっぱなしで期限切れたら通常移動
					}
				}
				else if (inp.button == true)
				{
					PlayerName.text = "case 2";
					if (param[3] >= MANAGE.CNT_TIMER_TURBO)
					{
						if (inp.vector.y > 0.4f)
						{
							vect.y = MANAGE.CNT_STEPS_TURBO;    // 押しっぱなしで期限切れたら通常移動
						}
						else if (inp.vector.y < -0.4f)
						{
							vect.y = -1 * MANAGE.CNT_STEPS_TURBO;    // 押しっぱなしで期限切れたら通常移動
						}
					}

					else
					{
						PlayerName.text = "case 3";
						if (inp.vector.y > 0.4f)
						{
							vect.y = MANAGE.CNT_STEPS_NORMAL;       // 射撃時スピードアップ
						}
						else if (inp.vector.y < -0.4f)
						{
							vect.y = -1 * MANAGE.CNT_STEPS_NORMAL;      // 射撃時スピードアップ
						}
					}

				}
				else
				{
					PlayerName.text = "case 4";
					param[3] = -1;
					if (inp.vector.y > 0.4f)
					{
						vect.y = MANAGE.CNT_STEPS_NORMAL;       // 射撃時スピードアップ
					}
					else if (inp.vector.y < -0.4f)
					{
						vect.y = -1 * MANAGE.CNT_STEPS_NORMAL;      // 射撃時スピードアップ
					}
				}
#endif
#if false
				//else // ボタン押されてない・通常速度での移動
				{
					param[3]++;
					if (param[3] >= MANAGE.CNT_TIMER_TURBO)
					{
						param[3] = -1;
					}
					if (inp.vector.y > 0.4f)
					{
						vect.y = MANAGE.CNT_STEPS_NORMAL;    // 通常移動
					}
					else if (inp.vector.y < -0.4f)
					{
						vect.y = -1 * MANAGE.CNT_STEPS_NORMAL;    // 通常移動
					}
				}
#endif
				this.transform.localPosition += vect;
				if (this.transform.localPosition.y > 220)	// 移動制限
				{
					Vector3 v = this.transform.localPosition;
					v.y = 220;
					this.transform.localPosition = v;
				}
				else if (this.transform.localPosition.y < -220)
				{
					Vector3 v = this.transform.localPosition;
					v.y = -220;
					this.transform.localPosition = v;
				}
				break;



			/*
			 ビーム



			 */

			case ObjectManager.TYPE.MYSHOT:
				if (count == 0)
				{
					obj_mode = ObjectManager.MODE.HIT;
					MainHit.enabled = true;
					MainHit.radius = HITSIZE_MYSHOT;
					NOHIT = false;
					LIFE = 1;
					power = 1;
					MainPic.color = COLOR_NORMAL;
					MainPic.sortingOrder = 4;
					MainPic.sprite = MANAGE.SPR_MYSHOT[0];
					GraphPic.enabled = false;
					this.transform.localScale = new Vector3(1, 1, 1);
					param[0] = 10;
					if(mode==0)
					{
						this.transform.localEulerAngles = new Vector3(0, 0, 90);
						this.transform.localPosition = new Vector3(-200, this.transform.localPosition.y, 0);
					}
					else
					{
						this.transform.localEulerAngles = new Vector3(0, 0, -90);
						this.transform.localPosition = new Vector3(200, this.transform.localPosition.y, 0);
					}
				}
				if (LIFE==0)
				{
					MANAGE.Return(this);	// ビーム削除
				}
				switch(count)
				{
					case 0:
						break;
					case 1:
						if (mode == 0)
						{
							this.transform.localPosition = new Vector3(-174, this.transform.localPosition.y, 0);
						}
						else
						{
							this.transform.localPosition = new Vector3(174, this.transform.localPosition.y, 0);
						}
						this.transform.localScale = new Vector3(1, 2, 1);
						break;
					case 2:
						if (mode == 0)
						{
							this.transform.localPosition = new Vector3(-138, this.transform.localPosition.y, 0);
						}
						else
						{
							this.transform.localPosition = new Vector3(138, this.transform.localPosition.y, 0);
						}
						this.transform.localScale = new Vector3(1, 3, 1);
						break;
					case 3:
						if (mode == 0)
						{
							this.transform.localPosition = new Vector3(-104, this.transform.localPosition.y, 0);
						}
						else
						{
							this.transform.localPosition = new Vector3(104, this.transform.localPosition.y, 0);
						}
						this.transform.localScale = new Vector3(1, 4, 1);
						break;
					case 4:
						if (mode == 0)
						{
							this.transform.localPosition = new Vector3(-80, this.transform.localPosition.y, 0);
						}
						else
						{
							this.transform.localPosition = new Vector3(80, this.transform.localPosition.y, 0);
						}
						this.transform.localScale = new Vector3(1, 5, 1);
						break;
					case 5:
						if (mode == 0)
						{
							this.transform.localPosition = new Vector3(-50, this.transform.localPosition.y, 0);
						}
						else
						{
							this.transform.localPosition = new Vector3(50, this.transform.localPosition.y, 0);
						}
						this.transform.localScale = new Vector3(1, 6, 1);
						break;
					case 10:
						MANAGE.Return(this);
						break;
				}
				// 実移動及び表示処理
				//pos = this.transform.localPosition;
				//pos += MANAGE.AngleToVector3(angle, WATER_MOVE_SPEED);
				//this.transform.localPosition = pos;
				//this.transform.localEulerAngles = new Vector3(0, 0, MANAGE.AngleToRotation(angle));
				//if (count >= param[0])
				{
					//MANAGE.Return(this);
				}
				break;



				/*
				 隕石(デブリ)
				 発光しているが気にしない

				 
				 
				 */
			case ObjectManager.TYPE.DEBRIS:
				if (count == 0)
				{
					obj_mode = ObjectManager.MODE.HIT;
					MainPic.sprite = MANAGE.SPR_ENEMY[0];
					MainHit.enabled = true;
					MainHit.radius = 0.32f;
					MainPic.color = COLOR_NORMAL;
					MainPic.sortingOrder = -2;
					GraphPic.enabled = false;
					NOHIT = false;
					power = 1;
					LIFE = 1;
					vect.x = 0;
					switch(this.transform.localPosition.x)	// 発生座標に応じて移動量変更
					{
						case -115:
							vect.y = +8;
							break;
						case -70:
							vect.y = -4;
							break;
						case -25:
							vect.y = +1;
							break;
						case 25:
							vect.y = -1;
							break;
						case 70:
							vect.y = +4;
							break;
						case 115:
							vect.y = -8;
							break;
					}
				}
				this.transform.localPosition += vect;
				MainPic.sprite = MANAGE.SPR_ENEMY[(count >> 2) % MANAGE.SPR_ENEMY.Length];
				if (
						(this.transform.localPosition.y >= 350)	// 画面外に出たら消滅
					|| (this.transform.localPosition.y <= -350)
					)
				{
					MANAGE.Return(this);
				}
				if (LIFE == 0)
				{
					for(int i=0;i<4;i++)
					{
						MANAGE.Set(ObjectManager.TYPE.NOHIT_EFFECT, 1, this.transform.localPosition, REPLAY.RandomRange(0, 256), REPLAY.RandomRange(2, 5));
					}
					MANAGE.Return(this);
				}
				break;


#if false

			/*
			壁



			*/
			case ObjectManager.TYPE.WALL:
				if (count == 0)
				{
					obj_mode = ObjectManager.MODE.HIT;
					MainPic.sprite = MANAGE.SPR_WALL[0];
					MainPic.color = COLOR_NORMAL;
					MainPic.sortingOrder = -1;
					GraphPic.enabled = false;
					NOHIT = false;
					MainHit.enabled = true;
					MainHit.radius = 0.30f;
					power = 1;
					LIFE = 1;
				}
				break;
#endif
#if false

			/*================================================================================
			 * 
			 * ここから敵
			 * 
			 ================================================================================*/
			case ObjectManager.TYPE.FIRE:
				if (count==0)
				{
					obj_mode = ObjectManager.MODE.HIT;
					MainPic.sprite = MANAGE.SPR_ENEMY[0];
					MainPic.color = COLOR_NORMAL;
					MainPic.sortingOrder = 3;
					GraphPic.enabled = false;
					NOHIT = false;
					power = 1;

					switch(mode)
					{
						case 0:
							LIFE = 300; // 火元・ちゃんと消火しないと駄目
							MainHit.enabled = false;    // 出現直後無敵(ヒット判定なし)
							MainHit.radius = HITSIZE_ENEMY;
							GraphPic.enabled = true;
							GraphPic.sprite = MANAGE.SPR_ENEMY[4];
							if (GraphPic.material.HasProperty("_Ratio") == true)    // シェーダーパラメータが存在すれば書き換え(自分周囲の円ゲージ制御)
							{
								GraphPic.material.SetFloat("_Ratio", 0.0f);
							}
							if (ModeManager.mode != ModeManager.MODE.TITLE)	// 火元発生音
							{
								SoundManager.Instance.PlaySE((int)SoundHeader.SE.CREATE_FIRE);
							}
							break;
						case 1:
							LIFE = 30;  // 飛び火1・消した方が良い・水に当たると消える
							MainHit.enabled = true;
							MainHit.radius = HITSIZE_ENEMY;
							vect = MANAGE.AngleToVector3(angle, speed) / 100.0f;	// 移動する
							break;
						case 2:
							LIFE = 10;  // 飛び火2・速いが勝手に消えることがある
							MainHit.enabled = true;
							MainHit.radius = HITSIZE_ENEMY;
							break;
					}
					param[0] = 0;	
					param[1] = 0;
					param[2] = 120;	// 火元(固定)の最大サイズ
				}
				MainPic.sprite = MANAGE.SPR_ENEMY[(count >> 2) % 4];
				switch(mode)
				{
					case 0: // 火元(動かない・ある程度まで拡大を続ける)
						if (count <= 30)
						{
							this.transform.localScale = new Vector3(((float)count / 30.0f) * 2, ((float)count / 30.0f) * 2, 1);
							if (count < 28)
							{
								GraphPic.sprite = MANAGE.SPR_ENEMY[7 - (count / 7)];
							}
							if (count == 30)
							{
								MainHit.enabled = true;
								MainHit.radius = HITSIZE_ENEMY;
								GraphPic.enabled = false;
								param[0] = 30;  // 炎の大きさ
								param[1] = 0;   // 放水を受けていないフレーム数(カウンタが上がるにつれ炎の大きさが増す)
							}
						}
						else
						{
							this.transform.localScale = new Vector3(((float)param[0] / 30.0f) * 2, ((float)param[0] / 30.0f) * 2, 1);
							param[1]++;
							if (param[1] >= 10)
							{
								param[1] = 0;
								param[0]++;
								if (param[0] > param[2])
								{
									param[0] = param[2];
								}
								if (param[0] > 30)
								{
									if (MANAGE.GetRestObject() > 600)
									{
										int rnd = REPLAY.RandomRange(0, 100);
										if (rnd > 50)
										{
											int ang = REPLAY.RandomRange(0, 256);
											int spd = REPLAY.RandomRange(1, 4);
											MANAGE.Set(ObjectManager.TYPE.FIRE, 1, this.transform.localPosition, ang, spd);
											MANAGE.CNT_MOVEFIRE_REMAIN++;
										}
									}
								}
							}
						}
						break;
					case 1: // 飛び火1(火元から出現・ゆっくり移動しながら火元を設置)
						this.transform.localPosition += vect;
						if (Mathf.Abs(this.transform.localPosition.x) > OFFSCREEN_MAX_X)
						{
							vect.x = 0 - vect.x;
						}
						if (Mathf.Abs(this.transform.localPosition.y) > OFFSCREEN_MAX_Y)
						{
							vect.y = 0 - vect.y;
						}
						if (count % 160 == 120)
						{
							if (MANAGE.CNT_FIRE > 0)
							{
								if (MANAGE.GetRestObject() > 600)
								{
									int rnd = REPLAY.RandomRange(0, 100);
									if (rnd > 70)
									{
										MANAGE.Set(ObjectManager.TYPE.FIRE, 0, this.transform.localPosition, 0, 0);
										MANAGE.CNT_FIRE--;
									}
								}
							}
						}
						break;
					case 2:	// 飛び火2(火元から出現・そこそこ素早い移動・勝手に消えることがある)
						break;
				}
				//vect = manage.AngleToVector3(angle, speed+main.difficulty);
				//this.transform.localPosition += vect;
				break;
#endif
			/******************************************************
			 * 
			 * 
			 * 
			 * 
			 * ここからエフェクトなど
			 * 
			 * 
			 *
			 ******************************************************
			 */
			case ObjectManager.TYPE.NOHIT_EFFECT:
				if (count == 0)
				{
					obj_mode = ObjectManager.MODE.NOHIT;
					MainHit.enabled = false;
					MainPic.enabled = true;
					LIFE = 1;
					vect = MANAGE.AngleToVector3(angle, speed * 0.05f);
					MainPic.sprite = MANAGE.SPR_CRUSH[0];
					MainPic.sortingOrder = 5;
					vect = MANAGE.AngleToVector3(angle, speed);
				}
				else if (count >= 16)
				{
					MANAGE.Return(this);
				}
				else
				{
					this.transform.localPosition += vect;
					this.transform.localScale = this.transform.localScale * 1.1f;
					MainPic.sprite = MANAGE.SPR_CRUSH[count >> 1];
				}
				break;
		}

		// 自前衝突判定を使う場合
		MANAGE.CheckHit(this);

		if (LIFE <= 0)	// 死亡確認
		{
			Dead();
		}

		count++;
	}







#if false
	/// <summary>
	/// 当たり判定部・スプライト同士が衝突した時に走る
	/// </summary>
	/// <param name="collider">衝突したスプライト当たり情報</param>
	void OnTriggerStay2D(Collider2D collider)
	{
		if (obj_mode == ObjectManager.MODE.NOHIT)
		{
			return;
		}
		ObjectCtrl other = collider.gameObject.GetComponent<ObjectCtrl>();
		if (other.obj_mode == ObjectManager.MODE.NOHIT)
		{
			return;
		}
		if (NOHIT == true)
		{
			return;
		}
		if (other.NOHIT == true)
		{
			return;
		}
		switch (other.obj_type)
		{
			case ObjectManager.TYPE.MYSHIP:
				{
					if (obj_type == ObjectManager.TYPE.FIRE)
					{
						// ゲームオーバー処理
					}
				}
				break;
			case ObjectManager.TYPE.MYSHOT:
				{
					if (obj_type == ObjectManager.TYPE.FIRE)
					{
						param[0]--;
						if (param[0] < 10)
						{
							param[0] = 10;
						}
						if (param[0] <= 20)
						{
							Damage(10);
						}
					}
				}
				break;
			case ObjectManager.TYPE.WATERPOOL:
				{
					switch(obj_type)
					{
						case ObjectManager.TYPE.FIRE:
							if (mode != 0)
							{
								Damage(10);
							}
							else
							{
								param[1] = 0;
							}
							break;
						case ObjectManager.TYPE.MYSHIP:
							manage.CNT_WATER += 6;
							if (manage.CNT_WATER > manage.WATER_MAX)
							{
								manage.CNT_WATER = manage.WATER_MAX;
							}
							other.param[0]--;
							if (other.param[0] < 1)
							{
								other.param[0] = 1;
							}
							break;
					}
				}
				break;
			case ObjectManager.TYPE.FIRE:
				{
					switch (obj_type)
					{
						case ObjectManager.TYPE.MYSHIP:
						case ObjectManager.TYPE.MYSHOT:
							{

							}
							break;
						case ObjectManager.TYPE.WATERPOOL:
							{

							}
							break;
					}
				}
				break;
		}
	}
#endif

	/// <summary>
	/// ダメージ与える
	/// </summary>
	/// <param name="damage">ダメージ量</param>
	public void Damage(int damage)
	{
		LIFE -= damage;
			if (LIFE <= 0)
			{
				//Dead();	// リプレイある時はダメージ関数で死亡処理を行わない
			}
	}

	/// <summary>
	///		死んだ時の処理全般
	/// </summary>
	public void Dead()
	{
		obj_mode = ObjectManager.MODE.NOHIT;
		switch (obj_type)
		{
			case ObjectManager.TYPE.MYSHOT:
				MANAGE.Return(this);
				break;
#if false
			case ObjectManager.TYPE.FIRE:
				if (mode == 0)
				{
					MANAGE.CNT_FIRE_REMAIN++;
					if (ModeManager.mode != ModeManager.MODE.TITLE)
					{
						SoundManager.Instance.PlaySE((int)SoundHeader.SE.DESTROY_FIRE1);
					}
				}
				else
				{
					MANAGE.CNT_MOVEFIRE_REMAIN--;
					if (ModeManager.mode == ModeManager.MODE.GAME_PLAY)
					{
						SoundManager.Instance.PlaySE((int)SoundHeader.SE.DESTROY_FIRE2);
					}
				}
				if (MANAGE.CNT_MOVEFIRE_REMAIN <= 0)
				{
					if (MANAGE.CNT_FIRE_REMAIN == MANAGE.CNT_FIRE_MAX)
					{
						// ゲームクリア
						MAIN.sw_game_clear = true;
					}
				}
				MANAGE.Return(this);
				break;
#endif
		}
		MainPic.color = COLOR_NORMAL;
		count = 0;
	}


	public void DisplayOff()
	{
		MainPic.enabled = false;
		MainHit.enabled = false;
		MainPic.color = COLOR_NORMAL;
		MainPic.sprite = null;
	}


}
