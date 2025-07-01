using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace pdftotext
{
    public class ProcesosModelosLaboral
    {
        // Instancia de la clase 'procesosPDF' para acceder a sus métodos y propiedades
        procesosPDF procesosPDF = new procesosPDF();

        // Texto que recoge el patron que encuentra en todos los procesos
        string patronEncontrado;

        //Diccionario en el que esta cada modelo que se puede buscar con el patron de busqueda de texto como un array para poder incluir mas de un patron de busqueda de cada uno
        public Dictionary<string, string[]> PatronesBusquedaLaboral = new Dictionary<string, string[]>
        {
            {
                "AFIA", new[]{
                "RESOLUCIÓN SOBRE RECONOCIMIENTO DE ALTA: (.+)", //Documento de afiliacion de alta (primera version)
                "INFORME DE SITUACIÓN DE ALTA(.+)" //Documento de afiliacion de alta (version v1.3 de junio-2025)
                }
            },
            {
                "AFIB", new []{
                "RESOLUCIÓN SOBRE RECONOCIMIENTO DE BAJA: (.+)"//Documento de afiliacion de baja}
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
            //Procesado del modelo de afiliacion de alta AFIA

            //Busca el modelo AFIA en los patrones, y si existe devuelve los que tenga creados
            if(!PatronesBusquedaLaboral.TryGetValue("AFIA", out string[] patrones))
            {
                return; // Si no se encuentra el modelo, salir del método
            }

            //Se usa el primer patron de AFIA que se encuentre en el texto
            string textoAFIA = "";
            patronEncontrado = null;

            //Procesa todos los patrones que tenga el modelo
            foreach(var patron in patrones)
            {
                textoAFIA = procesosPDF.ProcesaPatron(patron, 1);
                if(!string.IsNullOrEmpty(textoAFIA))
                {
                    patronEncontrado = patron; //Guardamos el patron que sirvio para encontrar el texto
                    break; // Sale del bucle al encontrar el primer patron
                }
            }

            // Una vez encontrado el texto, se extraen las variables observaciones 1 y 2
            if(!string.IsNullOrEmpty(textoAFIA))
            {
                // Se utiliza el patron encontrado para grabar las observacines 1 y 2
                if(patronEncontrado != null)
                {
                    int longitudBase = patronEncontrado.Length - 4; // Se resta 4 para quitar los caracteres finales del patron
                    if(longitudBase < 0)
                    {
                        longitudBase = 0; // Asegura que no se produzca un error si el patron es demasiado corto
                    }

                    busquedaLaboral.Observaciones1 = Program.quitaRaros(patronEncontrado.Substring(0, longitudBase));
                    // Se asegura que el texto encontrado sea mayor que el calculo de la longitud para evitar errores, y en ese caso se devuelve la segunda parte del texto a partir del patron
                    busquedaLaboral.Observaciones2 = textoAFIA.Length > longitudBase
                        ? Program.quitaRaros(textoAFIA.Substring(longitudBase)).Trim()
                        : "";
                }

                busquedaLaboral.CampoLibre1 = Program.quitaRaros(textoAFIA);
                busquedaLaboral.Modelo = "AFIA";
            }
        }

        public void ProcesarAFIB()
        {
            //Procesado del modelo de afiliacion de baja AFIB

            //Busca el modelo AFIB en los patrones, y si existe devuelve los que tenga creados
            if(!PatronesBusquedaLaboral.TryGetValue("AFIB", out string[] patrones))
            {
                return; // Si no se encuentra el modelo, salir del método
            }

            //Se usa el primer patron de AFIB que se encuentre en el texto
            string textoAFIB = "";
            patronEncontrado = null;

            //Procesa todos los patrones que tenga el modelo
            foreach(var patron in patrones)
            {
                textoAFIB = procesosPDF.ProcesaPatron(patron, 1);
                if(!string.IsNullOrEmpty(textoAFIB))
                {
                    patronEncontrado = patron; //Guardamos el patron que sirvio para encontrar el texto
                    break; // Sale del bucle al encontrar el primer patron
                }
            }

            if(!string.IsNullOrEmpty(textoAFIB))
            {
                // Se utiliza el patron encontrado para grabar las observaciones 1 y 2
                if(patronEncontrado != null)
                {
                    int longitudBase = patronEncontrado.Length - 4; // Se resta 4 para quitar los caracteres finales del patron
                    if(longitudBase < 0)
                    {
                        longitudBase = 0; // Asegura que no se produzca un error si el patron es demasiado corto
                    }

                    busquedaLaboral.Observaciones1 = Program.quitaRaros(patronEncontrado.Substring(0, longitudBase));
                    // Se asegura que el texto encontrado sea mayor que el calculo de la longitud para evitar errores, y en ese caso se devuelve la segunda parte del texto a partir del patron
                    busquedaLaboral.Observaciones2 = textoAFIB.Length > longitudBase
                        ? Program.quitaRaros(textoAFIB.Substring(longitudBase)).Trim()
                        : "";
                }

                busquedaLaboral.CampoLibre1 = Program.quitaRaros(textoAFIB);
                busquedaLaboral.Modelo = "AFIB";
            }
        }

        public void ProcesarAFIC()
        {
            // Procesado del modelo de afiliacion de baja AFIC

            //Busca el modelo AFIC en los patrones, y si existe devuelve los que tenga creados
            if(!PatronesBusquedaLaboral.TryGetValue("AFIC", out string[] patrones))
            {
                return; // Si no se encuentra el modelo, salir del método
            }

            //Se usa el primer patron de AFIC que se encuentre en el texto
            string textoAFIC = "";
            patronEncontrado = null;

            //Procesa todos los patrones que tenga el modelo
            foreach(var patron in patrones)
            {
                textoAFIC = procesosPDF.ProcesaPatron(patron, 1);
                if(!string.IsNullOrEmpty(textoAFIC))
                {
                    patronEncontrado = patron; //Guardamos el patron que sirvio para encontrar el texto
                    break; // Sale del bucle al encontrar el primer patron
                }
            }

            if(!string.IsNullOrEmpty(textoAFIC))
            {
                // Patron a buscar en el texto
                string pattern = @"(TESORERIA GENERAL DE LA SEGURIDAD SOCIAL)(.+)";
                Match match = Regex.Match(textoAFIC, pattern);

                if(match.Success)
                {
                    busquedaLaboral.Observaciones1 = "COMUNICACION SOBRE MODIFICACION DE CONTRATO DE TRABAJO";
                    busquedaLaboral.Observaciones2 = Program.quitaRaros(match.Groups[2].Value);
                }
                busquedaLaboral.CampoLibre1 = $"{busquedaLaboral.Observaciones1} - {busquedaLaboral.Observaciones2}";
                busquedaLaboral.Modelo = "AFIC";
            }
        }

        public void ProcesarAFIV()
        {
            //Pendiente de implementar cuando haya un documento de ejemplo (lo dejo comentado para terminarlo cuando proceda)

            // Procesado del modelo de afiliacion de baja AFIC

            //Busca el modelo AFIV en los patrones, y si existe devuelve los que tenga creados
            if(!PatronesBusquedaLaboral.TryGetValue("AFIV", out string[] patrones))
            {
                return; // Si no se encuentra el modelo, salir del método
            }

            //Se usa el primer patron de AFIV que se encuentre en el texto
            string textoAFIV = "";
            patronEncontrado = null;

            if(!string.IsNullOrEmpty(textoAFIV))
            {
                //Hacer la busqueda del texto que corresponda para grabar las observaciones 1 y 2
                busquedaLaboral.Observaciones1 = Program.quitaRaros("");
                busquedaLaboral.Observaciones2 = Program.quitaRaros("");

                busquedaLaboral.CampoLibre1 = $"{busquedaLaboral.Observaciones1} - {busquedaLaboral.Observaciones2}";
                busquedaLaboral.Modelo = "AFIV";
            }
        }

        public void ProcesarCER()
        {
            // Procesado del modelo de certificado de estar al corriente de pago

            //Busca el modelo CER en los patrones, y si existe devuelve los que tenga creados
            if(!PatronesBusquedaLaboral.TryGetValue("CER", out string[] patrones))
            {
                return; // Si no se encuentra el modelo, salir del método
            }

            //Se usa el primer patron de CER que se encuentre en el texto
            string textoCER = "";

            //Procesa todos los patrones que tenga el modelo
            foreach(var patron in patrones)
            {
                textoCER = procesosPDF.ProcesaPatron(patron, 1);
                if(!string.IsNullOrEmpty(textoCER))
                {
                    busquedaLaboral.Modelo = "CER";
                }
            }
        }

        public void ProcesarRNT()
        {
            // Procesado del modelo de la relacion nominal de trabajadores

            //Busca el modelo RNT en los patrones, y si existe devuelve los que tenga creados
            if(!PatronesBusquedaLaboral.TryGetValue("RNT", out string[] patrones))
            {
                return; // Si no se encuentra el modelo, salir del método
            }

            //Se usa el primer patron de RNT que se encuentre en el texto
            string textoRNT = "";

            //Procesa todos los patrones que tenga el modelo
            foreach(var patron in patrones)
            {
                textoRNT = procesosPDF.ProcesaPatron(patron, 1);
                if(!string.IsNullOrEmpty(textoRNT))
                {
                    busquedaLaboral.Modelo = "RNT";
                }
            }
        }

        public void ProcesarRLC()
        {
            // Procesado del modelo del recibo de liquidacion de cotizaciones

            //Busca el modelo RLC en los patrones, y si existe devuelve los que tenga creados
            if(!PatronesBusquedaLaboral.TryGetValue("RLC", out string[] patrones))
            {
                return; // Si no se encuentra el modelo, salir del método
            }

            //Se usa el primer patron de RNT que se encuentre en el texto
            string textoRLC = "";

            //Procesa todos los patrones que tenga el modelo
            foreach(var patron in patrones)
            {
                textoRLC = procesosPDF.ProcesaPatron(patron, 1);
                if(!string.IsNullOrEmpty(textoRLC))
                {
                    busquedaLaboral.Modelo = "RLC";
                }
            }
        }
    }
}
