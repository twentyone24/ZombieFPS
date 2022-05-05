using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// 实现角色的前后左右移动，以及射击
/// </summary>

namespace epoching.fps
{
    public class Game_role_control : MonoBehaviour
    {

        public static Game_role_control instance;

        //-----move------------------------------------
        #region 
        [Header("移动速度 Moving speed")]
        public float speed_forward;
        public float speed_back;
        public float speed_left_right;

        [Header("character controller")]
        public CharacterController character_controller;

        [Header("character animator")]
        public Animator role_animator;

        [Header("audio_source_foot_step")]
        public AudioSource audio_source_foot_step;

        //垂直玩家输入偏移量 Vertical player input offset
        private float vertical_offset;

        //水平玩家输入偏移量 Horizontal player input offset
        private float horizontal_offset;

        //移动方向 Moving direction
        private Vector3 moveDir;

        //is moving
        private bool is_moving = false;
        #endregion

        //-----fire---------------------------------------------------
        #region 
        [Header("muzzle_flash_light")]
        public Light muzzle_flash_light;

        [Header("muzzle_particles")]
        public ParticleSystem particle_muzzle;
        public ParticleSystem particle_spark;

        [Header("audio_source_fire")]
        public AudioSource audio_source_fire;

        [Header("弹壳预制体，不断生成就行了---弹壳产生的位置")]
        public GameObject prefab_casing;
        public Transform transform_casing_spawn;

        [Header("transform_main_camera")]
        public Transform transform_main_camera;

        [Header("impact effect")]
        public GameObject prefab_impact_bullet_sand;
        public GameObject prefab_impact_bullet_metal;
        public GameObject prefab_impact_bullet_stone;
        public GameObject prefab_impact_bullet_wood;
        public GameObject prefab_impact_bullet_wood_no_decal;

        [Header("开火状态，时间戳 Fire status, time stamp")]
        public bool is_firing;
        private float time_stamp = 0;

        [Header("一直按住开火按钮的时候的间隔时间 The interval time when the fire button is kept pressed")]
        public float fire_interval = 0.1f;
        //灯光闪烁的协程
        private Coroutine coroutine_light_flash;
        #endregion

        //----------------------------
        [Header("falsh blood animator")]
        public Animator animator_flash_blood;

        [Header("health")]
        public int health;

        //init transform
        private Vector3 init_position;
        private Quaternion init_rotation;


        //事件派发器
        public event Handler player_be_killed_event;

        void Awake()
        {
            Game_role_control.instance = this;
        }

        void Start()
        {
            this.init_position = this.transform.position;
            this.init_rotation = this.transform.rotation;
        }

        void Update()
        {
            if (Normal_game_control.instance.game_statu != Game_statu.gaming)
                return;

            //开火 fire
            #region
            if (this.is_firing && Time.time > this.time_stamp)
            {
                //隐藏鼠标鼠标 Hide mouse
                //Cursor.visible = false;

                //发射子弹间隔 Firing interval
                this.time_stamp = Time.time + this.fire_interval;

                //play animation
                this.role_animator.Play("fire", 0, 0f);

                //play sound
                this.audio_source_fire.Play();

                //Spawn casing prefab at spawnpoint
                Instantiate(this.prefab_casing, this.transform_casing_spawn.position, this.transform_casing_spawn.rotation);

                //fire_a_raycast_bullet
                this.fire_a_raycast_bullet();

            }
            #endregion

            //监听鼠标按键 Monitor mouse buttons
            if (Input.GetMouseButtonDown(0))
            {
                this.is_firing = true;
                this.time_stamp = Time.time;

                this.coroutine_light_flash = StartCoroutine(this.MuzzleFlashLight());
                this.particle_muzzle.Play();
                this.particle_spark.Play();

            }

            if (Input.GetMouseButtonUp(0))
            {
                this.is_firing = false;

                if (this.coroutine_light_flash != null)
                    StopCoroutine(this.coroutine_light_flash);
                if (this.muzzle_flash_light.enabled == true)
                    this.muzzle_flash_light.enabled = false;
                this.particle_muzzle.Stop();
                this.particle_spark.Stop();
            }
        }

        // Update is called once per frame
        void LateUpdate()
        {
            if (Normal_game_control.instance.game_statu != Game_statu.gaming)
                return;

            //获取输入的偏移量 Get the input offset
            this.vertical_offset = Input.GetAxis("Vertical");
            this.horizontal_offset = Input.GetAxis("Horizontal");


            //移动距离,向前和向后的速度不应该一样 Moving distance, forward and backward speed should not be the same
            this.moveDir = this.transform.right * horizontal_offset * this.speed_left_right * Time.deltaTime + this.transform.forward * this.vertical_offset * this.speed_back * Time.deltaTime;//forward back
            this.character_controller.Move(this.moveDir);


            if (moveDir == new Vector3(0, 0, 0))
            {
                if (this.is_moving == true)
                {
                    this.is_moving = false;
                    this.role_animator.SetBool("is_moving", this.is_moving);

                    //stop foot step sound
                    this.audio_source_foot_step.Stop();
                }
            }
            else
            {
                if (this.is_moving == false)
                {
                    this.is_moving = true;
                    this.role_animator.SetBool("is_moving", this.is_moving);

                    //play foot step sound
                    this.audio_source_foot_step.Play();
                }
            }

            //Debug.Log(this.moveDir);
        }

        void OnDestroy()
        {
            if (this.coroutine_light_flash != null)
                StopCoroutine(this.coroutine_light_flash);
            StopAllCoroutines();
        }

        //be_hit
        public void be_hit(int damage)
        {
            if (Normal_game_control.instance.game_statu == Game_statu.gaming)
            {
                this.health -= damage;
                this.flash_blood();

                if (this.health <= 0)
                {
                    this.player_be_killed_event();

                    //stop gun particle-------
                    if (this.coroutine_light_flash != null)
                        StopCoroutine(this.coroutine_light_flash);
                    if (this.muzzle_flash_light.enabled == true)
                        this.muzzle_flash_light.enabled = false;
                    this.particle_muzzle.Stop();
                    this.particle_spark.Stop();
                }
            }
        }

        //reset crystal
        public void reset()
        {
            //reset health
            this.health = Config.play_health;

            //reset position and rotation
            this.transform.position = this.init_position;
            this.transform.rotation = this.init_rotation;
            this.transform.Find("arms_and_gun").localRotation = Quaternion.Euler(0, 0, 0);

            //this.GetComponent<Rigidbody>().isKinematic = true;

        }

        //flash blood
        private void flash_blood()
        {
            this.animator_flash_blood.Play("flash_blood", 0);
        }

        //发射子弹 发射一个射线撞到啥子就是啥子 Fire a bullet
        public void fire_a_raycast_bullet()
        {

            RaycastHit hit;

            if (Physics.Raycast(this.transform_main_camera.position, this.transform_main_camera.forward, out hit, 200))
            {
                GameObject game_obj_be_hit = hit.transform.gameObject;
                string tag = hit.transform.tag;

                //game_obj_impact
                GameObject game_obj_impact;

                //击中敌人 Hit the enemy
                if (tag == "enemy")
                {
                    game_obj_impact = Instantiate(this.prefab_impact_bullet_wood_no_decal, hit.point, Quaternion.LookRotation(hit.normal));

                    hit.rigidbody.AddForce(-hit.normal * 1000f);
                    hit.transform.gameObject.GetComponent<Enemy_control>().be_hit(1, this.transform.position);
                }

                //击中油桶 Hit the powder keg
                else if (tag == "powder_keg")
                {
                    game_obj_impact = Instantiate(this.prefab_impact_bullet_wood_no_decal, hit.point, Quaternion.LookRotation(hit.normal));

                    hit.rigidbody.AddForce(-hit.normal * 90f);

                    game_obj_be_hit.GetComponent<Powder_keg_control>().be_hit();

                }
                //spawn impact
                else if (tag == "wood")
                {
                    game_obj_impact = Instantiate(this.prefab_impact_bullet_wood, hit.point, Quaternion.LookRotation(hit.normal));
                }

                else if (tag == "crystal")
                {
                    game_obj_impact = Instantiate(this.prefab_impact_bullet_metal, hit.point, Quaternion.LookRotation(hit.normal));
                }

                else if (tag == "stone")
                {
                    game_obj_impact = Instantiate(this.prefab_impact_bullet_stone, hit.point, Quaternion.LookRotation(hit.normal));
                }
                else
                {
                    game_obj_impact = Instantiate(this.prefab_impact_bullet_sand, hit.point, Quaternion.LookRotation(hit.normal));
                }

                Destroy(game_obj_impact, 2f);
            }
        }

        //Show light when shooting, then disable after set amount of time
        private IEnumerator MuzzleFlashLight()
        {
            while (true)
            {
                this.muzzle_flash_light.enabled = true;
                yield return new WaitForSeconds(0.02f);
                this.muzzle_flash_light.enabled = false;
                yield return new WaitForSeconds(0.02f);
            }

        }
    }
}