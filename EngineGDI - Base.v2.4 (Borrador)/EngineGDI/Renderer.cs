namespace EngineGDI
{
    // Componente Renderer:
    // Encapsula la textura y su tamaño base, y proporciona una función de dibujo
    // que recibe un Transform para saber cómo dibujarse en pantalla.
    public class Renderer
    {
        public string TexturePath { get; set; }
        public Vector2 BaseSize { get; set; }

        public Renderer(string texturePath, Vector2 baseSize)
        {
            TexturePath = texturePath;
            BaseSize = baseSize;
        }

        // Dibuja la textura usando las propiedades del Transform recibido.
        // angle: rotación en grados.
        // offsetX/offsetY: pivote de rotación (0..1).
        public void Draw(Transform transform, float angle = 0f, float offsetX = 0f, float offsetY = 0f)
        {
            Engine.Draw(TexturePath, transform.Position.X, transform.Position.Y,
                        transform.Scale.X, transform.Scale.Y, angle, offsetX, offsetY);
        }
    }
}
