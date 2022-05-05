using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace epoching.fps
{
    public class Powder_keg_manager : MonoBehaviour
    {
        //单例模式 Singleton mode
        public static Powder_keg_manager instance;

        [Header("火药桶预设 Gunpowder barrel presets")]
        public GameObject prefab_powder_keg;

        [Header("默认生成位置 Default generation location")]
        public Transform[] init_positins;

        void Awake()
        {
            Powder_keg_manager.instance = this;
        }

        //清除炮塔 Clear the turret
        public void clear_all_powder_kegs()
        {
            GameObject[] powder_keg_array = GameObject.FindGameObjectsWithTag("powder_keg");
            for (int i = 0; i < powder_keg_array.Length; i++)
            {
                Destroy(powder_keg_array[i].gameObject);
            }
        }

        //初始化生成5个固定位置的火药桶 Initially generate 5 fixed position gunpowder barrels
        public void init_create_powder_kegs()
        {
            for (int i = 0; i < this.init_positins.Length; i++)
            {
                //生成炮塔 Spawn turret
                GameObject powder_keg = Instantiate(this.prefab_powder_keg, this.init_positins[i].position, this.init_positins[i].rotation) as GameObject;

                //设置它的父物体为 tower manager 设置它的父物体为 tower manager
                powder_keg.transform.parent = this.transform;
            }
        }
    }
}
