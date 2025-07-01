using System.Text.RegularExpressions;

namespace pdftotext
{
    public class ProcesosModelosLaboral
    {
        // Instancia de la clase 'procesosPDF' para acceder a sus métodos y propiedades
        procesosPDF procesosPDF = new procesosPDF();

        // Texto que recoge el patron que encuentra en todos los procesos
        string patronEncontrado;

        //Diccionario con todos los modelos que se pueden buscar, y cada uno tiene el patron o patrones de busqueda. Si hay que incluir nuevo patrones, solo hay que poner una coma al final del patron y añadir el texto o patron que haya que buscar
        public Dictionary<string, string[]> PatronesBusquedaLaboral = new Dictionary<string, string[]>
        {
            {
                //Documento de afiliacion - alta
                "AFIA", new[]{
                "RESOLUCIÓN SOBRE RECONOCIMIENTO DE ALTA: (.+)", //Documento de afiliacion de alta (primera version)
                "INFORME DE SITUACIÓN DE ALTA(.+)" //Documento de afiliacion de alta (version v1.3 de junio-2025)
                }
            },
            {
                //Documento de afiliacion - baja
                "AFIB", new []{
                "RESOLUCIÓN SOBRE RECONOCIMIENTO DE BAJA: (.+)",//Documento de afiliacion de baja}
                "INFORME DE SITUACIÓN DE BAJA(.+)" //Documento de afiliacion de alta (version v1.3 de junio-2025)
                }
            },
            {
                //Documento de afiliacion - cambio de contrato
                "AFIC", new []{
                @"COMUNICACIÓN SOBRE\s+(\w+\s+)*\nCONTRATO DE TRABAJO\s+([\w+\s]+)$*"//Documento de afiliacion de variacion. Busca el texto seguido de uno o varios espacios, seguido de una o varias palabras separadas por espacios, seguido de un salto de linea, seguido del texto, seguido de uno o varios espacios, seguido de una o varias palabras que estan al final de la linea
                }
            },
            {
                //Documento de afiliacion - variacion
                "AFIV", new []{
                "" //Documento de afiliacion de variacion (no hay documento de ejemplo)
                }
            },
            {
                //Documento de certificado de corriente de pago
                "CER", new []{
                "CERTIFICADO DE ESTAR AL CORRIENTE"
                }
            },
            {
                //Documento de Relacion nominal de trabajadores (antiguo TC2)
                "RNT", new []{
                "[A-Z]RNT\\d{10}\\b" //Busca una letra mayuscula, seguida de RNT y seguida de 10 digitos que terminan la palabra.
                }
            },
            {
                //Documento de Recibo de liquidacion de coticaciones (antiguo TC1)
                "RLC", new []{
                @"[A-Z]RLC\d{10}\b" //Busca una letra mayuscula, seguida de RLC y seguida de 10 digitos que terminan la palabra.
                }
            },
            {
                //Documento de Trabajadores en alta
                "ITA", new []{
                @"INFORME DE TRABAJADORES EN ALTA A FECHA: \d{2}.\d{2}.\d{4}" //Busca el texto seguido dos digitos, seguido de un caracter, seguido de dos digitos, seguido de un caracter y seguido de 4 caracteres.
                }
            },
            {
                //Documento de Informe de datos para la cotizacion
                "IDC", new []{
                "Informe de Datos para la Cotización(.+)" //Busca el texto seguido de cualquier caracter
                }
            },
            {
                //Documento de huella de contrato
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

            //Se usa el primer patron que se encuentre en el texto
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

            //Procesa todos los patrones que tenga el modelo
            foreach(var patron in patrones)
            {
                textoAFIV = procesosPDF.ProcesaPatron(patron, 1);
                if(!string.IsNullOrEmpty(textoAFIV))
                {
                    //Hacer la busqueda del texto que corresponda para grabar las observaciones 1 y 2
                    busquedaLaboral.Observaciones1 = Program.quitaRaros("");
                    busquedaLaboral.Observaciones2 = Program.quitaRaros("");

                    busquedaLaboral.CampoLibre1 = $"{busquedaLaboral.Observaciones1} - {busquedaLaboral.Observaciones2}";
                    busquedaLaboral.Modelo = "AFIV";
                    break; // Sale del bucle al encontrar el primer patron
                }
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
                    break; // Sale del bucle al encontrar el primer patron
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
                    break; // Sale del bucle al encontrar el primer patron
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
                    break; // Sale del bucle al encontrar el primer patron
                }
            }
        }

        public void ProcesarITA()
        {
            // Procesado del modelo del informe de trabajadores de alta

            //Busca el modelo ITA en los patrones, y si existe devuelve los que tenga creados
            if(!PatronesBusquedaLaboral.TryGetValue("ITA", out string[] patrones))
            {
                return; // Si no se encuentra el modelo, salir del método
            }

            //Se usa el primer patron que se encuentre en el texto
            string textoITA = "";
            patronEncontrado = null;

            //Procesa todos los patrones que tenga el modelo
            foreach(var patron in patrones)
            {
                textoITA = procesosPDF.ProcesaPatron(patron, 1);
                if(!string.IsNullOrEmpty(textoITA))
                {

                    string pattern = @"(.+):(.+)";
                    Match match = Regex.Match(textoITA, pattern);

                    if(match.Success)
                    {
                        string fechaTmp = match.Groups[2].Value;
                        string formatoFechaTexto = " dd MM yyyy";
                        DateTime fecha = DateTime.ParseExact(fechaTmp, formatoFechaTexto, System.Globalization.CultureInfo.GetCultureInfo("es-ES")); //Se convierte la fecha en texto a fecha numerica
                        //fecha = fechaDT.ToString("dd/MM/yyyy");//Se convierte la fecha numeria a texto para almacenarla en la variable
                        busquedaLaboral.Observaciones1 = $"{match.Groups[1].Value} {fecha.ToString("dd/MM/yyyy")}";

                    }
                    busquedaLaboral.Modelo = "ITA";
                    busquedaLaboral.Ejercicio = textoITA.Substring(textoITA.Length - 4);
                    busquedaLaboral.Periodo = textoITA.Substring(textoITA.Length - 7, 2);
                    break; // Sale del bucle al encontrar el primer patron
                }
            }
        }

        public void ProcesarIDC()
        {
            // Procesado del modelo del informe de datos para la cotizacion

            //Busca el modelo IDC en los patrones, y si existe devuelve los que tenga creados
            if(!PatronesBusquedaLaboral.TryGetValue("IDC", out string[] patrones))
            {
                return; // Si no se encuentra el modelo, salir del método
            }

            //Se usa el primer patron que se encuentre en el texto
            string textoIDC = "";
            patronEncontrado = null;

            //Procesa todos los patrones que tenga el modelo
            foreach(var patron in patrones)
            {
                textoIDC = procesosPDF.ProcesaPatron(patron, 1);
                if(!string.IsNullOrEmpty(textoIDC))
                {
                    string pattern = @"(.+)-(.+)";
                    Match match = Regex.Match(textoIDC, pattern);

                    if(match.Success)
                    {
                        busquedaLaboral.Observaciones1 = Program.quitaRaros(match.Groups[1].Value);
                        busquedaLaboral.Observaciones2 = Program.quitaRaros(match.Groups[2].Value);
                        busquedaLaboral.CampoLibre1 = Program.quitaRaros(textoIDC);
                    }
                    busquedaLaboral.Modelo = "IDC";
                }
            }
        }

        public void ProcesarHUE()
        {
            // Procesado del modelo del informe de datos para la cotizacion

            //Busca el modelo HUE en los patrones, y si existe devuelve los que tenga creados
            if(!PatronesBusquedaLaboral.TryGetValue("HUE", out string[] patrones))
            {
                return; // Si no se encuentra el modelo, salir del método
            }

            //Se usa el primer patron que se encuentre en el texto
            string textoHUE = "";
            patronEncontrado = null;

            //Procesa todos los patrones que tenga el modelo
            foreach(var patron in patrones)
            {
                textoHUE = procesosPDF.ProcesaPatron(patron, 1);
                if(!string.IsNullOrEmpty(textoHUE))
                {
                    busquedaLaboral.Modelo = "HUE";
                    string observacionesHUE = textoHUE.Replace("\n", " ");
                    string texto = Program.quitaRaros(observacionesHUE);

                    //Rellena las observaciones 1 a 3 segun la longitud del texto
                    busquedaLaboral.Observaciones1 = texto.Length > 0 ? texto.Substring(0, Math.Min(50, texto.Length)) : null;
                    busquedaLaboral.Observaciones2 = texto.Length > 50 ? texto.Substring(50, Math.Min(50, texto.Length - 50)) : null;
                    busquedaLaboral.Observaciones3 = texto.Length > 100 ? texto.Substring(100) : null;
                }
            }
        }
    }
}
