namespace pdftotext
{
    public class inicializaVariables
    {
        public string textoCsv=string.Empty;
        public int indexCsv;
        public bool foundCsv;
        public string Csv = string.Empty;
        public string justificante = string.Empty;

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
}
