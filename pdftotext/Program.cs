using System.Reflection;

[assembly: AssemblyTitle("Convierte un PDF en texto y extrae los datos de los modelos de Hacienda")]
[assembly: AssemblyProduct("dsepdfatexto")]
[assembly: AssemblyDescription("Convierte un PDF en texto y extrae los datos de los modelos de Hacienda")]
//[assembly: AssemblyCompany("Diagram Software Europa S.L.")]
[assembly: AssemblyCopyright("© 2023 - Diagram Software Europa S.L.")]
[assembly: AssemblyVersion("1.2.2.0")]
[assembly: AssemblyFileVersion("1.2.2.0")]


namespace pdftotext
{
    /*
    Aplicacion para convertir a texto un PDF y buscar los datos de los modelos de Hacienda para archivarlos en el GAD.
    Con la version 1.2 se incluye la extraccion de datos de los modelos laborales
    Desarrollada por Carlos Clemente - 12/2023
    
    Uso:
        dsepdfatexto -h
        dsepdfatexto fichero.pdf -t textoPDF.txt -m datosModelo.txt  -l datosLaboral.txt
        dsepdfatexto carpeta [-rm | rl]

    Parametros:
        -h                 : Esta ayuda
        fichero.pdf        : nombre del fichero PDF a procesar (unico fichero)
        -t textoPDF.txt    : nombre del fichero en el que grabar el texto completo del PDF
        -m datosModelo.txt : nombre del fichero en el que grabar los campos localizados del modelo
        -l datosLaboral.txt: nombre del fichero en el que grabar los campos localizados del modelo laboral
        carpeta            : carpeta donde estan todos los ficheros PDF a procesar de forma masiva
        -rm                : parametro que indica que se procesen los ficheros de la ruta como modelos
        -rl                : parametro que indica que se procesen los ficheros de la rut como documentos laborales

     */

    class Program
    {
        //Variable para chequear si los parametros pasados son correctos
        public static bool continuar = false;

        //Definicion de variables para almacenar el texto del PDF (se definen aqui para que esten accesibles desde todas las clases)
        public static List<string> paginasPDF = new List<string>();
        public static string textoCompleto = string.Empty;

        //Instanciacion de la clase procesosPDF para acceder a los metodos definidos en ella
        public static procesosPDF proceso = new procesosPDF();

        // Obtener la información del ensamblado actual
        static Assembly assembly = Assembly.GetExecutingAssembly();

        // Obtener el atributo del copyright del ensamblado
        static AssemblyCopyrightAttribute copyrightAttribute =
            (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));

        //Obtener el atributo del nombre del ensamblado
        static AssemblyProductAttribute nombreProducto = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));

        // Obtener el valor de la propiedad Copyright y nombre del producto
        static string copyrightValue = copyrightAttribute?.Copyright;
        static string nombreValue = nombreProducto?.Product;

        static void Main(string[] args)
        {
            //Control para si no se ejecuta desde la linea de comnados, mostrar un mensaje
            switch (args.Length)
            {
                case 0:
                    //Si no se pasan argumentos debe ser porque se ha ejecutado desde windows
                    // Abre una ventana de consola para mostrar el mensaje
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.SetWindowSize(120, 22);
                    Console.SetBufferSize(120, 22);
                    Console.Clear();
                    Console.Title = $"{nombreValue} - {copyrightValue}";
                    Console.WriteLine("\r\n\tEsta aplicacion debe ejecutarse por linea de comandos y pasarle los parametros correspondientes.");
                    mensajeAyuda();
                    Console.SetWindowSize(120, 30);
                    Console.SetBufferSize(120, 30);
                    Console.ResetColor();
                    Console.Clear();
                    Environment.Exit(0);
                    break;

                case 1:
                    //Si solo se pasa un parametro puede ser la peticion de ayuda
                    if (args[0] == "-h")
                    {
                        mensajeAyuda();
                    }
                    break;

                default:
                    //Proceso de los parametros que se pasan
                    continuar = gestionParametros(args);

                    //Si los parametros pasados son correctos realiza los procesos que se hayan pasado como parametros
                    if (continuar)
                    {
                        switch (proceso.tipoProceso)
                        {
                            case "Modelos":
                                proceso.extraeTextoPDF();
                                proceso.grabaFichero(proceso.ficheroTexto, textoCompleto);
                                proceso.extraeDatosModelo();
                                break;

                            case "Laboral":
                                proceso.extraeTextoPDF();
                                proceso.grabaFichero(proceso.ficheroTexto, textoCompleto);
                                proceso.extraeDatosLaboral();
                                break;

                            case "Masivo":
                                foreach (string fichero in proceso.archivosPDF)
                                {
                                    paginasPDF.Clear();
                                    textoCompleto = string.Empty;
                                    proceso.ficheroPDF = fichero;
                                    string nombreFichero = Path.ChangeExtension(fichero, null);
                                    proceso.ficheroTexto = nombreFichero + "_TXT.txt";
                                    proceso.ficheroDatos = nombreFichero + "_DATOS.txt";
                                    proceso.extraeTextoPDF();
                                    proceso.grabaFichero(proceso.ficheroTexto, textoCompleto);

                                    switch (args[1])
                                    {
                                        case "-rm":
                                            proceso.extraeDatosModelo();
                                            break;

                                        case "-rl":
                                            proceso.extraeDatosLaboral();
                                            break;
                                    }
                                }
                                break;
                        }

                        
                    }
                    break;
            }
        }


        static bool gestionParametros(string[] parametros)
        {
            int totalParametros = parametros.Length;
            if (parametros[1] == "-rl" || parametros[1] == "-rm")
            {
                //Si el primer parametro es -rm (ruta de modelos) o -rl (ruta de documentos laborales) se va a procesar la ruta completa, que se controla desde la gestion de parametros. 
            }
            else
            {
                proceso.ficheroPDF = parametros[0];

                //Comprueba si existe el fichero PDF
                if (!File.Exists(proceso.ficheroPDF))
                {
                    proceso.grabaFichero("error_proceso.txt", "El fichero PDF no existe");
                    return false;
                }
            }
            //Procesado del resto de parametros
            int controlParametros = 0;
            for (int i = 1; i < totalParametros; i++)
            {
                switch (parametros[i])
                {
                    case "-t":
                        //Extraccion de texto completo del PDF. El nombre del fichero de salida debe venir a continuacion del parametro.
                        if (totalParametros > i + 1)
                        {
                            //El siguiente parametro debe ser el fichero de salida
                            if (parametros[i + 1].Length > 2)
                            {
                                proceso.ficheroTexto = parametros[i + 1];
                                proceso.grabaTexto = true;
                                controlParametros++;
                            }
                        }
                        break;

                    case "-m":
                        //Procesado de modelos PDF. El nombre del fichero de salida debe venir a continuacion del parametro
                        if (totalParametros > i + 1)
                        {
                            //El siguiente parametro debe ser el fichero de salida
                            if (parametros[i + 1].Length > 2)
                            {
                                proceso.ficheroDatos = parametros[i + 1];
                                proceso.tipoProceso = "Modelos";
                                controlParametros++;
                            }
                        }
                        break;


                    case "-l":
                        //Procesado de documentos laborales en PDF. El nombre del fichero de salida debe venir a continuacion del parametro
                        if (totalParametros > i + 1)
                        {
                            //El siguiente parametro debe ser el fichero de salida
                            if (parametros[i + 1].Length > 2)
                            {
                                proceso.ficheroDatos = parametros[i + 1];
                                proceso.tipoProceso = "Laboral";
                                controlParametros++;
                            }
                        }
                        break;

                    case "-rl":
                    case "-rm":
                        //Proceso de ficheros de la ruta indicada
                        if (totalParametros > i)
                        {
                            //El parametro anterior debe ser la ruta de los PDFs
                            if (parametros[i - 1].Length > 2)
                            {
                                proceso.ProcesoMasivo(parametros[i - 1]);
                                proceso.tipoProceso = "Masivo";
                                controlParametros++;
                            }
                        }
                        break;
                }
            }

            //Control si los parametros pasados son correctos
            if (controlParametros > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static void mensajeAyuda()
        {
            string mensaje =
                "\r\nUso:" +
                "\r\n\tdsepdfatexto fichero.pdf -t textoPDF.txt -m datosModelo.txt  -l datosLaboral.txt" +
                "\r\n\tdsepdfatexto carpeta [-rm | rl]" +
                "\r\n\r\nParametros:" +
                "\r\n\t-h                 : Esta ayuda" +
                "\r\n\tfichero.pdf        : nombre del fichero PDF a procesar (unico fichero)" +
                "\r\n\t-t textoPDF.txt    : nombre del fichero en el que grabar el texto completo del PDF" +
                "\r\n\t-m datosModelo.txt : nombre del fichero en el que grabar los campos localizados del modelo" +
                "\r\n\t-l datosLaboral.txt: nombre del fichero en el que grabar los campos localizados del modelo laboral" +
                "\r\n\tcarpeta            : carpeta donde estan todos los ficheros PDF a procesar de forma masiva" +
                "\r\n\t-rm                : parametro que indica que se procesen los ficheros de la ruta como modelos" +
                "\r\n\t-rl                : parametro que indica que se procesen los ficheros de la rut como documentos laborales" +
                "\r\n\r\n\r\nPulse una tecla para salir";

            Console.WriteLine(mensaje);
            Console.ReadKey();
        }

    }
}
