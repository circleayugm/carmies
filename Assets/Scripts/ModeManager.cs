/*******************************************************************************************************
 * 
 *	ModeManager.cs 
 *		現在のモードを返すグローバル変数を持つ
 *		ここを切り替えてシーンチェンジに移行するように組む
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 *	20221211	3日前くらいにWSc101用に再構成
 * 
 * 
********************************************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModeManager : MonoBehaviour {
	// モード
	public enum MODE
	{
		//INITIALIZE = 0,
		TITLE=0,
		GAME_DEMO,
		GAME_PLAY,
		//GAME_PAUSE,
		MAX
	};
	static readonly string[] MODE_SCENE = new string[]
	{
		//"00_initialize",
		"01_Title",
		"02_DemoGame",
		"04_MainGame",
		//"04_MainGame",
		"SCENE_MAX"
	};

	public static MODE mode = MODE.TITLE;	// 現在のモード

	// モードチェンジ関数
	public static MODE ChangeMode(MODE newmode)
	{
		if (mode != newmode)
		{
#if false
			if ((mode == MODE.GAME_PLAY) && (newmode == MODE.GAME_PAUSE))
			{
				// ポーズ音・ポーズ処理
				mode = newmode;
			}
			else if ((mode == MODE.GAME_PAUSE) && (newmode == MODE.GAME_PLAY))
			{
				// ポーズ解除音・ポーズ解除処理
				mode = newmode;
			}
			else 
#endif
			{
				mode = newmode;
				SceneManager.LoadScene(MODE_SCENE[(int)newmode]);
			}
		}
		return mode;
	}
}
