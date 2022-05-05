using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using LitJson;

namespace epoching.fps
{
    public class Enemy_control : MonoBehaviour
    {
        [Header("enemy type")]
        public Enemy_type enemy_type;

        [Header("enemy statu")]
        public Enemy_statu enemy_statu;

        [Header("audio clip")]
        public AudioClip audio_clip_born;
        public AudioClip audio_clip_attack;

        [Header("目标点范围 Target point range")]
        public float target_range;

        [Header("破碎动画预制体 Broken animation prefab")]
        public GameObject prefab_broken_animation;

        [Header("自动寻路控制器 Automatic path finding controller")]
        public NavMeshAgent nav;

        //health
        private int health;

        //animator
        private Animator animator;

        //audio source
        private AudioSource audio_source;

        //是否处于水晶的周围 Is it around the crystal
        private bool is_around_the_player = false;

        //target
        //private Vector3 v3_target;

        //控制水晶的脚本 Script to control crystal
        private GameObject game_obj_player;

        // Start is called before the first frame update
        void Start()
        {
            //find animator
            this.animator = this.GetComponent<Animator>();

            //find audio source
            this.audio_source = this.GetComponent<AudioSource>();

            //找到控制水晶的脚本 Find the script that controls the crystal
            this.game_obj_player = GameObject.FindGameObjectWithTag("Player");

            //set health
            switch (this.enemy_type)
            {
                case Enemy_type.ax:
                    this.health = 7;
                    break;
                case Enemy_type.hammer:
                    this.health = 5;
                    break;
                case Enemy_type.machete:
                    this.health = 3;
                    break;
                default:
                    break;
            }


            //播放enemy出生时候的声音 Play the voice of the enemy when he was born
            this.audio_source.clip = this.audio_clip_born;
            this.audio_source.Play();

            //速度随机一下 Randomize the speed
            //this.nav.speed = this.nav.speed * Random.Range(-0.2f, 0.2f) + this.nav.speed;
            this.change_to_running();
        }

        void Update()
        {

            if (Input.GetKeyDown(KeyCode.Space))
            {
                this.nav.isStopped = false;
            }

            //return;

            if (Normal_game_control.instance.game_statu != Game_statu.gaming)
                return;

            if (this.is_around_the_player == false)
            {
                if (this.enemy_statu == Enemy_statu.running)
                {
                    if (Vector3.Distance(this.transform.position, this.game_obj_player.transform.position) < this.target_range)
                    {
                        this.change_to_attack();
                        this.is_around_the_player = true;
                    }

                    this.nav.SetDestination(this.game_obj_player.transform.position);
                }
            }
            else
            {
                if (this.enemy_statu == Enemy_statu.attack)
                {
                    if (Vector3.Distance(this.transform.position, this.game_obj_player.transform.position) >= this.target_range)
                    {
                        this.change_to_running();
                        this.is_around_the_player = false;
                    }

                    this.transform.LookAt(new Vector3(this.game_obj_player.transform.position.x, 0, this.game_obj_player.transform.position.z));
                }
            }

        
        }

        //set speed
        public void set_speed(float speed)
        {
            this.nav.speed = speed;
        }

        //switch statu
        #region 
        //change_to_running
        public void change_to_running()
        {
            //if (this.enemy_statu == Enemy_statu.running)
            //    return;

            this.animator.SetBool("attack", false);



            //this.nav.SetDestination(GameObject.FindGameObjectWithTag("crystal").transform.position);

            //继续寻路 Keep on wayfinding
            this.nav.isStopped = false;

            //修改状态 Modify status
            this.enemy_statu = Enemy_statu.running;


        }

        //change_to_attack
        public void change_to_attack()
        {
            if (this.enemy_statu == Enemy_statu.attack)
                return;

            //停止寻路  Stop wayfinding
            this.nav.isStopped = true;

            //正面朝向水晶 Face the crystal
            this.transform.LookAt(this.game_obj_player.transform.position);

            //播放被打的动画 Play the beaten animation
            this.animator.SetBool("attack", true);

            //修改状态 Modify status
            this.enemy_statu = Enemy_statu.attack;
        }

        //change_to_dead
        public void change_to_dead()
        {
            if (this.enemy_statu == Enemy_statu.dead)
                return;

            //停止寻路 Stop wayfinding
            this.nav.isStopped = true;

            //这一关还存多少兵，减一 How many soldiers are left in this level, minus one
            Enemy_manager.instance.this_level_still_alive--;

            //删除这个物体 Delete this object
            Destroy(this.gameObject);
        }
        #endregion

        //碰撞检测 Impact checking
        //void OnTriggerEnter(Collider collider)
        //{
        //    if (this.enemy_statu != Enemy_statu.attack && this.enemy_statu != Enemy_statu.dead)
        //    {
        //        if (collider.gameObject.tag == "Player")
        //        {
        //            this.change_to_attack();
        //            this.is_around_the_player = true;
        //        }
        //    }
        //}

        //被外部攻击时，调用的函数,hurt被伤害值 When being attacked by the outside, the function called, hurt the damage value
        public void be_hit(int hurt, Vector3 position)
        {
            this.health -= hurt;


            Vector3 v3 = this.transform.position - position;
            this.transform.position +=new Vector3(v3.normalized.x,0, v3.normalized.z)*1.5f;

            if (this.health <= 0)
            {
                if (this.enemy_statu != Enemy_statu.dead)
                {
                    //生成破碎动画 Generate broken animation
                    GameObject game_obj_broken = Instantiate(this.prefab_broken_animation);
                    game_obj_broken.transform.position = new Vector3(this.transform.position.x, game_obj_broken.transform.position.y, this.transform.position.z);
                    game_obj_broken.transform.LookAt(new Vector3(position.x, 0, position.z));
                    game_obj_broken.GetComponent<Broken_control>().move_direction = (new Vector3(position.x, 0, position.z) - new Vector3(this.transform.position.x, 0, this.transform.position.z)).normalized;


                    this.change_to_dead();
                }
            }
        }

        //外部调用的游戏结束状态 Game over state called externally
        public void game_fail_over()
        {
            //不再寻路，不再攻击人 No more path finding, no more attacking people
            if (this.enemy_statu != Enemy_statu.dead)
            {
                if (this.nav)
                    this.nav.isStopped = true;

                //播放游戏结束idle动画 Play the game end idle animation
                this.animator.SetBool("game_over", true);
            }
        }

        //动画事件 Animation event
        #region 
        //攻击开始的时候，播放攻击的声音 When the attack starts, play the attack sound
        public void attack_start()
        {
            if (Normal_game_control.instance.game_statu != Game_statu.gaming)
                return;

            this.audio_source.clip = this.audio_clip_attack;
            this.audio_source.Play();

            if (Vector3.Distance(this.transform.position, this.game_obj_player.transform.position) < this.target_range)
            {
                this.game_obj_player.GetComponent<Game_role_control>().be_hit(1);
            }
        }

        #endregion
    }
}
