using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pdftotext
{
    public class ProcesosModelosLaboral
    {
        // Instancia de la clase 'procesosPDF' para acceder a sus métodos y propiedades
        procesosPDF procesosPDF = new procesosPDF();

        public Dictionary<string, string[]> PatronesBusquedaLaboral = new Dictionary<string, string[]>
        {
            {
                "AFIA", new[]{
                "RESOLUCIÓN SOBRE RECONOCIMIENTO DE ALTA(.+)", //Documento de afiliacion de alta
                "INFORME DE SITUACIÓN DE ALTA(.+)" //Documento de afiliacion de alta (version de junio-2025)//
                }
            },
            {
                "AFIB", new []{
                "RESOLUCIÓN SOBRE RECONOCIMIENTO DE BAJA(.+)"//Documento de afiliacion de baja}
                }
            },
            {
                "AFIC", new []{
                @"COMUNICACIÓN SOBRE\s+(\w+\s+)*\nCONTRATO DE TRABAJO\s+([\w+\s]+)$*"//Documento de afiliacion de variacion. Busca el texto seguido de uno o varios espacios, seguido de una o varias palabras separadas por espacios, seguido de un salto de linea, seguido del texto, seguido de uno o varios espacios, seguido de una o varias palabras que estan al final de la linea
                }
            },
            {
                "AFIV", new []{
                "" //Documento de afiliacion de variacion (no hay documento de ejemplo)
                }
            },
            {
                "CER", new []{
                "CERTIFICADO DE ESTAR AL CORRIENTE"
                }
            },
            {
                "RNT", new []{
                "[A-Z]RNT\\d{10}\\b" //Busca una letra mayuscula, seguida de RNT y seguida de 10 digitos que terminan la palabra.
                }
            },
            {
                "RLC", new []{
                @"[A-Z]RLC\d{10}\b" //Busca una letra mayuscula, seguida de RLC y seguida de 10 digitos que terminan la palabra.
                }
            },
            {
                "ITA", new []{
                @"INFORME DE TRABAJADORES EN ALTA A FECHA: \d{2}.\d{2}.\d{4}" //Busca el texto seguido dos digitos, seguido de un caracter, seguido de dos digitos, seguido de un caracter y seguido de 4 caracteres.
                }
            },
            {
                "IDC", new []{
                "RESOLUCIÓN SOBRE RECONOCIMIENTO DE BAJA(.+)"//Documento de afiliacion de baja}
                }
            },
            {
                "HUE", new []{
                @"COMUNICACIÓN[\s\S]*DEL CONTRATO DE[\s\S]*?\n(.+)" //Busca el texto seguido cualquier espacio o caracter (\S es lo contrario de un espacio), seguido de un salto de linea, seguido de cualquier caracter que no sea un salto de linea.
                }
            }
        };
        public void ProcesarAFIA()
        {
            if(!PatronesBusquedaLaboral.TryGetValue("AFIA", out string[] patrones))
            {
                return; // Si no se encuentra el modelo AFIA, salir del método
            }

            //Se usa el primer patron de AFIA que se encuentre en el texto
            string textoAFIA = "";
            string patronEncontrado = null;

            foreach(var patron in patrones)
            {
                textoAFIA = procesosPDF.ProcesaPatron(patron, 1);
                if(!string.IsNullOrEmpty(textoAFIA))
                {
                    patronEncontrado = patron; //Guardamos el patron que sirvio para encontrar el texto
                    break; // Sale del bucle al encontrar el primer patron
                }
            }

            if(!string.IsNullOrEmpty(textoAFIA))
            {
                if(textoAFIA.Contains(":"))
                {
                    string[] partes = textoAFIA.Split(new[] { ':' });
                    busquedaLaboral.Observaciones1 = partes[0].Trim();
                    busquedaLaboral.Observaciones2 = partes[1].Trim();
                }
                else if(patronEncontrado != null)
                {
                    int longitudBase = patronEncontrado.Length - 4; // Se resta 4 para quitar los caracteres finales del patron
                    if(longitudBase < 0)
                    {
                        longitudBase = 0; // Asegura que no se produzca un error si el patron es demasiado corto
                    }

                    busquedaLaboral.Observaciones1 = patronEncontrado.Substring(0, longitudBase);
                    busquedaLaboral.Observaciones2 = textoAFIA.Length > longitudBase
                        ? textoAFIA.Substring(longitudBase).Trim()
                        : "";
                }

                busquedaLaboral.Observaciones1 = quitaRaros(busquedaLaboral.Observaciones1);
                busquedaLaboral.Observaciones2 = quitaRaros(busquedaLaboral.Observaciones2);

                busquedaLaboral.CampoLibre1 = quitaRaros(textoAFIA);
                busquedaLaboral.Modelo = "AFIA";
            }
        }

        public string quitaRaros(string texto)
        {
            string cadena = texto.ToUpper();
            cadena = cadena.Replace('Á', 'A')
                  .Replace('É', 'E')
                  .Replace('Í', 'I')
                  .Replace('Ó', 'O')
                  .Replace('Ú', 'U');
            return cadena;
        }

    }
}
