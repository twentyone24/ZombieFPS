using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace epoching.fps
{
    public class Rotate_self_horizontal : MonoBehaviour
    {
        [Header("editor旋转速度 editor rotation speed")]
        public float editor_rotate_speed = 1;

        [Header("mobile旋转速度 mobile rotation speed")]
        public float mobile_rotate_speed = 1;

        //水平方向的旋转 Horizontal rotation
        private float rotate_offset;

        void LateUpdate()
        {

            if (Normal_game_control.instance.game_statu != Game_statu.gaming)
                return;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (Input.touchCount > 0)
                {
                    foreach (Touch tou in Input.touches)
                    {
                        //找出一根没在UI上的手指 Find a finger that is not on the UI
                        if (!EventSystem.current.IsPointerOverGameObject(tou.fingerId) && tou.phase == TouchPhase.Moved)
                        {
                            this.rotate_offset = tou.deltaPosition.x;
                            this.transform.Rotate(Vector3.up, this.rotate_offset * this.mobile_rotate_speed * Time.deltaTime);
                            break;
                        }
                    }
                }
            }
            else
            {
                this.rotate_offset = Input.GetAxis("Mouse X");
                this.transform.Rotate(Vector3.up, this.rotate_offset * this.editor_rotate_speed * Time.deltaTime);
            }
        }
    }
}
