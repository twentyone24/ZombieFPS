using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace epoching.fps
{
    public class Powder_keg_control : MonoBehaviour
    {
        [Header("爆炸的粒子特效 Explosive particle effects")]
        public ParticleSystem particle_boom;

        [Header("正常时候的粒子特效 Particle effects at normal times")]
        public GameObject particle_normal;

        [Header("球体碰撞器 Sphere collider")]
        public SphereCollider sphere_collider;

        [Header("油桶模型 Oil barrel model")]
        public GameObject powder_keg_model;

        [Header("碰撞体的半径 The radius of the collision body")]
        public float m_radius = 18f;

        [Header("health")]
        public int health;

        [Header("small_fire_effect")]
        public GameObject game_obj_small_fire_effect;

        //是否正处于爆炸状态 Is it exploding
        private bool is_booming = false;

        //外部调用的爆炸函数 Explosion function called externally
        private void boom()
        {
            if (this.is_booming == true)
                return;

            //修改状态为真正爆炸 Modify the status to real explosion
            this.is_booming = true;

            this.transform.rotation=Quaternion.Euler(-90, 0, 0);

            //碰撞器变大,一段时间后移除掉 The collider gets bigger and will be removed after a while
            this.sphere_collider.radius = m_radius;
            Destroy(this.sphere_collider, 1f);

            //隐藏正常时候的粒子特效 Hide normal particle effects
            this.particle_normal.SetActive(false);

            //播放爆炸粒子特效 Play explosive particle effects
            this.particle_boom.transform.SetParent(null);
            this.particle_boom.gameObject.SetActive(true);
            Destroy(this.particle_boom.gameObject, 2f);

            //隐藏油桶模型 Hidden oil barrel model
            this.powder_keg_model.SetActive(false);

            //destory self
            Destroy(this.gameObject, 5f);
        }

        //be hit
        public void be_hit()
        {
            this.game_obj_small_fire_effect.transform.localScale = new Vector3(this.game_obj_small_fire_effect.transform.localScale.x * 1.15f,
                this.game_obj_small_fire_effect.transform.localScale.y*1.15f, this.game_obj_small_fire_effect.transform.localScale.z * 1.15f);
            this.health--;
            if (this.health <= 0)
            {
                this.boom();
            }
        }

        //碰撞检测 Impact checking
        void OnCollisionEnter(Collision collider)
        {
            //正处于爆炸中 Is exploding
            if (this.is_booming)
            {
                //击中敌人 Hit the enemy
                if (collider.gameObject.tag == "enemy")
                {
                    GameObject enemy = collider.gameObject;

                    //减血 Blood loss
                    enemy.GetComponent<Enemy_control>().be_hit(10,this.transform.position);
                }

                if (collider.gameObject.tag == "Player")
                {
                    GameObject game_role = collider.gameObject;

                    //减血 Blood loss
                    //game_role.GetComponent<Game_role_control>().be_hit(20);

                    //Blast to the sky
                    //game_role.GetComponent<Rigidbody>().isKinematic = false;
                    //game_role.GetComponent<Rigidbody>().AddForce(Vector3.up * 600f);
                }
            }
        }
    }
}