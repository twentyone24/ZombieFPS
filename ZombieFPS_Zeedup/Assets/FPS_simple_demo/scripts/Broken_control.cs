using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Broken_control : MonoBehaviour
{
    bool is_animation_over = false;

    public Vector3 move_direction;

    void Start()
    {
        StartCoroutine(this.animation_over());
    }

    // Update is called once per frame
    void Update()
    {
        if (is_animation_over)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(this.transform.position.x, this.transform.position.y - 0.2f, this.transform.position.z), Time.deltaTime * 2f);

            if (this.transform.position.y < -2f)
            {
                Destroy(this.gameObject);
            }
        }
        else
        {
            this.transform.position -= move_direction * Time.deltaTime * 25;
        }
    }


    private IEnumerator animation_over()
    {

        yield return new WaitForSeconds(0.2f);

        Destroy(this.GetComponent<Rigidbody>());

        is_animation_over = true;

    }

    
}
