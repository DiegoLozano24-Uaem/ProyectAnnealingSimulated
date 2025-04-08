using System;
using System.Collections.Generic;
using System.Text;
using Debug = UnityEngine.Debug; // Esto evita el conlicto de nombres con el Debug 
using System.Diagnostics;
using UnityEngine;
using System.Linq;

public class OptimizedSimulatedAnnealing : MonoBehaviour
{
    public int columnas = 4, filas = 4;
    public float temperaturaInicial = 100.0f;
    public float factorEnfriamiento = 0.9f; // Más gradual que 0.95
    public int iteracionesPorTemperatura = 500; // Más iteraciones por temperatura
    
    private List<int[,]> mejoresSoluciones = new List<int[,]>();
    private int mejorEnergiaGlobal = int.MinValue;

    void Start()
    {
        int[,] tableroInicial = GenerarMatrizAleatoria();
        ImprimirMatriz(tableroInicial, "Matriz Inicial");
        
        RecocidoSimuladoOptimizado(tableroInicial);

        MostrarResultados();
    }

    int[,] GenerarMatrizAleatoria()
    {
        int[,] tablero = new int[filas, columnas];
        System.Random rand = new System.Random();
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                tablero[i, j] = rand.Next(0, 3); //0, 1, 2
            }
        }
        return tablero;
    }

    void RecocidoSimuladoOptimizado(int[,] tableroInicial)
    {
        int[,] mejorSolucion = (int[,])tableroInicial.Clone();
        int mejorEnergia = EvaluarEnergiaOptimizada(mejorSolucion);
        mejorEnergiaGlobal = mejorEnergia;
        
        float temperatura = temperaturaInicial;
        System.Random rand = new System.Random();
        Stopwatch reloj = new Stopwatch();
        reloj.Start();

        int iteracionesSinMejora = 0;
        const int maxIteracionesSinMejora = 1000;

        while (temperatura > 0.01f)
        {
            bool mejoraEnEstaTemperatura = false;

            for (int iter = 0; iter < iteracionesPorTemperatura; iter++)
            {
                int[,] nuevoTablero = GenerarVecinoInteligente(mejorSolucion, rand);
                int nuevaEnergia = EvaluarEnergiaOptimizada(nuevoTablero);
                int delta = nuevaEnergia - mejorEnergia;

                if (delta >= 0 || Math.Exp(delta / temperatura) > rand.NextDouble()) 
                {
                    mejorSolucion = (int[,])nuevoTablero.Clone();
                    mejorEnergia = nuevaEnergia;

                    if (nuevaEnergia > mejorEnergiaGlobal)
                    {
                        mejorEnergiaGlobal = nuevaEnergia;
                        mejoresSoluciones.Clear();
                        mejoresSoluciones.Add((int[,])nuevoTablero.Clone());
                        iteracionesSinMejora = 0;
                        mejoraEnEstaTemperatura = true;
                    }
                    else if (nuevaEnergia == mejorEnergiaGlobal)
                    {
                        string hash = MatrizAHash(nuevoTablero);
                        if (!mejoresSoluciones.Any(s => MatrizAHash(s) == hash))
                        {
                            mejoresSoluciones.Add((int[,])nuevoTablero.Clone());
                        }
                    }
                }

                // Reinicio aleatorio si estancado
                if (iteracionesSinMejora++ > maxIteracionesSinMejora)
                {
                    mejorSolucion = GenerarMatrizAleatoria();
                    mejorEnergia = EvaluarEnergiaOptimizada(mejorSolucion);
                    iteracionesSinMejora = 0;
                    temperatura = temperaturaInicial * 0.5f; // Reiniciar con temperatura media
                    break;
                }

                if (reloj.Elapsed.TotalMinutes >= 5) //Modificar la cantidad de minutos por lo que quiera que dure la búsqueda
                {
                    Debug.Log("Tiempo máximo de ejecución alcanzado (5 minutos)"); // También aquí.
                    return;
                }
            }

            if (!mejoraEnEstaTemperatura)
            {
                // Enfriamiento más lento si no hay mejora
                temperatura *= (float)Math.Pow(factorEnfriamiento, 0.5);
            }
            else
            {
                temperatura *= factorEnfriamiento;
            }
        }
    }

    int[,] GenerarVecinoInteligente(int[,] tablero, System.Random rand)
    {
        int[,] nuevoTablero = (int[,])tablero.Clone();
        
        // 70% de probabilidad de modificar columna problemática
        if (rand.NextDouble() < 0.7)
        {
            int col = ObtenerColumnaConPeorEnergia(tablero);
            int fila = rand.Next(0, filas);
            nuevoTablero[fila, col] = rand.Next(0, 3);
        }
        else
        {
            // 30% de probabilidad de cambio completamente aleatorio
            int fila = rand.Next(0, filas);
            int col = rand.Next(0, columnas);
            nuevoTablero[fila, col] = rand.Next(0, 3);
        }

        return nuevoTablero;
    }

    int ObtenerColumnaConPeorEnergia(int[,] tablero)
    {
        int peorColumna = 0;
        int peorEnergia = int.MaxValue;
        
        for (int j = 0; j < columnas; j++)
        {
            int energiaCol = CalcularEnergiaColumna(tablero, j);
            if (energiaCol < peorEnergia)
            {
                peorEnergia = energiaCol;
                peorColumna = j;
            }
        }
        return peorColumna;
    }

    int CalcularEnergiaColumna(int[,] tablero, int col)
    {
        int energia = 0;
        int count1 = 0, count2 = 0, count0 = 0;

        for (int i = 0; i < filas; i++)
        {
            if (tablero[i, col] == 1) count1++; //Contamos la cantidad de 1's en el tablero
            if (tablero[i, col] == 2) count2++; //Contamos la cantidad de 2's en el tablero
            if (tablero[i, col] == 0) count0++; //Contamos la cantidad de 0's en el tablero
        }

        if (count1 > 1) energia -= (count1 - 1) * 13;
        if (count2 > 1) energia -= (count2 - 1) * 13;
        if (count0 == filas) energia -= 13; //Penaliza el tablero llenos de 0's

        if (tablero[0, col] == 2)
        {
            bool soloCeros = true;
            for (int i = 1; i < filas; i++)
            {
                if (tablero[i, col] != 0)
                {
                    soloCeros = false;
                    break;
                }
            }
            if (soloCeros) energia -= 13;
        }

        if (tablero[filas - 1, col] == 1)
        {
            bool soloCeros = true;
            for (int i = 0; i < filas - 1; i++)
            {
                if (tablero[i, col] != 0)
                {
                    soloCeros = false;
                    break;
                }
            }
            if (soloCeros) energia -= 13;
        }

        int cerosDebajo1 = 0;
        int cerosEncima2 = 0;
        for (int i = 0; i < filas; i++)
        {
            if (tablero[i, col] == 1)
            {
                for (int k = i + 1; k < filas; k++)
                {
                    if (tablero[k, col] == 0) cerosDebajo1++;
                }
            }
            if (tablero[i, col] == 2)
            {
                for (int k = 0; k < i; k++)
                {
                    if (tablero[k, col] == 0) cerosEncima2++;
                }
            }
        }
        if (cerosDebajo1 != cerosEncima2) energia -= 13;

        bool encontrado1 = false;
        for (int i = 0; i < filas; i++)
        {
            if (tablero[i, col] == 1)
            {
                encontrado1 = true;
            }
            if (tablero[i, col] == 2 && encontrado1)
            {
                energia -= 13;
                break;
            }
        }

        return energia;
    }

    int EvaluarEnergiaOptimizada(int[,] tablero)
    {
        int energiaTotal = 0;

        // Evaluar cada columna
        for (int j = 0; j < columnas; j++)
        {
            energiaTotal += CalcularEnergiaColumna(tablero, j);
        }

        // Evaluar patrones entre filas
        for (int i = 0; i < filas - 2; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                if (tablero[i, j] == 2 && tablero[i + 1, j] == 0 && tablero[i + 2, j] == 1)
                {
                    energiaTotal += 1;
                }
            }
        }

        return energiaTotal;
    }

    string MatrizAHash(int[,] matriz) //Esta función convierte el tablero en una cadena de texto única para evitar duplicados
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                sb.Append(matriz[i, j]);
            }
        }
        return sb.ToString();
    }

    void MostrarResultados()
    {
        Debug.Log($"Mejor energía encontrada: {mejorEnergiaGlobal}");
        Debug.Log($"Número de soluciones óptimas encontradas: {mejoresSoluciones.Count}");

        foreach (var solucion in mejoresSoluciones)
        {
            ImprimirMatriz(solucion, "Solución Óptima");
        }
    }

    void ImprimirMatriz(int[,] tablero, string titulo)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{titulo} (Energía: {EvaluarEnergiaOptimizada(tablero)}):");
        
        for (int i = 0; i < filas; i++)
        {
            for (int j = 0; j < columnas; j++)
            {
                sb.Append(tablero[i, j] + " ");
            }
            sb.AppendLine();
        }
        
        Debug.Log(sb.ToString());
    }

    public List<int[,]> GetMejoresSoluciones()
    {
    return mejoresSoluciones;
    }
}
