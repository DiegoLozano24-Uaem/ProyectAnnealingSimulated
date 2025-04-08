// Asumiendo que también tienes una clase que gestiona la UI
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColocarPeones : MonoBehaviour
{
    public GameObject peonBlancoPrefab;
    public GameObject peonNegroPrefab;
    public CrearTablero tablero;
    public OptimizedSimulatedAnnealing recocidoSimulado;

    public Button botonSiguiente;
    private int indiceSolucionActual = 0;
    private List<int[,]> soluciones;
    private List<GameObject> peonesInstanciados = new List<GameObject>();

    [System.Obsolete] //FindObjectOfType es obsoleto
    void Start()
    {
        tablero = FindObjectOfType<CrearTablero>();
        recocidoSimulado = FindObjectOfType<OptimizedSimulatedAnnealing>();

        if (tablero == null || recocidoSimulado == null)
        {
            Debug.LogError("Faltan referencias al tablero o al algoritmo de recocido.");
            return;
        }

        soluciones = recocidoSimulado.GetMejoresSoluciones();

        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.AddListener(MostrarSiguienteSolucion);
        }

        MostrarSolucion(indiceSolucionActual);
    }

    void MostrarSolucion(int indice)
    {
        LimpiarPeones();

        int[,] solucion = soluciones[indice];
        Vector3[,] positions = tablero.positionCasillas;
        int filas = tablero.filas;
        int columnas = tablero.columnas;


        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                Vector3 offset = new Vector3(0, 0.5f, 0);
                if (solucion[i, j] == 1) // Peon blanco
                {
                    GameObject peon = Instantiate(peonBlancoPrefab, positions[i, j] + offset, Quaternion.identity, transform);
                    peonesInstanciados.Add(peon);
                }
                else if (solucion[i, j] == 2) // Peon negro
                {
                    GameObject peon = Instantiate(peonNegroPrefab, positions[i, j] + offset, Quaternion.identity, transform);
                    peonesInstanciados.Add(peon);
                }
            }
        }
    }

    void MostrarSiguienteSolucion()
    {
        indiceSolucionActual = (indiceSolucionActual + 1) % soluciones.Count;
        MostrarSolucion(indiceSolucionActual);
    }

    void LimpiarPeones()
    {
        foreach (GameObject peon in peonesInstanciados)
        {
            Destroy(peon);
        }
        peonesInstanciados.Clear();
    }
}
