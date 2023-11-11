using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneCtrl : MonoBehaviour
{

    ReplayManager REPLAY;
    ObjectManager MANAGE;


    public int count = -1;
    public bool sw_game_end = false;
    public bool sw_game_clear = false;


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        REPLAY = GameObject.Find("root_game").GetComponent<ReplayManager>();
        MANAGE = GameObject.Find("root_game").GetComponent<ObjectManager>();
        if (MANAGE != null)
        {
            while (MANAGE.SW_BOOT != true)
            {
                // ãNìÆë“Çøéûä‘
            }
        }
        if (ModeManager.mode != ModeManager.MODE.GAME_PLAY)
        {
            REPLAY.Load("replay.dat");
            if (REPLAY != null)
            {
                while (REPLAY.SW_READ != false)
                {
                    // ì«Ç›çûÇ›ë“Çøéûä‘
                }
            }
            REPLAY.SW_REPLAY = false;   // ÉäÉvÉåÉCí‚é~
        }
        }

        // Update is called once per frame
        void Update()
    {
        if (count < 0)
        {
            return;
		}
        switch(ModeManager.mode)
        {
            case ModeManager.MODE.TITLE:
                {
                    if (count==0)
                    {
                        REPLAY.Reset(0);
					}
                    if (Input.GetButtonDown("Fire1")==true)
                    {
                        ModeManager.ChangeMode(ModeManager.MODE.GAME_PLAY);
                        count = -1;
					}
                }
                break;
            case ModeManager.MODE.GAME_PLAY:
                {
                    if (count == 0)
                    {
                        REPLAY.Reset(0);
                    }
                    SetEnemy();

                }
                break;
		}
        count++;
    }



    void SetStartScene()
    {

	}
    void SetEnemy()
    {
        if ((count % 16) == 0)
        {
            if(REPLAY.RandomRange(0,100)>30)
            {
                MANAGE.Set(ObjectManager.TYPE.DEBRIS, 0, new Vector3(-115, -340, 0), 0, 0);
			}
            if (REPLAY.RandomRange(0, 100) > 30)
            {
                MANAGE.Set(ObjectManager.TYPE.DEBRIS, 0, new Vector3(115, 340, 0), 0, 0);
            }
        }
        if ((count % 32) == 0)
        {
            if (REPLAY.RandomRange(0, 100) > 30)
            {
                MANAGE.Set(ObjectManager.TYPE.DEBRIS, 0, new Vector3(-70, 340, 0), 0, 0);
            }
            if (REPLAY.RandomRange(0, 100) > 30)
            {
                MANAGE.Set(ObjectManager.TYPE.DEBRIS, 0, new Vector3(70, -340, 0), 0, 0);
            }

        }
        if ((count % 64) == 0)
        {
            if (REPLAY.RandomRange(0, 100) > 30)
            {
                MANAGE.Set(ObjectManager.TYPE.DEBRIS, 0, new Vector3(-25, -340, 0), 0, 0);
            }
            if (REPLAY.RandomRange(0, 100) > 30)
            {
                MANAGE.Set(ObjectManager.TYPE.DEBRIS, 0, new Vector3(25, 340, 0), 0, 0);
            }
        }

    }
}
