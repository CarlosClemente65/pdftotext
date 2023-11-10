using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf;
using System.Text;

namespace pdftotext
{
    internal class procesos
    {
        public string filePath = string.Empty;
        public string[] filesPDF;
        public int firstpage = 0;
        public int lastpage = 0;
        //La variable parametroOk sirve para controlar que se ha puesto al menos un parametro correcto
        public bool parametroOk = false;
        //public string opcion = "-f";
        public string outputFilePath = "";
        string folderPath = string.Empty;
        StringBuilder extractedText = new StringBuilder();
        public string csvPDF = string.Empty;
        public string expedientePDF = string.Empty;
        public string justificantePDF = string.Empty;
        public string nifPDF = string.Empty;
        public string nombrePDF = string.Empty;
        public string ejercicioPDF = string.Empty;
        public string modeloPDF = string.Empty;
        public string periodoPDF = string.Empty;

        //Control de los parametros que se han pasado, para chequear los obligatorios o si son incompatibles (-f y -l). Tambien permite validar la ruta (-r) o si la salida sera con las variables de Hacienda (-m)
        bool fOption = false;
        bool lOpcion = false;
        bool rOpcion = false;
        bool mOpcion = false;
        bool hOpcion = false;

        public procesos()
        {
            filesPDF = new string[0];
        }
        public void MensajeParametros(string mensaje)
        {
            Console.Clear();
            //Console.WriteLine();
            Console.WriteLine(mensaje);
            Console.WriteLine("\nUso:\tpdftotext [-h] <-f <archivo.pdf> | -l> <-r <ruta del archivo>> [-m] [-p [desde]-[hasta]]");
            Console.WriteLine("\n  Parametros:");
            Console.WriteLine("  -h\t(opcional)\tEsta ayuda");
            Console.WriteLine("  -f\t(obligatorio)\tNombre del fichero a procesar (si tiene espacios debe ir entre comillas dobles).\n\t\t\tEs incompatible con el parametro -l");
            Console.WriteLine("  -l\t(obligatorio)\tSe procesaran todos los ficheros que se encuentre en la ruta indicada con el parametro -r.\n\t\t\tEs incompatible con el parametro -f");
            Console.WriteLine("  -r\t(obligatorio)\tRuta donde estan ubicados los ficheros y donde se dejaran los resultados");
            Console.WriteLine("  -m\t(opcional)\tGenera un fichero con los datos principales del modelo de Hacienda\n");
            Console.WriteLine("  -p\t(opcional)\tIntervalo de paginas a extraer.\n\t\t\tPuede omitirse cualquiera de los dos y si no se indica el parametro se extraeran todas las paginas");
            Console.WriteLine("  \t\t\tEjemplo: -p 3 = pagina 3, -p 2-4 = paginas 2 a 4, -p -6 = paginas 1 a 6");
            Console.WriteLine("");
            Console.WriteLine("Pulse una tecla para continuar...");
            Console.ReadKey();
        }

        public void leerFicheros(string[] parametros)
        {
            string filePDF = Path.Combine(folderPath, filePath);//Almacena la ruta completa si es un solo fichero
            if (lOpcion)
            {
                //Si el argumento es -l almacena el nombre de todos los PDF de la carpeta
                if (Program.debug)
                {
                    //Ruta fija para las pruebas
                    folderPath = @"c:\borrar\pdftotext";
                }
                filesPDF = Directory.GetFiles(folderPath, "*.pdf");
            }
            else
            {
                //Chequea que el fichero pasado con el parametro -f existe
                if (!File.Exists(filePDF))
                {
                    Console.Clear();
                    Console.WriteLine($"El fichero {filePath}.pdf no existe.\nPulse una tecla para salir");
                    Console.ReadKey();
                    Environment.Exit(1);
                }

                //Como el argumento no es procesar todos los ficheros, se almacena el fichero pasado como parametro en la variable que procesa el array de ficheros
                filesPDF = new string[1];
                filesPDF[0] = filePDF;
            }

            //Mensaje si no hay ningun fichero PDF
            if (filesPDF.Length == 0)
            {
                Console.WriteLine("No hay ningun fichero PDF para procesar");
                return;
            }
        }

        public bool gestionArgumentos(string[] parametros)
        {
            //Proceso de los parametros pasados
            for (int i = 0; i < parametros.Length; i++)
            {
                string parametro = parametros[i].ToLower();
                switch (parametro)
                {
                    case "-h":
                        MensajeParametros("");
                        parametroOk = true; //Se pone a true para evitar el mensaje de parametros incorrectos
                        break;

                    case "-f":
                        if (lOpcion)
                        {
                            MensajeParametros("Las opciones -f y -l son incompatibles entre si");
                        }
                        fOption = true;

                        //Almacena el nombre del fichero a procesar
                        filePath = parametros[i + 1];
                        if (!filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            filePath += ".pdf";
                        }
                        parametroOk = true;
                        break;

                    case "-l":
                        if (fOption)
                        {
                            MensajeParametros("Las opciones -f y -l son incompatibles entre si");
                        }
                        lOpcion = true;
                        parametroOk = true;
                        break;

                    case "-p":
                        //Control de que se pasan las paginas inicio y fin separadas con un guion
                        int pageIndex;
                        if (parametros.Length > Array.IndexOf(parametros, parametro) + 1 && int.TryParse(parametros[Array.IndexOf(parametros, parametro) + 1], out pageIndex))
                        {
                            if (pageIndex < 0)
                            {
                                MensajeParametros("No se puede indicar un numero de pagina negativo");
                            }
                            firstpage = lastpage = pageIndex;
                            parametroOk = true;
                        }
                        else if (parametros.Length > Array.IndexOf(parametros, parametro) + 1)
                        {
                            string[] rangePages = parametros[Array.IndexOf(parametros, parametro) + 1].Split('-');
                            if (rangePages.Length == 1)
                            {
                                if (!int.TryParse(rangePages[0], out lastpage) || lastpage < 1)
                                {
                                    MensajeParametros($"El argumento {parametro} debe estar seguido de un numero entero positivo");
                                }
                                firstpage = lastpage;
                            }
                            else if (rangePages.Length == 2)
                            {
                                if (!int.TryParse(rangePages[0], out firstpage) || firstpage < 1)
                                {
                                    MensajeParametros($"El argumento {parametro} debe estar seguido de dos números enteros positivos separados por un guión (-)");
                                }
                                if (!int.TryParse(rangePages[1], out lastpage) || lastpage < 1)
                                {
                                    MensajeParametros($"El argumento {parametro} debe estar seguido de dos números enteros positivos separados por un guión (-)");
                                }
                            }
                            else
                            {
                                MensajeParametros($"El argumento {parametro} debe estar seguido de dos números enteros positivos separados por un guión (-) o de un número entero positivo.");
                            }
                            parametroOk = true;
                        }
                        else
                        {
                            MensajeParametros($"El argumento {parametro} debe estar seguido de dos números enteros positivos separados por un guión (-) o de un número entero positivo.");
                        }
                        break;

                    case "-m":
                        mOpcion = true;
                        parametroOk = true;
                        break;

                    case "-r":
                        folderPath = parametros[i + 1];
                        if (!Directory.Exists(folderPath))
                        {
                            MensajeParametros("Error. La ruta especificada no existe");
                            Environment.Exit(1);
                        }
                        parametroOk = true;
                        break;
                }
            }
            //Chequea si los parametros pasados son correctos.
            if (!parametroOk)
            {
                MensajeParametros("Parametros incorrectos");
            }
            else
            {
                //Si se ha pasado el parametro de ayuda, se devuelve falso para evitar que se ejecute el resto
                if (hOpcion)
                {
                    parametroOk = false;
                }
                else
                //Si falta algun parametro obligatorio, se muestra un mensaje y se devuelve falso para evitar que se ejecute el resto
                if (!fOption && !lOpcion && !rOpcion)
                {
                    MensajeParametros("Faltan parametros obligatorios");
                    parametroOk = false;
                }
            }

            return parametroOk;
        }

        public void grabarFichero()
        {
            //Hace un bucle para procesar todos los ficheros que se hayan leido
            foreach (string archivo in filesPDF)
            {
                filePath = archivo;
                //Se asigna al fichero de salida el mismo nombre con extension .txt
                outputFilePath = System.IO.Path.ChangeExtension(filePath, "txt");

                using (PdfDocument pdfDoc = new PdfDocument(new PdfReader(filePath)))
                {
                    SimpleTextExtractionStrategy extractionStrategy = new SimpleTextExtractionStrategy();
                    //Almacena el contenido del PDF

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

                    //Extrae el texto de las paginas del PDF y lo almacena en la variable extractedText
                    for (int i = firstpage; i <= lastpage; i++)
                    {
                        //Si se activa la variable debug, el fichero de salida se le añade el numero de linea para facilitar las busquedas.
                        if (Program.debug == true)
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

                    if (mOpcion)
                    {
                        //Extraer datos del modelo
                        extraeDatosModelo();
                    }
                    else
                        //Graba el fichero completo si no se ha pasado como parametro generar las variables del modelo de Hacienda
                        File.WriteAllText(outputFilePath, extractedText.ToString());
                }
                inicializaVariables();
            }
        }

        public void extraeDatosModelo()
        {
            buscar busca = new buscar();

            //Si se pasa como parametro '-m' se generan todos las busquedas de los datos del modelo
            using (StreamWriter writer = new StreamWriter(outputFilePath)) //Crea el StreamWriter para grabar en el fichero a traves de un using para liberar recursos cuando acabe.
            {
                string textoCompleto = extractedText.ToString().Trim();

                //Obtener el CSV del PDF
                csvPDF = busca.csv(textoCompleto);
                string textoCSV = string.Empty;
                if (csvPDF != string.Empty)
                {
                    textoCSV = $"CSV: {csvPDF}";
                }
                else
                {
                    textoCSV = "CSV no encontrado";
                }
                writer.WriteLine(textoCSV);

                //Obtener expediente del PDF
                expedientePDF = busca.expediente(textoCompleto, csvPDF);
                string textoExpediente = string.Empty;
                if (expedientePDF.Length > 0)
                {
                    textoExpediente = $"Expediente: {expedientePDF}";
                }
                else
                {
                    textoExpediente = "Expediente no encontrado";
                }
                writer.WriteLine(textoExpediente);

                //Obtener numero de justificante
                justificantePDF = busca.justificante(textoCompleto);
                string textoJustificante = string.Empty;
                if (justificantePDF.Length > 0)
                {
                    textoJustificante = $"Justificante: {justificantePDF}";
                }
                else
                {
                    textoJustificante = "Justificante no encontrado";
                }
                writer.WriteLine(textoJustificante);

                //Obtener NIF
                nifPDF = busca.Nif(textoCompleto, csvPDF);
                string textoNif = string.Empty;
                if (nifPDF.Length > 0)
                {
                    textoNif = $"NIF: {nifPDF}";
                }
                else
                {
                    textoNif = "NIF no encontrado";
                }
                writer.WriteLine(textoNif);

                //Obtener Nombre
                nombrePDF = busca.nombre(textoCompleto, nifPDF);
                string textoNombre = string.Empty;
                if (nombrePDF.Length > 0)
                {
                    textoNombre = $"Nombre: {nombrePDF}";
                }
                else
                {
                    textoNombre = "Nombre no encontrado";
                }
                writer.WriteLine(textoNombre);

                //Obtener ejercicio
                ejercicioPDF = busca.ejercicio(expedientePDF);
                string textoEjercicio = string.Empty;
                if (ejercicioPDF.Length > 0)
                {
                    textoEjercicio = $"Ejercicio: {ejercicioPDF}";
                }
                else
                {
                    textoEjercicio = "Ejercicio no encontrado";
                }
                writer.WriteLine(textoEjercicio);


                //Obtener modelo
                modeloPDF = busca.modelo(expedientePDF);
                string textoModelo = string.Empty;
                if (modeloPDF.Length > 0)
                {
                    textoModelo = $"Modelo: {modeloPDF}";
                }
                else
                {
                    textoModelo = "Modelo no encontrado";
                }

                writer.WriteLine(textoModelo);

                //Obtener periodo
                periodoPDF = busca.periodo();
                writer.WriteLine(periodoPDF);
            }

        }


        public void inicializaVariables()
        {

            //Borra los datos de las variables que se han obtenido en cada procesado de los ficheros.
            csvPDF = string.Empty;
            expedientePDF = string.Empty;
            justificantePDF = string.Empty;
            nifPDF = string.Empty;
            nombrePDF = string.Empty;
            ejercicioPDF = string.Empty;
            modeloPDF = string.Empty;
            periodoPDF = string.Empty;
            extractedText.Clear();
        }
    }
}
