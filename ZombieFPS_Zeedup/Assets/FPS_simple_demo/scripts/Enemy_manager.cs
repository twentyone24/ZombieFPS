using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;


//一波一波的出兵，管理这个波的脚本
//生成兵
//记录波
//切换波
//波数据
//Sent troops one after another, the script to manage this wave
//Generate pawn
//Record wave
//Switch wave
//Wave data
namespace epoching.fps
{
    public class Enemy_manager : MonoBehaviour
    {
        //单列模式 Single column mode
        public static Enemy_manager instance;

        //当前是第几波,最小为1 What is the current wave, the minimum is 1
        public int level_index = 0;

        [Header("map index")]
        public int map_index = 0;

        [Header("enemy prefab")]
        public GameObject[] enemy_array;

        [Header("The enemy generates a point object transform")]
        public Transform[] born_position;

        [Header("寄存下每一波现在还有好多人活着 There are still many people alive after saving every wave")]
        public int this_level_still_alive = -1;

        [Header("每一波兵出现的间隔时间 The time between each wave of soldiers")]
        public float level_interval;


        //这个地图一共有多少波兵 How many waves are there on this map
        private int this_map_enemy_wave_account;

        //生成第一波兵 Generate the first wave of soldiers
        void Awake()
        {
            Enemy_manager.instance = this;
        }


        void Start()
        {
            //这个地图总的关卡数 The total number of levels on this map
            this.this_map_enemy_wave_account = Config.json_data_enemy_create["maps"][this.map_index]["levels"].Count;
        }

        void Update()
        {
            if (Normal_game_control.instance.game_statu == Game_statu.gaming)
            {
                if (this.this_level_still_alive == 0)
                {
                    //兵已经死亡 Soldier is dead
                    this.this_level_still_alive = -1;

                    //出下一波兵 Put out the next wave
                    StartCoroutine(this.show_next_round_enemy());
                }
            }

        }

        public void reset()
        {
            this.level_index = 0;
        }

        //生成一波敌军 当前的level_index Generate a wave of enemy forces Current level_index
        public IEnumerator create_a_round_enemy()
        {
            Canvas_toast.toast(" wave " + (this.level_index + 1) + " ");

            JsonData json_data = Config.json_data_enemy_create["maps"][this.map_index]["levels"][this.level_index]["enemies"];

            //设置当前这波兵还剩多少 Set how many soldiers are left in the current wave
            this.this_level_still_alive = json_data.Count;

            for (int i = 0; i < json_data.Count; i++)
            {
                float num = Random.Range((float)json_data[i]["time"][0], (float)json_data[i]["time"][1]);
                yield return new WaitForSeconds(num);
                this.create_one_enemy(json_data[i]);
            }
        }

        //生成一个敌军 Spawn an enemy
        public void create_one_enemy(JsonData json_data)
        {
            //获取敌军生成点位 Acquire enemy spawn points
            int pos_index = (int)(json_data["position_index"][Random.Range(0, (int)json_data["position_index"].Count)]);

            //获取敌军类型索引 Get index of enemy type
            int type_index = (int)(json_data["type_index"][Random.Range(0, (int)json_data["type_index"].Count)]);

            //获取速度和游离度 Acquisition speed and freeness
            float speed = Random.Range((float)json_data["speed"][0], (float)json_data["speed"][1]);
            float freeness = Random.Range((float)json_data["freeness"][0], (float)json_data["freeness"][1]);

            //生成敌军 Spawn an enemy
            GameObject enemy = Instantiate(this.enemy_array[type_index],
                new Vector3(this.born_position[pos_index].position.x, 10, this.born_position[pos_index].position.z), this.enemy_array[type_index].transform.rotation) as GameObject;
            //设置速度 Set speed
            enemy.GetComponent<Enemy_control>().set_speed(speed);
        }

        //出下一波兵 Put out the next wave
        public IEnumerator show_next_round_enemy()
        {
            //先等2秒，让玩家休息一下 先等2秒，让玩家休息一下
            yield return new WaitForSeconds(this.level_interval);

            //增加关卡数 Increase the number of levels
            this.level_index++;

            if (this.level_index >= this_map_enemy_wave_account)
            {
                Normal_game_control.instance.change_to_game_over(true);
                this.level_index = 0;
            }
            else
            {
                //生成兵 Spawn pawn
                StartCoroutine(this.create_a_round_enemy());
            }
            yield return null;
        }
    }
}