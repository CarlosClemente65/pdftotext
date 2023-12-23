using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text;
using Microsoft.SqlServer.Server;
using System.Text.RegularExpressions;

namespace pdftotext
{
    internal class procesosPDF
    {
        //Se definen las variables a nivel de clase para que esten accesibles

        //Variables para la gestion de ficheros
        public string ficheroPDF; //Fichero PDF a procesar
        public string ficheroDatos; //Fichero de salida con los datos del modelo
        public string ficheroTexto; //Fichero de salida con el texto completo

        //Variables para la gestion de parametros
        public bool procesaModelo = false; //Si se pasa el parametro -m se procesan los datos del modelo
        public bool extraeTexto = false; //Si se pasa el parametro -t se graba el texto completo en un fichero
        public bool procesaLaboral = false;//Si se pasa el parametro -l se procesan los documentos laborales 
        //Añadir nuevas variables para otros usos en el futuro

        //Variables para la gestion de paginas del PDF 
        public int firstpage = 1;
        public int lastpage = 1;

        //Variables para la extraccion de los datos del PDF
        StringBuilder extractedText = new StringBuilder();

        
        public void extraeTextoPDF()
        {
            using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(ficheroPDF)))
            {
                lastpage = pdfDoc.GetNumberOfPages();

                //Extrae el texto de las paginas del PDF y lo almacena en la variable extractedText
                for (int i = firstpage; i <= lastpage; i++)
                {
                    ITextExtractionStrategy extractionStrategy = new SimpleTextExtractionStrategy();
                    string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), extractionStrategy);

                    //Se añade el texto de la pagina a la variable paginasPDF que es una lista con las paginas por separado
                    Program.paginasPDF.Add(pageText);

                    //Se añade el texto de la pagina a la variable extractedText que es un string con todo el texto del PDF
                    extractedText.Append(pageText);
                }

                //Se almacena el texto extraido para poder procesarlo
                Program.textoCompleto = extractedText.ToString().Trim();
            }
        }

        public void extraeDatosModelo()
        {
            //Instanciacion de la clase busqueda para usar los metodos
            busquedaModelo buscar = new busquedaModelo(Program.textoCompleto, Program.paginasPDF);

            //Hacemos la llamada al metodo buscarDatos para que se almacenen los datos del PDF
            buscar.buscarDatos();

            //Almacena el texto a grabar en el fichero
            string texto = string.Empty;
            texto += ($"NIF: {buscar.Nif} \n");
            if (buscar.NifConyuge != "")
            {
                texto += ($"NIF conyuge: {buscar.NifConyuge} \n");
            }
            texto += ($"Modelo: {buscar.Modelo} \n");
            texto += ($"Ejercicio: {buscar.Ejercicio} \n");
            texto += ($"Periodo: {buscar.Periodo} \n");
            if (buscar.Modelo == "100")
            {
                if (buscar.TributacionConjunta)
                {
                    texto += ("Tributacion: conjunta \n");
                }
                else
                {
                    texto += ("Tributacion: individual \n");
                }
            }
            texto += ($"Justificante: {buscar.Justificante} \n");
            texto += ($"CSV: {buscar.Csv} \n");
            texto += ($"Expediente: {buscar.Expediente} \n");
            if (!string.IsNullOrEmpty(buscar.fecha036))
            {
                texto += ($"Fecha presentacion 036/037: {buscar.fecha036} \n");
            }
            if (buscar.complementaria)
            {
                texto += ($"Complementaria: SI \n");
            }

            //Graba el fichero de datos con el texto creado
            grabaFichero(ficheroDatos, texto);
        }

        public void extraeDatosLaboral()
        {
            //Instanciacion de la clase busqueda para usar los metodos
            busquedaLaboral buscar = new busquedaLaboral(Program.textoCompleto, Program.paginasPDF);

            //Hacemos la llamada al metodo buscarDatos para que se almacenen los datos del PDF
            buscar.buscarDatos();

            //Almacena el texto a grabar en el fichero
            string texto = string.Empty;

            switch (buscar.Modelo)
            {
                case "CER":
                    texto += $"Modelo: {buscar.Modelo} \n";
                    texto += $"CIF: {buscar.Nif} \n";
                    texto += $"Ejercicio: {buscar.Ejercicio} \n";
                    texto += $"Periodo: {buscar.Periodo} \n";
                    break;

                case "RLC":
                case "RNT":
                    texto += $"Modelo: {buscar.Modelo} \n";
                    texto += $"Tipo de modelo: {buscar.TipoModelo}\n";
                    texto += $"CIF: {buscar.Nif} \n";
                    texto += $"Ejercicio: {buscar.Ejercicio} \n";
                    texto += $"Periodo: {buscar.Periodo} \n";
                    texto += $"Codigo cuenta cotizacion: {buscar.CCC}\n";

                    break;

                case "AFIA":
                    texto += $"Modelo: AFI (alta) \n";
                    texto += $"Codigo cuenta cotizacion: {buscar.CCC}\n";
                    texto += $"NIF trabajador: {buscar.DniTrabajador}\n";
                    texto += $"Fecha efecto: {buscar.FechaEfecto} \n";
                    break;

                case "AFIB":
                    texto += $"Modelo: AFI (baja) \n";
                    texto += $"Codigo cuenta cotizacion: {buscar.CCC}\n";
                    texto += $"NIF trabajador: {buscar.DniTrabajador}\n";
                    texto += $"Fecha efecto: {buscar.FechaEfecto} \n";
                    break;

                case "IDC":
                    if (buscar.BajaIDC.Length > 0)
                    {
                        texto += $"Modelo: IDC (baja) \n";
                    }
                    else
                    {
                        texto += $"Modelo: IDC (alta) \n";
                    }
                    texto += $"Codigo cuenta cotizacion: {buscar.CCC}\n";
                    texto += $"NIF trabajador: {buscar.DniTrabajador}\n";
                    texto += $"Fecha efecto: {buscar.AltaIDC} \n";
                    break;

                case "ITA":
                    texto += $"Modelo: {buscar.Modelo} \n";
                    texto += $"CIF: {buscar.Nif} \n";
                    texto += $"Ejercicio: {buscar.Ejercicio} \n";
                    texto += $"Periodo: {buscar.Periodo} \n";
                    texto += $"Codigo cuenta cotizacion: {buscar.CCC}\n";
                    break;

            }

            //Graba el fichero de datos con el texto creado
            grabaFichero(ficheroDatos, texto);

        }

        public string ProcesaPatron(string patronRegex, int pagina, string Modelo="")
        {

            //Metodo para extraer el texto segun el patron de busqueda pasado
            Regex regex = new Regex(patronRegex);
            MatchCollection matches = regex.Matches(Program.paginasPDF[pagina - 1].ToString());

            //Si encuentra algo 
            if (matches.Count > 0)
            {
                if (Modelo == "RLC")
                {
                    return matches[1].Value;
                }
                return matches[0].Value;
            }
            else
            {
                return "";
            }
        }

        public void grabaFichero(string pathFichero, string texto)
        {
            File.WriteAllText(pathFichero, texto);
        }
    }
}
