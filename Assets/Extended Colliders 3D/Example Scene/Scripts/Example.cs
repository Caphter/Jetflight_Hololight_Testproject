namespace ExtendedColliders3D {
    using UnityEngine;

    public class Example : MonoBehaviour {

        //Properties.
        public GameObject sphere;

        //Update.
        public void Update() {

            //Rotate the scene.
            transform.parent.eulerAngles = new Vector3(transform.parent.eulerAngles.x, transform.parent.eulerAngles.y + (Time.deltaTime * 10),
                    transform.parent.eulerAngles.z);

            //Detect pressing L, C or R to generate a sphere on the left, centre or right respectively.
            if (Input.GetKeyDown(KeyCode.L)) {
                GameObject newSphere = Instantiate(sphere);
                newSphere.transform.position = new Vector3(-11.09958f, 16, 3.5f);
            }
            else if (Input.GetKeyDown(KeyCode.C)) {
                GameObject newSphere = Instantiate(sphere);
                newSphere.transform.position = new Vector3(Random.Range(-0.01f, 0.01f), 16, Random.Range(-0.01f, 0.01f));
            }
            else if (Input.GetKeyDown(KeyCode.R)) {
                GameObject newSphere = Instantiate(sphere);
                newSphere.transform.position = new Vector3(11.09958f, 16, -3.5f);
            }
        }
    }
}