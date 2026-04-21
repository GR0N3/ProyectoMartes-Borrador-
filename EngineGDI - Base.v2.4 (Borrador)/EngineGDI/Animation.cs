using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngineGDI
{
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



        public Animation(string id, List<string> frames, float speed, bool isLoopEnabled)
        {
            this.id = id;
            this.frames = frames;
            this.speed = speed;
            this.isLoopEnabled = isLoopEnabled;
        }

        public void Reset()
        {
            this.currentFrameIndex = 0;
            this.currentAnimationTime = 0f;
        }

        public void SetSpeed(float p_speed) 
        {
            speed = p_speed;
        }

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
