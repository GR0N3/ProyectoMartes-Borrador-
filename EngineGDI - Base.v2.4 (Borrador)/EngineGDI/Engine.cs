using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Media;
using System.Windows.Forms;


namespace EngineGDI
{
    public static class Engine
    {
        private class DrawCommand
        {
            public string TexturePath;
            public float X, Y, ScaleX, ScaleY;
            public float Angle, OffsetX, OffsetY;
        }
        private class TextCommand
        {
            public string Text;
            public float X, Y;
            public int FontSize;
            public Color Color;
        }
        private static Dictionary<string, Image> textures = new Dictionary<string, Image>();
        private static Dictionary<string, SoundPlayer> sounds = new Dictionary<string, SoundPlayer>();
        private static List<DrawCommand> drawQueue = new List<DrawCommand>();
        private static List<TextCommand> textQueue = new List<TextCommand>();
        private static Dictionary<int, Font> fontCache = new Dictionary<int, Font>();
        private static Dictionary<int, Brush> brushCache = new Dictionary<int, Brush>();
        private static string uiFontFamily = "Consolas";
        private static FontStyle uiFontStyle = FontStyle.Bold;
        private static PrivateFontCollection uiPrivateFonts;
        private static FontFamily uiFontFamilyObject;
        private static GameForm window;
        public static bool IsWindowOpen { get; private set; } = false;
        public static Form Window => window;

        private static HashSet<Keys> pressedKeys = new HashSet<Keys>();
        private static HashSet<Keys> handledKeys = new HashSet<Keys>();
        private static HashSet<Keys> releasedKeys = new HashSet<Keys>();
        private static HashSet<Keys> handledReleasedKeys = new HashSet<Keys>();

        private static List<string> debugMessages = new List<string>();
        private static Font debugFont = new Font("Consolas", 12);
        private static Brush debugBrush = Brushes.White;
        public static void Initialize(string title = "Game", int width = 800, int height = 600, bool fullscreen = false)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            window = new GameForm
            {
                Text = title,
                ClientSize = new Size(width, height),
                StartPosition = FormStartPosition.CenterScreen
            };
            if (fullscreen)
                window.WindowState = FormWindowState.Maximized;
            window.FormClosed += (s, e) => IsWindowOpen = false;
            window.KeyDown += (s, e) =>
            {
                if (!pressedKeys.Contains(e.KeyCode))
                {
                    pressedKeys.Add(e.KeyCode);
                    handledKeys.Remove(e.KeyCode);
                }

                releasedKeys.Remove(e.KeyCode);
                handledReleasedKeys.Remove(e.KeyCode);
            };
            window.KeyUp += (s, e) =>
            {
                pressedKeys.Remove(e.KeyCode);
                handledKeys.Remove(e.KeyCode);
                releasedKeys.Add(e.KeyCode);
                handledReleasedKeys.Remove(e.KeyCode);
            };
            window.Show();
            window.Focus();
            window.KeyPreview = true;
            IsWindowOpen = true;
        }
        public static void UpdateWindow()
        {
            if (window != null && window.Created)
                Application.DoEvents();
        }
        public static void PlaySound(string path)
        {
            if (!sounds.ContainsKey(path))
                sounds[path] = new SoundPlayer(path);
            sounds[path].Play();
        }
        public static void Draw(string path, float x, float y, float scaleX = 1f, float scaleY = 1f, float angle = 0f, float offsetX = 0f, float offsetY = 0f)
        {
            if (!textures.ContainsKey(path))
                textures[path] = Image.FromFile(path);
            drawQueue.Add(new DrawCommand
            {
                TexturePath = path,
                X = x,
                Y = y,
                ScaleX = scaleX,
                ScaleY = scaleY,
                Angle = angle,
                OffsetX = offsetX,
                OffsetY = offsetY
            });
        }

        public static void DrawText(string text, float x, float y, int fontSize)
        {
            DrawText(text, x, y, fontSize, Color.White);
        }

        public static void DrawText(string text, float x, float y, int fontSize, Color color)
        {
            textQueue.Add(new TextCommand
            {
                Text = text,
                X = x,
                Y = y,
                FontSize = fontSize,
                Color = color
            });
        }

        public static void SetUIFont(string fontFamily)
        {
            SetUIFont(fontFamily, FontStyle.Bold);
        }

        public static void SetUIFont(string fontFamily, FontStyle style)
        {
            if (string.IsNullOrWhiteSpace(fontFamily)) return;
            uiFontFamily = fontFamily;
            uiFontStyle = style;
            uiFontFamilyObject = null;
            if (uiPrivateFonts != null)
            {
                uiPrivateFonts.Dispose();
                uiPrivateFonts = null;
            }
            foreach (var kv in fontCache)
                kv.Value.Dispose();
            fontCache.Clear();
        }

        public static bool SetUIFontFromFile(string fontFilePath)
        {
            return SetUIFontFromFile(fontFilePath, FontStyle.Bold);
        }

        public static bool SetUIFontFromFile(string fontFilePath, FontStyle style)
        {
            if (string.IsNullOrWhiteSpace(fontFilePath)) return false;

            try
            {
                uiPrivateFonts?.Dispose();
                uiPrivateFonts = new PrivateFontCollection();
                uiPrivateFonts.AddFontFile(fontFilePath);

                if (uiPrivateFonts.Families == null || uiPrivateFonts.Families.Length == 0)
                    return false;

                uiFontFamilyObject = uiPrivateFonts.Families[0];
                uiFontFamily = uiFontFamilyObject.Name;
                uiFontStyle = style;

                foreach (var kv in fontCache)
                    kv.Value.Dispose();
                fontCache.Clear();

                return true;
            }
            catch
            {
                return false;
            }
        }
        public static void Clear(string sprite, Color color)
        {
            window.ClearColor = color;
        }

        public static bool OnKeyDown(Keys key)
        {
            if (pressedKeys.Contains(key) && !handledKeys.Contains(key))
            {
                handledKeys.Add(key);
                return true;
            }
            return false;
        }

        public static bool OnKeyUp(Keys key)
        {
            if (releasedKeys.Contains(key) && !handledReleasedKeys.Contains(key))
            {
                handledReleasedKeys.Add(key);
                return true;
            }
            return false;
        }
        public static bool IsKeyDown(Keys key)
        {
            return pressedKeys.Contains(key);
        }
        // Alias de compatibilidad: mismo comportamiento que OnKeyDown
        public static bool IsKeyPressed(Keys key)
        {
            return OnKeyDown(key);
        }
        public static void DebugLog(string message)
        {
            debugMessages.Add(message);
        }
        public static void ClearDebug()
        {
            debugMessages.Clear();
        }
        private class GameForm : Form
        {
            public Color ClearColor = Color.Black;
            public GameForm()
            {
                DoubleBuffered = true;
            }
            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.Clear(ClearColor);
                foreach (var cmd in drawQueue)
                {
                    if (textures.ContainsKey(cmd.TexturePath))
                    {
                        var img = textures[cmd.TexturePath];
                        float width = img.Width * cmd.ScaleX;
                        float height = img.Height * cmd.ScaleY;
                        // Transformación: traslación al punto, rotación, luego dibujar con offset
                        e.Graphics.TranslateTransform(cmd.X, cmd.Y);
                        e.Graphics.RotateTransform(cmd.Angle);
                        e.Graphics.DrawImage(
                            img,
                            -cmd.OffsetX * width,
                            -cmd.OffsetY * height,
                            width,
                            height
                        );
                        e.Graphics.ResetTransform();
                    }
                }
                for (int i = 0; i < textQueue.Count; i++)
                {
                    var cmd = textQueue[i];
                    int fontKey = cmd.FontSize;
                    if (!fontCache.TryGetValue(fontKey, out var font))
                    {
                        font = uiFontFamilyObject != null
                            ? new Font(uiFontFamilyObject, cmd.FontSize, uiFontStyle)
                            : new Font(uiFontFamily, cmd.FontSize, uiFontStyle);
                        fontCache[fontKey] = font;
                    }

                    int brushKey = cmd.Color.ToArgb();
                    if (!brushCache.TryGetValue(brushKey, out var brush))
                    {
                        brush = new SolidBrush(cmd.Color);
                        brushCache[brushKey] = brush;
                    }

                    e.Graphics.DrawString(cmd.Text, font, brush, cmd.X, cmd.Y);
                }
                float debugY = 10;
                foreach (var msg in debugMessages)
                {
                    e.Graphics.DrawString(msg, debugFont, debugBrush, 10, debugY);
                    debugY += debugFont.Height + 2;
                }
                drawQueue.Clear();
                textQueue.Clear();
            }
        }
    }
}
