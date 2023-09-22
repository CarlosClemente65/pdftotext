using System.Text;

namespace pdftotext
{
    public class buscar
    {
        private string opcion;
        private string outputFilePath;
        private StringBuilder extractedText;

        public buscar(string tipo, string ficheroSalida, StringBuilder textoExtraido)
        {
            //Constructor que recibe los parametros e inicializa variables
            opcion = tipo;
            outputFilePath = ficheroSalida;
            extractedText = textoExtraido;
        }

        public void busqueda()
        {
            //Localizar y grabar los valores individuales en el fichero
            //En el caso de que la opcion sea grabar un fichero con los datos del modelo
            if (opcion == "-m")
            {
                File.WriteAllText(outputFilePath, extractedText.ToString());

                {
                    string textoCompleto = extractedText.ToString().Trim();

                    //Obtener el CSV del PDF
                    string csvPDF = csv(textoCompleto);

                    //Obtener expediente del PDF
                    string expedientePDF = expediente(textoCompleto, csvPDF);

                    //Obtener numero de justificante
                    string justificantePDF = justificante(textoCompleto);

                    //Obtener NIF
                    string NifPDF = Nif(textoCompleto, csvPDF);

                    //Obtener Nombre
                    string nombrePDF = nombre(textoCompleto, NifPDF);

                    //Obtener ejercicio
                    string ejercicioPDF = ejercicio(expedientePDF);

                    //Obtener periodo
                    string periodoPDF = periodo();

                    //Obtener modelo
                    string modeloPDF = modelo(expedientePDF);
                }
            }
        }

        string csv(string textoCompleto)
        {
            //Extraer el CSV
            string textoCsv = "Verificación "; //Localiza esta cadena en el texto del PDF
            int indexCsv = textoCompleto.IndexOf(textoCsv); //Asigna la posicion del texto a buscar si lo encuentra
            bool foundCsv = false;//Permite controlar si se ha localizado el CSV
            string Csv = ""; //Inicializa la variable que almacenara el CSV

            using (StreamWriter writer = new StreamWriter(outputFilePath)) //Crea el StreamWriter para grabar en el fichero a traves de un using para liberar recursos cuando acabe.
            {
                while (indexCsv >= 0 && !foundCsv) //Se ejecuta mientras no se encuentre el CSV 
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
            }

            InicializaVariables variables = new InicializaVariables();
            variables.InicializarCSV();
            return Csv;
        }

        string expediente(string textoCompleto, string csvEncontrado)
        {
            // Extraer el expediente
            string Expediente = "";
            string textoExpediente = csvEncontrado;
            int indexExpediente = textoCompleto.IndexOf(textoExpediente);
            bool foundExpediente = false;

            using (StreamWriter writer = new StreamWriter(outputFilePath)) //Crea el StreamWriter para grabar en el fichero a traves de un using para liberar recursos cuando acabe.
            {
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
            }
            InicializaVariables variables = new InicializaVariables();
            variables.InicializarCSV();
            return Expediente;
        }

        string justificante(string textoCompleto)
        {
            //Extraer el numero de justificante
            string justificante = "";
            string textoJustificante = "justificante: ";
            int indexJustificante = textoCompleto.IndexOf(textoJustificante);
            bool foundJustificante = false;

            using (StreamWriter writer = new StreamWriter(outputFilePath)) //Crea el StreamWriter para grabar en el fichero a traves de un using para liberar recursos cuando acabe.
            {
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
            }
            return justificante;
        }

        string Nif(string textoCompleto, string Csv)
        {
            //Extraer NIF
            string Nif = "";
            string textoNif = Csv;
            int indexNif = textoCompleto.IndexOf(textoNif);
            bool foundNif = false;

            using (StreamWriter writer = new StreamWriter(outputFilePath)) //Crea el StreamWriter para grabar en el fichero a traves de un using para liberar recursos cuando acabe.
            {
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
                return Nif;
            }
        }


        string nombre(string textoCompleto, string Nif)
        {
            //Extraer Nombre
            string Nombre = "";
            string textoNombre = Nif;
            int indexNombre = textoCompleto.IndexOf(textoNombre);
            bool foundNombre = false;

            using (StreamWriter writer = new StreamWriter(outputFilePath)) //Crea el StreamWriter para grabar en el fichero a traves de un using para liberar recursos cuando acabe.
            {
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
            }
            return Nombre;
        }

        string ejercicio(string Expediente)
        {
            //Extraer ejercicio
            string ejercicio = Expediente.Substring(0, 4);

            using (StreamWriter writer = new StreamWriter(outputFilePath)) //Crea el StreamWriter para grabar en el fichero a traves de un using para liberar recursos cuando acabe.
            {
                writer.WriteLine($"Ejercicio: {ejercicio}");
                return ejercicio;
            }
        }

        string periodo()
        {
            //Metodo para extraer el periodo del modelo (falta por desarrollar

            //Extraer periodo
            string periodo = "";
            return periodo;
        }

        string modelo(string Expediente)
        {
            //Extraer modelo
            string modelo = Expediente.Substring(4, 3);
            //Nota: esta parte esta comentada porque inicialmente buscaba el modelo de este modo, pero se puede obtener del propio expediente; dejo el codigo por si en las pruebas no funciona. Segun veo el codigo, si se utiliza habria que pasar como parametro el textoCompleto

            ////string modelo = "";
            ////string textoModelo= "justificante:";
            ////int indexModelo= textoCompleto.IndexOf(textoModelo);
            ////bool foundModelo = false;
            ////while (indexModelo>= 0 && !foundModelo)
            ////{
            ////    int inicioModelo = indexModelo+ textoModelo.Length;
            ////    int largoModelo = textoCompleto.IndexOf("\n", inicioModelo) - inicioModelo;
            ////    if (largoModelo <= 0)
            ////    {
            ////        largoModelo= textoCompleto.Length - inicioModelo;
            ////    }
            ////    modelo= textoCompleto.Substring(inicioModelo, largoModelo).Trim();
            ////    if (modelo.Length > 0)
            ////    {
            ////        foundModelo= true;
            ////        writer.WriteLine($"Modelo: {modelo}");
            ////    }
            ////    indexModelo= textoCompleto.IndexOf(textoModelo, indexModelo + 1);

            using (StreamWriter writer = new StreamWriter(outputFilePath)) //Crea el StreamWriter para grabar en el fichero a traves de un using para liberar recursos cuando acabe.
            {
                writer.WriteLine($"Modelo: {modelo}");
            }
            return modelo;
        }

    }

}
