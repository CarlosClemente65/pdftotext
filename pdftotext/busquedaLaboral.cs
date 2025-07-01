using System.Text.RegularExpressions;

namespace pdftotext
{
    public class busquedaLaboral
    {
        //Instanciacion de la clase procesosPDF para usar sus metodos en esta
        procesosPDF procesosPDF = new procesosPDF();

        //Definicion de variables para extraer datos (se ponen como estaticas para poder acceder desde la clase 'ProcesosModelosLaboral'
        public static string Modelo { get; set; } = string.Empty;
        public static string ModeloNum { get; set; } = string.Empty;
        public static string TipoModelo { get; set; } = string.Empty;
        public static string Nif { get; set; } = string.Empty;
        public static string Ejercicio { get; set; } = string.Empty;
        public static string Periodo { get; set; } = string.Empty;
        public static string CCC { get; set; } = string.Empty;
        public static string DniTrabajador { get; set; } = string.Empty;
        public static string FechaEfecto { get; set; } = string.Empty;
        public static string PeriodoIDC { get; set; } = string.Empty; //En los IDC se necesita esta fecha para comparar con el alta y la fecha del contrato para saber si es una variacion.
        public static string TipoIDC { get; set; } = string.Empty; //Controla el tipo de IDC (alta, baja o variacion)
        public static string Observaciones1 { get; set; } = string.Empty; //Permite pasar observaciones al GAD
        public static string Observaciones2 { get; set; } = string.Empty; //Permite pasar observaciones al GAD
        public static string Observaciones3 { get; set; } = string.Empty; //Permite pasar observaciones al GAD
        public static string CampoLibre1 { get; set; } = string.Empty; //Permite pasar observaciones al GAD
        public static string CampoLibre2 { get; set; } = string.Empty; //Permite pasar observaciones al GAD

        //Patrones de busqueda comunes
        string patronNif = @"\b(?=(?:\w*[A-Z]){1,2})(?=\w*\d)\w{9,10}\b"; //Busca una palabra de 9 o 10 caracteres (\w{9,10}) y obliga a que haya alguna letra mayuscula y algun numero, pero el primer caracter es opcional (en los documentos laborales le pueden poner un cero delante)

        string patronPeriodoL00 = @"\d{2}/\d{4}.*\d{2}/\d{4}"; //Busca dos digitos, seguido de una barra inclinada, seguido de 4 digitos, seguido de cero o varios espacios, seguido de dos digitos, seguido de una barra inclinada y seguido de 4 digitos.


        public busquedaLaboral()
        {
            //Construcctor de la clase que inicializa las variables (se inicializan en la definicion de las propiedades
        }

        public void buscarDatos()
        {
            // Localiza primero el modelo de los que hay para luego poder grabar los datos necesarios de cada uno
            BuscarModeloLaboral();

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

        private void BuscarModeloLaboral()
        {
            // Permite asignar un modelo de los que contemplamos en la aplicacion, segun los patrones que estan definidos de cada uno de ellos.
            Modelo = "000"; //Inicializa el modelo a 000 para que no de error si no encuentra ningun modelo
            ProcesosModelosLaboral procesosLaboral = new ProcesosModelosLaboral();
            try
            {
                foreach(var modelo in procesosLaboral.PatronesBusquedaLaboral.Keys)
                {
                    switch(modelo)
                    {
                        case "AFIA":
                            procesosLaboral.ProcesarAFIA();
                            break;

                        case "AFIB":
                            procesosLaboral.ProcesarAFIB();
                            break;

                        case "AFIC":
                            procesosLaboral.ProcesarAFIC();
                            break;

                        case "AFIV":
                            //Este modelo no esta implementado por no tener ejemplo, lo dejo comentado para cuando podamos conseguir un modelo.
                            //procesosLaboral.ProcesarAFIV();
                            break;

                        case "CER":
                            procesosLaboral.ProcesarCER();
                            break;

                        case "RNT":
                            procesosLaboral.ProcesarRNT();
                            break;

                        case "RLC":
                            procesosLaboral.ProcesarRLC();
                            break;

                        case "ITA":
                            procesosLaboral.ProcesarITA();
                            break;

                        case "IDC":
                            procesosLaboral.ProcesarIDC();
                            break;

                        case "HUE":
                            procesosLaboral.ProcesarHUE();
                            break;

                        default:
                            break;
                    }

                    if(Modelo != "000") // Si ya ha encontrado un modelo, sale del bucle
                    {
                        break;
                    }
                }
            }
            catch(Exception ex)
            {
                //Si se produce alguna excepcion, se graba un fichero con el error.
                procesosPDF.grabaFichero("error_proceso.txt", "Error al buscar el numero de modelo.\r\n" + ex.Message);
            }
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
            string patronTipoModelo = @"L\d{2}(.+)"; //Busca una L seguida de 2 digitos, seguida de un caracter y seguida de cero o mas palabras.   L\d{2}.\w*

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
            string patronPeriodo = @"Fecha: \d{2}[-/.]\d{2}[-/.]\d{4}"; //Busca el texto seguido de una fecha separada por un guion, una barra inclinada o un punto
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
                    string patronPeriodoL03 = @"Fecha de Control: \d{2]/\d{4}\b"; //Busca el texto seguido de 2 digitos, seguido de una barra inclinada, seguido de 4 digitos y termina la palabra.

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
            string patronAltaIDC = @"ALTA:\s*\d{2}.\d{2}.\d{4}"; //Busca el texto seguido de uno o varios espacios y despues la fecha en formato dd.mm.aaaa (el separador puede ser cualquier caracter)

            string patronBajaIDC = @"BAJA:\s*\d{2}.\d{2}.\d{4}"; //Busca el texto seguido de uno o varios espacios y despues la fecha en formato dd.mm.aaaa (el separador puede ser cualquier caracter)

            string patronPeriodoIDC = @"PERIODO:\s*DESDE\s*\d{2}.\d{2}.\d{4}"; //Busca el texto seguido de uno o varios espacios y despues la fecha en formato dd.mm.aaaa (el separador puede ser cualquier caracter)";

            string patronFechaIDC = @"FECHA:\s*\d{2}.\d{2}.\d{4}"; //Busca el texto seguido de uno o varios espacios y despues la fecha en formato dd.mm.aaaa (el separador puede ser cualquier caracter)";

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
                    string patronCCCHuella = @"(\d\s){6}\d{7}(\s\d){2}"; //Busca 6 digitos separados por espacio, seguido de 7 digitos y seguido de 2 digitos separados por espacio (en la huella el CCC viene separado por espacios)
                    cccTmp = procesosPDF.ProcesaPatron(patronCCCHuella, 1);
                    break;

                case "IDC":
                    //string patronCCCIDC = @"C\.?C\.?C\.?: \d{2}[-. \n]?\d{7}[-. ]?\d{2}";
                    string patronCCCIDC = @"C\.?C\.?C\.?:\s*(\d{2}\s\d{9}|\d{4}\s\d{2}\s\d{9})(?=\s|$)"; // Busca C.C.C.: (con puntos opcionales). Despues puede haber una de las dos opciones siguientes: 2 digitos, espacio, 9 digitos o bien 4 digitos, espacio, dos digitos, espacio, 9 digitos. Ademas al final se asegura que despues del ultimo numero de 9 digitos venga un espacio o un fin de linea

                    //Mismo patron que el anterior, pero dejando como opcionales los espacios que hay entre los digitos 2 + 9 y 4 + 2 + 9 (no tiene uso pero lo dejo para si en el futuro cambia el formato
                    string patronCCCIDC2 = @"C\.?C\.?C\.?:\s*(\d{2}\s?\d{9}|\d{4}\s?\d{2}\s?\d{9})(?=\s|$)";

                    cccTmp = procesosPDF.ProcesaPatron(patronCCCIDC, 1,"","",2);//El ultimo parametro es para que devuelva el segundo CCC que encuentra que se corresponde con el centro de trabajo (el primero es el principal de la empresa).
                    break;

                default:
                    string patronCCC = @"\d{4}[-. \n]\d{2}[-. \n]?\d{7}[-. ]?\d{2}"; //Busca 4 digitos, seguido de un separador (guion, punto, espacio o salto de linea), seguido de 2 digitos, seguido de un separador opcional (guion, punto, espacio o salto de linea), seguido de 7 digitos, seguido de un separador opcional (guion, punto o espacio) y seguido de 2 digitos; (en algunos documentos el CCC le ponen guiones para separar.

                    cccTmp = procesosPDF.ProcesaPatron(patronCCC, 1);
                    break;

            }

            if(cccTmp.Length > 0)
            {
                //Si encuentra el CCC le quita los espacios, guiones o saltos de linea que pueda tener
                cccTmp = cccTmp.Replace(" ", "").Replace("-", "").Replace("\n", "");
                CCC = cccTmp.Substring(cccTmp.Length - 11); //Solo se cogen los ultimos 11 digitos porque en algun modelo el CCC no le ponen los 4 digitos del Regimen de la empresa
            }
        }

        private void BuscarFechaEfecto()
        {
            //Hace falta para los modelos RNT, RLC, AFIA, AFIB, AFIC y HUE.
            string fechaEfectoTmp = string.Empty;
            switch(Modelo)
            {
                case "AFIC":
                    string patronFechaEfectoAFIC = @"con efectos de \d{2}[-/.]\d{2}[-/.]\d{4}"; //Busca el texto seguido de una fecha separada por un guion, una barra inclinada o un punto.

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
                    string patronFechaHuella = @"Fecha de Inicio (\w+\s+)*:\s\d{2}[-/.]\d{2}[-/.]\d{4}"; //Busca el texto seguido de una o varias palabras separadas por espacios, seguido de dos puntos, seguido de un espacio, seguido de una fecha separada por guiones, barra inclinada o puntos.

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
                    }
                    break;
                case "RLC":
                case "RNT":
                    string patronFechaEfectoRNTRLC = @"(\d{2}-\d{2}-\d{4})\s+(?:\w+\s+)?\d{2}:\d{2}:\d{2}"; // Busca una fecha con formato dd-mm-aaaa, seguido de uno o mas espacios o saltos de linea, seguido de una palabra con espacios al final o no (no se captura esta parte), seguido de una hora con formato HH:mm:ss

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
                    string patronFechaEfecto = @"se indica a continuación: (\d{1,2} de \S+ de \d{4})"; //Busca el texto seguido de la fecha en formato dia de mes de año

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
}

