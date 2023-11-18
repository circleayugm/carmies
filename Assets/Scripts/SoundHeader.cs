/* ==========================================================================================================
 * 
 *		SoundHeader.cs
 *		Circle-AY.Info / U.G.M.
 *		
 *		音周りの定義ファイル
 *		番号で呼ぶと見通しが悪いので名前を定義する
 *		SoundManagerの再生関数と同時に使われることを想定している
 * 
 *		20150321	とりあえず作成
 *		20220810	RSc100用に更新
 *		20221213	WSc101用に更新
 * 
 * ========================================================================================================== 
 */

using UnityEngine;
using System.Collections;

public class SoundHeader
{

	//曲列挙.
	public enum BGM
	{
		GAME_PLAY=0,
		CLEAR,
		Count
	}

	//SE列挙.
	public enum SE
	{
		SYNTH=0,
		BEAM,
		BANG1,
		BANG2,

		Count
	}
		
	//音声列挙.
	public enum VOICE
	{
		Count
	}

}
