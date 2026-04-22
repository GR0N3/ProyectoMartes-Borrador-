using System.Collections.Generic;

namespace EngineGDI
{
    // Maneja el fondo con efecto parallax:
    // Fondo.png se desplaza y hace loop recién cuando llega a -fondoLoopDistanciaX (por defecto -3072).
    // Estrellas.png se desplaza más rápido y SI hace loop (se dibuja doble para que no se corte).
    public class BackgroundManager
    {
        private string fondoSprite;
        private string estrellasSprite;
        private float screenWidth;
        private float velocidadFondo;
        private float velocidadEstrellas;
        private float fondoLoopDistanciaX;

        private float fondoX;
        private float estrellasX;

        // Constructor:
        // - screenWidth: ancho de pantalla (se usa para el loop de estrellas).
        // - fondoSprite: ruta de Fondo.png.
        // - estrellasSprite: ruta de Estrellas.png.
        // - velocidadFondo: velocidad del fondo (parallax lento).
        // - velocidadEstrellas: velocidad de estrellas (parallax rápido).
        // - fondoLoopDistanciaX: ancho real del sprite de fondo para el loop (ej: 3072).
        public BackgroundManager(
            float screenWidth,
            string fondoSprite = "Textures/BackGrounds/Fondo.png",
            string estrellasSprite = "Textures/BackGrounds/Estrellas.png",
            float velocidadFondo = 30f,
            float velocidadEstrellas = 80f,
            float fondoLoopDistanciaX = 3072f)
        {
            this.screenWidth = screenWidth;
            this.fondoSprite = fondoSprite;
            this.estrellasSprite = estrellasSprite;
            this.velocidadFondo = velocidadFondo;
            this.velocidadEstrellas = velocidadEstrellas;
            this.fondoLoopDistanciaX = fondoLoopDistanciaX;
            fondoX = 0f;
            estrellasX = 0f;
        }

        // Update:
        // - Desplaza el fondo y las estrellas hacia la izquierda.
        // - Fondo: cuando su X llega a -fondoLoopDistanciaX, se suma fondoLoopDistanciaX para continuar el loop.
        // - Estrellas: cuando su X llega a -screenWidth, se suma screenWidth para loopear sin corte.
        public void Update(float deltaTime)
        {
            fondoX -= velocidadFondo * deltaTime;
            if (fondoX <= -fondoLoopDistanciaX)
                fondoX += fondoLoopDistanciaX;

            estrellasX -= velocidadEstrellas * deltaTime;
            if (estrellasX <= -screenWidth)
                estrellasX += screenWidth;
        }

        // Render:
        // - Dibuja el fondo dos veces: en fondoX y fondoX + fondoLoopDistanciaX, para que siempre haya una imagen "pegada" a la otra.
        // - Dibuja las estrellas dos veces: en estrellasX y estrellasX + screenWidth, para lograr el loop continuo.
        public void Render(float scaleX = 1f, float scaleY = 1f)
        {
            Engine.Draw(fondoSprite, fondoX, 0f, scaleX, scaleY);
            Engine.Draw(fondoSprite, fondoX + fondoLoopDistanciaX, 0f, scaleX, scaleY);

            Engine.Draw(estrellasSprite, estrellasX, 0f, scaleX, scaleY);
            Engine.Draw(estrellasSprite, estrellasX + screenWidth, 0f, scaleX, scaleY);
        }
    }
}
