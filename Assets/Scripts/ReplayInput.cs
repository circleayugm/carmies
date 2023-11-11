/*
 *	ReplayInput.cs
 *		リプレイ入力用クラス変数
 * 
 * 
 * 
 * 
 *	20221211	3日前くらいに作成
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayInput
{
	public uint random_seed{ get; set; }
	public uint player{ get; set; }
	public Vector2 vector{ get; set; }
	public bool button{ get; set; }
	public int count{ get; set; }

	public ReplayInput()
	{
		random_seed = 0;            // ランダムシード
		player = 1;					// プレイヤー番号(コントローラー取得・パケット探索用)
		vector = new Vector2(0, 0);	// スティックの傾き
		button = false;				// ボタン
		count = -1;					// 進行カウンタ
	}
}
