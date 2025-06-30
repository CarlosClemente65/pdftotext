using System.Reflection.Emit;
using System.Security;
using System.Text.RegularExpressions;

namespace pdftotext
{
    public class busquedaLaboral
    {
        //Instanciacion de la clase procesosPDF para usar sus metodos en esta
        procesosPDF procesosPDF = new procesosPDF();

        //Definicion de variables para extraer datos
        public string Modelo { get; private set; }
        public string ModeloNum { get; private set; }
        public string TipoModelo { get; private set; }
        public string Nif { get; private set; }
        public string Ejercicio { get; private set; }
        public string Periodo { get; private set; }
        public string CCC { get; private set; }
        public string DniTrabajador { get; private set; }
        public string FechaEfecto { get; private set; }
        public string PeriodoIDC { get; private set; }
        public string TipoIDC { get; private set; }
        public string Observaciones1 { get; private set; }
        public string Observaciones2 { get; private set; }
        public string Observaciones3 { get; private set; }
        public string CampoLibre1 { get; private set; }
        public string CampoLibre2 { get; private set; }

        //Tipos de documento
        string CER = string.Empty; //Documento de certificado de corriente de pago
        string RLC = string.Empty; //Documento de Recibo de liquidacion de coticaciones (antiguo TC1)
        string RNT = string.Empty; //Documento de Relacion nominal de trabajadores (antiguo TC2)
        string ITA = string.Empty; //Documento de Trabajadores en alta
        string AFIA = string.Empty; //Documento de afiliacion - alta
        string AFIB = string.Empty; //Documento de afiliacion - baja
        string AFIV = string.Empty; //Documento de afiliacion - variacion
        string AFIC = string.Empty; //Documento de afiliacion - cambio de contrato
        string IDC = string.Empty; //Documento de Informe de datos para la cotizacion
        string HUE = string.Empty; //Documento de huella de contrato



        #region patrones de busqueda
        //Definicion de patrones de busqueda de datos

        string patronNif = @"\b(?=(?:\w*[A-Z]){1,2})(?=\w*\d)\w{9,10}\b"; //Busca una palabra de 9 o 10 caracteres (\w{9,10}) y obliga a que haya alguna letra mayuscula y algun numero, pero el primer caracter es opcional (en los documentos laborales le pueden poner un cero delante)

        string patronPeriodo = @"Fecha: \d{2}[-/.]\d{2}[-/.]\d{4}"; //Busca el texto seguido de una fecha separada por un guion, una barra inclinada o un punto

        string patronPeriodoL00 = @"\d{2}/\d{4}.*\d{2}/\d{4}"; //Busca dos digitos, seguido de una barra inclinada, seguido de 4 digitos, seguido de cero o varios espacios, seguido de dos digitos, seguido de una barra inclinada y seguido de 4 digitos.

        string patronPeriodoL03 = @"Fecha de Control: \d{2]/\d{4}\b"; //Busca el texto seguido de 2 digitos, seguido de una barra inclinada, seguido de 4 digitos y termina la palabra.

        string patronTipoModelo = @"L\d{2}(.+)"; //Busca una L seguida de 2 digitos, seguida de un caracter y seguida de cero o mas palabras.   L\d{2}.\w*


        string patronCCC = @"\d{4}[-. \n]\d{2}[-. \n]?\d{7}[-. ]?\d{2}"; //Busca 4 digitos, seguido de un guion, un punto o un espacio, seguido de 2 digitos, seguido de un guion, un punto o un espacio (es opcional), seguido de 7 digitos, seguido de un guion, un punto o un espacio (es opcional) y seguido de 2 digitos; (en algunos documentos el CCC le ponen guiones para separar.
        string patronCCCIDC = @"C\.?C\.?C\.?: \d{2}[-. \n]?\d{7}[-. ]?\d{2}";

        string patronCCCHuella = @"(\d\s){6}\d{7}(\s\d){2}"; //Busca 6 digitos separados por espacio, seguido de 7 digitos y seguido de 2 digitos separados por espacio (en la huella el CCC viene separado por espacios)

        string patronCER = "CERTIFICADO DE ESTAR AL CORRIENTE";

        string patronRLC = @"[A-Z]RLC\d{10}\b"; //Busca una letra mayuscula, seguida de RLC y seguida de 10 digitos que terminan la palabra.

        string patronRNT = "[A-Z]RNT\\d{10}\\b"; //Busca una letra mayuscula, seguida de RNT y seguida de 10 digitos que terminan la palabra.

        string patronITA = @"INFORME DE TRABAJADORES EN ALTA A FECHA: \d{2}.\d{2}.\d{4}"; //Busca el texto seguido dos digitos, seguido de un caracter, seguido de dos digitos, seguido de un caracter y seguido de 4 caracteres.

        string patronAFIA = "RESOLUCIÓN SOBRE RECONOCIMIENTO DE ALTA(.+)";//Documento de afiliacion de alta

        string patronAFIA_1 = "INFORME DE SITUACIÓN DE ALTA(.+)";//Documento de afiliacion de alta (version de junio-2025)

        string patronAFIB = "RESOLUCIÓN SOBRE RECONOCIMIENTO DE BAJA(.+)";//Documento de afiliacion de baja

        string patronAFIV = ""; //Documento de afiliacion de variacion (no hay documento de ejemplo)

        string patronAFIC = @"COMUNICACIÓN SOBRE\s+(\w+\s+)*\nCONTRATO DE TRABAJO\s+([\w+\s]+)$*"; //Documento de afiliacion de variacion. Busca el texto seguido de uno o varios espacios, seguido de una o varias palabras separadas por espacios, seguido de un salto de linea, seguido del texto, seguido de uno o varios espacios, seguido de una o varias palabras que estan al final de la linea

        string patronFechaEfecto = @"se indica a continuación: (\d{1,2} de \S+ de \d{4})"; //Busca el texto seguido de la fecha en formato dia de mes de año

        string patronFechaEfectoAFIC = @"con efectos de \d{2}[-/.]\d{2}[-/.]\d{4}"; //Busca el texto seguido de una fecha separada por un guion, una barra inclinada o un punto.

        string patronFechaEfectoRNTRLC = @"(\d{2}-\d{2}-\d{4})\s+(?:\w+\s+)?\d{2}:\d{2}:\d{2}";

        string patronFechaHuella = @"Fecha de Inicio (\w+\s+)*:\s\d{2}[-/.]\d{2}[-/.]\d{4}"; //Busca el texto seguido de una o varias palabras separadas por espacios, seguido de dos puntos, seguido de un espacio, seguido de una fecha separada por guiones, barra inclinada o puntos.

        string patronIDC = "Informe de Datos para la Cotización(.+)"; //Busca el texto seguido de cualquier caracter

        string patronAltaIDC = @"ALTA:\s*\d{2}.\d{2}.\d{4}"; //Busca el texto seguido de uno o varios espacios y despues la fecha en formato dd.mm.aaaa (el separador puede ser cualquier caracter)

        string patronBajaIDC = @"BAJA:\s*\d{2}.\d{2}.\d{4}"; //Busca el texto seguido de uno o varios espacios y despues la fecha en formato dd.mm.aaaa (el separador puede ser cualquier caracter)

        string patronPeriodoIDC = @"PERIODO:\s*DESDE\s*\d{2}.\d{2}.\d{4}"; //Busca el texto seguido de uno o varios espacios y despues la fecha en formato dd.mm.aaaa (el separador puede ser cualquier caracter)";

        string patronFechaIDC = @"FECHA:\s*\d{2}.\d{2}.\d{4}"; //Busca el texto seguido de uno o varios espacios y despues la fecha en formato dd.mm.aaaa (el separador puede ser cualquier caracter)";

        string patronHuella = @"COMUNICACIÓN[\s\S]*DEL CONTRATO DE[\s\S]*?\n(.+)"; //Busca el texto seguido cualquier espacio o caracter (\S es lo contrario de un espacio), seguido de un salto de linea, seguido de cualquier caracter que no sea un salto de linea.


        #endregion

        #region Nuevos patrones de busqueda

        Dictionary<string, string[]> PatronesBusquedaLaboral = new Dictionary<string, string[]>
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

        #endregion

        public busquedaLaboral(string textoCompleto, List<string> paginasPDF)
        {
            //Construcctor de la clase que inicializa las variables
            Program.textoCompleto = textoCompleto;
            Program.paginasPDF = paginasPDF;
            this.Modelo = string.Empty;
            this.ModeloNum = string.Empty;
            this.TipoModelo = string.Empty;
            this.Nif = string.Empty;
            this.Ejercicio = string.Empty;
            this.Periodo = string.Empty;
            this.CCC = string.Empty;
            this.DniTrabajador = string.Empty;
            this.FechaEfecto = string.Empty;
            this.PeriodoIDC = string.Empty; //En los IDC se necesita esta fecha para comparar con el alta y la fecha del contrato para saber si es una variacion.
            this.TipoIDC = string.Empty; //Controla el tipo de IDC (alta, baja o variacion)
            this.Observaciones1 = string.Empty; //Permite pasar observaciones al GAD
            this.Observaciones2 = string.Empty; //Permite pasar observaciones al GAD
            this.Observaciones3 = string.Empty; //Permite pasar observaciones al GAD
            this.CampoLibre1 = string.Empty; //Permite pasar observaciones al GAD
            this.CampoLibre2 = string.Empty; //Permite pasar observaciones al GAD
        }

        public void buscarDatos()
        {
            BuscarModelo();

            switch(Modelo)
            {
                case "CER":
                    // Certificado estar al corriente de pago
                    BuscarPeriodoCER();
                    BuscarNif();
                    ModeloNum = "993";
                    TipoModelo = "03";
                    break;

                case "ITA":
                    //Documento ITA
                    BuscarNif();
                    BuscarCCC();
                    CampoLibre1 = $"CCC: {CCC}";
                    ModeloNum = "991";
                    TipoModelo = "07";
                    break;

                case "RNT":
                case "RLC":
                    //Documento RNT y RLC
                    BuscarNif();
                    BuscarTipoDocumento();
                    BuscarPeriodoRLCyRNT();
                    BuscarFechaEfecto();
                    BuscarCCC();
                    CampoLibre1 = $"CCC: {CCC}";
                    if(Modelo == "RNT")
                    {
                        ModeloNum = "996";
                    }
                    else
                    {
                        ModeloNum = "997";
                    }

                    switch(TipoModelo)
                    {
                        case "L00":
                            TipoModelo = "00";
                            break;

                        case "L13":
                            TipoModelo = "02";
                            break;

                        case "L03":
                            TipoModelo = "05";
                            break;

                        case "L90":
                            TipoModelo = "90";
                            break;
                    }
                    break;

                case "AFIA"://Documento AFI de alta
                case "AFIB"://Documento AFI de baja
                case "AFIC": //Documento AFI de cambio de contrato
                    BuscarDniTrabajador();
                    BuscarFechaEfecto();
                    BuscarCCC();
                    CampoLibre2 = FechaEfecto;
                    ModeloNum = "991";
                    if(Modelo == "AFIA")
                    {
                        TipoModelo = "00";
                    }
                    else if(Modelo == "AFIB")
                    {
                        TipoModelo = "01";
                    }
                    else
                    {
                        TipoModelo = "03";
                    }
                    break;

                case "AFIV":
                    //Documento AFI de variacion (de este tipo no tenemos documento de ejemplo)
                    ModeloNum = "991";
                    TipoModelo = "02";
                    break;

                case "IDC":
                    //Documentos IDC
                    BuscarDniTrabajador();
                    BuscarFechaIDC();
                    BuscarCCC();
                    ModeloNum = "991";
                    break;

                case "HUE":
                    //Documentos de huella de contratos
                    BuscarDniTrabajador();
                    BuscarFechaEfecto();
                    BuscarCCC();
                    ModeloNum = "992";
                    TipoModelo = "00";
                    break;

                case "000":
                    ModeloNum = "Modelo no reconocido";
                    break;
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


        #region Metodos de busqueda
        private void BuscarModelo()
        {
            Modelo = "000";
            try
            {
                AFIA = procesosPDF.ProcesaPatron(patronAFIA, 1);
                if(AFIA == "")
                {
                    AFIA = procesosPDF.ProcesaPatron(patronAFIA_1, 1);
                }
                if(!string.IsNullOrEmpty(AFIA))
                {
                    if(AFIA.Contains(":"))
                    {
                        string[] partes = AFIA.Split(new[] { ':' });
                        Observaciones1 = partes[0].Trim();
                        Observaciones2 = partes[1].Trim();

                    }
                    else
                    {
                        Observaciones1 = patronAFIA_1.Substring(0, patronAFIA_1.Length - 4);
                        Observaciones2 = AFIA.Substring(Observaciones1.Length).Trim();
                    }

                    Observaciones1 = quitaRaros(Observaciones1);
                    Observaciones2 = quitaRaros(Observaciones2);


                    //string pattern = @"(.+?)\s*:\s*(.+)";
                    //Match match = Regex.Match(AFIA, pattern);

                    //if (match.Success)
                    //{
                    //Observaciones1 = quitaRaros(match.Groups[1].Value);
                    //Observaciones2 = quitaRaros(match.Groups[2].Value);
                    //}
                    CampoLibre1 = quitaRaros(AFIA);
                    Modelo = "AFIA";
                }

                AFIB = procesosPDF.ProcesaPatron(patronAFIB, 1);
                if(!string.IsNullOrEmpty(AFIB))
                {
                    string pattern = @"(.+?)\s*:\s*(.+)";
                    Match match = Regex.Match(AFIB, pattern);

                    if(match.Success)
                    {
                        Observaciones1 = quitaRaros(match.Groups[1].Value);
                        Observaciones2 = quitaRaros(match.Groups[2].Value);
                    }
                    CampoLibre1 = quitaRaros(AFIB);
                    Modelo = "AFIB";
                }

                AFIC = procesosPDF.ProcesaPatron(patronAFIC, 1);
                if(!string.IsNullOrEmpty(AFIC))
                {
                    string pattern = @"(TESORERIA GENERAL DE LA SEGURIDAD SOCIAL)(.+)";
                    Match match = Regex.Match(AFIC, pattern);

                    if(match.Success)
                    {
                        Observaciones1 = "COMUNICACION SOBRE MODIFICACION DE CONTRATO DE TRABAJO";
                        Observaciones2 = quitaRaros(match.Groups[2].Value);
                    }
                    CampoLibre1 = Observaciones1 + " - " + Observaciones2;
                    Modelo = "AFIC";
                }

                ////Este documento no tenemos ejemplo del PDF. Lo dejo comentado por si lo conseguimos poder implantarlo
                //AFIV = procesosPDF.ProcesaPatron(patronAFIV, 1);
                //if (!string.IsNullOrEmpty(AFIV))
                //{
                //    Modelo = "AFIV";
                //}

                CER = procesosPDF.ProcesaPatron(patronCER, 1);
                if(!string.IsNullOrEmpty(CER))
                {
                    Modelo = "CER";
                }

                RNT = procesosPDF.ProcesaPatron(patronRNT, 1);
                if(!string.IsNullOrEmpty(RNT))
                {
                    Modelo = "RNT";
                }

                RLC = procesosPDF.ProcesaPatron(patronRLC, 1);
                if(!string.IsNullOrEmpty(RLC))
                {
                    Modelo = "RLC";
                }

                ITA = procesosPDF.ProcesaPatron(patronITA, 1);
                if(!string.IsNullOrEmpty(ITA))
                {
                    string pattern = @"(.+):(.+)";
                    Match match = Regex.Match(ITA, pattern);

                    if(match.Success)
                    {
                        string fechaTmp = match.Groups[2].Value;
                        string formatoFechaTexto = " dd MM yyyy";
                        DateTime fecha = DateTime.ParseExact(fechaTmp, formatoFechaTexto, System.Globalization.CultureInfo.GetCultureInfo("es-ES")); //Se convierte la fecha en texto a fecha numerica
                        //fecha = fechaDT.ToString("dd/MM/yyyy");//Se convierte la fecha numeria a texto para almacenarla en la variable
                        Observaciones1 = quitaRaros(match.Groups[1].Value) + " " + fecha.ToString("dd/MM/yyyy");
                    }
                    Modelo = "ITA";
                    Ejercicio = ITA.Substring(ITA.Length - 4);
                    Periodo = ITA.Substring(ITA.Length - 7, 2);
                }

                IDC = procesosPDF.ProcesaPatron(patronIDC, 1);
                if(!string.IsNullOrEmpty(IDC))
                {
                    string pattern = @"(.+)-(.+)";
                    Match match = Regex.Match(IDC, pattern);

                    if(match.Success)
                    {
                        Observaciones1 = quitaRaros(match.Groups[1].Value);
                        Observaciones2 = quitaRaros(match.Groups[2].Value);
                        CampoLibre1 = quitaRaros(IDC);
                    }
                    Modelo = "IDC";
                }

                HUE = procesosPDF.ProcesaPatron(patronHuella, 1);
                if(!string.IsNullOrEmpty(HUE))
                {
                    Modelo = "HUE";
                    string observacionesHUE = HUE.Replace("\n", " ");
                    if(observacionesHUE.Length > 100)
                    {
                        Observaciones1 = quitaRaros(observacionesHUE.Substring(0, 50));
                        Observaciones2 = quitaRaros(observacionesHUE.Substring(50, 50));
                        Observaciones3 = quitaRaros(observacionesHUE.Substring(100));
                    }
                    else if(observacionesHUE.Length > 50)
                    {
                        Observaciones1 = quitaRaros(observacionesHUE.Substring(0, 50));
                        Observaciones2 = quitaRaros(observacionesHUE.Substring(50));
                    }
                    else
                    {
                        Observaciones1 = quitaRaros(observacionesHUE.Substring(0));
                    }
                }

            }
            catch(Exception ex)
            {
                //Si se produce alguna excepcion, se graba un fichero con el error.
                procesosPDF.grabaFichero("error_proceso.txt", "Error al buscar el numero de modelo.\r\n" + ex.Message);
            }
        }


        private void BuscarModeloLaboral()
        {
            try
            {
                foreach(var modelo in PatronesBusquedaLaboral.Keys)
                {
                    string patronUsado;
                    string textoEncontrado = BuscarPrimerPatronValido(PatronesBusquedaLaboral[modelo], 1, out patronUsado);
                    if(!string.IsNullOrEmpty(textoEncontrado))
                    {
                        Modelo = modelo;

                        if(procesadoresPorModelo.ContainsKey(modelo))
                        {
                            procesadoresPorModelo[modelo].Invoke(textoEncontrado);
                        }
                        else
                        {
                            // Procesamiento genérico o por defecto
                            CampoLibre1 = quitaRaros(textoEncontrado);
                        }

                        break; // Para tras encontrar el primer modelo válido
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        public string BuscarPrimerPatronValido(string[] patrones, int pagina, out string patronEncontrado)
        {
            foreach(string patron in patrones)
            {
                string resultado = ProcesaPatron(patron, pagina);
                if(!string.IsNullOrEmpty(resultado))
                {
                    patronEncontrado = patron;
                    return resultado; // devuelve el texto que coincide con ese patrón
                }
            }
            patronEncontrado = null;
            return ""; // no encontró nada
        }


        private void BuscarNif()
        {
            Nif = procesosPDF.ProcesaPatron(patronNif, 1);
            if(Nif.Length > 0)
            {
                Nif = Nif.Substring(1, 9);
            }
        }

        private void BuscarDniTrabajador()
        {
            //Busqueda del DNI del trabajador
            if(Modelo == "HUE")
            {
                DniTrabajador = procesosPDF.ProcesaPatron(patronNif, 1, Modelo, "", 1);
            }
            else
            {
                DniTrabajador = procesosPDF.ProcesaPatron(patronNif, 1, Modelo, "", 0);
            }

            if(DniTrabajador.Length > 0)
            {
                DniTrabajador = DniTrabajador.Substring(DniTrabajador.Length - 9);
            }
        }


        private void BuscarTipoDocumento()
        {
            //Busqueda del tipo de documento. L00 => 00, L13 => 02, L03 => 5, L90 => 90
            string tipoModeloTmp = procesosPDF.ProcesaPatron(patronTipoModelo, 1);
            if(tipoModeloTmp.Length > 0)
            {
                TipoModelo = tipoModeloTmp.Substring(0, 3);
                Observaciones1 = Modelo + " - " + tipoModeloTmp;
            }
        }

        private void BuscarPeriodoCER()
        {
            //Periodo para el documento de certificado
            string periodoTmp = procesosPDF.ProcesaPatron(patronPeriodo, 1);
            if(periodoTmp.Length > 0)
            {
                string pattern = @"(.+)(\d{2}[-/.]\d{2}[-/.]\d{4})";
                Match match = Regex.Match(periodoTmp, pattern);

                if(match.Success)
                {
                    Observaciones1 = "CERTIFICADO CORRIENTE DE PAGO EN LA SEG.SOCIAL";
                    Observaciones2 = match.Groups[2].Value;
                }
                Ejercicio = periodoTmp.Substring(periodoTmp.Length - 4);
                Periodo = periodoTmp.Substring(10, 2);
            }
        }

        private void BuscarPeriodoRLCyRNT()
        {
            //Busqueda del periodo en el campo "Periodo liquidacion": el L03 se coje de la fecha de control, el resto del periodo de liquidacion.
            switch(TipoModelo)
            {
                case "L03":
                    string periodoL03 = string.Empty;
                    periodoL03 = procesosPDF.ProcesaPatron(patronPeriodoL03, 1, Modelo, TipoModelo);
                    if(periodoL03.Length > 0)
                    {
                        Ejercicio = periodoL03.Substring(periodoL03.Length - 4);
                        Periodo = periodoL03.Substring(periodoL03.Length - 7, 2);
                    }
                    break;

                case "L90":
                    string periodoL90 = string.Empty;
                    if(Modelo == "RLC")
                    {
                        //Si el modelo es el RLC la fecha aparece en segunda posicion y se tiene que pasar la posicion del valor a extraer
                        periodoL90 = procesosPDF.ProcesaPatron(patronPeriodoL00, 1, Modelo, TipoModelo, 1);
                    }
                    else
                    {
                        //Si el modelo es el RNT solo aparece una fecha y no se pasa la posicion del valor a extraer
                        periodoL90 = procesosPDF.ProcesaPatron(patronPeriodoL00, 1, Modelo, TipoModelo);
                    }
                    if(periodoL90.Length > 0)
                    {
                        Ejercicio = periodoL90.Substring(3, 4);
                        Periodo = periodoL90.Substring(0, 2);
                    }
                    break;

                default:
                    string periodoTmp = procesosPDF.ProcesaPatron(patronPeriodoL00, 1, Modelo, TipoModelo);
                    if(periodoTmp.Length > 0)
                    {
                        Ejercicio = periodoTmp.Substring(3, 4);
                        Periodo = periodoTmp.Substring(0, 2);
                    }
                    break;

            }
        }

        private void BuscarFechaIDC()
        {
            //En el IDC se necesitan buscar las fechas de alta, periodo,baja o fecha contrato para luego saber si es una variacion o baja
            string altaIDC = procesosPDF.ProcesaPatron(patronAltaIDC, 1);
            if(altaIDC.Length > 0)
            {
                altaIDC = altaIDC.Substring(altaIDC.Length - 10);
            }

            string periodoIDC = procesosPDF.ProcesaPatron(patronPeriodoIDC, 1);
            if(periodoIDC.Length > 0)
            {
                PeriodoIDC = periodoIDC.Substring(periodoIDC.Length - 10);
                FechaEfecto = PeriodoIDC;
                CampoLibre2 = PeriodoIDC;
            }

            string fechaIDC = procesosPDF.ProcesaPatron(patronFechaIDC, 1);
            if(fechaIDC.Length > 0)
            {
                fechaIDC = fechaIDC.Substring(fechaIDC.Length - 10);
            }

            string bajaIDC = procesosPDF.ProcesaPatron(patronBajaIDC, 1);
            if(bajaIDC.Length > 0)
            {
                TipoIDC = "05";
                //bajaIDCTmp = bajaIDCTmp.Substring(bajaIDCTmp.Length - 10);
            }
            else
            {
                if(altaIDC != PeriodoIDC || PeriodoIDC != fechaIDC || altaIDC != fechaIDC)
                {
                    //Si alguna de las fechas es diferente se trata de un IDC de variacion
                    TipoIDC = "06";
                }
                else
                {
                    //Si no es un IDC de variacion se trata de un IDC de alta
                    TipoIDC = "04";
                }
            }
            TipoModelo = TipoIDC;
        }


        private void BuscarCCC()
        {
            //Hace falta para los modelos anteriores y en los nuevos por si hay varios centros de trabajo
            string cccTmp = string.Empty;

            switch(Modelo)
            {
                case "HUE":
                    cccTmp = procesosPDF.ProcesaPatron(patronCCCHuella, 1);
                    break;

                case "IDC":
                    cccTmp = procesosPDF.ProcesaPatron(patronCCCIDC, 1);
                    break;

                default:
                    cccTmp = procesosPDF.ProcesaPatron(patronCCC, 1);
                    break;

            }

            if(cccTmp.Length > 0)
            {
                cccTmp = cccTmp.Replace(" ", "").Replace("-", "").Replace("\n", "");
                CCC = cccTmp.Substring(cccTmp.Length - 11); //Solo se cogen los ultimos 11 digitos porque en algun modelo el CCC no le ponen los 4 digitos del Regimen de la empresa
            }
        }

        private void BuscarFechaEfecto()
        {
            //Hace falta para los modelos anteriores.
            string fechaEfectoTmp = string.Empty;
            switch(Modelo)
            {
                case "AFIC":
                    fechaEfectoTmp = procesosPDF.ProcesaPatron(patronFechaEfectoAFIC, 1);
                    if(fechaEfectoTmp.Length > 0)
                    {
                        string pattern = @"(.*)(\d{2}[-/.]\d{2}[-/.]\d{4})";
                        Match match = Regex.Match(fechaEfectoTmp, pattern);

                        if(match.Success)
                        {
                            FechaEfecto = match.Groups[2].Value;
                            //FechaEfecto = fechaEfectoTmp.Substring(fechaEfectoTmp.Length - 10);
                        }
                    }
                    break;

                case "HUE":
                    fechaEfectoTmp = procesosPDF.ProcesaPatron(patronFechaHuella, 1);
                    if(fechaEfectoTmp.Length > 0)
                    {
                        string pattern = @"(.*)(\d{2}[-/.]\d{2}[-/.]\d{4})";
                        Match match = Regex.Match(fechaEfectoTmp, pattern);

                        if(match.Success)
                        {
                            FechaEfecto = match.Groups[2].Value;
                            //FechaEfecto = fechaEfectoTmp.Substring(fechaEfectoTmp.Length - 10);
                        }
                        //FechaEfecto = fechaEfectoTmp.Substring(fechaEfectoTmp.Length - 10);
                    }
                    break;
                case "RLC":
                case "RNT":
                    fechaEfectoTmp = procesosPDF.ProcesaPatron(patronFechaEfectoRNTRLC, 1);
                    if(fechaEfectoTmp.Length > 0)
                    {
                        string pattern = @"(.*)(\d{2}[-/.]\d{2}[-/.]\d{4})";
                        Match match = Regex.Match(fechaEfectoTmp, pattern);

                        if(match.Success)
                        {
                            FechaEfecto = match.Groups[2].Value;
                        }
                    }
                    break;

                default:
                    fechaEfectoTmp = procesosPDF.ProcesaPatron(patronFechaEfecto, 1);
                    if(fechaEfectoTmp.Length > 0)
                    {
                        //Al texto que le llega del PDF le hace un regex para obtener unicamente la fecha (segunda parte del patron)
                        string pattern = @"(.*:\s*)(\d{1,2}\s+de\s+[^\s]+\s+de\s+\d{4})";
                        Match match = Regex.Match(fechaEfectoTmp, pattern);

                        if(match.Success)
                        {
                            fechaEfectoTmp = match.Groups[2].Value;
                        }
                        string formatoFechaTexto = "d 'de' MMMM 'de' yyyy";
                        DateTime fecha = DateTime.ParseExact(fechaEfectoTmp, formatoFechaTexto, System.Globalization.CultureInfo.GetCultureInfo("es-ES")); //Se convierte la fecha en texto a fecha numerica
                        FechaEfecto = fecha.ToString("dd/MM/yyyy");//Se convierte la fecha numeria a texto para almacenarla en la variable

                    }
                    break;
            }
        }
    }

    #endregion
}

