using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace epoching.fps
{

    public class Normal_game_control : MonoBehaviour
    {
        //单列模式 Singleton mode
        public static Normal_game_control instance;

        //游戏状态 game Statu
        public Game_statu game_statu;

        [Header("UI canvas")]
        public GameObject game_obj_ui_canvas;

        [Header("game role control")]
        public Game_role_control game_role_control;


        void Awake()
        {
            //Singleton mode
            Normal_game_control.instance = this;
        }

        void Start()
        {
            //listen event
            this.game_role_control.player_be_killed_event += on_player_be_killed_event;

            //切换到游戏运行状态 Switch to game running state
            this.change_to_game_start();
        }

        void Update()
        {
            //按ESC键 退出游戏 Press the ESC key to exit the game
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        //切换到游戏开始状态 Switch to game start state
        public void change_to_game_start()
        {
            //显示鼠标 Show mouse
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;

            //显示遮挡UI Show occlusion UI
            Canvas_confirm_box.confirm_box("Welcome To The Valley", "A large wave of skeleton soldiers is coming, go kill them", "cancel", "Kill Them", true, delegate () { }, delegate ()
            {
                this.change_to_gaming();

            });

            this.game_statu = Game_statu.game_start;
        }

        //切换到游戏进行状态 Switch to game progress
        public void change_to_gaming()
        {
            //隐藏鼠标鼠标 Hide mouse
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            //reset player
            this.game_role_control.reset();

            //移除掉之前所有的士兵 Remove all previous soldiers
            GameObject[] enemy_array = GameObject.FindGameObjectsWithTag("enemy");
            for (int i = 0; i < enemy_array.Length; i++)
            {
                Destroy(enemy_array[i].gameObject);
            }

            //移除掉所有的油桶 Remove all the oil drums
            if (Powder_keg_manager.instance != null)
                Powder_keg_manager.instance.clear_all_powder_kegs();

            //reset Enemy_manager
            Enemy_manager.instance.reset();

            //重新生成火药桶 Regenerate the gunpowder barrel
            if (Powder_keg_manager.instance != null)
                Powder_keg_manager.instance.init_create_powder_kegs();

            //开始生成敌军 Start generating enemy troops
            if (Enemy_manager.instance != null)
                StartCoroutine(Enemy_manager.instance.create_a_round_enemy());


            //修改状态 Modify status
            this.game_statu = Game_statu.gaming;
        }

        //切换到游戏结束状态 Switch to game over state
        public void change_to_game_over(bool is_win)
        {
            //显示鼠标 Show mouse
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            //所有的士兵进入到idle状态，停止攻击和寻路 All soldiers enter the idle state, stop attacking and find their way
            GameObject[] enemy_array = GameObject.FindGameObjectsWithTag("enemy");
            for (int i = 0; i < enemy_array.Length; i++)
            {
                enemy_array[i].gameObject.GetComponent<Enemy_control>().game_fail_over();
            }

            if (is_win == false)
            {
                Audio_control.instance.play_fail_audio();

                //显示结束页面 Show end page
                Canvas_confirm_box.confirm_box("Game Over", "You have been killed！", "cancel", "Restart", true, delegate () { }, delegate ()
                {
                    this.change_to_gaming();
                });
            }
            else
            {
                Audio_control.instance.play_win();


                //显示结束页面 Show end page
                Canvas_confirm_box.confirm_box("Victory", "Congratulations on killing all the skeleton soldiers！", "cancel", "Restart", true, delegate () { }, delegate ()
                {
                    this.change_to_gaming();
                });

                Audio_control.instance.play_win();
            }



            //Modify status
            this.game_statu = Game_statu.game_over;
        }

        //event
        #region 
        //crystal be broken event
        private void on_player_be_killed_event()
        {
            this.change_to_game_over(false);
        }
        #endregion
    }


}
