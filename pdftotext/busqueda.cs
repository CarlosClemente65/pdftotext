using System.Text.RegularExpressions;

namespace pdftotext
{
    public class busqueda
    {
        //Definicion de variables a nivel de clase con sus metodos get y set
        private string textoCompleto; //Almacena el texto completo del PDF
        private List<string> paginasPDF; //Almacena cada una de las paginas del PDF
        public string Justificante { get; private set; }
        public string Modelo { get; private set; }
        public string Expediente { get; private set; }
        public string Ejercicio { get; private set; }
        public string Csv { get; private set; }
        public string Nombre { get; private set; } //Necesario para que no de error la clase procesos_anterior
        public string Nif { get; private set; }
        public string NifConyuge { get; private set; } //Necesario para la renta
        public bool TributacionConjunta { get; private set; } //Necesario para la renta
        public string Periodo { get; private set; }
        //private bool anual; //Determina si se trata de un modelo anual

        //Lista de modelos anuales
        List<string> modelosAnuales = new List<string>
        {
        "100",
        "180",
        "182",
        "184",
        "187",
        "190",
        "193",
        "200",
        "296",
        "347",
        "390",
        "714"
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
            "216",
            "232",
            "296",
            "303",
            "309",
            "322",
            "347",
            "349",
            "353",
            "390",
            "583",
            "714",
            "720"
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

            //Si se trata de la renta, hay que buscar el tipo de tributacion y ademas devolver el NIF del conyuge
            if (Modelo == "100")
            {
                buscarDatosRenta();
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
                string patronRegex = "\\b[0-9\\d]{13}\\b";
                Justificante = ExtraeTexto(patronRegex, 1);
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
                string patronRegex = "20\\d{2}" + Modelo + "[A-Z\\d].*";
                Expediente = ExtraeTexto(patronRegex, 1);
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
                string patronRegex = @"20\d{2}\s\d[\d|T]";
                Periodo = ExtraeTexto(patronRegex, 2).Substring(5, 2);
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
                //El csv siempre es una cadena de 16 caracteres formada por letras mayusculas y numeros, y se excluyen las cadenas que tengan un numero de 4 digitos que empieze por 20 para evitar que se confunda con el expediente que puede tener la misma longitud
                string patronRegex = @"(?=.*[A-Z])(?=.*\d)\b(?!20\d{2})[A-Z\d]{16}\b";
                Csv = ExtraeTexto(patronRegex, 1);
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
                //El NIF suele estar seguido del nombre y se busca siempre en la pagina 2 porque en la pagina 1 esta el presentador que puede ser distinto. La expresion se asegura que haya alguna letra mayuscula, la primera palabra debe ser el NIF (empieza por letra o numero, le siguen 7 numeros y termina con letra o numero), y la segunda parte busca hasta un total de 4 palabras que se supone que es el nombre; en todo caso el nombre luego no se usa.
                string patronRegex = @"(?=.*[A-Z])\b[A-Z0-9]\d{7}[A-Z0-9]\b(?:\s*\S+[ \t]*){0,4}";
                string NifNombre = ExtraeTexto(patronRegex, 2);
                NifNombre = NifNombre.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
                Nif = NifNombre.Substring(0, 9);
            }
            catch
            {
                Nif = "No encontrado";
            }
        }

        private void buscarDatosRenta()
        {
            try
            {
                string patronRegex = string.Empty;
                Regex regex;
                MatchCollection matches;

                //Busca si se trata de una renta conjunta en todo el texto ya que en la pagina 2 solo hay las equis de las casillas pero no se puede saber cual corresponde, por eso se busca el texto de la casilla 0461 en la liquidacion y si existe es porque ha aplicado la reduccion y por lo tanto es conjunta.
                patronRegex = "Reducción por tributación conjunta";
                regex = new Regex(patronRegex);
                matches = regex.Matches(textoCompleto);
                if (matches.Count > 0)
                {
                    TributacionConjunta = true;
                }

                //Busca el NIF y nombre del mismo modo que se hace en el metodo BuscarNif (las paginas en la lista comienzan desde la 0 por eso se coje la 1 que realmente corresponde a la 2
                patronRegex = @"(?=.*[A-Z])\b[A-Z0-9]\d{7}[A-Z0-9]\b(?:\s*\S+[ \t]*){0,4}";
                regex = new Regex(patronRegex);
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
