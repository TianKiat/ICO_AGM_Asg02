using UnityEngine;
using System.Collections;
 
public class HomingMissile : MonoBehaviour {
    //properties
    private Transform target;
    [SerializeField]
    private float speed = 5;//speed
    [SerializeField]
    private float autoDetonateTime = 2;
    [SerializeField]
    private float homingSensitivity = .1f;//sensitivity
    [SerializeField]
    private GameObject explosion;//explosion prefab
    void Start()
    {
        StartCoroutine(AutoDetonate());//start the auto detonate sequence
    }
    void Update()
    {
        if (target != null)
        {
            Vector3 relativePos = target.position - transform.position;// calculate the relative position to target
            Quaternion rotation = Quaternion.LookRotation(relativePos);// rotate to target
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, homingSensitivity);//slerp to target
        }
        transform.Translate(0,0,speed * Time.deltaTime,Space.Self);//move toward target
    }
    private void ExplodeSelf() 
    {
        Instantiate(explosion,transform.position,Quaternion.identity);
        Destroy(gameObject);
    }

    private IEnumerator AutoDetonate() 
    {
        yield return new WaitForSeconds(autoDetonateTime);
        ExplodeSelf();
    }
    void OnTriggerEnter(Collider other) 
    {
        //destroy self
        ExplodeSelf();
    }
}