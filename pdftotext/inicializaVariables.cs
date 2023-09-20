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
            textoCsv = string.Empty;
            indexCsv = 1;
            foundCsv = false;
            Csv = string.Empty;
            justificante = string.Empty;
        }
       
    }
}
