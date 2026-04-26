namespace EngineGDI
{
    // Administra el cambio y ejecución de escenas del juego.
    // Se implementa como Singleton para asegurar una única instancia global responsable del flujo de escenas.
    internal sealed class SceneManager
    {
        private static readonly SceneManager instance = new SceneManager();

        private IScene current;

        // Devuelve la instancia única del SceneManager.
        public static SceneManager Instance
        {
            get { return instance; }
        }

        private SceneManager() { }

        // Cambia la escena activa por otra (por ejemplo: MainMenu → Game).
        public void ChangeScene(IScene scene)
        {
            current = scene;
        }

        // Ejecuta el input de la escena activa.
        public void Input()
        {
            current?.Input();
        }

        // Actualiza la escena activa.
        public void Update(float deltaTime)
        {
            current?.Update(deltaTime);
        }

        // Renderiza la escena activa.
        public void Render()
        {
            current?.Render();
        }
    }
}
