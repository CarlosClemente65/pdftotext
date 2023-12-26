namespace pdftotext
{
    public class busquedaLaboral
    {
        //Instanciacion de la clase procesosPDF para usar sus metodos en esta
        procesosPDF procesosPDF = new procesosPDF();

        //Definicion de variables para extraer datos
        public string Modelo { get; private set; }
        public string TipoModelo { get; private set; }
        public string Nif { get; private set; }
        public string Ejercicio { get; private set; }
        public string Periodo { get; private set; }
        public string CCC { get; private set; }
        public string DniTrabajador { get; private set; }
        public string FechaEfecto { get; private set; }
        public string AltaIDC { get; private set; }
        public string BajaIDC { get; private set; }

        //Tipos de documento
        string CER = string.Empty;
        string RLC = string.Empty;
        string RNT = string.Empty;
        string ITA = string.Empty;
        string AFIA = string.Empty; //Modelo afiliacion alta
        string AFIB = string.Empty; //Modelo afiliacion baja
        string AFIV = string.Empty; //Modelo afiliacion variacion
        string AFIC = string.Empty; //Modelo afiliacion cambio de contrato
        string IDC = string.Empty;


        #region patrones de busqueda
        //Definicion de patrones de busqueda de datos
        string patronNif = @"\b(?=(?:\w*[A-Z]){1,2})(?=\w*\d)\w{10}\b"; //Busca una palabra de 10 caracteres (\w{10}) y obliga a que haya alguna letra mayuscula y algun numero
        string patronPeriodo = @"Fecha: \d{2}[/|.]\d{2}[/|.]\d{4}";
        string patronPeriodoL00 = @"\d{2}/\d{4}.*\d{2}/\d{4}";
        string patronPeriodoL03 = @"Fecha de Control: \d{2]/\d{4}\b";
        string patronTipoModelo = @"L\d{2}.\w*";
        string patronCCC = @"\d{4}\s\d{2}\s?\d{9}";
        string patronCER = "CERTIFICADO DE ESTAR AL CORRIENTE";
        string patronRLC = @"[A-Z]RLC\d{10}\b";
        string patronRNT = "[A-Z]RNT\\d{10}\\b";
        string patronITA = @"INFORME DE TRABAJADORES EN ALTA A FECHA: \d{2}.\d{2}.\d{4}";
        string patronAFIA = "RESOLUCIÓN SOBRE RECONOCIMIENTO DE ALTA";//Documento de afiliacion de alta
        string patronAFIB = "RESOLUCIÓN SOBRE RECONOCIMIENTO DE BAJA";//Documento de afiliacion de baja
        string patronAFIV = ""; //Documento de afiliacion de variacion
        string patronAFIC = ""; //Documento de afiliacion de variacion
        string patronFechaEfecto = @"se indica a continuación: (\S+ \S+ \S+ \S+ \S+)";
        string patronIDC = "Informe de Datos para la Cotización";
        string patronaltaIDC = @"ALTA:\s*\d{2}.\d{2}.\d{4}";
        string patronbajaIDC = @"BAJA:\s*\d{2}.\d{2}.\d{4}";


        #endregion

        public busquedaLaboral(string textoCompleto, List<string> paginasPDF)
        {
            //Construcctor de la clase que inicializa las variables
            Program.textoCompleto = textoCompleto;
            Program.paginasPDF = paginasPDF;
            this.Modelo = string.Empty;
            this.TipoModelo = string.Empty;
            this.Nif = string.Empty;
            this.Ejercicio = string.Empty;
            this.Periodo = string.Empty;
            this.CCC = string.Empty;
            this.DniTrabajador = string.Empty;
            this.FechaEfecto = string.Empty;
            this.AltaIDC = string.Empty;
            this.BajaIDC = string.Empty;
        }

        public void buscarDatos()
        {
            BuscarModelo();

            switch (Modelo)
            {
                case "CER":
                    // Certificado estar al corriente de pago
                    BuscarPeriodoCER();
                    BuscarNif();
                    break;

                case "ITA":
                    //Documento ITA
                    BuscarNif();
                    BuscarCCC();
                    break;

                case "RNT":
                case "RLC":
                    //Documento RNT y RLC
                    BuscarNif();
                    BuscarTipoDocumento();
                    BuscarPeriodoRLCyRNT();
                    BuscarCCC();
                    break;

                case "AFIA":
                case "AFIB":
                    //Documento AFI (alta y baja)
                    BuscarDniTrabajador();
                    BuscarFechaEfecto();
                    BuscarCCC();
                    break;

                case "AFIV":
                    //Documento AFI de variacion (la fecha de alta y de contrato son diferentes)
                    break;

                case "AFIC":
                    //Documento AFI de cambio de contrato (pendiente de ver el documento)
                    break;

                case "IDC":
                    //Documentos IDC
                    //Hay que incluir para la busqueda de los IDC de baja (supongo que seran con la fecha de baja que hay en el documento) y variacion (si la fecha de alta es diferente a la de contrato)
                    BuscarDniTrabajador();
                    BuscarFechaIDC();
                    BuscarCCC();
                    break;
            }
        }


        #region Metodos de busqueda
        private void BuscarModelo()
        {
            try
            {
                AFIA = procesosPDF.ProcesaPatron(patronAFIA, 1);
                if (!string.IsNullOrEmpty(AFIA))
                {
                    Modelo = "AFIA";
                }

                AFIB = procesosPDF.ProcesaPatron(patronAFIB, 1);
                if (!string.IsNullOrEmpty(AFIB))
                {
                    Modelo = "AFIB";
                }

                AFIV = procesosPDF.ProcesaPatron(patronAFIV, 1);
                if (!string.IsNullOrEmpty(AFIV))
                {
                    Modelo = "AFIV";
                }

                AFIC = procesosPDF.ProcesaPatron(patronAFIC, 1);
                if (!string.IsNullOrEmpty(AFIC))
                {
                    Modelo = "AFIC";
                }

                CER = procesosPDF.ProcesaPatron(patronCER, 1);
                if (!string.IsNullOrEmpty(CER))
                {
                    Modelo = "CER";
                }


                RNT = procesosPDF.ProcesaPatron(patronRNT, 1);
                if (!string.IsNullOrEmpty(RNT))
                {
                    Modelo = "RNT";
                }

                RLC = procesosPDF.ProcesaPatron(patronRLC, 1);
                if (!string.IsNullOrEmpty(RLC))
                {
                    Modelo = "RLC";
                }

                ITA = procesosPDF.ProcesaPatron(patronITA, 1);
                if (!string.IsNullOrEmpty(ITA))
                {
                    Modelo = "ITA";
                    Ejercicio = ITA.Substring(ITA.Length - 4);
                    Periodo = ITA.Substring(ITA.Length - 7, 2);
                }

                IDC = procesosPDF.ProcesaPatron(patronIDC, 1);
                if (!string.IsNullOrEmpty(IDC))
                {
                    Modelo = "IDC";
                }
            }
            catch
            {
                //Si no se encuentran los datos, se graba un fichero con el error.
                procesosPDF.grabaFichero("error_proceso.txt", "Documento no reconocido");
            }
        }


        private void BuscarNif()
        {
            Nif = procesosPDF.ProcesaPatron(patronNif, 1);
            if (Nif.Length > 0)
            {
                Nif = Nif.Substring(1, 9);
            }
        }

        private void BuscarDniTrabajador()
        {
            //Busqueda del DNI del trabajador
            DniTrabajador = procesosPDF.ProcesaPatron(patronNif, 1);
            if (DniTrabajador.Length > 0)
            {
                DniTrabajador = DniTrabajador.Substring(1, 9);
            }
        }


        private void BuscarTipoDocumento()
        {
            //Busqueda del tipo de documento. L00 => 00, L13 => 02, L03 => 5, L90 => 90
            string tipoModeloTmp = procesosPDF.ProcesaPatron(patronTipoModelo, 1);
            if (tipoModeloTmp.Length > 0)
            {
                TipoModelo = tipoModeloTmp.Substring(0, 3);
            }
        }

        private void BuscarPeriodoCER()
        {
            //Periodo para el documento de certificado
            string periodoTmp = procesosPDF.ProcesaPatron(patronPeriodo, 1);
            if (periodoTmp.Length > 0)
            {
                Ejercicio = periodoTmp.Substring(periodoTmp.Length - 4);
                Periodo = periodoTmp.Substring(10, 2);
            }
        }

        private void BuscarPeriodoRLCyRNT()
        {
            //Busqueda del periodo en el campo "Periodo liquidacion": el L03 se coje de la fecha de control, el resto del periodo de liquidacion.
            if (TipoModelo == "L03")
            {
                string periodoTmp = procesosPDF.ProcesaPatron(patronPeriodoL03, 1, Modelo, TipoModelo);
                if (periodoTmp.Length > 0)
                {
                    Ejercicio = periodoTmp.Substring(periodoTmp.Length - 4);
                    Periodo = periodoTmp.Substring(periodoTmp.Length - 7, 2);
                }
            }
            else
            {
                string periodoTmp = procesosPDF.ProcesaPatron(patronPeriodoL00, 1, Modelo, TipoModelo) ;
                if (periodoTmp.Length > 0)
                {
                    Ejercicio = periodoTmp.Substring(3, 4);
                    Periodo = periodoTmp.Substring(0, 2);
                }
            }
        }

        private void BuscarFechaIDC()
        {
            string altaIDCTmp = procesosPDF.ProcesaPatron(patronaltaIDC,1);
            if(altaIDCTmp.Length > 0)
            {
                AltaIDC = altaIDCTmp.Substring(altaIDCTmp.Length - 10);
            }

            string bajaIDCTmp = procesosPDF.ProcesaPatron(patronbajaIDC, 1);
            if (bajaIDCTmp.Length > 0)
            {
                BajaIDC = bajaIDCTmp.Substring(altaIDCTmp.Length - 10);
            }
        }


        private void BuscarCCC()
        {
            //Hace falta para los modelos anteriores y en los nuevos por si hay varios centros de trabajo
            string cccTmp = procesosPDF.ProcesaPatron(patronCCC, 1);
            if (cccTmp.Length > 0)
            {
                cccTmp = cccTmp.Replace(" ", "");
                CCC = cccTmp.Substring(cccTmp.Length - 11); //Solo se cogen los ultimos 11 digitos porque en algun modelo el CCC no le ponen los 4 digitos del Regimen de la empresa
            }
        }

        private void BuscarFechaEfecto()
        {
            //Hace falta para los modelos anteriores.
            string fechaEfectoTmp = procesosPDF.ProcesaPatron(patronFechaEfecto, 1);
            if (fechaEfectoTmp.Length > 0)
            {
                fechaEfectoTmp = fechaEfectoTmp.Substring(26, fechaEfectoTmp.Length - 27);
                string formatoFechaTexto = "dd 'de' MMMM 'de' yyyy";
                DateTime fecha = DateTime.ParseExact(fechaEfectoTmp, formatoFechaTexto, System.Globalization.CultureInfo.GetCultureInfo("es-ES")); //Se convierte la fecha en texto a fecha numerica
                FechaEfecto = fecha.ToString("dd.MM.yyyy");//Se convierte la fecha numeria a texto para almacenarla en la variable
            }
        }

        #endregion
    }
}
