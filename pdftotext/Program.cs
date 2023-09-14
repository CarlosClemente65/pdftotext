using System;
using System.IO;
using System.Text;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Org.BouncyCastle.Bcpg;

namespace pdftotext
{

    public class inicializaVariables
    {
        public string textoCsv;
        public int indexCsv;
        public bool foundCsv;
        public string Csv;
        public string justificante;
        public void InicializarCSV()
        {
            textoCsv = "";
            indexCsv = 1;
            foundCsv = false;
            Csv = "";
            justificante = "";
        }
        public void InicializarJustificante()
        {

        }
    }
    class Program
    {
        public static void MensajeParametros(string mensaje)
        {
            Console.Clear();
            //Console.WriteLine();
            Console.WriteLine(mensaje);
            Console.WriteLine("\nUso:\tpdftotext fichero.pdf [-f | -m] [-l procesa todos los ficheros .pdf de la carpeta] [-p pagina o intervalo de paginas separadas por un guion]");
            Console.WriteLine("\t-l   Si es el primer parametro, se procesan todos los ficheros .PDF de la carpeta");
            Console.WriteLine("\t-f   Genera texto completo del PDF (si se omite sera la opcion por defecto)");
            Console.WriteLine("\t-m   Genera un fichero con los datos principales del modelo de Hacienda");
            Console.WriteLine("\t     Las opciones -f y -m son incompatibles entre si");
            Console.WriteLine("\t-p   Especifica la pagina o intervalo de paginas a extraer:");
            Console.WriteLine("\t     3 = pagina 3, 2-4 = paginas 2 a 4");
            Console.WriteLine("");
            Console.WriteLine("Pulse una tecla para continuar...");
            Console.ReadKey();
        }
    
        static void Main(string[] args)
        {
            bool debug = true;
            string filePath = "";
            string[] filesPDF = new string[0];
            string opcion = "-f";
            int firstpage=0;
            int lastpage=0;
            bool fOption = false;
            bool mOption = false;
            bool parametroOk = false;
            //Si no se pasan parametros muestra un error
            if (args.Length == 0)
            {
                MensajeParametros("\nDebe proporcionar el nombre del fichero a tratar o el parametro -l para procesar toda la carpeta");
                return;
            }
            else
            {
                //Si el argumento es -l almacena el nombre de todos los PDF de la carpeta
                if (args[0] == "-l")
                {
                    parametroOk = true;
                    opcion = "-l";
                    if (debug)
                    {
                        //Ruta fija para las pruebas
                        string folderpath = @"c:\borrar\pdftotext";
                        filesPDF = Directory.GetFiles(folderpath, "*.pdf");
                    }
                    else
                    {
                        filesPDF = Directory.GetFiles(".", "*.pdf");
                    }
                                        
                    //Mensaje si no hay ningun fichero PDF
                    if (filesPDF.Length == 0) 
                    {
                        Console.WriteLine("No hay ningun fichero PDF para procesar");
                        return;
                    }
                }
                else
                {
                    //Almacena el nombre del fichero a procesar (primer parametro)
                    filePath = args[0];
                    if (!filePath.EndsWith(".pdf"))
                    {
                        filePath += ".pdf";
                    }
                    //Chequea que el fichero pasado existe
                    if (!File.Exists(filePath))
                    {
                        Console.Clear();
                        Console.WriteLine($"El fichero {filePath}.pdf no existe");
                        Console.WriteLine("Pulse una tecla para salir");
                        Console.ReadKey();
                        return;
                    }
                    //Chequeo que el fichero pasado es un PDF
                    if (Path.GetExtension(filePath) != ".pdf")
                    {
                        Console.Clear();
                        Console.WriteLine("El archivo debe ser de tipo PDF");
                        Console.WriteLine("Pulse una tecla para salir");
                        Console.ReadKey();
                        return;
                    }
                }
                //Proceso de los argumentos que se pasan. La variable parametroOk sirve para controlar que se ha puesto al menos un parametro correcto
                for (int i = 1; i < args.Length; i++)
                {
                    string arg = args[i].ToLower();
                    switch(arg)
                    {
                        case "-f":
                            //Las variables fOption y mOption sirven para chequear que no se pasan los dos parametros
                            if (mOption)
                            {
                                MensajeParametros("Las opciones -f y -m son incompatibles entre si");
                            }
                            fOption = true;
                            parametroOk = true;
                            break;

                        case "-m":
                            if (fOption)
                            {
                                MensajeParametros("Las opciones -f y -m son incompatibles entre si");
                            }
                            opcion ="-m";
                            mOption = true;
                            parametroOk = true;
                            break;

                        case "-p":
                            //Control de que se pasan las paginas inicio y fin separadas con un guion
                            int pageIndex;
                            if (args.Length > Array.IndexOf(args, arg) + 1 && int.TryParse(args[Array.IndexOf(args, arg) + 1], out pageIndex))
                            {
                                if (pageIndex < 0)
                                {
                                    MensajeParametros("No se puede indicar un numero de pagina negativo");
                                    return;
                                }
                                firstpage = lastpage = pageIndex;
                                parametroOk = true;
                            }
                            else if (args.Length > Array.IndexOf(args, arg) + 1)
                            {
                                string[] rangePages = args[Array.IndexOf(args, arg) + 1].Split('-');
                                if (rangePages.Length == 1)
                                {
                                    if (!int.TryParse(rangePages[0], out lastpage) || lastpage <1)
                                    {
                                        MensajeParametros($"El argumento {arg} debe estar seguido de un numero entero positivo");
                                        return;
                                    }
                                    firstpage = lastpage;
                                }
                                else if (rangePages.Length == 2)
                                {
                                    if (!int.TryParse(rangePages[0], out firstpage)|| firstpage <1)
                                    {
                                        MensajeParametros($"El argumento {arg} debe estar seguido de dos números enteros positivos separados por un guión (-)");
                                        return;
                                    }
                                    if (!int.TryParse(rangePages[1], out lastpage)|| lastpage <1)
                                    {
                                        MensajeParametros($"El argumento {arg} debe estar seguido de dos números enteros positivos separados por un guión (-)");
                                        return;
                                    }
                                }
                                else
                                {
                                    MensajeParametros($"El argumento {arg} debe estar seguido de dos números enteros positivos separados por un guión (-) o de un número entero positivo.");
                                    return;
                                }
                                parametroOk = true;
                            }
                            else
                            {
                                MensajeParametros($"El argumento {arg} debe estar seguido de dos números enteros positivos separados por un guión (-) o de un número entero positivo.");
                                return;
                            }
                            break;
                    }
                }
                if (!parametroOk)
                {
                    MensajeParametros("Algún parametro es incorrecto");
                    return;
                }
            }
            //Si el argumento no es procesar todos los ficheros, se almacena el fichero pasado como parametro en la variable que procesa el array de ficheros
            if (args[0] != "-l")
            {
                filesPDF[0] = filePath;
            }
            //Proceso de todos los ficheros (en el paso anterior se ha podido almacenar el unico fichero pasado como parametro
            foreach (string archivo in filesPDF)
            {
                filePath = archivo;
                //Se asigna al fichero de salida el mismo nombre con extension .txt
                string outputFilePath = System.IO.Path.ChangeExtension(filePath, "txt");
                using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(filePath)))
                {
                    SimpleTextExtractionStrategy extractionStrategy = new SimpleTextExtractionStrategy();
                    //Almacena el contenido del PDF
                    StringBuilder extractedText = new StringBuilder();
                    //Controla si los numeros de pagina indicados estan en el intervalo de paginas del fichero
                    if (firstpage > pdfDoc.GetNumberOfPages() || lastpage > pdfDoc.GetNumberOfPages())
                    {
                        Console.Clear();
                        Console.WriteLine($"El intervalo de paginas {firstpage}-{lastpage} indicado no existe en el fichero {filePath}");
                        Console.WriteLine("\nPulse una tecla para continuar...");
                        Console.ReadLine();
                        return;
                    }
                    //Si no se pasa la primera pagina se asigna la 1
                    if (firstpage == 0)
                    {
                        firstpage = 1;
                    }
                    //Si no se pasa la ultima pagina, se asigna el total de paginas del fichero
                    if (lastpage == 0)
                    {
                        lastpage = pdfDoc.GetNumberOfPages();
                    }
                    for (int i = firstpage; i <= lastpage; i++)
                    { 
                        if (debug == true)
                        {
                            int lineNumber = 1;
                            //Extrae el texto de la pagina y le añade el numero de linea
                            string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), extractionStrategy);
                            foreach (string line in pageText.Split('\n'))
                            {
                                extractedText.AppendLine($"Linea {lineNumber++}: {line.Trim()}"); // Agrega el número de línea al principio de cada línea
                            }
                        }
                        else
                        {
                            //Extrae el texto completo de la pagina
                            extractedText.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), extractionStrategy));
                        }
                }
                    //Pone a cero las paginas por si se procesan varios ficheros
                    firstpage = 0;
                    lastpage = 0;

                    //Graba el fichero completo si el parametro es -f o -l
                    if (opcion == "-f" || opcion =="-l")
                    {
                        File.WriteAllText(outputFilePath, extractedText.ToString());
                    }
                    //Localizar y grabar los valores individuales en el fichero
                    else if (opcion == "-m")
                    {
                        File.WriteAllText(outputFilePath, extractedText.ToString());
                        inicializaVariables variables = new inicializaVariables();
                        using (StreamWriter writer = new StreamWriter(outputFilePath))
                        {
                            string textoCompleto = extractedText.ToString().Trim();

                            //Extraer el CSV
                            string textoCsv = "Verificación ";
                            int indexCsv = textoCompleto.IndexOf(textoCsv);
                            bool foundCsv = false;
                            string Csv = "";
                            while (indexCsv >= 0 && !foundCsv)
                            {
                                int inicioCsv = indexCsv + textoCsv.Length;
                                int largoCsv = textoCompleto.IndexOf("\n", inicioCsv) - inicioCsv;
                                if (largoCsv <= 0)
                                {
                                    largoCsv = textoCompleto.Length - inicioCsv;
                                }
                                Csv = textoCompleto.Substring(inicioCsv, 16).Trim();
                                if (Csv.Length > 0)
                                {
                                    foundCsv = true;
                                    writer.WriteLine($"CSV: {Csv}");
                                }
                                indexCsv = textoCompleto.IndexOf(textoCsv, indexCsv + 1);
                            }
                            if (foundCsv == false)
                            {
                                writer.WriteLine($"CSV no encontrado");
                            }
                            variables.InicializarCSV();

                            // Extraer el expediente
                            string Expediente = "";
                            string textoExpediente = Csv;
                            int indexExpediente = textoCompleto.IndexOf(textoExpediente);
                            bool foundExpediente = false;
                            while (indexExpediente >= 0 && !foundExpediente)
                            {
                                int inicioExpediente = indexExpediente - textoExpediente.Length - 1;
                                int largoExpediente = textoCompleto.IndexOf("\n", inicioExpediente) - inicioExpediente;
                                if (largoExpediente <= 0)
                                {
                                    largoExpediente = textoCompleto.Length - inicioExpediente;
                                }
                                Expediente = textoCompleto.Substring(inicioExpediente, 16).Trim();
                                if (Expediente.Length > 0)
                                {
                                    foundExpediente = true;
                                    writer.WriteLine($"Expediente: {Expediente}");
                                }
                                indexExpediente = textoCompleto.IndexOf(textoExpediente, indexExpediente + 1);
                            }
                            if (foundExpediente == false)
                            {
                                writer.WriteLine($"Expediente no encontrado");
                            }
                            variables.InicializarCSV();

                            //Extraer el numero de justificante
                            string justificante = "";
                            string textoJustificante = "justificante: ";
                            int indexJustificante = textoCompleto.IndexOf(textoJustificante);
                            bool foundJustificante = false;
                            while (indexJustificante >= 0 && !foundJustificante)
                            {
                                int inicioJustificante = indexJustificante + textoJustificante.Length;
                                int largoJustificante = textoCompleto.IndexOf("\n", inicioJustificante) - inicioJustificante;
                                if (largoJustificante <= 0)
                                {
                                    largoJustificante = textoCompleto.Length - inicioJustificante;
                                }
                                justificante = textoCompleto.Substring(inicioJustificante, largoJustificante).Trim();
                                if (justificante.Length > 0)
                                {
                                    foundJustificante = true;
                                    writer.WriteLine($"Justificante: {justificante}");
                                }
                                indexJustificante = textoCompleto.IndexOf(textoJustificante, indexJustificante + 1);
                            }

                            //Extraer NIF
                            string Nif = "";
                            string textoNif = Csv;
                            int indexNif = textoCompleto.IndexOf(textoNif);
                            bool foundNif = false;
                            while (indexNif >= 0 && !foundNif)
                            {
                                int inicioNif = indexNif + textoNif.Length + 1;
                                int largoNif = textoCompleto.IndexOf("\n", inicioNif) - inicioNif;
                                if (largoNif <= 0)
                                {
                                    largoNif = textoCompleto.Length - inicioNif;
                                }
                                Nif = textoCompleto.Substring(inicioNif, largoNif);
                                if (Nif.Length > 0)
                                {
                                    foundNif = true;
                                    writer.WriteLine($"NIF: {Nif}");
                                }
                                indexNif = textoCompleto.IndexOf(textoNif, indexNif + 1);
                            }
                            if (foundNif == false)
                            {
                                writer.WriteLine($"NIF no encontrado");
                            }

                            //Extraer Nombre
                            string Nombre = "";
                            string textoNombre = Nif;
                            int indexNombre = textoCompleto.IndexOf(textoNombre);
                            bool foundNombre = false;
                            while (indexNombre >= 0 && !foundNombre)
                            {
                                int inicioNombre = indexNombre + textoNombre.Length + 1;
                                int largoNombre = textoCompleto.IndexOf("\n", inicioNombre) - inicioNombre;
                                if (largoNombre <= 0)
                                {
                                    largoNombre = textoCompleto.Length - inicioNombre;
                                }
                                Nombre = textoCompleto.Substring(inicioNombre, largoNombre);
                                if (Nombre.Length > 0)
                                {
                                    foundNombre = true;
                                    writer.WriteLine($"Nombre: {Nombre}");
                                }
                                indexNombre = textoCompleto.IndexOf(textoNombre, indexNombre + 1);
                            }
                            if (foundNombre == false)
                            {
                                writer.WriteLine($"Nombre no encontrado");
                            }

                            //Extraer ejercicio
                            string ejercicio = Expediente.Substring(0, 4);
                            writer.WriteLine($"Ejercicio: {ejercicio}");

                            //Extraer periodo
                            string periodo = "";


                            //Extraer modelo
                            string modelo = Expediente.Substring(4, 3);
                            //string modelo = "";
                            //string textoModelo= "justificante:";
                            //int indexModelo= textoCompleto.IndexOf(textoModelo);
                            //bool foundModelo = false;
                            //while (indexModelo>= 0 && !foundModelo)
                            //{
                            //    int inicioModelo = indexModelo+ textoModelo.Length;
                            //    int largoModelo = textoCompleto.IndexOf("\n", inicioModelo) - inicioModelo;
                            //    if (largoModelo <= 0)
                            //    {
                            //        largoModelo= textoCompleto.Length - inicioModelo;
                            //    }
                            //    modelo= textoCompleto.Substring(inicioModelo, largoModelo).Trim();
                            //    if (modelo.Length > 0)
                            //    {
                            //        foundModelo= true;
                            writer.WriteLine($"Modelo: {modelo}");
                            //    }
                            //    indexModelo= textoCompleto.IndexOf(textoModelo, indexModelo + 1);
                        }

                    }
                }
            }
         
        }
    }
}
