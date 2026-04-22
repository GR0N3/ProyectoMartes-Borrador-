using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineGDI
{
    // Animation:
    // Gestiona una animación por frames (lista de rutas a imágenes) con control de velocidad y loop.
    public class Animation
    {
        private string id;
        private bool isLoopEnabled;
        private List<string> frames;
        private float speed = 0f;
        private float currentAnimationTime = 0f;
        private int currentFrameIndex = 0;

        public string Id => id;

        public string currentFrame => frames[currentFrameIndex];

        // Constructor:
        // - id: nombre/identificador lógico de la animación
        // - frames: lista de rutas a sprites (0..N)
        // - speed: tiempo entre frames (segundos)
        // - isLoopEnabled: si true vuelve al frame 0; si false se queda en el último frame
        public Animation(string id, List<string> frames, float speed, bool isLoopEnabled)
        {
            this.id = id;
            this.frames = frames;
            this.speed = speed;
            this.isLoopEnabled = isLoopEnabled;
        }

        // Resetea la animación al inicio (frame 0) y reinicia el acumulador de tiempo.
        public void Reset()
        {
            this.currentFrameIndex = 0;
            this.currentAnimationTime = 0f;
        }

        // Permite cambiar la velocidad (tiempo entre frames) en runtime.
        public void SetSpeed(float p_speed) 
        {
            speed = p_speed;
        }

        // Avanza la animación:
        // - acumula deltaTime (se usa Program.deltaTime)
        // - cuando supera "speed" avanza de frame
        // - si llega al final, hace loop o se queda en el último frame según isLoopEnabled
        public void Update() 
        {
            currentAnimationTime += Program.deltaTime;

            if (currentAnimationTime >= speed) 
            {
                currentFrameIndex++;
                currentAnimationTime = 0f;

                if (currentFrameIndex >= frames.Count) 
                {
                    if (isLoopEnabled) 
                    {
                        currentFrameIndex = 0;
                    }
                    else 
                    {
                        currentFrameIndex = frames.Count - 1;
                    }
                }
            }
        }

    }
}
