using System.Text.RegularExpressions;

namespace pdftotext
{
    public class busqueda
    {
        //Definicion de variables necesarias para procesar datos
        private string textoCompleto; //Almacena el texto completo del PDF
        private List<string> paginasPDF; //Almacena cada una de las paginas del PDF

        //Definicion de variables a nivel de clase con sus metodos get y set
        public string Justificante { get; private set; }
        public string Modelo { get; private set; }
        public string Expediente { get; private set; }
        public string Ejercicio { get; private set; }
        public string Csv { get; private set; }
        public string Nif { get; private set; }
        public string NifConyuge { get; private set; } //Necesario para la renta
        public bool TributacionConjunta { get; private set; } //Necesario para la renta
        public string Periodo { get; private set; }
        public string fecha036 { get; private set; }
        public bool complementaria { get; private set; }

        #region patrones de busqueda
        //Definicion de patrones de busqueda de datos
        private string patronJustificante = @"\b[0-9\d]{13}\b";

        private string patronExpediente = string.Empty; //Este patron se define en el metodo porque cambia si el modelo es el 036
        
        private string patronPeriodo = string.Empty; //Este patron se define en el metodo porque cambia segun el modelo

        //El csv siempre es una cadena de 16 caracteres formada por letras mayusculas y numeros, y se excluyen las cadenas que tengan un numero de 4 digitos que empieze por 20 para evitar que se confunda con el expediente que puede tener la misma longitud
        //string patronRegex = @"(?=.*[A-Z])(?=.*\d)\b(?!20\d{2})[A-Z\d]{16}\b"; //Esta expresion no localiza siempre el numero, con la siguiente si que suele acertar
        string patronCsv = @"Verificación(\s|:\s)[A-Z\d]{16}\b";

        //El NIF suele estar seguido del nombre y se busca siempre en la pagina 2 porque en la pagina 1 esta el presentador que puede ser distinto. La expresion se asegura que haya alguna letra mayuscula, la primera palabra debe ser el NIF (empieza por letra o numero, le siguen 7 numeros y termina con letra o numero), y la segunda parte busca hasta un total de 4 palabras que se supone que es el nombre; en todo caso el nombre luego no se usa.
        string patronNif = @"(?=.*[A-Z])\b[A-Z0-9]\d{7}[A-Z0-9]\b(?:\s*\S+[ \t]*){0,4}";

        //Busca si se trata de una renta conjunta en todo el texto ya que en la pagina 2 solo estan las equis de las casillas pero no se puede saber cual corresponde, por eso se busca el texto de la casilla 0461 en la liquidacion y si existe es porque ha aplicado la reduccion y por lo tanto es conjunta.
        string patronRentaConjunta = "Reducción por tributación conjunta";

        string patronNifRenta = @"(?=.*[A-Z])\b[A-Z0-9]\d{7}[A-Z0-9]\b(?:\s*\S+[ \t]*){0,4}";

        string patronFecha036 = @"[0-9]{2}.[0-9]{2}.[0-9]{4} a las [0-9]{2}.[0-9]{2}.[0-9]{2}";

        #endregion


       

        //Lista de modelos anuales
        List<string> modelosAnuales = new List<string>
        {
            "036",
            "100",
            "180",
            "182",
            "184",
            "187",
            "190",
            "193",
            "200",
            "232",
            "296",
            "347",
            "390",
            "714",
            "720",
            "840"
        };

        //Lista de periodos validos
        List<string> periodosValidos = new List<string>
        {
            "1T",
            "2T",
            "3T",
            "4T",
            "01",
            "02",
            "03",
            "04",
            "05",
            "06",
            "07",
            "08",
            "09",
            "10",
            "11",
            "12",
            "0A",
            "1P",
            "2P",
            "3P"
        };

        //Lista de modelos validos
        List<string> modelosValidos = new List<string>
        {
            "036",
            "037",
            "100",
            "102",
            "111",
            "115",
            "123",
            "130",
            "131",
            "180",
            "182",
            "184",
            "187",
            "190",
            "193",
            "200",
            "202",
            "210",
            "211",
            "216",
            "218",
            "232",
            "296",
            "303",
            "309",
            "322",
            "347",
            "349",
            "353",
            "390",
            "569",
            "583",
            "714",
            "720",
            "840"
        };


        public busqueda(string textoCompleto, List<string> paginasPDF)
        {
            //Construcctor de la clase que inicializa las variables
            this.textoCompleto = textoCompleto;
            this.paginasPDF = paginasPDF;
            this.Justificante = string.Empty;
            this.Expediente = string.Empty;
            this.Csv = string.Empty;
            this.Modelo = string.Empty;
            this.Ejercicio = string.Empty;
            this.Periodo = string.Empty;
            this.Nif = string.Empty;
            this.NifConyuge = string.Empty;
            this.TributacionConjunta = false;
        }

        public void buscarDatos()
        {
            BuscarJustificante();

            //El modelo siempre son los 3 primeros digitos del justificante
            Modelo = Justificante.Substring(0, 3);
            switch (Modelo)
            {
                case "890":
                case "893":
                    Modelo = "840";
                    break;
            }

            //Valida que el modelo encontrado esta en la lista de modelos validos
            if (!modelosValidos.Contains(Modelo))
            {
                Modelo = "Modelo no encontrado";
                Periodo = "Periodo no encontrado";
            }
            else
            {
                //Si se trata de un modelo anual fija el periodo a 0A y sino se busca
                if (modelosAnuales.Contains(Modelo))
                {
                    Periodo = "0A";
                }
                else
                {
                    BuscarPeriodo();
                }
            }

            BuscarExpediente();
            BuscarCsv();
            BuscarComplementaria();

            if (Modelo == "036" || Modelo == "037")
            {
                BuscarFecha036();
            }

            //Si se trata de la renta, hay que buscar el tipo de tributacion y ademas devolver el NIF del conyuge
            if (Modelo == "100")
            {
                BuscarDatosRenta();
            }
            else
            {
                BuscarNif();
            }
        }

        #region Metodos de busqueda
        private void BuscarJustificante()
        {
            try
            {
                //El justificante siempre es una cadena de 13 numeros
                Justificante = ExtraeTexto(patronJustificante, 1);
            }
            catch
            {
                Justificante = "No encontrado";
            }
        }

        private void BuscarExpediente()
        {
            try
            {
                //El expediente es una cadena que empieza por el año, le sigue el modelo y el resto son letras mayusculas o numeros hasta el final de la linea, por eso se asigna al ejercicio los 4 primeros digitos

                //En el modelo 036 el codigo de modelo del expediente es C36
                if (Modelo == "036")
                {
                    string modelo036 = "C36";
                    patronExpediente = "20\\d{2}" + modelo036 + "[A-Z\\d].*";
                }
                else
                {
                    patronExpediente = "20\\d{2}" + Modelo + "[A-Z\\d].*";
                }

                //string parametroExpediente = "20\\d{2}" + modelo036 + "[A-Z\\d].*";
                
                Expediente = ExtraeTexto(patronExpediente, 1);
                Ejercicio = Expediente.Substring(0, 4);
            }
            catch
            {
                Expediente = "No encontrado";
                Ejercicio = "No encontrado";
            }
        }

        private void BuscarPeriodo()
        {
            try
            {
                //El periodo suele estar siempre a continuacion del ejecicio y puede estar en la misma linea o en la siguiente.
                //string patronRegex = @"\b20\d{2}[\s]\d[\d|T]"; Modificada por la siguiente expresion ya que esta no localiza el periodo si esta en una segunda linea

                
                switch (Modelo)
                {
                    //El modelo 210 el periodo viene antes del año
                    case "210":
                        patronPeriodo = @"\b(\d[0-9A-Z])(\s)20\d{2}\b";
                        Periodo = ExtraeTexto(patronPeriodo, 2).Substring(0, 2);
                        break;

                    //El modelo 349 el periodo va detras del justificante
                    case "349":
                        patronPeriodo = @"\b[0-9\d]{13}\b\s\d[0-9A-Z]\b";
                        Periodo = ExtraeTexto(patronPeriodo, 2);
                        Periodo = Periodo.Substring(Periodo.Length - 2);
                        break;

                    default:
                        patronPeriodo = @"\b20\d{2}(\s)(\d[0-9A-Z])\b";
                        Periodo = ExtraeTexto(patronPeriodo, 2).Substring(5, 2);
                        break;

                }

                if (!periodosValidos.Contains(Periodo))
                {
                    Periodo = "Periodo no encontrado";
                }
            }
            catch
            {
                Periodo = "Periodo no encontrado";
            }
        }

        private void BuscarCsv()
        {
            try
            {
                Csv = ExtraeTexto(patronCsv, 1);
                Csv = Csv.Substring(Csv.Length - 16, 16);
            }
            catch
            {
                Csv = "No encontrado";
            }
        }

        private void BuscarNif()
        {
            //Busca el NIF siempre que no se trate de una renta
            try
            {
                string NifNombre = ExtraeTexto(patronNif, 2);
                NifNombre = NifNombre.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
                Nif = NifNombre.Substring(0, 9);
            }
            catch
            {
                Nif = "No encontrado";
            }
        }

        private void BuscarDatosRenta()
        {
            try
            {
                Regex regex;
                MatchCollection matches;
                regex = new Regex(patronRentaConjunta);
                matches = regex.Matches(textoCompleto);
                if (matches.Count > 0)
                {
                    TributacionConjunta = true;
                }

                //En la renta el NIF del titular aparece en segundo lugar, y el del conyuge en primer lugar, por eso si la renta es conjunta se devuelve el segundo NIF encontrado (indice 1) para el titular y el del conyuge es el primero (indice 0)
                regex = new Regex(patronNifRenta);
                matches = regex.Matches(paginasPDF[1]);

                //NIF del titular
                Nif = matches[1].Value.Substring(0, 9);

                //Si la tributacion es conjunta se almacena el Nif del conyuge
                if (TributacionConjunta)
                {
                    NifConyuge = matches[0].Value.Substring(0, 9);
                }
            }
            catch
            {
                Nif = "No encontrado";
            }
        }

        private void BuscarFecha036()
        {
            //Busca la fecha de presentacion del modelo 036/037
            try
            {
                
                fecha036 = ExtraeTexto(patronFecha036, 1);
                fecha036 = fecha036.Substring(0, 10);
            }
            catch
            {
                fecha036 = "Fecha presentacion no encontrada";
            }

        }

        private void BuscarComplementaria()
        {
            try
            {
                string justificanteComplementaria = string.Empty;
                Regex regex;
                MatchCollection matches;
                //El patron es igual que el de la busqueda del justificante
                regex = new Regex(patronJustificante);
                matches = regex.Matches(textoCompleto.ToString());

                switch (Modelo)
                {
                    //En el modelo 200 el justificante de la complementaria aparece en segunda posicion (indice 1)
                    case "200":
                        if (matches.Count > 0)
                        {
                            justificanteComplementaria = matches[1].Value;
                        }
                        break;

                    //En el resto de modelos el justificante de la complementaria aparece en tercera posicion (indice 2)
                    default:
                        if (matches.Count > 1)
                        {
                            justificanteComplementaria = matches[2].Value;
                        }
                        break;
                }

                //No es posible que el justificante de la complementaria sea igual al de la declaracion, por lo tanto ha debido haber un error al buscarlo y se deja vacio
                if (justificanteComplementaria != Justificante)
                {
                    complementaria = true;
                }
            }
            catch
            {
                //Si hay algun error y no se encuentra la complementaria no hacemos nada.
            }
        }

        #endregion

        private string ExtraeTexto(string patronRegex, int pagina)
        {
            //Metodo para extraer el texto segun el patron de busqueda pasado
            Regex regex = new Regex(patronRegex);
            MatchCollection matches = regex.Matches(paginasPDF[pagina - 1].ToString());

            if (matches.Count > 0)
            {
                return matches[0].Value;
            }
            else
            {
                return matches[0].Value;
            }
        }
    }
}
