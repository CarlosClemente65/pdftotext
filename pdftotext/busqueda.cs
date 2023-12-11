using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace pdftotext
{
    public class busqueda
    {
        private string textoCompleto;
        private List<string> paginasPDF;
        public string Justificante { get; private set; }
        public string Modelo { get; private set; }
        public string Expediente { get; private set; }
        public string Ejercicio { get; private set; }
        public string Csv { get; private set; }
        public string Nif { get; private set; }
        public string Nombre { get; private set; }
        public string Periodo { get; private set; }
        private bool anual;

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
            this.textoCompleto = textoCompleto;
            this.paginasPDF = paginasPDF;
            this.Justificante = string.Empty;
            this.Modelo = string.Empty;
            this.Expediente = string.Empty;
            this.Ejercicio = string.Empty;
            this.Csv = string.Empty;
            this.Nif = string.Empty;
            this.Nombre = string.Empty;
            this.Periodo = string.Empty;
        }

        public void buscarDatos()
        {
            BuscarJustificante();

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
            BuscarNif();
        }

        #region Metodos de busqueda
        private void BuscarJustificante()
        {
            try
            {
                string patronRegex = "\\b[0-9\\d]{13}\\b";
                Justificante = ExtraeTexto(patronRegex, 1, false);
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
                string patronRegex = "20\\d{2}" + Modelo + "[A-Z\\d].*";
                Expediente = ExtraeTexto(patronRegex, 1, false);
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
                string patronRegex = @"20\d{2}\s\d[\d|T]";
                Periodo = ExtraeTexto(patronRegex, 2, false).Substring(5, 2);
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
                string patronRegex = @"(?=.*[A-Z])(?=.*\d)\b(?!20\d{2})[A-Z\d]{16}\b";
                Csv = ExtraeTexto(patronRegex, 1, false);
            }
            catch
            {
                Csv = "No encontrado";
            }
        }

        private void BuscarNif()
        {
            //Busca el NIF seguido del nombre
            try
            {
                string patronRegex = @"(?=.*[A-Z])\b[A-Z0-9]\d{7}[A-Z0-9]\b(?:\s*\S+[ \t]*){0,4}";
                string NifNombre = ExtraeTexto(patronRegex, 2, true);
                NifNombre = NifNombre.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
                Nif = NifNombre.Substring(0, 9);
                Nombre = NifNombre.Substring(10);
            }
            catch
            {
                Nif = "No encontrado";
                Nombre = "No encontrado";
            }
        }

        #endregion

        //Metodo para extraer el texto segun el patron de busqueda pasado
        private string ExtraeTexto(string patronRegex, int pagina, bool nombre)
        {
            Regex regex = new Regex(patronRegex);
            MatchCollection matches = regex.Matches(paginasPDF[pagina - 1].ToString());

            if (matches.Count > 1)
            {
                if (Modelo == "100" && nombre)
                {
                    return matches[1].Value;
                }
                else
                {
                    return matches[0].Value;
                }
            }
            else
            {
                return matches[0].Value;
            }
        }
    }
}
