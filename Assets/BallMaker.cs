using System.Collections.Generic;
using UnityEngine;

public class BallMaker : MonoBehaviour
{
    [SerializeField] GameObject ball;

    List<GameObject> created = new List<GameObject>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 1000))
            {
                if (hit.collider.gameObject.tag == "Plane")
                {
                    GameObject newBall = Instantiate(ball, hit.point, Quaternion.identity);
                    created.Add(newBall);
                }
            }
        }

        if (Input.GetMouseButton(1))
        {
            created.ForEach((ball) => {
                Destroy(ball);
            });
            created.Clear();
        }
    }
}
