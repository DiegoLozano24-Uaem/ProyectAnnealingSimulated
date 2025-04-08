using UnityEngine;

public class CrearTablero : MonoBehaviour
{
   
    public GameObject casillaBlancaPrefab;
    public GameObject casillaNegraPrefab;
    public int filas = 8;
    public int columnas = 8;
    public float tamanoCasilla = 1.0f;

    public Vector3[,] positionCasillas;

    [System.Obsolete] //FindObjectOfType es obsoleto
    void Start()
{
    var recocido = FindObjectOfType<OptimizedSimulatedAnnealing>();
    if (recocido != null)
    {
        filas = recocido.filas;
        columnas = recocido.columnas;
    }

    GenerarTablero();
}

    void GenerarTablero()
    {
        positionCasillas = new Vector3[filas, columnas];

        // Calculamos el desplazamiento para centrar el tablero
            Vector3 offset = new Vector3((columnas - 1) * tamanoCasilla / 2, 0, (filas - 1) * tamanoCasilla / 2);

        for (int i = 0; i < filas; i++)
        {
            
            for (int j = 0; j < columnas; j++)
            {
                GameObject casillaPrefab = (i + j) % 2 == 0 ? casillaBlancaPrefab : casillaNegraPrefab;
                //La i es para que se acomoden correctamente las filas
                Vector3 position = new Vector3(j * tamanoCasilla,  i * tamanoCasilla, 0) - offset;

                positionCasillas[i, j] = position;
                
                Instantiate(casillaPrefab, position, Quaternion.identity, transform);
            }
        }
    }

}
