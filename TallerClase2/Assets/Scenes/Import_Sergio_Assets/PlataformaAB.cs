using UnityEngine;

public class PlataformaAB : MonoBehaviour
{
    public Transform puntoA;
    public Transform puntoB;
    public float speed = 2f;

    private Vector3 target;
    private Vector3 ultimaPos;
    public Vector3 delta {  get; private set; } // Movimiento frame a frame

    void Start()
    {
        target = puntoB.position;
        ultimaPos = transform.position;
    }

    void Update()
    {
        // Mover entre A y B
        transform.position = Vector3.MoveTowards(transform.position, target, speed);

        // Guardar cuanto se movio en este frame
        delta = transform.position - ultimaPos;
        ultimaPos = transform.position;

        // Cambiar destino al llegar
        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            target = (target == puntoA.position) ? puntoB.position : puntoA.position;
        }
    }
}
