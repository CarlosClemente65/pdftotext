namespace pdftotext
{
    //La clase "buscar_anterior" se desarrollo en la version v1.0 para buscar los datos teniendo en cuenta las lineas
    //Con la version v1.1 las busquedas se hacen mediante expresiones regulares y se usan mas parametros para otro tipo de gestion.
    //Con la ultima version v2.0 (la actual) solo se pasan 3 parametros con el fichero a procesar y los ficheros de salida

    class Program
    {
        //La variable debug permite añadir el numero de linea al texto extraido
        public static bool debug = false;
        public static bool continuar = false;

        static void Main(string[] args)
        {
            //Instanciacion de la clase procesosPDF para acceder a los metodos definidos en ella
            procesosPDF proceso = new procesosPDF();

            //Proceso de los parametros que se pasan
            continuar = proceso.gestionParametros(args);

            if (continuar)
            {
                //Extrae el texto del PDF
                proceso.extraeTextoPDF();

                //Extrae datos del modelo
                proceso.extraeDatosModelo();

            }
        }
    }
}
