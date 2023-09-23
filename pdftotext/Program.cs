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
        public static bool debug = false;
        public static bool continuar = false;

        static void Main(string[] args)
        {
            //Instanciacion de la clase procesos para acceder a los metodos definidos en ella
            procesos proceso = new procesos();


            //Si no se pasan parametros muestra un error
            //if (args.Length == 0)
            //{
            //    proceso.MensajeParametros("\nDebe proporcionar el nombre del fichero a tratar o el parametro -l para procesar toda la carpeta");

            //}
            //else
            //{
            //Procesado de los argumentos que se pasan. 
            continuar = proceso.gestionArgumentos(args);

            if (continuar)
            {
                //Proceso para leer el nombre de los ficheros y almacenarlo en la variable filesPDF
                proceso.leerFicheros(args);

                //Proceso de todos los ficheros (en el paso anterior se ha podido almacenar el unico fichero pasado como parametro
                proceso.grabarFichero();
            }

            //}
        }
    }
}
