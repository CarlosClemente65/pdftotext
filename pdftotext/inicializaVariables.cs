namespace pdftotext
{
    public class InicializaVariables
    {
        public string textoCsv=string.Empty;
        public int indexCsv;
        public bool foundCsv;
        public string Csv = string.Empty;
        public string justificante = string.Empty;

        public void InicializarCSV()
        {
            //Nota: no tengo claro porque hay que inicializar variables y he creado una clase; ver si es necesario inicializarlas por algun motivo, y si es asi, hay que ponerlas en el propio metodo buscar antes de devolver el resultado.

            textoCsv = string.Empty;
            indexCsv = 1;
            foundCsv = false;
            Csv = string.Empty;
            justificante = string.Empty;
        }
       
    }
}
