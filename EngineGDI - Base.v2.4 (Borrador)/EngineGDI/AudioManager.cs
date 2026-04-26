using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace EngineGDI
{
    // AudioManager:
    // - Centraliza la reproducción de audio del juego.
    // - Se implementa como Singleton para que exista un único punto de control (1 instancia global).
    // - Maneja dos tipos de audio:
    //   1) Música en loop (solo 1 a la vez).
    //   2) Efectos (SFX) disparados por eventos (Enter, disparo, impacto, etc).
    internal sealed class AudioManager
    {
        // Singleton: instancia única creada una vez.
        private static readonly AudioManager instance = new AudioManager();

        // Singleton: acceso global a la instancia única.
        public static AudioManager Instance
        {
            get { return instance; }
        }

        // Rutas de audio (relativas al directorio de ejecución).
        private const string MenuMusicPath = "Sounds/Micrology.wav";
        private const string GameMusicPath = "Sounds/Rivals8Bit.wav";
        private const string ButtonEffectPath = "Sounds/Buttom_Effect.wav";
        private const string LaserEffectPath = "Sounds/Laser_Effect.wav";
        private const string HitEffectPath = "Sounds/Hit_Effect.wav";

        // Trackea qué música está actualmente activa para evitar reiniciar el mismo loop.
        private string currentMusicPath;

        private readonly MediaPlayer musicPlayer = new MediaPlayer();
        private readonly MediaPlayer sfxPlayer = new MediaPlayer();

        private double musicVolume = 1.0;
        private readonly Dictionary<string, double> musicPerTrackVolume = new Dictionary<string, double>();
        private double sfxVolume = 1.0;
        private readonly Dictionary<string, double> sfxPerEffectVolume = new Dictionary<string, double>();

        // Constructor privado: nadie puede hacer new AudioManager() desde afuera.
        private AudioManager()
        {
            musicPlayer.MediaEnded += (s, e) =>
            {
                musicPlayer.Position = System.TimeSpan.Zero;
                musicPlayer.Play();
            };

            sfxPerEffectVolume[ButtonEffectPath] = 1.0;
            sfxPerEffectVolume[LaserEffectPath] = 1.0;
            sfxPerEffectVolume[HitEffectPath] = 1.0;

            musicPerTrackVolume[MenuMusicPath] = 2.0;
            musicPerTrackVolume[GameMusicPath] = 0.5;
        }

        public void SetMusicVolume(double volume01)
        {
            musicVolume = Clamp01(volume01);
            ApplyCurrentMusicVolume();
        }

        public void SetMenuMusicVolume(double volume01)
        {
            musicPerTrackVolume[MenuMusicPath] = Clamp01(volume01);
            ApplyCurrentMusicVolume();
        }

        public void SetGameMusicVolume(double volume01)
        {
            musicPerTrackVolume[GameMusicPath] = Clamp01(volume01);
            ApplyCurrentMusicVolume();
        }

        public void SetSfxVolume(double volume01)
        {
            sfxVolume = Clamp01(volume01);
        }

        public void SetButtonEffectVolume(double volume01)
        {
            sfxPerEffectVolume[ButtonEffectPath] = Clamp01(volume01);
        }

        public void SetLaserEffectVolume(double volume01)
        {
            sfxPerEffectVolume[LaserEffectPath] = Clamp01(volume01);
        }

        public void SetHitEffectVolume(double volume01)
        {
            sfxPerEffectVolume[HitEffectPath] = Clamp01(volume01);
        }

        // Música del menú principal en loop.
        public void PlayMenuMusic()
        {
            PlayMusicLoop(MenuMusicPath);
        }

        // Música del gameplay en loop.
        public void PlayGameMusic()
        {
            PlayMusicLoop(GameMusicPath);
        }

        // Efecto de confirmación de botón (Enter).
        public void PlayButtonEffect()
        {
            PlaySfx(ButtonEffectPath);
        }

        // Efecto al disparar.
        public void PlayLaserEffect()
        {
            PlaySfx(LaserEffectPath);
        }

        // Efecto al impactar bala contra asteroide.
        public void PlayHitEffect()
        {
            PlaySfx(HitEffectPath);
        }

        // Detiene la música actual (si existe).
        public void StopMusic()
        {
            if (currentMusicPath == null) return;

            try
            {
                musicPlayer.Stop();
                musicPlayer.Close();
            }
            catch { }

            currentMusicPath = null;
        }

        // Reproduce música en loop, asegurando que solo una música esté activa a la vez.
        private void PlayMusicLoop(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            // Si ya está sonando la misma música, no reiniciar.
            if (currentMusicPath == path) return;

            // Si cambia la música (menú ↔ juego), se detiene la anterior.
            StopMusic();
            currentMusicPath = path;

            try
            {
                string absolutePath = Path.GetFullPath(path);
                musicPlayer.Open(new System.Uri(absolutePath));
                musicPlayer.Position = System.TimeSpan.Zero;
                ApplyCurrentMusicVolume();
                musicPlayer.Play();
            }
            catch
            {
            }
        }

        private void ApplyCurrentMusicVolume()
        {
            if (currentMusicPath == null) return;

            double perTrack = 1.0;
            if (musicPerTrackVolume.TryGetValue(currentMusicPath, out var v))
                perTrack = v;

            musicPlayer.Volume = Clamp01(musicVolume * perTrack);
        }

        // Reproduce un efecto una vez.
        // Se hace Stop() antes de Play() para que el SFX se reinicie si se dispara seguido (ej: muchos disparos).
        private void PlaySfx(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;

            try
            {
                double perEffect = 1.0;
                if (sfxPerEffectVolume.TryGetValue(path, out var v))
                    perEffect = v;

                sfxPlayer.Stop();
                sfxPlayer.Close();
                sfxPlayer.Volume = Clamp01(sfxVolume * perEffect);

                string absolutePath = Path.GetFullPath(path);
                sfxPlayer.Open(new System.Uri(absolutePath));
                sfxPlayer.Position = System.TimeSpan.Zero;
                sfxPlayer.Play();
            }
            catch
            {
            }
        }

        private static double Clamp01(double value)
        {
            if (value < 0.0) return 0.0;
            if (value > 1.0) return 1.0;
            return value;
        }
    }
}
