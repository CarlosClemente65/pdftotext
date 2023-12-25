<h1 align="center"> dsepdfatext </h1>
<br>

<h2> Extraccion de texto de un PDF y obtencion de variables de modelos de Hacienda y de documentos laborales para su tratamiento </h2>
<br>
<h4> @ Carlos Clemente (Diagram Software Europa S.L.) - 12/2023 </h4>

<h3>Descripcion</h3>
Permite extraer los datos de un PDF a un fichero de texto, y buscar las variables de los modelos de Hacienda o de documentos laborales para su tratamiento
<br><br>

### Control versiones

* v1.0 Primera version que extrae el texto completo del PDF
* v1.1 Incorporada la opcion para la extraccion de datos de modelos
* v1.2 Incorporada la opcion para la extraccion de datos de documentos laborales
<br><br>


### Uso:
<code>dsepdfatext.exe fichero.pdf -m datosModelo.txt -l datosLaboral.txt -t textoPDF.txt</code>
<br><br>

### Notas:
* El parametro -m es el fichero con los campos localizados del modelo
* El parametro -l es el fichero con los campos localizados del modelo laboral
* El parametro -t es el texto completo del PDFli>
* El orden de estos parametros es indiferente