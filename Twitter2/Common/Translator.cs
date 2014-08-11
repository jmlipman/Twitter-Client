using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twitter2.Common
{
    class Translator
    {

        //Cantidad aproximada de traducciones, que se saltarán como en una pila de control
        int salto = 50;
        /**
         * Id del idioma:
         * 0 --> español
         * 1 --> ingles
         */ 
        int id_idioma; 


        Dictionary<String, int> identificadores = new Dictionary<String, int>();
        Dictionary<int, String> traduccion = new Dictionary<int, String>();

        /**
         * Constructor que inicializa todas las cadenas de texto, y setea el idioma predefinido (español).
         */
        public Translator(String idioma)
        {

            List<String> id_strings = new List<String>(); //identificadores (cadenas)
            List<String> id_español = new List<String>(); //traducción al español
            List<String> id_ingles = new List<String>(); //traducción al inglés

            id_strings.Add("sendtweet");
            id_español.Add("Enviar Tweet");
            id_ingles.Add("Send Tweet");

            id_strings.Add("loading...");
            id_español.Add("Cargando...");
            id_ingles.Add("Loading...");

            id_strings.Add("sending...");
            id_español.Add("Enviando...");
            id_ingles.Add("Sending...");

            id_strings.Add("steps:");
            id_español.Add("Pasos:");
            id_ingles.Add("Steps:");

            /*
             * 1.-Introduce tu nombre de usuario y contraseña en la ventana de la izquierda y.
             *    haz click en el botón de "Autorizar Aplicación".
             *    
             * 2.-Introduce el número (PIN) que aparece en la ventana de la izquierda 
             *    en la caja de texto de debajo y haz click en "Autenticar".
             * 
             * */
            //TODO TRADUCIR
            id_strings.Add("instructions");
            id_español.Add("1.-Introduce tu nombre de usuario y contraseña en la ventana de la izquierda y " +
            "haz click en el botón de \"Autorizar la aplicación\".\n\n" +
            "2.-Introduce el número (PIN) en la caja de texto de debajo " +
            "y haz click en \"Autenticar\".");
            id_ingles.Add("1.-Enter your username and password in the left window and click \"Authorize app\".\n\n"+
                "2.-Enter the digits (PIN) into the textbox below and click \"Authenticate\".");

            id_strings.Add("notstoringcredentials");
            id_español.Add("Tu nombre de usuario y contraseña NO serán almacenados en ningún momento.");
            id_ingles.Add("Your username and password will NOT be stored anytime.");

            id_strings.Add("authenticate");
            id_español.Add("Autenticar");
            id_ingles.Add("Authenticate");

            id_strings.Add("whatshappening");
            id_español.Add("¿Qué está pasando?");
            id_ingles.Add("What's happening?");

            id_strings.Add("multipletweets");
            id_español.Add("Multiples Tweets");
            id_ingles.Add("Multiple Tweets");

            id_strings.Add("verticalorientation");
            id_español.Add("Orientación Vertical");
            id_ingles.Add("Vertical Orientation");

            id_strings.Add("horizontalorientation");
            id_español.Add("Orientación Horizontal");
            id_ingles.Add("Horizontal Orientation");

            //Añadimos las traducciones a los diccionarios
            int cont = 0;
            foreach (String str in id_strings)
                identificadores.Add(str, cont++);

            cont=0;
            foreach (String str in id_español)
                traduccion.Add(cont++, str);

            cont = 0;
            foreach (String str in id_ingles)
                traduccion.Add(cont++ + salto * 1, str);
            
            //Establecemos el id del idioma
            switch (idioma)
            {
                case "es": id_idioma = 0; break;
                case "en": id_idioma = 1; break;
                default: id_idioma = 1; break;
            }

            
        }
        /**
         * Esta funcion devuelve una cadena (traducción) en base a un identificador.
         * @param cadena identificadora para obtener la traducción.
         * @return cadena traducida.
         */ 
        public String getCadena(String cadena)
        {
            return traduccion[identificadores[cadena]+salto*getIdiomaActual()];
        }

        /**
         * Esta funcion devuelve una cadena (traducción) en base a un identificador.
         * También se encarga de hacer un reemplazamiento.
         * Esto es util para traducciones en las que ciertas cosas cambian de posición.
         * @param cadena identificadora para obtener la traducción.
         * @param cadena2 que va a ser el reemplazo de la cadena traducida.
         * @return cadena traducida.
         */
        public String getCadena(String cadena, String cadena2)
        {
            String tmp = traduccion[identificadores[cadena]+salto*getIdiomaActual()];
            return tmp.Replace("(=*=)",cadena2);
        }

        /**
         * Nos devuelve el id del idioma actual.
         */ 
        public int getIdiomaActual() { return this.id_idioma; }

        /**
         * Establece el idioma actual.
         */ 
        public void setIdioma(int id) { this.id_idioma = id; }
    }
}

