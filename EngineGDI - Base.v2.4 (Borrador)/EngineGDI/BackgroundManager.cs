using System.Collections.Generic;

namespace EngineGDI
{
    // Maneja el fondo con efecto parallax:
    // Fondo.png se desplaza pero NO hace loop (se queda en -screenWidth).
    // Estrellas.png se desplaza más rápido y SI hace loop (se dibuja doble para que no se corte).
    public class BackgroundManager
    {
        private string fondoSprite;
        private string estrellasSprite;
        private float screenWidth;
        private float velocidadFondo;
        private float velocidadEstrellas;

        private float fondoX;
        private float estrellasX;

        // Crea el manager del fondo.
        // screenWidth: ancho de pantalla (ej: 1024), se usa para el loop de estrellas y el límite del fondo.
        // fondoSprite: ruta de Fondo.png
        // estrellasSprite: ruta de Estrellas.png
        // velocidadFondo: velocidad del fondo (px/seg aprox)
        // velocidadEstrellas: velocidad de estrellas (px/seg aprox) para parallax
        public BackgroundManager(
            float screenWidth,
            string fondoSprite = "Textures/BackGrounds/Fondo.png",
            string estrellasSprite = "Textures/BackGrounds/Estrellas.png",
            float velocidadFondo = 30f,
            float velocidadEstrellas = 80f)
        {
            this.screenWidth = screenWidth;
            this.fondoSprite = fondoSprite;
            this.estrellasSprite = estrellasSprite;
            this.velocidadFondo = velocidadFondo;
            this.velocidadEstrellas = velocidadEstrellas;
            fondoX = 0f;
            estrellasX = 0f;
        }

        // Actualiza posiciones:
        // Fondo: se mueve a la izquierda y cuando llega a -screenWidth se queda ahí (sin loop).
        // Estrellas: se mueven a la izquierda y cuando llegan a -screenWidth vuelven a 0 (loop).
        public void Update(float deltaTime)
        {
            fondoX -= velocidadFondo * deltaTime;
            if (fondoX <= -screenWidth)
                fondoX = -screenWidth;

            estrellasX -= velocidadEstrellas * deltaTime;
            if (estrellasX <= -screenWidth)
                estrellasX += screenWidth;
        }

        // Dibuja:
        // Fondo (una sola vez)
        // Estrellas (dos veces): en X y X+screenWidth para lograr el loop sin cortes.
        public void Render(float scaleX = 1f, float scaleY = 1f)
        {
            Engine.Draw(fondoSprite, fondoX, 0f, scaleX, scaleY);

            Engine.Draw(estrellasSprite, estrellasX, 0f, scaleX, scaleY);
            Engine.Draw(estrellasSprite, estrellasX + screenWidth, 0f, scaleX, scaleY);
        }
    }
}
