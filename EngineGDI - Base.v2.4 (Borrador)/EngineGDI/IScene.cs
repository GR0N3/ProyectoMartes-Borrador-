namespace EngineGDI
{
    // Contrato mínimo de una escena del juego (menú, gameplay, etc.).
    // Cada escena encapsula su propio input, update y render.
    internal interface IScene
    {
        // Lee y procesa input (teclado) para la escena.
        void Input();

        // Actualiza la lógica de la escena.
        void Update(float deltaTime);

        // Dibuja la escena.
        void Render();
    }
}
