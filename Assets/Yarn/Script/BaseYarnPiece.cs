using UnityEngine;

public class BaseYarnPiece : MonoBehaviour
{
    [SerializeField] private Transform node;
    [SerializeField] private Rigidbody2D nodeRigidbody;

    [SerializeField] private Vector2 velocity;

    private int step;

    void Update()
    {
        if (step < 60)
        {
            if (step % 2 == 0)
            {
                if (node.transform.position.x < 4)
                {
                    nodeRigidbody.linearVelocity = new Vector3(velocity.x, 0, 0);
                }
                else
                {
                    nodeRigidbody.linearVelocity = new Vector3(0, velocity.y, 0);

                    step++;
                }
            }
            else
            {
                if (node.transform.position.x > -4)
                {
                    nodeRigidbody.linearVelocity = new Vector3(-velocity.x, 0, 0);
                }
                else
                {
                    nodeRigidbody.linearVelocity = new Vector3(0, velocity.y, 0);

                    step++;
                }
            }
        }
    }
}
